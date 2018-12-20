using System;
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

            var phrase = MidiHelper.ReadMidi(@"TestMidi\void.mid");
            foreach (var element in phrase.Elements)
            {
                Console.WriteLine(NoteHelper.NumberToNote(element.Note) + "\t" + element.Duration);
            }

            var scaleName = ScaleHelper.FindMatchingScales(phrase)[0].Scale.Name;
            Console.WriteLine(scaleName);


            //MidiHelper.SaveMidiAsCsv(@"TestMidi\void.mid");

            Console.ReadLine();
        }
    }
}
