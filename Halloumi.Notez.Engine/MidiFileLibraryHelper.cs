using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine
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
            Parallel.ForEach(renamedFiles, renamedFile =>
            {
                UpdateMidiChannelNamesToMatchInstruments(renamedFile);
            });

            //foreach (var renamedFile in renamedFiles)
            //{
            //    UpdateMidiChannelNamesToMatchInstruments(renamedFile);
            //}
        }

        private static string GetNewFileName(string sourceFile, string destinationFolder)
        {
            var newFile = Path.GetFileNameWithoutExtension(sourceFile)
                .Replace("_", " ")
                .Replace(".", " ")
                .Trim();

                newFile = TitleCase(newFile);

                for (int i = 0; i < 10; i++)
                    newFile = newFile.Replace($"({i})", "");

                newFile = newFile.Trim();

            if (!newFile.Contains(" - "))
            {
                var artistName = Path.GetFileName(Path.GetDirectoryName(sourceFile)).Replace("_", " ")
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

                var nameEvent = trackChunk.Events.Where(x => x is SequenceTrackNameEvent).FirstOrDefault() as SequenceTrackNameEvent;
                var changeEvent = trackChunk.Events.Where(x => x is ProgramChangeEvent).FirstOrDefault() as ProgramChangeEvent;

                var channelEvent = trackChunk.Events.Where(x => x is ChannelEvent).FirstOrDefault() as ChannelEvent;

                if (changeEvent != null && channelEvent != null)
                {
                    var instrumentName = (channelEvent.Channel == 9)
                        ? "Drums"
                        : ((MidiInstrument)(int)changeEvent.ProgramNumber).ToString();
                    instrumentName += "\0";

                    if (nameEvent == null)
                        trackChunk.Events.Insert(0, new SequenceTrackNameEvent(instrumentName));
                    else 
                        nameEvent.Text = instrumentName;
                }

            }

            var newFile = Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + "2.mid");
            midi.Write(newFile, true);

            File.Delete(filepath);
            File.Move(newFile, filepath);

        }

        //public static void RenameFiles(string folder)
        //{
        //    if (!Directory.Exists(folder))
        //        return;

        //    var files = Directory.GetFiles(folder)
        //        .Where(x => Path.GetExtension(x).ToLower() == ".mid").ToList();
            
        //    foreach (var file in files)
        //    {
        //        var newFile = Path.GetFileNameWithoutExtension(file)
        //            .Replace("_", " ")
        //            .Trim();
                
        //        newFile = TitleCase(newFile);

        //        for (int i = 0; i < 10; i++)
        //            newFile = newFile.Replace($"({i})", "");

        //        newFile = newFile.Trim();

        //        newFile = Path.Combine(folder, newFile + ".mid");

        //        if (newFile.ToLower() == file.ToLower())
        //            continue;

        //        if (File.Exists(newFile))
        //            File.Delete(newFile);

        //        File.Move(file, newFile);
        //    }
        //}

        private static string TitleCase(string value)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(value);
        }

        private static List<string> LoadPlaylist(string playlistFile)
        {
            if (!File.Exists(playlistFile) || Path.GetExtension(playlistFile) != ".mpl")
                return new List<string>();

            return File.ReadAllLines(playlistFile).ToList().Where(x => File.Exists(x)).ToList();
        }
    }
}
