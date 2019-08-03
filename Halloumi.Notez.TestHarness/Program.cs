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
            var sourceLibrary = new SectionGenerator(folder);

            while (true)
            {
                foreach (string midiFile in Directory.EnumerateFiles(".", "*.mid")) File.Delete(midiFile);
                var now = DateTime.Now.ToString("yyyymmddhhss");
                sourceLibrary.GenerateRiffs(now, 20);

                //sourceLibrary.MergeSourceClips();

                Console.WriteLine("push any key..");
                Console.ReadLine();
            }
            
        }

    }
}



