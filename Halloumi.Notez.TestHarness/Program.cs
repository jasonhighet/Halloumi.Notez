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
            var generator = new PhraseGenerator();
            
            const int riffCount = 8;
            for (var i = 0; i < riffCount; i++)
            {
                var phrase = generator.GeneratePhrase();
                MidiHelper.SaveToMidi(phrase, "Riff" + i + ".mid");
            }
            // Console.ReadLine();
        }
    }
}
