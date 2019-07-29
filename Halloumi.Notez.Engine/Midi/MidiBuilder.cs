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
        private const int DrumChannel = 9;
        private readonly List<TrackChunk> _trackChunks;

        private readonly TrackChunk _tempoChunk;

        public MidiBuilder(IList<Phrase> phrases, string name = "")
        {
            var bpm = phrases[0].Bpm;
            if (bpm == 0M) bpm = 120M;

            _tempoChunk = new TrackChunk(new SetTempoEvent(GetBpmAsMicroseconds(bpm)));
            AddTimeSignatureEvent(_tempoChunk);
            AddNameEvent(_tempoChunk, name);

            _trackChunks = new List<TrackChunk>();

            foreach (var phrase in phrases)
            {
                var trackChunk = new TrackChunk();
                AddNameEvent(trackChunk, phrase.Description);
                SetInstrument(trackChunk, phrase);
                _trackChunks.Add(trackChunk);
            }

            foreach (var sourcePhrase in phrases)
            {
                var index = phrases.IndexOf(sourcePhrase);
                var phrase = sourcePhrase.Clone();


                PhraseHelper.UnmergeRepeatedNotes(phrase);
                //PhraseHelper.UpdateDurationsFromPositions(phrase, phrase.PhraseLength);
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
                return (FourBitNumber) trackIndex;

            return (FourBitNumber) (trackIndex + 1);
        }

        private static long GetBpmAsMicroseconds(decimal bpm)
        {
            return Convert.ToInt64( (1 / (bpm / 60)) * 1000000);
        }

        private void AddNoteOn(int trackIndex, int note, Phrase phrase)
        {
            var noteNumber = GetMidiNote(note);
            var channel = GetChannel(trackIndex, phrase);
            var trackChunk = _trackChunks[trackIndex];

            trackChunk.Events.Add(new NoteOnEvent
            {
                DeltaTime = 0,
                Velocity = (SevenBitNumber)100,
                NoteNumber = noteNumber,
                Channel = channel
            });
        }

        private static SevenBitNumber GetMidiNote(int note)
        {
            const int noteOffset = 24;
            return (SevenBitNumber)(note + noteOffset);
        }

        private void AddNoteOff(int trackIndex, int note, decimal lengthInThirtySecondNotes, Phrase phrase)
        {
            var noteLength = Convert.ToInt64(24 * lengthInThirtySecondNotes);
            var noteNumber = GetMidiNote(note);
            var channel = GetChannel(trackIndex, phrase);

            var trackChunk = _trackChunks[trackIndex];
            trackChunk.Events.Add(new NoteOffEvent
            {
                DeltaTime = noteLength,
                Velocity = (SevenBitNumber)64,
                NoteNumber = noteNumber,
                Channel = channel
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
