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
            var generator = new PhraseGenerator(32, @"Doom");
            for (var i = 1; i <= riffCount; i++)
            {
                var midiPath = @"RandomRiff" + i + ".mid";
                var phrase = generator.GeneratePhrase();

                phrase = NoteHelper.ShiftNotes(phrase, 1, Interval.Octave, Direction.Down);
                PhraseHelper.DuplicatePhrase(phrase);
                PhraseHelper.DuplicatePhrase(phrase);

                MidiHelper.SaveToMidi(phrase, midiPath, MidiInstrument.DistortedGuitar);
            }

            Console.ReadLine();
        }
    }
}
