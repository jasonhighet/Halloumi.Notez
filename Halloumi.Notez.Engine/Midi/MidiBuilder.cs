using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Tools;

namespace Halloumi.Notez.Engine.Midi
{
    public class MidiBuilder
    {
        private readonly List<TrackChunk> _trackChunks;

        private readonly TrackChunk _bpmChunk;

        public MidiBuilder(List<Tuple<string, MidiInstrument>> tracks, decimal bpm = 120)
        {
            _bpmChunk = new TrackChunk(new SetTempoEvent(GetBpmAsMicroseconds(bpm)));
            _trackChunks = new List<TrackChunk>();

            foreach (var track in tracks)
            {
                var trackChunk = new TrackChunk();
                trackChunk.Events.Add(new ProgramChangeEvent((SevenBitNumber)Convert.ToInt32(track.Item2)));
                trackChunk.Events.Add(new SequenceTrackNameEvent(track.Item1 + "\0"));
                AddTimeSignatureEvent(trackChunk);
                _trackChunks.Add(trackChunk);
            }
            
        }

        private long GetBpmAsMicroseconds(decimal bpm)
        {
            return Convert.ToInt64( (1 / (bpm / 60)) * 1000000);
        }

        public void AddNote(int trackIndex, int note, decimal lengthInThirtySecondNotes)
        {
            AddNoteOn(trackIndex, note);
            AddNoteOff(trackIndex, note, lengthInThirtySecondNotes);
        }

        public void AddNoteOn(int trackIndex, int note)
        {
            const int noteOffset = 24;
            var noteNumber = (SevenBitNumber)(note + noteOffset);

            var trackChunk = _trackChunks[trackIndex];
            trackChunk.Events.Add(new NoteOnEvent
            {
                DeltaTime = 0,
                Velocity = (SevenBitNumber)100,
                NoteNumber = noteNumber,
                Channel = (FourBitNumber)0
            });
        }
        public void AddNoteOff(int trackIndex, int note, decimal lengthInThirtySecondNotes)
        {
            const int noteOffset = 24;

            var noteLength = Convert.ToInt64(24 * lengthInThirtySecondNotes);
            var noteNumber = (SevenBitNumber)(note + noteOffset);

            var trackChunk = _trackChunks[trackIndex];
            trackChunk.Events.Add(new NoteOffEvent
            {
                DeltaTime = noteLength,
                Velocity = (SevenBitNumber)64,
                NoteNumber = noteNumber,
                Channel = (FourBitNumber)0
            });
        }

        public void SaveToFile(string filepath)
        {
            var chunks = new List<MidiChunk> { _bpmChunk };
            chunks.AddRange(_trackChunks);
            var format = _trackChunks.Count == 1 ? MidiFileFormat.SingleTrack : MidiFileFormat.MultiTrack;

            var newMidi = new MidiFile(chunks);
            newMidi.Write(filepath, true, format);
        }

        public void SaveToCsvFile(string filepath)
        {
            var chunks = new List<MidiChunk> { _bpmChunk };
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
