using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using System.IO;
using System.Linq;

namespace Halloumi.Notez.Engine.OldGenerator
{
    public class RiffGeneratorOld
    {
        public void GenerateRiffs(string folder)
        {
            const int riffCount = 24;

            DeleteMidiFiles();

            var generator = new PhraseGeneratorOld(32, folder);
            for (var i = 1; i <= riffCount; i++)
            {
                var phrase = generator.GeneratePhrase();
                string midiPath = GetFileName(i, phrase);
                phrase.Instrument = MidiInstrument.OverdrivenGuitar;

                var section = new Section();
                section.Phrases.Add(phrase);

                MidiHelper.SaveToMidi(section, midiPath);
            }
        }

        private static string GetFileName(int index, Phrase phrase)
        {
            var lowNote = phrase.Elements.Min(x => x.Note);
            var getUpCount = GetUpCount(lowNote);
            if (getUpCount == 0)
                return @"RandomRiff" + index + ".mid";
            else
                return @"RandomRiff" + index + "_" + getUpCount + ".mid";
        }

        private static void DeleteMidiFiles()
        {
            foreach (string file in Directory.EnumerateFiles(".", "*.mid"))
            {
                File.Delete(file);
            }
        }

        private static int GetUpCount(int lowNote)
        {
            var noteName = NoteHelper.NumberToNote(lowNote);
            if (noteName == "B2" || noteName == "C3")
                return 0;

            return 35 - lowNote;
        }
    }
}
