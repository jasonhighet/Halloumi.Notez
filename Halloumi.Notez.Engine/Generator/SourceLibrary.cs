using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Halloumi.Notez.Engine.Generator
{
    public class SourceLibrary
    {
        private List<Clip> Clips { get; set; }

        public void LoadLibrary(string folder)
        {
            LoadClips(folder);
            CalculateScales();
            MashToScale();
            MergeChords();
            MergeRepeatedNotes();
            CalculateLengths();
            FindPatterns();

            var sections = InstrumentClips().Select(x => x.Section).Distinct();
            foreach (var section in sections)
            {
                Console.WriteLine(section);

                var clips = InstrumentClips().Where(x => x.Section == section).ToList();

                var bassGuitar = clips.FirstOrDefault(x => x.Name.EndsWith(" 3"));

                var mainGuitar = clips.Where(x => !x.Name.EndsWith(" 3"))
                    .OrderBy(x => GetAverageNote(x.Phrase))
                    .ThenByDescending(x => x.Phrase.Elements.Sum(y => y.Duration))
                    .ThenBy(x => x.Phrase.Elements.Count)
                    .ThenBy(x => x.Name.Substring(x.Name.Length - 1, 1))
                    .FirstOrDefault();

                var altGuitar = clips.Except(new List<Clip> { bassGuitar, mainGuitar }).FirstOrDefault();

                if (bassGuitar == null || mainGuitar == null || altGuitar == null)
                    throw new ApplicationException("missing clips");

                var bassDiff = RoundToNearestMultiple(GetAverageNote(bassGuitar.Phrase) - GetAverageNote(mainGuitar.Phrase), 12);
                var altDiff = RoundToNearestMultiple(GetAverageNote(altGuitar.Phrase) - GetAverageNote(mainGuitar.Phrase), 12);

                var phrases = new List<Phrase>
                {
                    mainGuitar.Phrase.Clone(),

                    NoteHelper.ShiftNotes(altGuitar.Phrase, altDiff * -1, Interval.Step, altDiff < 0 ? Direction.Up : Direction.Down),
                    NoteHelper.ShiftNotes(bassGuitar.Phrase, bassDiff * -1, Interval.Step, bassDiff < 0 ? Direction.Up : Direction.Down),
                };

                var length = phrases.Max(x => x.PhraseLength);
                foreach (var phrase in phrases)
                {
                    foreach (var element in phrase.Elements)
                    {
                        element.ChordNotes.Clear();
                        element.RepeatDuration = 0;
                    }
                    PhraseHelper.MergeRepeatedNotes(phrase);
                    foreach (var element in phrase.Elements)
                    {
                        element.RepeatDuration = 0;
                    }
                    while (phrase.PhraseLength < length)
                    {
                        PhraseHelper.DuplicatePhrase(phrase);
                    }
                }

                var basePhrase = new Phrase { PhraseLength = length };
                var notePositions = phrases.SelectMany(x => x.Elements).GroupBy(x => x.Position).OrderBy(x => x.Key);

                var nextPostion = 0M;
                foreach (var position in notePositions)
                {
                    if (position.Key < nextPostion)
                        continue;

                    var distinctNotes = position.GroupBy(x => new { x.Note, x.Duration })
                        .Select(x => new
                        {
                            x.Key.Note,
                            x.Key.Duration,
                            Count = x.Count()
                        })
                        .OrderByDescending(x => x.Count);

                    var mostCommonNote = distinctNotes
                        .OrderByDescending(x => x.Count)
                        .ThenByDescending(x => x.Duration)
                        .ThenBy(x => x.Note)
                        .Take(1)
                        .Select(x => new PhraseElement() { Duration = x.Duration, Note = x.Note, Position = position.Key })
                        .FirstOrDefault();

                    if (mostCommonNote == null)
                        throw new ApplicationException("null note");

                    basePhrase.Elements.Add(mostCommonNote);
                    nextPostion = position.Key + mostCommonNote.Duration;
                }


                if (basePhrase.Elements[0] != null && basePhrase.Elements[0].Position != 0)
                    basePhrase.Elements[0].Position = 0;

                PhraseHelper.MergeNotes(basePhrase);
                PhraseHelper.UpdateDurationsFromPositions(basePhrase, basePhrase.PhraseLength);



                foreach (var element in basePhrase.Elements)
                {
                    Console.WriteLine($"{NoteHelper.NumberToNote(element.Note)},{Math.Round(element.Duration, 2)}".PadRight(8));
                }
                Console.WriteLine();




                // for each note
                //      pick lowest/average
                //      if not over lapping last note, add
                // join


                //foreach (var avgNote in avgNotes)
                //{
                //    Console.Write(avgNote.Name
                //        + ":" + NoteHelper.NumberToNote((int)avgNote.AvgNote)
                //        + ":" + avgNote.NoteCount
                //        + "\t");
                //}
                Console.WriteLine("");


                //var avgNotes = clips.Select(x => new
                //{
                //    Name = x.Name.Substring(x.Name.Length - 1, 1),
                //    AvgNote = GetAverageNote(x),
                //    NoteCount = x.Phrase.Elements.Sum(y => y.HasRepeatingNotes ? y.RepeatCount : 1)
                //}).ToList();


                //Console.Write(section.PadRight(30));
                //foreach (var avgNote in avgNotes)
                //{
                //    Console.Write(avgNote.Name
                //        + ":" + NoteHelper.NumberToNote((int)avgNote.AvgNote)
                //        + ":" + avgNote.NoteCount
                //        + "\t");
                //}
                //Console.WriteLine("");
            }

        }
        private static int RoundToNearestMultiple(int value, int factor)
        {
            return (int)Math.Round((value / (double)factor), MidpointRounding.AwayFromZero) * factor;
        }

        private static int GetAverageNote(Phrase phrase)
        {
            return (int)Math.Round((phrase.Elements.Average(y => y.Note) + phrase.Elements.Min(y => y.Note)) / 2);
        }

        private void FindPatterns()
        {
            foreach (var clip in InstrumentClips())
            {
                PatternFinder.FindPatterns(clip.Phrase);
            }
        }

        private void MergeRepeatedNotes()
        {
            foreach (var clip in InstrumentClips())
            {
                PhraseHelper.MergeRepeatedNotes(clip.Phrase);
            }

        }



        private void MergeChords()
        {
            foreach (var clip in InstrumentClips())
            {
                PhraseHelper.MergeChords(clip.Phrase);
            }
        }


        private void CalculateLengths()
        {
            var sections = Clips
                .GroupBy(x => x.Section, (key, group) => new
                {
                    Name = key,
                    Lengths = group.Select(x => x.Phrase.PhraseLength)
                        .GroupBy(x => x)
                        .Select(x => new { Count = x.Count(), Length = x.Key })
                        .OrderByDescending(x => x.Count)
                        .ToList()
                })
                .ToList();

            var invalidClips = Clips.Where(x => !ValidLength(x.Phrase.PhraseLength)).ToList();
            foreach (var clip in invalidClips)
            {
                var section = sections.FirstOrDefault(x => x.Name == clip.Section);

                var closestValidMatch = section?.Lengths
                    .Where(x => ValidLength(x.Length))
                    .OrderBy(x => clip.Phrase.PhraseLength - x.Length)
                    .FirstOrDefault();

                if (closestValidMatch == null) continue;

                var diff = closestValidMatch.Length - clip.Phrase.PhraseLength;
                if (diff > 0 && (diff / closestValidMatch.Length) < .25M)
                {
                    clip.Phrase.PhraseLength = closestValidMatch.Length;
                }
            }


            invalidClips = Clips.Where(x => !ValidLength(x.Phrase.PhraseLength)).ToList();
            foreach (var clip in invalidClips)
            {
                Console.WriteLine("Invalid Length:" + clip.Name + " " + clip.Phrase.PhraseLength);
            }

            var validClips = Clips.Where(x => ValidLength(x.Phrase.PhraseLength)).ToList();
            foreach (var clip in validClips)
            {
                while (PhraseHelper.IsPhraseDuplicated(clip.Phrase))
                {
                    var halfLength = clip.Phrase.Elements.Count / 2;
                    clip.Phrase.Elements.RemoveAll(x => clip.Phrase.Elements.IndexOf(x) >= halfLength);
                    clip.Phrase.PhraseLength /= 2M;
                }
            }

            Clips.RemoveAll(x => !ValidLength(x.Phrase.PhraseLength));

        }

        private static bool ValidLength(decimal length)
        {
            return length == 2
                   || length == 4
                   || length == 8
                   || length == 16
                   || length == 32
                   || length == 64
                   || length == 128
                   || length == 256;
        }

        private void MashToScale()
        {
            foreach (var clip in InstrumentClips())
            {
                if (clip.ScaleMatchIncomplete)
                {
                    clip.Phrase = ScaleHelper.MashNotesToScale(clip.Phrase, clip.Scale);
                    Console.WriteLine(clip.Name.PadRight(20) + clip.Scale + ((clip.ScaleMatchIncomplete) ? $"({clip.ScaleMatch.DistanceFromScale})" : ""));
                }
                clip.Phrase = ScaleHelper.TransposeToScale(clip.Phrase, clip.Scale, "C Harmonic Minor");
            }
        }

        private IEnumerable<Clip> InstrumentClips()
        {
            return Clips.Where(x => !x.File.EndsWith(" 4.mid"));
        }


        private void CalculateScales()
        {
            foreach (var clip in InstrumentClips())
            {
                var scales = ScaleHelper.FindMatchingScales(clip.Phrase);
                var minDistance = scales.Min(x => x.DistanceFromScale);
                clip.MatchingScales = scales.Where(x => x.DistanceFromScale == minDistance).ToList();
            }

            var sections = InstrumentClips()
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

            var songs = InstrumentClips()
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

            var artists = InstrumentClips()
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


            foreach (var clip in InstrumentClips())
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
                var sectionClips = InstrumentClips().Where(x => x.Section == section.Name).ToList();
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
                    if (clip.MatchingScales.Select(x => x.Scale.Name).Contains(primaryScale)) continue;

                    clip.ScaleMatch = ScaleHelper.MatchPhraseToScale(clip.Phrase, primaryScale);
                    clip.ScaleMatchIncomplete = true;
                }
            }
        }

        private void LoadClips(string folder)
        {
            Clips = Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories)
                .OrderBy(x => Path.GetFileNameWithoutExtension(x) + "")
                .Select(x => new Clip
                {
                    File = x,
                    Name = Path.GetFileNameWithoutExtension(x),
                    Song = Regex.Replace(Path.GetFileNameWithoutExtension(x) + "", @"[\d-]", string.Empty),
                    Section = (Path.GetFileNameWithoutExtension(x) + "").Split(' ')[0],
                    Artist = (Path.GetFileNameWithoutExtension(x) + "").Split('-')[0],
                    Phrase = MidiHelper.ReadMidi(x),
                })
                .ToList();
        }

        private static int GetSectionRank(SectionCounts section, string scaleName)
        {
            var counts = section.ScaleCounts.FirstOrDefault(x => x.Scale == scaleName);

            return counts?.Count ?? 0;
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
