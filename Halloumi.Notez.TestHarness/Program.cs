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
            var folder = @".\TestMidi\Death";
            //    var sourceLibrary = new SourceLibrary();
            //    sourceLibrary.LoadLibrary(folder);

            var files = Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories)
                .Select(x => new { Phrase = MidiHelper.ReadMidi(x), File = x })
                .Where(x=> IsDamaged(x.Phrase))
                .Select(x=>x.File)
                .ToList();

            Console.WriteLine(files.Count);

            foreach (var file in files)
            {
                var midi = MidiFile.Read(file);
                //if (midi.Chunks.Count == 0)
                //    throw new ApplicationException("Invalid Midi File");

                //if (!(midi.Chunks[0] is TrackChunk chunk))
                //    throw new ApplicationException("Invalid Midi File");

                ////var noteOns = chunk.Events.Where(x => x is NoteOnEvent).ToList();

                //var i = 4;
                //while (chunk.Events[i] is PitchBendEvent)
                //{
                //    chunk.Events[i].DeltaTime = 0;
                //    i++;
                //}


                var newFile = @"C:\Users\jason\Desktop\metalmidi\test\" + Path.GetFileName(file);

                midi.Write(newFile, true, MidiFileFormat.SingleTrack);

                //using (var manager = new TimedEventsManager(chunk.Events))
                //{
                //    foreach (var element in manager.Events)
                //    {
                //    }
                //}

            }

            //var counts = midis.Select(x => x.PhraseLength - x.Elements.Min(y => y.Position))
            //    .GroupBy(x => x)
            //    .Select(x => new { Count = x.Count(), Length = x.Key })
            //    .Where(x=>x.Length != 16 && x.Length != 32 && x.Length != 64 && x.Length != 128 && x.Length != 256)
            //    .OrderBy(x=> x.Length)
            //    .ToList();

            //var midi = MidiHelper.ReadMidi(".\\TestMidi\\Death\\bridge\\ATG-Blinded4 1.mid");


            Console.ReadLine();
        }

        public static bool IsDamaged(Phrase phrase)
        {
            //if (phrase.Elements.Min(y => y.Position) == 0)
            //    return false;

            var length = phrase.PhraseLength - phrase.Elements.Min(y => y.Position);

            if (length != 16 && length != 32 && length != 64 && length != 128 && length != 256)
                return true;

            return false;
        }
    }
}



