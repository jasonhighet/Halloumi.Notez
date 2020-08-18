using Halloumi.Notez.Engine.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Halloumi.Notez.Engine.Tabs
{
    public class TabParser
    {
        public TabParser() : this("E,B,G,D,A,E")
        {
      
        }

        public TabParser(string tabTuning)
        {
            SetTabTuning(tabTuning);
            NoteDivision = 32;
        }

        public int NoteDivision { get; set; }

        public void SetTabTuning(string tabTuning)
        {
            var notes = tabTuning.Split(',').ToList();
            notes.Reverse();

            TabLines = new List<TabLine>();

            var lastRoot = int.MinValue;
            foreach (var note in notes)
            {
                var number = NoteHelper.NoteToNumber(note);

                if (number <= lastRoot)
                    number += 12;

                TabLines.Add(new TabLine() {Number = number});

                lastRoot = number;
            }

            TabLines.Reverse();
        }

        public List<TabNote> TabNotes { get; private set; }

        public List<TabLine> TabLines { get; private set; }

        public string GetTuningDescription()
        {
            var roots = TabLines.Select(x => NoteHelper.NumberToNoteOnly(x.Number)).ToList();
            return string.Join(",", roots);
        }


        public void LoadTuningFromTabText(string tabText)
        {
            var regex = new Regex("[^A-Z#]");

            var roots = ConvertTextToTabLines(tabText.ToUpper())
                .Select(x => regex.Replace(x, " "))
                .Select(x => x.Trim() + "  ")
                .Select(x => x.Substring(0, 2))
                .Select(x => x.Trim())
                .Where(x => x != "")
                .ToList();

            var tuning = string.Join(",", roots);

            if (tuning != "")
                SetTabTuning(tuning);
        }

        private static List<string> ConvertTextToTabLines(string text)
        {
            var lines = new List<string>(text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries));

            var lastDash = GetLastDashPosition(lines);
            if (lastDash < 0) return lines;

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length <= lastDash)
                    lines[i] = lines[i].PadRight(lastDash + 1, '-');
                else
                    lines[i] = lines[i].Substring(0, lastDash + 1);
            }

            return lines;
        }

        private IEnumerable<TabNote> GetTabNotesFromTabLine(int lineIndex, string tabLine)
        {
            var regex = new Regex("([0-9])+");

            return from Match match in regex.Matches(tabLine)
                select new TabNote()
                {
                    Line = lineIndex,
                    PositionInCharacters = match.Index,
                    Fret = Convert.ToInt32(match.Value),
                    Number = TabLines[lineIndex].Number + Convert.ToInt32(match.Value)
                };
        }

        public void LoadTabFromTabText(string[] tabText)
        {
            LoadTabFromTabText(string.Join("\r\n", tabText));
        }

        public void LoadTabFromTabText(string tabText)
        {
            tabText = tabText.Replace("—", "-");

            TabNotes = new List<TabNote>();
            LoadTuningFromTabText(tabText);

            var tabLines = ConvertTextToTabLines(tabText);

            for (var i = 0; i < tabLines.Count; i++)
            {
                TabNotes.AddRange(GetTabNotesFromTabLine(i, tabLines[i]));
            }
            TabNotes = TabNotes.OrderBy(x => x.PositionInCharacters).ThenBy(x => x.Line).ToList();

            var chords = TabNotes
                .GroupBy(m => new {Position = m.PositionInCharacters})
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(r => r.Line)
                    .Skip(1))
                .ToList();

            TabNotes = TabNotes.Except(chords).ToList();

            CalculatePositionsAndLengths(tabLines);
        }

        public List<int> GetDistinctNoteNumbers()
        {
            return TabNotes.Select(x => x.Number).Distinct().OrderBy(x => x).ToList();
        }

        private void CalculatePositionsAndLengths(IEnumerable<string> tabLines)
        {
            var offset = TabNotes[0].PositionInCharacters;
            var tabWidth = GetLastDashPosition(tabLines);
            var stepLength = 1M / Convert.ToDecimal(NoteDivision);


            tabWidth -= offset;
            foreach (var tabNote in TabNotes)
            {
                tabNote.PositionInCharacters -= offset;
                tabNote.TabWidth = tabWidth;
            }


            for (var i = 0; i < TabNotes.Count; i++)
            {
                var tabNote = TabNotes[i];
                var nextTabNote = (i < TabNotes.Count - 1) ? TabNotes[i + 1] : null;

                if (nextTabNote != null)
                    tabNote.LengthInCharacters = nextTabNote.PositionInCharacters - tabNote.PositionInCharacters;
                else
                    tabNote.LengthInCharacters = tabNote.TabWidth - tabNote.PositionInCharacters;
            }


            foreach (var tabNote in TabNotes)
            {
                tabNote.Length = Math.Round(tabNote.IdealLengthPercent * NoteDivision, 0) /
                                 Convert.ToDecimal(NoteDivision);
                if (tabNote.Length == 0) tabNote.Length = stepLength;
            }
            CalculatePositions(TabNotes);


            var remainder = 1M - TabNotes.Sum(x => x.Length);
            var stepsToDistribute = Convert.ToInt32(remainder / stepLength);
            var removeSteps = stepsToDistribute < 0;

            while (Math.Abs(remainder) >= stepLength)
            {
                var possibilities = new Dictionary<TabNote, List<TabNote>>();
                foreach (var tabNote in TabNotes)
                {
                    var possibility = TabNotes.Select(x => x.Clone()).ToList();
                    var changedTabNote = possibility.FirstOrDefault(x => x.PositionInCharacters == tabNote.PositionInCharacters);
                    if(changedTabNote == null) continue;

                    if (removeSteps)
                    {
                        if (changedTabNote.Length > stepLength)
                        {
                            changedTabNote.Length -= stepLength;
                            possibilities.Add(tabNote, possibility);
                        }
                    }
                    else
                    {
                        changedTabNote.Length += stepLength;
                        possibilities.Add(tabNote, possibility);
                    }
                    CalculatePositions(possibility);
                }

                var tabNoteToChange = possibilities.OrderBy(x => x.Value.Sum(y => y.DifferenceFromIdeal)).FirstOrDefault();
                var tentativeChangedTabNote = tabNoteToChange.Value.FirstOrDefault(x => x.PositionInCharacters == tabNoteToChange.Key.PositionInCharacters);

                if (tentativeChangedTabNote != null)
                    tabNoteToChange.Key.Length = tentativeChangedTabNote.Length;

                CalculatePositions(TabNotes);

                remainder = 1M - TabNotes.Sum(x => x.Length);
            }

            foreach (var tabNote in TabNotes)
                tabNote.Length *= NoteDivision;
        }

        private static void CalculatePositions(IEnumerable<TabNote> tabNotes)
        {
            var position = 0M;
            foreach (var tabNote in tabNotes)
            {
                tabNote.Position = position;
                position += tabNote.Length;
            }
        }

        private static int GetLastDashPosition(IEnumerable<string> tabLines)
        {
            return tabLines.Select(line => line.LastIndexOf("-", StringComparison.Ordinal)).Concat(new[] {0}).Max();
        }


        public void LoadTabFromPhrase(Phrase phrase)
        {
            TabNotes = new List<TabNote>();

            var lowestNote = TabLines.Last().Number;
            var lowestNote2 = TabLines[4].Number;
            var lowestNote3 = TabLines[3].Number;

            foreach (var element in phrase.Elements)
            {
                var fret = element.Note - lowestNote;
                var line = TabLines.IndexOf(TabLines.Last());

                if (fret >= 10)
                {
                    line--;
                    fret = element.Note - lowestNote2;
                    if (fret >= 10)
                    {
                        line--;
                        fret = element.Note - lowestNote3;
                    }
                }

                var tabNote = new TabNote()
                {
                    Number = element.Note,
                    Length = element.Duration,
                    Fret = fret,
                    Line = line,
                    LengthInCharacters = Convert.ToInt32(element.Duration * 2) + 2
                };


                TabNotes.Add(tabNote);
            }

            var position = 0M;
            var positionInCharacters = 0;
            foreach (var tabNote in TabNotes)
            {
                tabNote.Position = position;
                position += tabNote.Length;

                tabNote.PositionInCharacters = positionInCharacters;
                positionInCharacters += tabNote.LengthInCharacters;
            }
        }



        public class TabLine
        {
            public int Number { get; set; }

            public string Note => NoteHelper.NumberToNote(Number);
        }

        public class TabNote
        {
            public int Line { get; set; }
            public int Fret { get; set; }
            public int Number { get; set; }
            public string Note => NoteHelper.NumberToNote(Number);
            public int PositionInCharacters { get; set; }
            public int LengthInCharacters { get; set; }

            public decimal IdealPositionPercent => PositionInCharacters / (decimal) TabWidth;

            public decimal IdealLengthPercent => LengthInCharacters / (decimal) TabWidth;

            public int TabWidth { get; set; }
        
            public decimal Length { get; set; }
            public decimal Position { get; set; }

            public decimal DifferenceFromIdeal => (Math.Abs(IdealPositionPercent - Position) + Math.Abs(IdealLengthPercent - Length)) / 2;
    

            public TabNote Clone()
            {
                return new TabNote()
                {
                    Line = Line,
                    Length = Length,
                    Position = Position,
                    Number = Number,
                    LengthInCharacters = LengthInCharacters,
                    PositionInCharacters = PositionInCharacters,
                    Fret = Fret,
                    TabWidth = TabWidth
                };
            }
         }
    }
}