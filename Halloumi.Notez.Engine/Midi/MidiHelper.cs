using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tools;
using Halloumi.Notez.Engine.Notes;

namespace Halloumi.Notez.Engine.Midi
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
            phrase = phrase.Clone();
            
            
            PhraseHelper.UnmergeRepeatedNotes(phrase);
            PhraseHelper.UpdateDurationsFromPositions(phrase, phrase.PhraseLength);
            //PhraseHelper.UnmergeChords(phrase);


            var midiBuilder = new MidiBuilder(phrase.Description, phrase.Bpm, instrument);

            var positions = phrase.Elements.Select(x => x.Position)
                .Union(phrase.Elements.Select(x => x.Position + x.Duration))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            foreach (var position in positions)
            {
                var notesOff = phrase.Elements.Where(x => x.Position + x.Duration == position).ToList();
                foreach (var noteOff in notesOff)
                {
                    midiBuilder.AddNoteOff(noteOff.Note, noteOff.Duration);
                }

                var notesOn = phrase.Elements.Where(x => x.Position == position).ToList();
                foreach (var noteOn in notesOn)
                {
                    midiBuilder.AddNoteOn(noteOn.Note);
                }

            }

                //foreach (var element in phrase.Elements)
                //{
                //    midiBuilder.AddNote(element.Note, element.Duration);
                //}

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
                    .Select(x => new Tuple<decimal, int>(x.Time / 24M,
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

            phrase.PhraseLength = NoteHelper.GetTotalDuration(phrase);

            phrase.Elements = phrase.Elements.OrderBy(x => x.Position).ThenBy(x => x.Note).ToList();

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
