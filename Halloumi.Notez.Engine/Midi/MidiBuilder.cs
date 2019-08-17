using Halloumi.Notez.Engine.Notes;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine.Midi
{
    public class MidiBuilder
    {
        private const int DrumChannel = 9;
        private readonly List<TrackChunk> _trackChunks;


        public MidiBuilder(Section section)
        {
            var bpm = section.Bpm;
            if (bpm == 0M) bpm = 120M;

            _trackChunks = new List<TrackChunk>();

            foreach (var phrase in section.Phrases)
            {
                var trackChunk = new TrackChunk();
                AddBpmEvent(trackChunk, bpm);
                AddTimeSignatureEvent(trackChunk);

                AddNameEvent(trackChunk, phrase.Description);
                SetInstrument(trackChunk, phrase);
                _trackChunks.Add(trackChunk);
            }

            foreach (var sourcePhrase in section.Phrases)
            {
                var index = section.Phrases.IndexOf(sourcePhrase);
                var phrase = sourcePhrase.Clone();

                PhraseHelper.UnmergeRepeatedNotes(phrase);
                PhraseHelper.UnmergeChords(phrase);

                using (var notesManager = _trackChunks[index].ManageNotes())
                {
                    var notes = phrase.Elements
                        .Select(x => new Note
                        (
                            GetMidiNoteNumber(x.Note),
                            GetMidiNoteLength(x.Duration),
                            GetMidiNoteLength(x.Position)
                        )
                        {
                            Velocity = (SevenBitNumber)x.Velocity
                        }).ToList();

                    notesManager.Notes.Add(notes);
                }

                var notesOn = _trackChunks[index].Events.OfType<NoteOnEvent>().ToList();
                foreach (var noteOn in notesOn)
                    noteOn.Channel = GetChannel(index, sourcePhrase);

                var notesOff = _trackChunks[index].Events.OfType<NoteOffEvent>().ToList();
                foreach (var noteOff in notesOff)
                    noteOff.Channel = GetChannel(index, sourcePhrase);


            }
        }

        private static void AddBpmEvent(TrackChunk trackChunk, decimal bpm)
        {
            trackChunk.Events.Add(new SetTempoEvent(GetBpmAsMicroseconds(bpm)));
        }

        private void SetInstrument(TrackChunk chunk, Phrase phrase)
        {
            var programChange = new ProgramChangeEvent()
            {
                ProgramNumber = GetProgramNumber(phrase),
                Channel = GetChannel(_trackChunks.Count, phrase)
            };

            chunk.Events.Add(programChange);
        }

        private static void AddNameEvent(TrackChunk chunk, string name)
        {
            if (name == "") return;
            chunk.Events.Add(new SequenceTrackNameEvent(name + "\0"));
        }


        private static SevenBitNumber GetProgramNumber(Phrase phrase)
        {
            return (SevenBitNumber)Convert.ToInt32(phrase.Instrument);
        }

        private static FourBitNumber GetChannel(int trackIndex, Phrase phrase)
        {
            if (phrase.IsDrums)
                return (FourBitNumber)DrumChannel;

            if (trackIndex < DrumChannel)
                return (FourBitNumber)trackIndex;

            return (FourBitNumber)(trackIndex + 1);
        }

        private static long GetBpmAsMicroseconds(decimal bpm)
        {
            return Convert.ToInt64(1 / (bpm / 60) * 1000000);
        }

        private static SevenBitNumber GetMidiNoteNumber(int note)
        {
            const int noteOffset = 24;
            return (SevenBitNumber)(note + noteOffset);
        }

        private static long GetMidiNoteLength(decimal lengthInThirtySecondNotes)
        {
            return Convert.ToInt64(24 * lengthInThirtySecondNotes);
        }

        public void SaveToFile(string filepath)
        {
            var chunks = new List<MidiChunk>();
            chunks.AddRange(_trackChunks);
            var format = _trackChunks.Count == 1 ? MidiFileFormat.SingleTrack : MidiFileFormat.MultiTrack;

            var newMidi = new MidiFile(chunks);
            newMidi.Write(filepath, true, format);
        }

        public void SaveToCsvFile(string filepath)
        {
            var chunks = new List<MidiChunk>();
            chunks.AddRange(_trackChunks);
            var newMidi = new MidiFile(chunks);
            var csvConverter = new CsvConverter();
            csvConverter.ConvertMidiFileToCsv(newMidi, filepath, true);
        }

        private static void AddTimeSignatureEvent(TrackChunk trackChunk)
        {
            trackChunk.Events.Add(new TimeSignatureEvent
            {
                ClocksPerClick = 36,
                Denominator = 4,
                Numerator = 4,
                DeltaTime = 0,
                ThirtySecondNotesPerBeat = 8
            });
        }
    }
}
