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
        private const string BaseScale = "C Harmonic Minor";

        private List<Clip> Clips { get; set; }

        public void LoadLibrary(string folder)
        {
            LoadClips(folder);
            CalculateScales();
            MashToScale();
            MergeChords();
            MergeRepeatedNotes();
            CalculateLengths();
            CalculateBasePhrases();

            for (var i = 0; i < 30; i++)
            {
                GenerateRiff("riff" + i);
            }

            

            //FindPatterns();
        }

        private void GenerateRiff(string name)
        {
            var random = new Random(DateTime.Now.Millisecond);
            var clips = Clips
                .Where(x => x.ClipType == ClipType.BasePhrase)
                .OrderBy(x => random.Next())
                .Take(1)
                .ToList();

            var inititialClip = clips[0];
            var minDuration = inititialClip.Phrase.Elements.Min(x => x.Duration);

            clips.AddRange(Clips.Where(x => x.ClipType == ClipType.BasePhrase)
                .Where(x => x != inititialClip)
                .OrderBy(x => (x.Phrase.Elements.Min(y => y.Duration) - minDuration) * -1)
                .ThenBy(x => random.Next())
                .Take(3)
                .ToList());

            var bassClip = Clips.FirstOrDefault(x => x.ClipType == ClipType.BassGuitar && x.Section == inititialClip.Section);
            if (bassClip == null)
                throw new ApplicationException("No bass clip");


            var mergedPhrase = MergePhrases(clips.Select(x => x.Phrase).ToList());
            mergedPhrase.Phrase.Bpm = 200;

            NoteHelper.ShiftNotes(mergedPhrase.Phrase, bassClip.BaseIntervalDiff, Interval.Step);
            MidiHelper.SaveToMidi(mergedPhrase.Phrase, name + ".mid", MidiInstrument.ElectricBassFinger);
        }

        private void CalculateBasePhrases()
        {
            var sections = InstrumentClips().Select(x => x.Section).Distinct().ToList();
            foreach (var section in sections)
            {
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

                mainGuitar.ClipType = ClipType.MainGuitar;
                altGuitar.ClipType = ClipType.AltGuitar;

                var bassDiff = RoundToNearestMultiple(GetAverageNote(bassGuitar.Phrase) - GetAverageNote(mainGuitar.Phrase), 12);
                var altDiff = RoundToNearestMultiple(GetAverageNote(altGuitar.Phrase) - GetAverageNote(mainGuitar.Phrase), 12);

                mainGuitar.BaseIntervalDiff = 0;
                altGuitar.BaseIntervalDiff = altDiff;
                bassGuitar.BaseIntervalDiff = bassDiff;

                var phrases = new List<Phrase>
                {
                    mainGuitar.Phrase.Clone(),

                    NoteHelper.ShiftNotes(altGuitar.Phrase, altDiff * -1, Interval.Step, altDiff < 0 ? Direction.Up : Direction.Down),
                    NoteHelper.ShiftNotes(bassGuitar.Phrase, bassDiff * -1, Interval.Step, bassDiff < 0 ? Direction.Up : Direction.Down),
                };

                var basePhrase = MergePhrases(phrases).Phrase;
                basePhrase.Description = section;
                basePhrase.Bpm = 200M;
                PhraseHelper.DuplicatePhrase(basePhrase);
                PhraseHelper.DuplicatePhrase(basePhrase);
                PhraseHelper.DuplicatePhrase(basePhrase);

                var clip = new Clip
                {
                    Phrase = basePhrase,
                    Artist = mainGuitar.Artist,
                    ClipType = ClipType.BasePhrase,
                    BaseIntervalDiff = 0,
                    Name = section,
                    Scale = mainGuitar.Scale,
                    Section = mainGuitar.Section,
                    Song = mainGuitar.Song
                };

                Clips.Add(clip);
            }
        }

        private static MergedPhrase MergePhrases(List<Phrase> sourcePhrases)
        {
            var mergedPhrase = new MergedPhrase()
            {
                SourcePhrase = sourcePhrases,
                SourceIndexes = new List<Tuple<string, decimal, decimal>>()
            };

            var length = sourcePhrases.Max(x => x.PhraseLength);
            foreach (var phrase in sourcePhrases)
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

            var newPhrase =  new Phrase { PhraseLength = length };

            var positions = sourcePhrases.SelectMany(x => x.Elements)
                .Select(x => x.Position)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            foreach (var position in positions)
            {
                var distinctElements = sourcePhrases
                    .Select(phrase => phrase.Elements.Where(x => x.Position <= position)
                        .OrderByDescending(x => x.Position)
                        .FirstOrDefault())
                    .Where(element => element != null)
                    .ToList();

                var distinctNotes = distinctElements
                    .GroupBy(x => new { x.Note, x.Duration })
                    .Select(x => new
                    {
                        x.Key.Note,
                        x.Key.Duration,
                        Count = x.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var newElement = distinctNotes
                    .OrderByDescending(x => x.Count)
                    .ThenByDescending(x => x.Duration)
                    .ThenBy(x => x.Note)
                    .Take(1)
                    .Select(x => new PhraseElement() { Duration = x.Duration, Note = x.Note, Position = position })
                    .FirstOrDefault();

                if(newElement == null)
                    throw new ApplicationException("no new element");

                var sourceElement = distinctElements
                    .FirstOrDefault(x => x.Note == newElement.Note && x.Duration == newElement.Duration);

                if (sourceElement == null)
                    throw new ApplicationException("no source element");

                foreach (var phrase in sourcePhrases)
                {
                    if(phrase.Elements.Contains(sourceElement))
                        mergedPhrase.SourceIndexes.Add(new Tuple<string, decimal, decimal>(phrase.Description, position, sourceElement.Position));
                }

                newElement.Duration = 0.1M;
                newPhrase.Elements.Add(newElement);
            }

            if (newPhrase.Elements[0] != null && newPhrase.Elements[0].Position != 0)
                newPhrase.Elements[0].Position = 0;

            PhraseHelper.UpdateDurationsFromPositions(newPhrase, newPhrase.PhraseLength);

            mergedPhrase.Phrase = newPhrase;

            return mergedPhrase;

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
            foreach (var clip in Clips.Where(x => x.ClipType == ClipType.BasePhrase))
            {
                var patterns = PatternFinder.FindPatterns(clip.Phrase);
                var tempoPatterns = PatternFinder.FindPatterns(clip.Phrase, true);

                Console.WriteLine(clip.Phrase.Description
                                      + " has "
                                      + clip.Phrase.PhraseLength
                                      + " notes and "
                                      + patterns.Count + " patterns and "
                                      + tempoPatterns.Count + " tempo patterns");


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
                clip.Phrase = ScaleHelper.TransposeToScale(clip.Phrase, clip.Scale, BaseScale);
            }
        }

        private IEnumerable<Clip> InstrumentClips()
        {
            return Clips.Where(x => x.ClipType != ClipType.Drums && x.ClipType != ClipType.BasePhrase);
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
                    ClipType = GetClipType(x)
                })
                .ToList();
        }

        private static ClipType GetClipType(string filename)
        {
            if (filename.EndsWith(" 4.mid"))
                return ClipType.Drums;
            if (filename.EndsWith(" 3.mid"))
                return ClipType.BassGuitar;

            return filename.EndsWith(" 2.mid") ? ClipType.AltGuitar : ClipType.MainGuitar;
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
            public int BaseIntervalDiff { get; set; }

            public ClipType ClipType { get; set; }

        }

        private class MergedPhrase
        {
            public Phrase Phrase { get; set; }
            public List<Phrase> SourcePhrase { get; set; }
            public List<Tuple<string, decimal, decimal>> SourceIndexes { get; set; }
    }

        private enum ClipType
        {
            MainGuitar,
            AltGuitar,
            BassGuitar,
            Drums,
            BasePhrase
        }
    }
}
