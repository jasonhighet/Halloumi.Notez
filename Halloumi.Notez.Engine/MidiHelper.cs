using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine
{
    public class MidiHelper
    {
        public static void SaveToMidi(Phrase phrase, string filepath)
        {
            var builder = BuildMidi(phrase);
            builder.SaveToFile(filepath);
        }

        public static void SaveToCsv(Phrase phrase, string filepath)
        {
            var builder = BuildMidi(phrase);
            builder.SaveToCsvFile(filepath);

        }

        private static MidiBuilder BuildMidi(Phrase phrase)
        {

            var midiBuilder = new MidiBuilder();

            foreach (var element in phrase.Elements)
            {
                midiBuilder.AddNote(element.Note, element.Duration);
            }

            return midiBuilder;
        }
    }
}
