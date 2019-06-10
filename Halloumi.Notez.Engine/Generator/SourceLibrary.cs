using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Generator
{
    public class SourceLibrary
    {
        private List<Clip> Clips { get; set; }

        public void LoadLibrary(string folder)
        {
            LoadClips(folder);
            CalculateScales();

            foreach (var clip in Clips)
            {
                if (clip.ScaleMatchIncomplete)
                    clip.Phrase = ScaleHelper.MashNotesToScale(clip.Phrase, clip.Scale);

                clip.Phrase = ScaleHelper.TransposeToScale(clip.Phrase, clip.Scale, "C Harmonic Minor");
                Console.WriteLine(clip.Name.PadRight(20) + clip.Scale + ((clip.ScaleMatchIncomplete) ? $"({clip.ScaleMatch.DistanceFromScale})" : ""));
            }
        }

        private void CalculateScales()
        {
            foreach (var clip in Clips)
            {
                var scales = ScaleHelper.FindMatchingScales(clip.Phrase);
                var minDistance = scales.Min(x => x.DistanceFromScale);
                clip.MatchingScales = scales.Where(x => x.DistanceFromScale == minDistance).ToList();
            }

            var sections = Clips
                .GroupBy(x => x.Section, (key, group) => new SectionCounts()
                {
                    Name = key,
                    ScaleCounts = group.SelectMany(x => x.MatchingScales)
                        .Select(x => x.Scale.Name)
                        .GroupBy(x => x)
                        .Select(x => new ScaleCount() { Count = x.Count(), Scale = x.Key })
                        .OrderByDescending(x => x.Count)
                        .ToList()
                })
                .ToList();

            var songs = Clips
                .GroupBy(x => x.Song, (key, group) => new SectionCounts()
                {
                    Name = key,
                    ScaleCounts = group.SelectMany(x => x.MatchingScales)
                        .Select(x => x.Scale.Name)
                        .GroupBy(x => x)
                        .Select(x => new ScaleCount() { Count = x.Count(), Scale = x.Key })
                        .OrderByDescending(x => x.Count)
                        .ToList()
                })
                .ToList();

            var artists = Clips
                .GroupBy(x => x.Artist, (key, group) => new SectionCounts()
                {
                    Name = key,
                    ScaleCounts = group.SelectMany(x => x.MatchingScales)
                        .Select(x => x.Scale.Name)
                        .GroupBy(x => x)
                        .Select(x => new ScaleCount() { Count = x.Count(), Scale = x.Key })
                        .OrderByDescending(x => x.Count)
                        .ToList()
                })
                .ToList();


            foreach (var clip in Clips)
            {
                var matchingScales = clip.MatchingScales.Select(x => x.Scale.Name).ToList();

                var section = sections.FirstOrDefault(x => x.Name == clip.Section);
                var song = songs.FirstOrDefault(x => x.Name == clip.Song);
                var artist = artists.FirstOrDefault(x => x.Name == clip.Artist);
                clip.Scale = matchingScales
                    .OrderByDescending(x => GetSectionRank(section, x))
                    .ThenByDescending(x => GetSectionRank(song, x))
                    .ThenByDescending(x => GetSectionRank(artist, x))
                    .ThenByDescending(x => x.EndsWith("Minor") ? 1 : 0)
                    .FirstOrDefault();
            }

            foreach (var section in sections)
            {
                var sectionClips = Clips.Where(x => x.Section == section.Name).ToList();
                var scaleCount = sectionClips.Select(x => x.Scale).GroupBy(x => x).Count();
                if (scaleCount == 1)
                    continue;
                if (scaleCount > 2)
                    throw new ApplicationException("Too many scales");

                var primaryScale = sectionClips
                    .Select(x => x.Scale)
                    .GroupBy(x => x)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key).First();

                foreach (var clip in sectionClips)
                {
                    clip.Scale = primaryScale;
                    if (!clip.MatchingScales.Select(x => x.Scale.Name).Contains(primaryScale))
                    {
                        clip.ScaleMatch = ScaleHelper.MatchPhraseToScale(clip.Phrase, primaryScale);
                        clip.ScaleMatchIncomplete = true;
                    }
                }
            }

            var riffs = this.Clips.Select(x => x.Phrase).ToList();

            var counts = riffs.GroupBy(x => x.PhraseLength, (key, group) => new
            {
                Length = key,
                Count = group.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

            foreach (var count in counts)
            {
                Console.WriteLine(count.Length + "\t" + count.Count);
            }

            var riff = Clips.FirstOrDefault(x => x.Phrase.PhraseLength == 136);

            NoteHelper.GetTotalDuration(riff.Phrase);

            Console.WriteLine(riff.Name +" " + NoteHelper.GetTotalDuration(riff.Phrase));


            // var generator = new PhraseGenerator(riffLength: 64, sourceRiffs: riffs);

        }

        private void LoadClips(string folder)
        {
            Clips = Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories)
                .Where(x => !x.EndsWith(" 4.mid"))
                .Where(x => x.Contains("ATG-"))
                .OrderBy(x => Path.GetFileNameWithoutExtension(x))
                .Select(x => new Clip
                {
                    File = x,
                    Name = Path.GetFileNameWithoutExtension(x),
                    Song = Regex.Replace(Path.GetFileNameWithoutExtension(x), @"[\d-]", string.Empty),
                    Section = Path.GetFileNameWithoutExtension(x).Split(' ')[0],
                    Artist = Path.GetFileNameWithoutExtension(x).Split('-')[0],
                    Phrase = MidiHelper.ReadMidi(x),
                })
                .ToList();
        }

        private static int GetSectionRank(SectionCounts section, string scaleName)
        {
            if (!section.ScaleCounts.Exists(x => x.Scale == scaleName))
                return 0;

            return section.ScaleCounts.FirstOrDefault(x => x.Scale == scaleName).Count;
        }


        private class SectionCounts
        {
            public string Name { get; set; }
            public List<ScaleCount> ScaleCounts { get; set; }
        }

        private class ScaleCount
        {
            public string Scale { get; set; }
            public int Count { get; set; }
        }

        private class Clip
        {
            public string File { get; set; }
            public string Name { get; set; }
            public string Song { get; set; }
            public string Artist { get; set; }
            public string Section { get; set; }
            public Phrase Phrase { get; set; }
            public List<ScaleHelper.ScaleMatch> MatchingScales { get; set; }
            public string Scale { get; set; }
            public bool ScaleMatchIncomplete { get; set; }
            public ScaleHelper.ScaleMatch ScaleMatch { get; set; }
        }
    }
}
