using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tools;

namespace Halloumi.Notez.Engine
{
    public class MidiHelper
    {
        private const int NoteOffset = 24;

        public static void SaveToMidi(Phrase phrase, string filepath, MidiInstrument instrument = MidiInstrument.AcousticGrandPiano)
        {
            var builder = BuildMidi(phrase, instrument);
            builder.SaveToFile(filepath);
        }

        public static void SaveToCsv(Phrase phrase, string filepath)
        {
            var builder = BuildMidi(phrase, MidiInstrument.AcousticGrandPiano);
            builder.SaveToCsvFile(filepath);
        }

        private static MidiBuilder BuildMidi(Phrase phrase, MidiInstrument instrument)
        {

            var midiBuilder = new MidiBuilder(phrase.Description, phrase.Bpm, instrument);

            foreach (var element in phrase.Elements)
            {
                midiBuilder.AddNote(element.Note, element.Duration);
            }

            return midiBuilder;
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
                    .Select(x => new Tuple<decimal, int>((decimal) x.Time / 24,
                        ((NoteOffEvent) x.Event).NoteNumber - NoteOffset))
                    .ToList();

                foreach (var element in phrase.Elements)
                {
                    var offNote = offNotes
                        .FirstOrDefault(x => x.Item1 > element.Position && x.Item2 == element.Note);
                    if (offNote == null) throw new ApplicationException("No off note found");
                    element.Duration = offNote.Item1 - element.Position;

                    element.Duration = Math.Round(element.Duration * 2, MidpointRounding.AwayFromZero) / 2;
                }
            }

            phrase.PhraseLength = NoteHelper.GetTotalDuration(phrase);

            phrase.Description = Path.GetFileName(filepath);

            return phrase;
        }

        private static PhraseElement GetNewPhraseElement(TimedEvent timedEvent)
        {
            var noteOn = timedEvent.Event as NoteOnEvent;
            if (noteOn == null) throw new ArgumentNullException();

            var element = new PhraseElement()
            {
                Note = noteOn.NoteNumber - NoteOffset,
                Position = (decimal)timedEvent.Time / 24
            };

            element.Position = Math.Round(element.Position * 2, MidpointRounding.AwayFromZero) / 2;

            return element;
        }

    }
}
