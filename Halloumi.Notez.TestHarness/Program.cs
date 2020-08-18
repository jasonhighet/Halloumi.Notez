using Halloumi.Notez.Engine.Generator;
using System;
using System.IO;
using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using Halloumi.Notez.Engine.Tabs;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = @"..\..\..\Halloumi.Notez.Engine\SourceMidi\doom";

            var section = MidiHelper.ReadMidi(folder + @"\AC-Gasoline1.mid");
            var phrase = section.Phrases[0];
            NoteHelper.ShiftNotesDirect(phrase, 2, Interval.Step);


            var tab = TabHelper.GenerateTab(phrase, "E,B,G,D,A,D");
            Console.WriteLine(tab);

            Console.WriteLine("push any key..");
            Console.ReadLine();

            
        }

    }
}



