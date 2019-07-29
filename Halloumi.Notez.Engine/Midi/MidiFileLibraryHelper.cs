using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Midi
{
    public class MidiFileLibraryHelper
    {
        public static void CopyPlaylistFiles(string playlistFile, string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            var files = LoadPlaylist(playlistFile);
            foreach (var file in files)
            {
                var newFile = GetNewFileName(file, destinationFolder);
                File.Copy(file, newFile, true);
            }
            
            var renamedFiles = Directory.GetFiles(destinationFolder);
            Parallel.ForEach(renamedFiles, UpdateMidiChannelNamesToMatchInstruments);
        }

        private static string GetNewFileName(string sourceFile, string destinationFolder)
        {
            var newFile = Path.GetFileNameWithoutExtension(sourceFile + "")
                .Replace("_", " ")
                .Replace(".", " ")
                .Trim();

                newFile = TitleCase(newFile);

                for (var i = 0; i < 10; i++)
                    newFile = newFile.Replace($"({i})", "");

                newFile = newFile.Trim();

            if (!newFile.Contains(" - "))
            {
                var artistName = Path.GetFileName(Path.GetDirectoryName(sourceFile) + "").Replace("_", " ")
                    .Replace(".", " ")
                    .Trim();

                artistName = TitleCase(artistName);

                if (newFile.ToLower().StartsWith(artistName.ToLower()))
                {
                    newFile = newFile.Substring(artistName.Length).Trim();
                }

                newFile = artistName + " - " + newFile;
            }

            newFile = Path.Combine(destinationFolder, newFile + ".mid");
            return newFile;
        }

        public static void UpdateMidiChannelNamesToMatchInstruments(string filepath)
        {
            MidiFile midi;

            try
            {
                midi = MidiFile.Read(filepath);
            }
            catch(Exception e)
            {
                Console.WriteLine("Cannot load midi file " + filepath);
                Console.WriteLine(e.Message);
                return;
            }
                
            foreach (var chunk in midi.Chunks)
            {
                if (!(chunk is TrackChunk trackChunk))
                    continue;

                if (!(trackChunk.Events.FirstOrDefault(x => x is ChannelEvent) is ChannelEvent channelEvent))
                    continue;

                if (!(trackChunk.Events.FirstOrDefault(x => x is ProgramChangeEvent) is ProgramChangeEvent changeEvent))
                    continue;

                var instrumentName = (channelEvent.Channel == 9)
                    ? "Drums"
                    : ((MidiInstrument)(int)changeEvent.ProgramNumber).ToString();
                instrumentName += "\0";

                if (!(trackChunk.Events.FirstOrDefault(x => x is SequenceTrackNameEvent) is SequenceTrackNameEvent nameEvent))
                    trackChunk.Events.Insert(0, new SequenceTrackNameEvent(instrumentName));
                else
                    nameEvent.Text = instrumentName;
            }

            var newFile = Path.Combine(Path.GetDirectoryName(filepath) + "", Path.GetFileNameWithoutExtension(filepath) + "2.mid");
            midi.Write(newFile, true);

            File.Delete(filepath);
            File.Move(newFile, filepath);
        }
       
        private static string TitleCase(string value)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(value);
        }

        private static IEnumerable<string> LoadPlaylist(string playlistFile)
        {
            if (!File.Exists(playlistFile) || Path.GetExtension(playlistFile) != ".mpl")
                return new List<string>();

            return File.ReadAllLines(playlistFile).ToList().Where(File.Exists).ToList();
        }
    }
}
