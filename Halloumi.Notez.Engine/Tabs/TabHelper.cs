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

        public static string GenerateTab(Phrase phrase, string tuning = "E,B,G,D,A,E", bool oneLineIfPossible = false)
        {

            phrase = phrase.Clone();
            while (PhraseHelper.IsPhraseDuplicated(phrase))
            {
                PhraseHelper.TrimPhrase(phrase, phrase.PhraseLength / 2);
            }

            var tabParser = new TabParser(tuning);

            var lowNotes = phrase.Elements.Where(x=> x.Note < tabParser.TabLines.Last().Number).ToList();
            foreach(var lowNote in lowNotes ) 
            {
                lowNote.Note = NoteHelper.ShiftNote(lowNote.Note, 1, Interval.Octave);
            }

            var tabText = "";
            var tabSections = Convert.ToInt32(phrase.PhraseLength / 32);
            for (var i = 0; i < tabSections; i++)
            {
                var start = i * 32;
                
                var sectionPhrase = phrase.Clone();
                PhraseHelper.CropPhrase(sectionPhrase, start, 32);
                tabParser.LoadTabFromPhrase(sectionPhrase, oneLineIfPossible);
                tabText += BuildTab(tabParser);
            }

            return tabText;

            
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
            builder.AppendLine();

            return builder.ToString();
        }

    }
}
