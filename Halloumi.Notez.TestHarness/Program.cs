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
            const int riffCount = 8;
            for (var i = 1; i <= riffCount; i++)
            {
                var midiPath = @"RandomRiff" + i + ".mid";

                var generator = new PhraseGenerator();
                var phrase = generator.GeneratePhrase();
                MidiHelper.SaveToMidi(phrase, midiPath);
            }

            Console.ReadLine();
        }
    }
}
