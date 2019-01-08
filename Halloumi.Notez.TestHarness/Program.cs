using System;
using System.IO;
using System.Linq;
using Halloumi.Notez.Engine;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var riffs = Directory.GetFiles("TestMidi", "*.mid")
                .Select(MidiHelper.ReadMidi)
                .Where(riff => NoteHelper.GetTotalDuration(riff) == 32)
                .Where(riff => ScaleHelper.FindMatchingScales(riff).Select(x=> x.Scale.Name).Contains("C Natural Minor"))
                .ToList();

            Console.ReadLine();
        }
    }
}
