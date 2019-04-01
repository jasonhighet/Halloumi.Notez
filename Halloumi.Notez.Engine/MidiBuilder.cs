using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Tools;

namespace Halloumi.Notez.Engine
{
    public class MidiBuilder
    {
        private readonly TrackChunk _trackChunk;

        private readonly TrackChunk _bpmChunk;

        public MidiBuilder(string name = "Riff", decimal bpm = 120, MidiInstrument instrument = MidiInstrument.AcousticGrandPiano)
        {
            _bpmChunk = new TrackChunk(new SetTempoEvent(GetBpmAsMicroseconds(bpm)));

            _trackChunk = new TrackChunk();

            _trackChunk.Events.Add(new ProgramChangeEvent((SevenBitNumber)Convert.ToInt32(instrument)));

            _trackChunk.Events.Add(new SequenceTrackNameEvent(name + "\0"));
            //_trackChunk.Events.Add(new TextEvent(name + "\0"));

            AddTimeSignatureEvent();
            //AddTimeSignatureEvent();
        }

        private long GetBpmAsMicroseconds(decimal bpm)
        {
            return Convert.ToInt64( (1 / (bpm / 60)) * 1000000);
        }

        public void AddNote(int note, decimal lengthInThirtySecondNotes)
        {
            const int noteOffset = 24;

            var noteLength = Convert.ToInt64(24 * lengthInThirtySecondNotes);
            var noteNumber = (SevenBitNumber)(note + noteOffset);

            _trackChunk.Events.Add(new NoteOnEvent
            {
                DeltaTime = 0,
                Velocity = (SevenBitNumber)100,
                NoteNumber = noteNumber,
                Channel = (FourBitNumber)0
            });

            _trackChunk.Events.Add(new NoteOffEvent
            {
                DeltaTime = noteLength,
                Velocity = (SevenBitNumber)64,
                NoteNumber = noteNumber,
                Channel = (FourBitNumber)0
            });
        }

        public void SaveToFile(string filepath)
        {
            var newMidi = new MidiFile(new List<MidiChunk> { _bpmChunk, _trackChunk });
            newMidi.Write(filepath, true, MidiFileFormat.SingleTrack);
        }

        public void SaveToCsvFile(string filepath)
        {
            var newMidi = new MidiFile(new List<MidiChunk> { _bpmChunk, _trackChunk });
            var csvConverter = new CsvConverter();
            csvConverter.ConvertMidiFileToCsv(newMidi, filepath, true);
        }

        private void AddTimeSignatureEvent()
        {
            _trackChunk.Events.Add(new TimeSignatureEvent
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
