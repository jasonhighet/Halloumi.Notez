using System;
using Halloumi.Notez.Engine.Notes;
using System.IO;
using System.Linq;
using System.Text;
using Halloumi.Notez.Engine.Midi;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Halloumi.Notez.Engine.Tabs
{
    public static class TabHelper
    {
        public static Phrase ParseTab(string tabText)
        {

            var tab = new TabParser();
            tab.LoadTabFromTabText(tabText);

            var phrase = new Phrase
            {
                Elements = tab
                    .TabNotes
                    .Select(x => new PhraseElement { Note = x.Number, Duration = x.Length })
                    .ToList()
            };

            PhraseHelper.UpdatePositionsFromDurations(phrase);

            return phrase;
        }

        public static Phrase ParseTabFile(string filepath)
        {
            var tabText = File.ReadAllLines(filepath);

                var tab = new TabParser();
                tab.LoadTabFromTabText(tabText);

                var phrase = new Phrase
                {
                    Elements = tab
                        .TabNotes
                        .Select(x => new PhraseElement {Note = x.Number, Duration = x.Length})
                        .ToList()
                };

            PhraseHelper.UpdatePositionsFromDurations(phrase);

            return phrase;

        }

        public static string GenerateTab(Phrase phrase, string tuning = "E,B,G,D,A,E")
        {

            phrase = phrase.Clone();
            while (PhraseHelper.IsPhraseDuplicated(phrase))
            {
                PhraseHelper.TrimPhrase(phrase, phrase.PhraseLength / 2);
            }

            var tabParser = new TabParser(tuning);

            while (phrase.Elements.Min(x => x.Note) > tabParser.TabLines.Last().Number)
            {
                NoteHelper.ShiftNotesDirect(phrase, 1, Interval.Octave, Direction.Down);
            }

            while (phrase.Elements.Min(x => x.Note) < tabParser.TabLines.Last().Number)
            {
                NoteHelper.ShiftNotesDirect(phrase, 1, Interval.Octave);
            }



            tabParser.LoadTabFromPhrase(phrase);


            return BuildTab(tabParser);
        }

        private static string BuildTab(TabParser parser)
        {
            var builder = new StringBuilder();

            foreach (var tabLine in parser.TabLines)
            {
                var line = parser.TabLines.IndexOf(tabLine);
                builder.Append(NoteHelper.NumberToNoteOnly(tabLine.Number).PadRight(3));
                foreach (var tabNote in parser.TabNotes)
                {
                    builder.Append(tabNote.Line == line
                        ? tabNote.Fret.ToString().PadRight(tabNote.LengthInCharacters)
                        : string.Empty.PadRight(tabNote.LengthInCharacters));
                }

                builder.AppendLine();
            }

            builder.Append("   ");
            foreach (var tabNote in parser.TabNotes)
            {
                builder.Append(tabNote.Note.PadRight(tabNote.LengthInCharacters));
            }
            builder.AppendLine();

            return builder.ToString();
        }

    }
}
