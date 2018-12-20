using Melanchall.DryWetMidi.Smf;
using System;
using System.IO;
using Melanchall.DryWetMidi.Tools;

namespace Halloumi.Notez.Engine
{
    public class MidiHelper
    {
        public static void SaveToMidi(Phrase phrase, string filepath)
        {
            var builder = BuildMidi(phrase);
            builder.SaveToFile(filepath);
        }

        public static void SaveToCsv(Phrase phrase, string filepath)
        {
            var builder = BuildMidi(phrase);
            builder.SaveToCsvFile(filepath);

        }

        private static MidiBuilder BuildMidi(Phrase phrase)
        {

            var midiBuilder = new MidiBuilder();

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

            const int noteOffset = 24;

            if (midi.Chunks.Count == 0)
                throw new ApplicationException("Invalid Midi File");

            if (!(midi.Chunks[0] is TrackChunk chunk))
                throw new ApplicationException("Invalid Midi File");

            var phrase = new Phrase();

            PhraseElement phraseElement = null;
            long deltaTime = 0;
            foreach (var midiEvent in chunk.Events)
            {
                deltaTime += midiEvent.DeltaTime;

                if (midiEvent is NoteOnEvent)
                {
                    var noteOn = midiEvent as NoteOnEvent;
                    phraseElement = new PhraseElement()
                    {
                        Note = noteOn.NoteNumber - noteOffset,
                        Duration = (int)(noteOn.DeltaTime / 24M)
                };
                }
                else if (midiEvent is NoteOffEvent)
                {
                    if (phraseElement == null) continue;

                    phraseElement.Duration = (int)(deltaTime / 24M);
                    phrase.Elements.Add(phraseElement);

                    deltaTime = 0;
                }
            }

            return phrase;
        }
    }
}
