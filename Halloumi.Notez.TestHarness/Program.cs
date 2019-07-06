using Halloumi.Notez.Engine;
using Halloumi.Notez.Engine.Generator;
using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = @"..\..\..\Halloumi.Notez.Engine\TestMidi\Death\";
            var sourceLibrary = new SourceLibrary(folder);
            //sourceLibrary.GenerateRiffs("riff", 10);
            sourceLibrary.RunTests();

            //var midi = MidiHelper.ReadMidi(@"riff0.mid");
            //PatternFinder.FindPatterns(phrase);
            Console.WriteLine("Finished..");
            Console.ReadLine();
        }


    }
}



