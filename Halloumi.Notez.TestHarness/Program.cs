using Halloumi.Notez.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            const int riffCount = 24;

            DeleteMidiFiles();

            var generator = new PhraseGenerator(32, @"Doom\Main");
            for (var i = 1; i <= riffCount; i++)
            {

                var phrase = generator.GeneratePhrase();
                string midiPath = GetFileName(i, phrase);

                MidiHelper.SaveToMidi(phrase, midiPath, MidiInstrument.OverdrivenGuitar);
            }

            //Console.ReadLine();
        }

        private static string GetFileName(int index, Phrase phrase)
        {
            var lowNote = phrase.Elements.Min(x => x.Note);
            var getUpCount = GetUpCount(lowNote);
            var midiPath = @"RandomRiff" + index + "_" + getUpCount + ".mid";
            return midiPath;
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
