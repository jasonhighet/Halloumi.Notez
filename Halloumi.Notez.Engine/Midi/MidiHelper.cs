using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tools;
using Halloumi.Notez.Engine.Notes;
using Melanchall.DryWetMidi.Common;

namespace Halloumi.Notez.Engine.Midi
{
    public class MidiHelper
    {
        private const int NoteOffset = 24;

        public static void SaveToMidi(Phrase phrase, string filepath)
        {
            SaveToMidi(new List<Phrase>() { phrase }, filepath);
        }

        public static void SaveToMidi(List<Phrase> phrases, string filepath)
        {
            var name = Path.GetFileNameWithoutExtension(filepath);
            var builder = new MidiBuilder(phrases, name);
            builder.SaveToFile(filepath);
        }

        public static void SaveToMidi(Section section, string filepath)
        {
            SaveToMidi(section.Phrases, filepath);
        }

        public static void SaveToCsv(List<Phrase> phrases, string filepath)
        {
            var name = Path.GetFileNameWithoutExtension(filepath);
            var builder = new MidiBuilder(phrases, name);
            builder.SaveToCsvFile(filepath);
        }

        public static void SaveMidiAsCsv(string filepath)
        {
            var midi = MidiFile.Read(filepath);
            var csvConverter = new CsvConverter();

            filepath = Path.Combine(Path.GetDirectoryName(filepath) + "", Path.GetFileNameWithoutExtension(filepath) + ".csv");
            csvConverter.ConvertMidiFileToCsv(midi, filepath, true);
        }


        public static Phrase ReadMidi(string filepath)
        {
            var midi = MidiFile.Read(filepath);

            if (midi.Chunks.Count == 0)
                throw new ApplicationException("Invalid Midi File");

            if (!(midi.Chunks[0] is TrackChunk chunk))
                throw new ApplicationException("Invalid Midi File");

            var phrase = new Phrase();

            using (var manager = new TimedEventsManager(chunk.Events))
            {
                phrase.Elements = manager.Events
                    .Where(x => x.Event is NoteOnEvent)
                    .Select(GetNewPhraseElement)
                    .ToList();

                var offNotes = manager.Events
                    .Where(x => x.Event is NoteOffEvent)
                    .Select(x => new Tuple<decimal, int>(Convert.ToDecimal(x.Time) / 24M,
                        ((NoteOffEvent) x.Event).NoteNumber - NoteOffset))
                    .ToList();

                foreach (var element in phrase.Elements)
                {
                    var offNote = offNotes
                        .FirstOrDefault(x => x.Item1 > element.Position && x.Item2 == element.Note);
                    if (offNote == null) throw new ApplicationException("No off note found");
                    element.Duration = offNote.Item1 - element.Position;
                }
            }

            phrase.IsDrums = chunk.Events.Where(x => x is ChannelEvent).Any(x => ((ChannelEvent)x).Channel == (FourBitNumber)10);

            phrase.PhraseLength = NoteHelper.GetTotalDuration(phrase);

            phrase.Elements = phrase.Elements.OrderBy(x => x.Position).ThenBy(x => x.Note).ToList();

            phrase.Description = Path.GetFileName(filepath);

            return phrase;
        }

        private static PhraseElement GetNewPhraseElement(TimedEvent timedEvent)
        {
            if (!(timedEvent.Event is NoteOnEvent noteOn)) throw new ArgumentNullException();

            var element = new PhraseElement()
            {
                Note = noteOn.NoteNumber - NoteOffset,
                Position = Convert.ToDecimal(timedEvent.Time) / 24M
            };

            return element;
        }

        public static void RunTests(string folder)
        {

            var phrases = Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories)
                .OrderBy(x => Path.GetFileNameWithoutExtension(x) + "")
                .Where(x=>x.EndsWith("ATG-Blinded2 1.mid"))
                .Select(ReadMidi)
                .ToList();

            foreach (var sourcePhrase in phrases)
            {
                SaveToMidi(sourcePhrase, "test.mid");
                var testPhrase = ReadMidi("test.mid");

                if (testPhrase.Elements.Count != sourcePhrase.Elements.Count)
                {
                    Console.WriteLine("Error saving " + sourcePhrase.Description + " - different note counts");
                }

                foreach (var testElement in testPhrase.Elements)
                {
                    var index = testPhrase.Elements.IndexOf(testElement);
                    if (index >= sourcePhrase.Elements.Count)
                        break;

                    var sourceElement = sourcePhrase.Elements[index];
                    if (testElement.Note == sourceElement.Note && testElement.Duration == sourceElement.Duration &&
                        testElement.OffPosition == sourceElement.OffPosition && testElement.Position == sourceElement.Position) continue;

                    Console.WriteLine($"Error saving {sourcePhrase.Description} - different values on note {index}");
                    Console.WriteLine($"{sourceElement.Position},{testElement.Position}\t{sourceElement.Note},{testElement.Note}\t{sourceElement.Duration},{testElement.Duration}\t{sourceElement.OffPosition},{testElement.OffPosition}");
                }
            }
        }

    }
}
