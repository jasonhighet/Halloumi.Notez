using Halloumi.Notez.Engine.Notes;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tools;
using System;
using System.IO;
using System.Linq;

namespace Halloumi.Notez.Engine.Midi
{
    public class MidiHelper
    {
        private const int NoteOffset = 24;
        private const int DrumChannel = 9;

        public static void SaveToMidi(Section section, string filepath)
        {
            var builder = new MidiBuilder(section);
            builder.SaveToFile(filepath);
        }

        public static void SaveToCsv(Section section, string filepath)
        {
            var builder = new MidiBuilder(section);
            builder.SaveToCsvFile(filepath);
        }

        public static void SaveMidiAsCsv(string filepath)
        {
            var midi = MidiFile.Read(filepath);
            var csvConverter = new CsvConverter();

            filepath = Path.Combine(Path.GetDirectoryName(filepath) + "", Path.GetFileNameWithoutExtension(filepath) + ".csv");
            csvConverter.ConvertMidiFileToCsv(midi, filepath, true);
        }


        public static Section ReadMidi(string filepath)
        {
            var midi = MidiFile.Read(filepath);

            var section = new Section(Path.GetFileNameWithoutExtension(filepath));
            foreach (var midiChunk in midi.Chunks)
            {
                if (!(midiChunk is TrackChunk chunk))
                    continue;

                var phrase = new Phrase();

                var programEvent = chunk.Events.OfType<ProgramChangeEvent>().FirstOrDefault();
                if (programEvent != null)
                    phrase.Instrument = (MidiInstrument)(int) programEvent.ProgramNumber;

                var nameEvent = chunk.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault();
                if (nameEvent != null)
                    phrase.Description = nameEvent.Text.Replace("\0", "");

                var tempoEvent = chunk.Events.OfType<SetTempoEvent>().FirstOrDefault();
                if (tempoEvent != null)
                    phrase.Bpm = Math.Round(1M / (tempoEvent.MicrosecondsPerQuarterNote / 60M) * 1000000M, 2);

                using (var manager = new TimedEventsManager(chunk.Events))
                {
                    phrase.Elements = manager.Events
                        .Where(x => x.Event is NoteOnEvent)
                        .Select(GetNewPhraseElement)
                        .ToList();

                    var offNotes = manager.Events
                        .Where(x => x.Event is NoteOffEvent)
                        .Select(x => new Tuple<decimal, int>(Convert.ToDecimal(x.Time) / 24M,
                            ((NoteOffEvent)x.Event).NoteNumber - NoteOffset))
                        .ToList();

                    foreach (var element in phrase.Elements)
                    {
                        var offNote = offNotes
                            .FirstOrDefault(x => x.Item1 > element.Position && x.Item2 == element.Note);
                        if (offNote == null) throw new ApplicationException("No off note found");
                        element.Duration = offNote.Item1 - element.Position;
                    }
                }

                phrase.IsDrums = chunk.Events.OfType<NoteOnEvent>().Any(x => x.Channel == (FourBitNumber)DrumChannel);

                phrase.PhraseLength = NoteHelper.GetTotalDuration(phrase);

                phrase.Elements = phrase.Elements.OrderBy(x => x.Position).ThenBy(x => x.Note).ToList();

                section.Phrases.Add(phrase);
            }

            if (section.Phrases.Count == 0)
                throw new ApplicationException("Invalid Midi File");

            // remove blank tempo phrase
            if (section.Phrases.Count > 1 
                && section.Phrases[0].PhraseLength == 0 
                && section.Phrases[0].Instrument == MidiInstrument.AcousticGrandPiano
                && !section.Phrases[0].IsDrums)
            {
                section.Phrases.RemoveAt(0);
            }

            return section;
        }

        private static PhraseElement GetNewPhraseElement(TimedEvent timedEvent)
        {
            if (!(timedEvent.Event is NoteOnEvent noteOn)) throw new ArgumentNullException();

            var element = new PhraseElement()
            {
                Note = noteOn.NoteNumber - NoteOffset,
                Position = Convert.ToDecimal(timedEvent.Time) / 24M,
                Velocity = (int)noteOn.Velocity
            };

            return element;
        }

        public static void RunTests(string folder)
        {

            var sectionFiles = Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories)
                .OrderBy(x => Path.GetFileNameWithoutExtension(x) + "")
                //.Where(x=>x.EndsWith("ATG-Blinded2 1.mid"))
                .ToList();

            foreach (var sectionFile in sectionFiles)
            {
                var section = ReadMidi(sectionFile);
                TestMidi(section, sectionFile);
            }
        }

        public static void TestMidi(Section section, string description)
        {
            SaveToMidi(section, "test.mid");

            var testSection = ReadMidi("test.mid");
            if (testSection.Phrases.Count != section.Phrases.Count)
            {
                Console.WriteLine($"Error saving {description} - different phrase counts");
            }

            foreach (var testPhrase in testSection.Phrases)
            {
                var sourcePhrase = section.Phrases[testSection.Phrases.IndexOf(testPhrase)];

                if (testPhrase.Elements.Count != sourcePhrase.Elements.Count)
                {
                    Console.WriteLine($"Error saving {description} {sourcePhrase.Description} - different note counts");
                }
                if (testPhrase.Bpm != sourcePhrase.Bpm)
                {
                    Console.WriteLine($"Error saving {description} {sourcePhrase.Description} - different bpm");
                }
                if (testPhrase.Instrument != sourcePhrase.Instrument)
                {
                    Console.WriteLine($"Error saving {description} {sourcePhrase.Description} - different instruments");
                }
                if (testPhrase.IsDrums != sourcePhrase.IsDrums)
                {
                    Console.WriteLine($"Error saving {description} {sourcePhrase.Description} - different drum settings");
                }
                if (testPhrase.Description != sourcePhrase.Description)
                {
                    Console.WriteLine($"Error saving {description} {sourcePhrase.Description} - different descriptions");
                }
                if (testPhrase.PhraseLength != sourcePhrase.PhraseLength)
                {
                    Console.WriteLine($"Error saving {description} {sourcePhrase.Description} - different lengths");
                }


                foreach (var testElement in testPhrase.Elements)
                {
                    var index = testPhrase.Elements.IndexOf(testElement);
                    if (index >= sourcePhrase.Elements.Count)
                        break;

                    var sourceElement = sourcePhrase.Elements[index];
                    if (testElement.Note == sourceElement.Note 
                        && Math.Abs(testElement.Duration - sourceElement.Duration) < 0.00000000000000000001M
                        && Math.Abs(testElement.OffPosition - sourceElement.OffPosition) < 0.00000000000000000001M
                        && Math.Abs(testElement.Position - sourceElement.Position) < 0.00000000000000000001M) continue;

                    Console.WriteLine($"Error saving {description} {sourcePhrase.Description} - different values on note {index}");
                    Console.WriteLine(
                        $"{sourceElement.Position},{testElement.Position}\t{sourceElement.Note},{testElement.Note}\t{sourceElement.Duration},{testElement.Duration}\t{sourceElement.OffPosition},{testElement.OffPosition}");
                }
            }
        }
    }
}
