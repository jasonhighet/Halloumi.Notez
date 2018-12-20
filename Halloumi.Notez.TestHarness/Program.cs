using System;
using System.IO;
using Halloumi.Notez.Engine;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            //var phrase = TabHelper.ParseTab("Notes.tab");

            //foreach (var element in phrase.Elements)
            //{
            //    Console.WriteLine(NoteHelper.NumberToNote(element.Note) + "\t" + element.Duration);
            //}

            //var scaleName = ScaleHelper.FindMatchingScales(phrase)[0].Scale.Name;
            //Console.WriteLine(scaleName);

            //phrase = ScaleHelper.TransposeToScale(phrase, scaleName, "F Natural Minor");
            //foreach (var element in phrase.Elements)
            //{
            //    Console.WriteLine(NoteHelper.NumberToNote(element.Note) + "\t" + element.Duration);
            //}
            //scaleName = ScaleHelper.FindMatchingScales(phrase)[0].Scale.Name;
            //Console.WriteLine(scaleName);

            //MidiHelper.SaveToCsv(phrase, "test.csv");
            //MidiHelper.SaveToMidi(phrase, "test.mid");

            var files = Directory.GetFiles("TestMidi", "*.mid");

            foreach (var file in files)
            {
                Console.WriteLine(file);

                var phrase = MidiHelper.ReadMidi(file);
                Console.WriteLine(NoteHelper.GetTotalDuration(phrase));

                var scaleName = ScaleHelper.FindMatchingScales(phrase)[0].Scale.Name;
                Console.WriteLine(scaleName);

            }




            //MidiHelper.SaveMidiAsCsv(@"TestMidi\void.mid");

            Console.ReadLine();
        }
    }
}
