using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Halloumi.Notez.Engine.Notes;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Tools;

namespace Halloumi.Notez.Engine.Midi
{
    public class MidiBuilder
    {
        private readonly List<TrackChunk> _trackChunks;

        private readonly TrackChunk _tempoChunk;

        public MidiBuilder(List<Phrase> phrases, string name = "")
        {
            var bpm = phrases[0].Bpm;
            if (bpm == 0M) bpm = 120M;

            _tempoChunk = new TrackChunk(new SetTempoEvent(GetBpmAsMicroseconds(bpm)));
            AddTimeSignatureEvent(_tempoChunk);
            if (name != "")
            {
                _tempoChunk.Events.Add(new SequenceTrackNameEvent(name + "\0"));
            }

            _trackChunks = new List<TrackChunk>();

            foreach (var phrase in phrases)
            {
                var trackChunk = new TrackChunk();
                trackChunk.Events.Add(new SequenceTrackNameEvent(phrase.Description + "\0"));
                trackChunk.Events.Add(new ProgramChangeEvent((SevenBitNumber)Convert.ToInt32(phrase.Instrument)));
                _trackChunks.Add(trackChunk);
            }

            foreach (var sourcePhrase in phrases)
            {
                var index = phrases.IndexOf(sourcePhrase);
                var phrase = sourcePhrase.Clone();


                PhraseHelper.UnmergeRepeatedNotes(phrase);
                PhraseHelper.UpdateDurationsFromPositions(phrase, phrase.PhraseLength);
                PhraseHelper.UnmergeChords(phrase);

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
                        var delta = notesOff.First() == noteOff ? noteOff.Duration : 0M;
                        AddNoteOff(index, noteOff.Note, delta, phrase);
                    }

                    var notesOn = phrase.Elements.Where(x => x.Position == position).ToList();
                    foreach (var noteOn in notesOn)
                    {
                        AddNoteOn(index, noteOn.Note, phrase);
                    }
                }
            }

        }

        private static long GetBpmAsMicroseconds(decimal bpm)
        {
            return Convert.ToInt64( (1 / (bpm / 60)) * 1000000);
        }

        //public void AddNote(int trackIndex, int note, decimal lengthInThirtySecondNotes)
        //{
        //    AddNoteOn(trackIndex, note);
        //    AddNoteOff(trackIndex, note, lengthInThirtySecondNotes);
        //}

        private void AddNoteOn(int trackIndex, int note, Phrase phrase)
        {
            const int noteOffset = 24;
            var noteNumber = (SevenBitNumber)(note + noteOffset);

            var channel = phrase.IsDrums ? 10 : trackIndex;
            var trackChunk = _trackChunks[trackIndex];

            if (!phrase.IsDrums)
                trackChunk.Events.Add(new ProgramChangeEvent((SevenBitNumber)Convert.ToInt32(phrase.Instrument)));
            
            trackChunk.Events.Add(new NoteOnEvent
            {
                DeltaTime = 0,
                Velocity = (SevenBitNumber)100,
                NoteNumber = noteNumber,
                Channel = (FourBitNumber)channel
            });
        }

        private void AddNoteOff(int trackIndex, int note, decimal lengthInThirtySecondNotes, Phrase phrase)
        {
            const int noteOffset = 24;

            var noteLength = Convert.ToInt64(24 * lengthInThirtySecondNotes);
            var noteNumber = (SevenBitNumber)(note + noteOffset);

            var channel = phrase.IsDrums ? 10 : trackIndex;
            var trackChunk = _trackChunks[trackIndex];
            trackChunk.Events.Add(new NoteOffEvent
            {
                DeltaTime = noteLength,
                Velocity = (SevenBitNumber)64,
                NoteNumber = noteNumber,
                Channel = (FourBitNumber)channel
            });
        }

        public void SaveToFile(string filepath)
        {
            var chunks = new List<MidiChunk> { _tempoChunk };
            chunks.AddRange(_trackChunks);
            var format = _trackChunks.Count == 1 ? MidiFileFormat.SingleTrack : MidiFileFormat.MultiTrack;

            var newMidi = new MidiFile(chunks);
            newMidi.Write(filepath, true, format);
        }

        public void SaveToCsvFile(string filepath)
        {
            var chunks = new List<MidiChunk> { _tempoChunk };
            chunks.AddRange(_trackChunks);
            var newMidi = new MidiFile(chunks);
            var csvConverter = new CsvConverter();
            csvConverter.ConvertMidiFileToCsv(newMidi, filepath, true);
        }

        private void AddTimeSignatureEvent(TrackChunk trackChunk)
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
