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
            //MidiFileLibraryHelper.CopyPlaylistFiles(@"E:\OneDrive\Music\Midi\metal\doom.mpl", @"C:\Users\jason\Desktop\metalmidi\doom");


            //var folder = @"..\..\..\Halloumi.Notez.Engine\SourceMidi\Doom\";
            //var drums = MidiHelper.ReadMidi(folder + "drums.mid");
            //foreach (string midiFile in Directory.EnumerateFiles(".", "*.mid")) File.Delete(midiFile);

            //foreach (string midiFile in Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories))
            //{
            //    if (Path.GetFileName(midiFile) == "drums.mid")
            //        continue;

            //    Console.WriteLine(midiFile);

            //    var section = MidiHelper.ReadMidi(midiFile);
            //    section.Phrases[0].Bpm = 70;
            //    section.Phrases[0].Instrument = MidiInstrument.DistortedGuitar;

            //    section.Phrases.Add(section.Phrases[0].Clone());
            //    section.Phrases[1].Instrument = MidiInstrument.OverdrivenGuitar;

            //    var bassPhrase = section.Phrases[0].Clone();
            //    NoteHelper.ShiftNotesDirect(bassPhrase, 1, Interval.Octave, Direction.Down);
            //    bassPhrase.Instrument = MidiInstrument.ElectricBassPick;
            //    section.Phrases.Add(bassPhrase);

            //    var drumPhrase = drums.Phrases[0].Clone();
            //    drumPhrase.IsDrums = true;

            //    section.Phrases.Add(drumPhrase);

            //    for (int i = 0; i < 3; i++)
            //    {
            //        NoteHelper.ShiftNotesDirect(section.Phrases[i], 2, Interval.Octave, Direction.Down);
            //        VelocityHelper.ApplyVelocityStrategy(section.Phrases[i], "FlatHigh");
            //    }
            //    PhraseHelper.EnsureLengthsAreEqual(section.Phrases);

            //    MidiHelper.SaveToMidi(section, Path.GetFileName(midiFile));
            //}
            //Console.ReadLine();

            var folder = @"..\..\..\Halloumi.Notez.Engine\SourceMidi\Death\";
            var sourceLibrary = new SectionGenerator(folder);

            while (true)
            {
                foreach (string midiFile in Directory.EnumerateFiles(".", "*.mid")) File.Delete(midiFile);
                var now = DateTime.Now.ToString("yyyymmddhhss");

                //sourceLibrary.MergeSourceClips();
                sourceLibrary.GenerateRiffs(now, 20, "Kreator-Rippin2");

                Console.WriteLine("push any key..");
                Console.ReadLine();
            }

        }

    }
}



