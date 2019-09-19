using Halloumi.Notez.Engine.Generator;
using System;
using System.IO;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = @"..\..\..\Halloumi.Notez.Engine\SourceMidi\";

            var sourceLibrary = new SectionGenerator(folder);
            sourceLibrary.LoadLibrary("Death", true);


            foreach (var midiFile in Directory.EnumerateFiles(".", "*.mid")) File.Delete(midiFile);
            var now = DateTime.Now.ToString("yyyymmddhhss");

            //sourceLibrary.GenerateRiffs(now, 20, new SectionGenerator.SourceFilter() { SeedArtist = "SL" });
            //sourceLibrary.GenerateRiffs(now, 20);
             
            Console.WriteLine("push any key..");
            Console.ReadLine();

            sourceLibrary.MergeSourceClips();
        }

    }
}



