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
            var folder = @"..\..\..\Halloumi.Notez.Engine\TestMidi\Death\thrash\";
            var sourceLibrary = new SourceLibrary();
            sourceLibrary.LoadLibrary(folder);

            //var phrase = MidiHelper.ReadMidi(@"C: \Users\jason\Documents\GitHub\Halloumi.Notez\Halloumi.Notez.Engine\TestMidi\death\thrash\ATG-Blinded2 1.mid");
            //PatternFinder.FindPatterns(phrase);
            Console.ReadLine();
        }


    }
}



