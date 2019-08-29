using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace Halloumi.Notez.Engine.Generator
{
    public class SectionGenerator
    {
        private List<Clip> Clips { get; set; }

        private readonly Random _random = new Random();

        private readonly string _folder;

        private readonly GeneratorSettings _generatorSettings;

        public SectionGenerator(string folder, string library, bool clearCache = false)
        {
            _folder = folder;
            var settingFile = Path.Combine(folder, library + ".generatorSettings.json");
            _generatorSettings = JsonConvert.DeserializeObject<GeneratorSettings>(File.ReadAllText(settingFile));

            if (clearCache || !LoadCache())
            {
                Clips = LoadMidi();
                CalculateScales();
                MashToScale();
                MergeChords();
                MergeRepeatedNotes();
                CalculateLengths();
                CalculateBasePhrases();
                CalculateDrumAverages();
            }

            SaveCache();
        }

        private string GetLibraryFolder()
        {
            return Path.Combine(_folder, _generatorSettings.LibraryFolder);
        }

        private string GetSecondaryLibraryFolder()
        {
            return Path.Combine(_folder, _generatorSettings.SecondaryLibraryFolder);
        }


        private void SaveCache()
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(GetCacheFilename(), FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, Clips);
            stream.Close();
        }

        private string GetCacheFilename()
        {
            return _generatorSettings.LibraryFolder + ".cache.bin";
        }

        private bool LoadCache()
        {
            if (!File.Exists(GetCacheFilename()))
                return false;

            try
            {
                var formatter = new BinaryFormatter();
                var stream = new FileStream(GetCacheFilename(), FileMode.Open, FileAccess.Read, FileShare.None);
                Clips = formatter.Deserialize(stream) as List<Clip>;
                stream.Close();

                Console.WriteLine("Loading clips cache");

                return true;
            }
            catch
            {
                return false;
            }
        }



        public void GenerateRiffs(string name, int count = 0, SourceFilter filter = null)
        {
            if (count == 0)
                GenerateSection(name, filter);

            for (var i = 0; i < count; i++)
            {
                GenerateSection(name + i, filter);
            }
        }

        public void MergeSourceClips()
        {
            var sections = Clips.Where(x => !x.IsSecondary).Select(x => x.Section).Distinct().ToList();

            foreach (var sectionName in sections)
            {
                var section = new Section();
                foreach (var channel in _generatorSettings.Channels)
                {
                    var channelClip = Clips.First(x => x.ClipType == channel.Name && x.Section == sectionName);

                    var channelPhrase = channelClip.Phrase.Clone();
                    channelPhrase.IsDrums = channel.IsDrums;
                    channelPhrase.Description = channel.Name;
                    channelPhrase.Instrument = channel.Instrument;
                    channelPhrase.Bpm = _generatorSettings.Bpm;
                    channelPhrase.PhraseLength = NoteHelper.GetTotalDuration(channelPhrase);

                    section.Phrases.Add(channelPhrase);
                }

                MidiHelper.SaveToMidi(section, Path.Combine(GetLibraryFolder(), sectionName + ".mid"));
            }

            var filesToDelete = Directory.EnumerateFiles(GetLibraryFolder(), "*.mid", SearchOption.AllDirectories)
                .Where(IsSingleChannelMidiFile)
                .ToList();

            foreach (var fileToDelete in filesToDelete)
                File.Delete(fileToDelete);
        }


        private List<Clip> GenerateRandomClips(IEnumerable<Clip> sourceBaseClips)
        {
            const decimal maxLength = 64M;

            var sourcePhrases = sourceBaseClips.Select(x => x.Phrase.Clone()).ToList();

            var phraseLength = sourcePhrases[0].PhraseLength;
            if (phraseLength > maxLength)
                phraseLength = maxLength;

            PhraseHelper.EnsureLengthsAreEqual(sourcePhrases);
            var probabilities = GenerateProbabilities(sourcePhrases);
            PhraseHelper.EnsureLengthsAreEqual(sourcePhrases, phraseLength);

            var clips = new List<Clip>();
            var randomSourceCount = GetBellCurvedRandom(_generatorSettings.RandomSourceCount - 1, _generatorSettings.RandomSourceCount + 1);

            for (var i = 0; i < randomSourceCount; i++)
            {
                var basePhrase = GenratePhraseBasic(probabilities, phraseLength / 2);
                PhraseHelper.DuplicatePhrase(basePhrase);


                var patterns = PatternFinder.FindPatterns(sourcePhrases.OrderBy(x => _random.Next()).FirstOrDefault())
                    .OrderByDescending(x => x.Value.PatternType)
                    .ThenBy(x => x.Value.WindowSize)
                    .ThenBy(x => x.Value.ToList().FirstOrDefault().Value.Start)
                    .ToList();

                basePhrase = ApplyPatterns(patterns, basePhrase, probabilities);
                basePhrase.Bpm = _generatorSettings.Bpm;

                PhraseHelper.UpdateDurationsFromPositions(basePhrase, phraseLength);

                var name = "Random-Random" + i;
                var artist = "Random";
                var song = "Random-Random";
                basePhrase.Description = name;


                clips.Add(new Clip()
                {
                    Phrase = basePhrase,
                    ClipType = "BasePhrase",
                    Section = name,
                    Artist = artist,
                    Name = name,
                    Song = song
                });


                foreach (var channel in _generatorSettings.Channels.Where(x => !x.IsDrums).ToList())
                {
                    var channelPhrase = basePhrase.Clone();
                    NoteHelper.ShiftNotes(channelPhrase, channel.DefaultStepDifference, Interval.Step);
                    clips.Add(new Clip()
                    {
                        Phrase = channelPhrase,
                        ClipType = channel.Name,
                        Section = name,
                        Artist = artist,
                        Name = name,
                        Song = song
                    });
                }

            }


            return clips;
        }

        private Phrase ApplyPatterns(IReadOnlyCollection<KeyValuePair<string, PatternFinder.Pattern>> patterns, Phrase sourcePhrase, PhraseProbabilities probabilities)
        {
            var phraseLength = sourcePhrase.PhraseLength;
            var newPhrase = sourcePhrase.Clone();

            foreach (var pattern in patterns)
            {
                var sectionStartPositions = new List<decimal>();
                foreach (var window in pattern.Value.Select(x => x.Value))
                {
                    sectionStartPositions.Add(window.Start);
                    sectionStartPositions.Add(window.End + 1);
                }
                foreach (var position in sectionStartPositions)
                {
                    var element = newPhrase.Elements.FirstOrDefault(x => x.Position == position);
                    if (element != null) continue;

                    element = GetNewRandomElement(position, probabilities);
                    if (element != null)
                        newPhrase.Elements.Add(element);
                }
            }

            foreach (var pattern in patterns)
            {
                var firstWindow = pattern.Value.OrderBy(x => _random.Next()).FirstOrDefault().Value;
                foreach (var window in pattern.Value.Select(x => x.Value))
                {
                    if (window == firstWindow)
                        continue;

                    newPhrase.Elements.RemoveAll(x => x.Position >= window.Start && x.Position <= window.End);

                    var repeatingElements = newPhrase.Elements
                        .Where(x => x.Position >= firstWindow.Start && x.Position <= firstWindow.End)
                        .Select(x => x.Clone())
                        .ToList();

                    foreach (var element in repeatingElements)
                    {
                        element.Position = element.Position - firstWindow.Start + window.Start;
                        if (pattern.Value.PatternType == PatternFinder.PatternType.Perfect) continue;

                        var probability = probabilities.NoteProbabilities.FirstOrDefault(x => x.Position == element.Position);
                        if (probability != null)
                            element.Note = GetRandomNote(probability.Notes);
                    }

                    newPhrase.Elements.AddRange(repeatingElements);
                }
            }

            newPhrase.Elements = newPhrase.Elements.OrderBy(x => x.Position).ToList();
            newPhrase.Elements.RemoveAll(x => x.Position >= phraseLength);

            PhraseHelper.UpdateDurationsFromPositions(newPhrase, phraseLength);

            return newPhrase;
        }

        private Phrase GenratePhraseBasic(PhraseProbabilities probabilities, decimal phraseLength)
        {
            var noteCount = GetBellCurvedRandom(probabilities.MinNotes, probabilities.MaxNotes + 1);
            var phrase = new Phrase() { PhraseLength = phraseLength };

            var selectedNotes =
            (from onoffProbability in probabilities.NoteProbabilities
             let noteOn = GetRandomBool(onoffProbability.OnOffChance)
             where noteOn
             select onoffProbability).ToList();

            while (selectedNotes.Count > noteCount)
            {
                var leastPopularNote = selectedNotes.OrderBy(x => x.OnOffChance).FirstOrDefault();
                selectedNotes.Remove(leastPopularNote);
            }
            while (selectedNotes.Count < noteCount)
            {
                var mostPopularNote = probabilities.NoteProbabilities.Except(selectedNotes)
                    .OrderByDescending(x => x.OnOffChance)
                    .FirstOrDefault();
                selectedNotes.Add(mostPopularNote);
            }

            selectedNotes = selectedNotes.Where(x => x != null).ToList();

            selectedNotes = selectedNotes.OrderBy(x => x.Position).ToList();

            foreach (var note in selectedNotes)
            {
                var randomNote = GetRandomNote(note.Notes);

                phrase.Elements.Add(new PhraseElement
                {
                    Position = note.Position,
                    Duration = 1,
                    Note = randomNote
                });
            }
            phrase.Elements.RemoveAll(x => x.Position >= phraseLength);

            PhraseHelper.UpdateDurationsFromPositions(phrase, phraseLength);
            return phrase;
        }

        private int GetBellCurvedRandom(int min, int maxInclusive)
        {
            var range = maxInclusive - min;
            return min + Convert.ToInt32(Math.Round(range * GetBellCurvedRandom()));
        }


        private double GetBellCurvedRandom()
        {
            return (Math.Pow(2 * _random.NextDouble() - 1, 3) / 2) + .5;
        }

        private int GetRandomNote(Dictionary<int, int> noteNumbers)
        {
            var numbers = new List<int>();
            foreach (var noteNumber in noteNumbers)
            {
                for (var i = 0; i < noteNumber.Value; i++)
                {
                    numbers.Add(noteNumber.Key);
                }
            }

            var randomIndex = _random.Next(0, numbers.Count);

            return numbers[randomIndex];
        }

        private bool GetRandomBool(double chanceOfTrue)
        {
            var randomNumber = _random.NextDouble();
            return randomNumber <= chanceOfTrue;
        }

        private PhraseElement GetNewRandomElement(decimal position, PhraseProbabilities probabilities)
        {
            var probability = probabilities.NoteProbabilities.FirstOrDefault(x => x.Position == position);
            if (probability == null)
                return null;

            var randomNote = GetRandomNote(probability.Notes);

            return new PhraseElement
            {
                Position = position,
                Duration = 1,
                Note = randomNote
            };
        }



        private static PhraseProbabilities GenerateProbabilities(IReadOnlyCollection<Phrase> phrases)
        {
            var probabilities = new PhraseProbabilities();

            var allNotes = phrases.SelectMany(x => x.Elements).GroupBy(x => new
            {
                Position = Math.Round(x.Position, 8),
                x.Note
            })
                .Select(x => new
                {
                    x.Key.Position,
                    x.Key.Note,
                    Count = x.Count()
                })
                .OrderBy(x => x.Position)
                .ThenBy(x => x.Note)
                .ToList();

            probabilities.NoteProbabilities = allNotes
                .Select(x => x.Position)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new NoteProbability()
                {
                    Position = x,
                    OnOffChance = allNotes.Where(y => y.Position == x).Sum(y => y.Count) / Convert.ToDouble(phrases.Count),
                })
                .ToList();
            foreach (var prob in probabilities.NoteProbabilities)
            {
                prob.Notes = allNotes.Where(y => y.Position == prob.Position).ToDictionary(y => y.Note, y => y.Count);
            }

            probabilities.MinNotes = phrases.Select(x => x.Elements.Count).Min();
            probabilities.MaxNotes = phrases.Select(x => x.Elements.Count).Max();


            var chords = phrases.SelectMany(x => x.Elements)
                .Where(x => x.IsChord)
                .Select(x => x.Position)
                .ToList();

            if (chords.Count > 0)
                Console.WriteLine(chords);


            var repeatingNotes = phrases.SelectMany(x => x.Elements)
                .Where(x => x.HasRepeatingNotes)
                .Select(x => x.Position)
                .ToList();

            if (repeatingNotes.Count > 0)
                Console.WriteLine(repeatingNotes);

            return probabilities;

        }

        private void GenerateSection(string filename, SourceFilter filter)
        {
            var sourceCount = GetBellCurvedRandom(_generatorSettings.SourceCount - 1, _generatorSettings.SourceCount + 1);
            if (sourceCount < 2) sourceCount = 2;
            var sourceBaseClips = LoadSourceBasePhraseClips(sourceCount, filter);

            var randomClips = GenerateRandomClips(sourceBaseClips);
            sourceBaseClips.AddRange(randomClips.Where(x => x.ClipType == "BasePhrase"));

            var mergedPhrase = MergePhrases(sourceBaseClips.Select(x => x.Phrase).ToList());
            mergedPhrase.Phrase.Bpm = _generatorSettings.Bpm;

            var section = new Section();
            foreach (var channel in _generatorSettings.Channels)
            {
                Phrase channelPhrase;
                if (channel.IsDrums)
                {
                    channelPhrase = sourceBaseClips
                        //.Where(x => !x.IsSecondary)
                        .OrderByDescending(x => _random.Next())
                        .Select(
                            drums => Clips.FirstOrDefault(
                                x => x.ClipType == channel.Name && x.Artist == drums.Artist && x.Song == drums.Song &&
                                     x.Section == drums.Section))
                        .Where(x => x != null)
                        .Select(x => x.Phrase.Clone())
                        .FirstOrDefault();
                }
                else
                {
                    var channelClips = Clips.Where(x => x.ClipType == channel.Name).ToList();
                    channelClips.AddRange(randomClips.Where(x => x.ClipType == channel.Name));
                    channelPhrase = GeneratePhraseFromBasePhrase(mergedPhrase, sourceBaseClips, channelClips);

                    foreach (var element in channelPhrase.Elements.Where(x => x.Note < -24))
                    {
                        element.Note += 12;
                    }
                }

                if (channelPhrase == null)
                    continue;

                channelPhrase.IsDrums = channel.IsDrums;
                channelPhrase.Description = channel.Name;
                channelPhrase.Instrument = channel.Instrument;

                section.Phrases.Add(channelPhrase);

                VelocityHelper.ApplyVelocityStrategy(channelPhrase, channel.VelocityStrategy);
            }

            if (!filename.EndsWith(".mid"))
                filename += ".mid";

            PhraseHelper.EnsureLengthsAreEqual(section.Phrases);
            MidiHelper.SaveToMidi(section, filename);
        }


        private List<Clip> LoadSourceBasePhraseClips(int count, SourceFilter filter)
        {
            var clips = new List<Clip>();
            if (filter == null) filter = new SourceFilter();
            
            clips.AddRange(Clips
                .Where(x => x.ClipType == "BasePhrase" && !x.IsSecondary)
                .Where(x => string.IsNullOrEmpty(filter.SeedArtist) || x.Artist == filter.SeedArtist)
                .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                .Where(x => string.IsNullOrEmpty(filter.SeedSection) || x.Section == filter.SeedSection)
                .OrderBy(x => _random.Next())
                .Take(1));
            

            if (clips.Count == 0)
            {
                clips.AddRange(Clips
                    .Where(x => x.ClipType == "BasePhrase" && !x.IsSecondary)
                    .OrderBy(x => _random.Next())
                    .Take(1));
            }

            var inititialClip = clips[0];
            var minDuration = inititialClip.Phrase.Elements.Min(x => x.Duration);

            clips.AddRange(Clips.Where(x => x.ClipType == "BasePhrase")
                .Where(x => x != inititialClip)
                .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                .Where(x => x.Phrase.Elements.Min(y => y.Duration) == minDuration)
                .OrderBy(x => Math.Abs(x.AvgDistanceBetweenSnares - inititialClip.AvgDistanceBetweenSnares))
                .ThenBy(x => Math.Abs(x.AvgDistanceBetweenKicks - inititialClip.AvgDistanceBetweenKicks))
                .Take(10)
                .OrderBy(x => _random.Next())
                .Take(count - 1)
                .ToList());

            var missing = count - clips.Count;
            if (missing > 0)
                clips.AddRange(Clips.Where(x => x.ClipType == "BasePhrase")
                    .Where(x => x != inititialClip)
                    .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                    .Where(x => x.Phrase.Elements.Min(y => y.Duration) > minDuration)
                    .OrderBy(x => Math.Abs(x.AvgDistanceBetweenSnares - inititialClip.AvgDistanceBetweenSnares))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenKicks - inititialClip.AvgDistanceBetweenKicks))
                    .Take(10)
                    .OrderBy(x => _random.Next())
                    .Take(missing)
                    .ToList());
            return clips;
        }

        private static Phrase GeneratePhraseFromBasePhrase(MergedPhrase mergedPhrase, IEnumerable<Clip> sourceBaseClips, IReadOnlyCollection<Clip> sourceInstrumentClips)
        {
            var instrumentClips = new List<Clip>();
            var instrumentPhrases = new List<Phrase>();

            foreach (var clip in sourceBaseClips)
            {
                var instrumentClip = sourceInstrumentClips.FirstOrDefault(x => x.Section == clip.Section);
                if (instrumentClip == null)
                    throw new ApplicationException("No instrument clip");

                instrumentClips.Add(instrumentClip);
                instrumentPhrases.Add(instrumentClip.Phrase.Clone());
                instrumentPhrases.Last().Description = instrumentClip.Section;
                NoteHelper.ShiftNotesDirect(instrumentPhrases.Last(), instrumentClip.BaseIntervalDiff * -1, Interval.Step);
            }


            PhraseHelper.EnsureLengthsAreEqual(instrumentPhrases, mergedPhrase.Phrase.PhraseLength);

            var instrumentPhrase = mergedPhrase.Phrase.Clone();
            instrumentPhrase.Elements.Clear();

            var nextPosition = 0M;
            PhraseElement lastElement = null;
            foreach (var sourceIndex in mergedPhrase.SourceIndexes)
            {
                var sourcePhrase = instrumentPhrases.FirstOrDefault(x => x.Description == sourceIndex.Item1);
                var sourceElement = sourcePhrase?.Elements.FirstOrDefault(x => x.Position == sourceIndex.Item2);

                if (sourceElement == null)
                    sourceElement = sourcePhrase?.Elements
                        .Where(x => x.Note == sourceIndex.Item3)
                        .OrderBy(x => Math.Abs(x.Position - sourceIndex.Item2))
                        .FirstOrDefault();

                if (sourceElement == null)
                    sourceElement = sourcePhrase?.Elements.FirstOrDefault(x => x.Position < sourceIndex.Item2);

                if (sourceElement == null)
                    continue;

                if (sourceIndex.Item2 < nextPosition)
                    continue;

                sourceElement = sourceElement.Clone();
                sourceElement.Position = sourceIndex.Item2;

                if (sourceElement.Position < nextPosition && lastElement != null)
                    lastElement.Duration = sourceElement.Position - lastElement.Position;

                if (sourceElement.Position + sourceElement.Duration > mergedPhrase.Phrase.PhraseLength)
                    sourceElement.Duration = mergedPhrase.Phrase.PhraseLength - sourceElement.Position;

                instrumentPhrase.Elements.Add(sourceElement);

                nextPosition = sourceElement.Position + sourceElement.Duration;
                lastElement = sourceElement;

            }
            var intervalDiff = instrumentClips[0].BaseIntervalDiff;
            NoteHelper.ShiftNotesDirect(instrumentPhrase, intervalDiff, Interval.Step);


            return instrumentPhrase;
        }

        private void CalculateBasePhrases()
        {
            var sections = InstrumentClips().Select(x => x.Section).Distinct().ToList();
            foreach (var section in sections)
            {
                var clips = InstrumentClips().Where(x => x.Section == section).ToList();

                var primaryClip = clips.Where(x => GetGeneratorSettingsByClip(x) != null
                    && GetGeneratorSettingsByClip(x).IsPrimaryRiff)
                    .OrderBy(x => GetAverageNote(x.Phrase))
                    .ThenByDescending(x => x.Phrase.Elements.Sum(y => y.Duration))
                    .ThenBy(x => x.Phrase.Elements.Count)
                    .ThenBy(x => GetGeneratorSettingsByClip(x).MidiChannel)
                    .First();

                var phrases = new List<Phrase>
                {
                    primaryClip.Phrase.Clone()
                };

                foreach (var channel in _generatorSettings.Channels.Where(x => !x.IsDrums))
                {
                    var channelClip = clips.FirstOrDefault(x => GetGeneratorSettingsByClip(x) == channel);
                    if (channelClip == null || channelClip == primaryClip)
                        continue;

                    var diff = RoundToNearestMultiple(GetAverageNote(channelClip.Phrase) - GetAverageNote(primaryClip.Phrase), 12);
                    channelClip.BaseIntervalDiff = diff;

                    var shiftedPhrase = NoteHelper.ShiftNotes(channelClip.Phrase, diff * -1, Interval.Step, diff < 0 ? Direction.Up : Direction.Down);
                    phrases.Add(shiftedPhrase);
                }

                var basePhrase = MergePhrases(phrases).Phrase;
                basePhrase.Description = section;

                var clip = new Clip
                {
                    Phrase = basePhrase,
                    Artist = primaryClip.Artist,
                    ClipType = "BasePhrase",
                    BaseIntervalDiff = 0,
                    Name = section,
                    Scale = primaryClip.Scale,
                    Section = primaryClip.Section,
                    Song = primaryClip.Song,
                    Filename = primaryClip.Filename,
                    IsSecondary = primaryClip.IsSecondary
                };

                Clips.Add(clip);
            }
        }

        private GeneratorSettings.Channel GetGeneratorSettingsByClip(Clip clip)
        {
            var channel = _generatorSettings.Channels.FirstOrDefault(x => x.Name == clip.ClipType);
            if (channel == null)
                throw new ApplicationException(clip.Name + " has no matching channel settings");

            return channel;
        }


        private static MergedPhrase MergePhrases(IReadOnlyCollection<Phrase> sourcePhrases)
        {
            var mergedPhrase = new MergedPhrase()
            {
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

            var newPhrase = new Phrase { PhraseLength = length };

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

                if (newElement == null)
                    throw new ApplicationException("no new element");

                var sourceElement = distinctElements
                    .FirstOrDefault(x => x.Note == newElement.Note && x.Duration == newElement.Duration);

                if (sourceElement == null)
                    throw new ApplicationException("no source element");

                foreach (var phrase in sourcePhrases)
                {
                    if (phrase.Elements.Contains(sourceElement))
                        mergedPhrase.SourceIndexes.Add(new Tuple<string, decimal, decimal>(phrase.Description, position, sourceElement.Note));
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
            return (int)Math.Round(value / (double)factor, MidpointRounding.AwayFromZero) * factor;
        }

        private static int GetAverageNote(Phrase phrase)
        {
            return (int)Math.Round((phrase.Elements.Average(y => y.Note) + phrase.Elements.Min(y => y.Note)) / 2);
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


            invalidClips = Clips.Where(x => x.Phrase.Elements.Any(y => y.Note < -24)).ToList();
            foreach (var clip in invalidClips)
            {
                Console.WriteLine("Invalid Notes:" + clip.Name);
            }

            Clips.RemoveAll(x => x.Phrase.Elements.Any(y => y.Note < -12));
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
                    Console.WriteLine(clip.Name.PadRight(20) + clip.Scale + (clip.ScaleMatchIncomplete ? $"({clip.ScaleMatch.DistanceFromScale})" : ""));
                }
                clip.Phrase = ScaleHelper.TransposeToScale(clip.Phrase, clip.Scale, _generatorSettings.Scale);
            }
        }

        private IEnumerable<Clip> InstrumentClips()
        {
            return Clips.Where(x => x.ClipType != "BasePhrase" && !GetGeneratorSettingsByClip(x).IsDrums);
        }

        private IEnumerable<Clip> DrumClips()
        {
            return Clips.Where(x => x.ClipType != "BasePhrase" && GetGeneratorSettingsByClip(x).IsDrums);
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
                if (scaleCount > 3)
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

        private List<Clip> LoadMidi()
        {
            var clips = LoadMidiInFolder(GetLibraryFolder());

            if (!string.IsNullOrEmpty(_generatorSettings.SecondaryLibraryFolder))
            {
                clips.AddRange(LoadMidiInFolder(GetSecondaryLibraryFolder(), true, _generatorSettings.SecondaryLibraryLengthMultiplier));
            }

            return clips;

        }

        private List<Clip> LoadMidiInFolder(string folder, bool isSecondary = false, decimal lengthModifier = 0)
        {
            var clips = Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories)
                .Where(IsSingleChannelMidiFile)
                .OrderBy(x => Path.GetFileNameWithoutExtension(x) + "")
                .Select(x => new Clip
                {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Song = GetSongNameFromFilename(x),
                    Section = GetSectionNameFromFilename(x),
                    Artist = GetArtistNameFromFilename(x),
                    Phrase = MidiHelper.ReadMidi(x).Phrases[0],
                    ClipType = GetClipTypeByFilename(x),
                    Filename = x,
                    IsSecondary = isSecondary
                })
                .ToList();


            foreach (var clip in clips.Where(x => GetGeneratorSettingsByClip(x).IsDrums))
            {
                clip.Phrase.IsDrums = true;
            }

            var multiChannelMidis = Directory.EnumerateFiles(folder, "*.mid", SearchOption.AllDirectories)
                .OrderBy(x => Path.GetFileNameWithoutExtension(x) + "")
                .Where(x => !IsSingleChannelMidiFile(x))
                .ToList();

            foreach (var multiChannelMidi in multiChannelMidis)
            {
                var section = MidiHelper.ReadMidi(multiChannelMidi);
                foreach (var phrase in section.Phrases)
                {
                    var clip = new Clip
                    {
                        Name = Path.GetFileNameWithoutExtension(multiChannelMidi),
                        Song = GetSongNameFromFilename(multiChannelMidi),
                        Section = GetSectionNameFromFilename(multiChannelMidi),
                        Artist = GetArtistNameFromFilename(multiChannelMidi),
                        Phrase = phrase,
                        ClipType = _generatorSettings.Channels[section.Phrases.IndexOf(phrase)].Name,
                        Filename = multiChannelMidi,
                        IsSecondary = isSecondary
                    };

                    if (!clips.Exists(x => x.Song == clip.Song && x.Section == clip.Section && x.Artist == clip.Artist && x.ClipType == clip.ClipType))
                        clips.Add(clip);
                }
            }

            if (lengthModifier == 0) return clips;

            foreach (var clip in clips)
            {
                PhraseHelper.ChangeLength(clip.Phrase, lengthModifier);
            }

            return clips;
        }

        private string GetArtistNameFromFilename(string filename)
        {
            filename = GetSectionNameFromFilename(filename);
            filename = filename.Split('-')[0];

            return filename;
        }

        private string RemoveFileEnding(string filename)
        {
            foreach (var channel in _generatorSettings.Channels)
            {
                if (filename.EndsWith(channel.FileEnding + ".mid"))
                    filename = filename.Replace(channel.FileEnding + ".mid", "");
            }

            return filename;
        }



        private string GetSectionNameFromFilename(string filename)
        {
            filename = RemoveFileEnding(filename);
            filename = (Path.GetFileNameWithoutExtension(filename) + "").Replace(" ", "");
            return filename;
        }

        private string GetSongNameFromFilename(string filename)
        {
            filename = GetSectionNameFromFilename(filename);
            filename = filename.Split('-')[1];
            filename = Regex.Replace(filename, @"[\d-]", string.Empty);

            return filename;
        }

        private bool IsSingleChannelMidiFile(string filename)
        {
            return _generatorSettings.Channels.Any(channel => filename.EndsWith(channel.FileEnding + ".mid"));
        }

        private void CalculateDrumAverages()
        {
            foreach (var drumClip in DrumClips())
            {
                var kicks = drumClip.Phrase.Elements.Where(x => DrumHelper.IsBassDrum(x.Note)).ToList();
                if (kicks.Count < 2)
                    continue;

                var totalKickDiff =
                (
                    from kick in kicks
                    let next = kicks.Where(x => x.Position > kick.Position).OrderBy(x => x.Position).FirstOrDefault()
                    where next != null
                    select next.Position - kick.Position
                ).Sum();

                drumClip.AvgDistanceBetweenKicks = totalKickDiff / (kicks.Count - 1);


                var snares = drumClip.Phrase.Elements.Where(x => DrumHelper.IsSnareDrum(x.Note)).ToList();
                if (snares.Count < 2)
                    continue;

                var totalSnareDiff =
                (
                    from snare in snares
                    let next = snares.Where(x => x.Position > snare.Position).OrderBy(x => x.Position).FirstOrDefault()
                    where next != null
                    select next.Position - snare.Position
                ).Sum();

                drumClip.AvgDistanceBetweenSnares = totalSnareDiff / (snares.Count - 1);
            }
        }

        private string GetClipTypeByFilename(string filename)
        {
            foreach (var channel in _generatorSettings.Channels)
            {
                if (filename.EndsWith(channel.FileEnding + ".mid"))
                    return channel.Name;
            }

            throw new ApplicationException("Unknown midi extension");
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

        [Serializable]
        private class Clip
        {
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

            public string ClipType { get; set; }
            public decimal AvgDistanceBetweenKicks { get; set; }
            public decimal AvgDistanceBetweenSnares { get; set; }
            public string Filename { get; internal set; }

            public bool IsSecondary { get; set; }
        }

        private class MergedPhrase
        {
            public Phrase Phrase { get; set; }
            public List<Tuple<string, decimal, decimal>> SourceIndexes { get; set; }
        }

        public class PhraseProbabilities
        {
            public List<NoteProbability> NoteProbabilities { get; set; }
            public int MinNotes { get; set; }
            public int MaxNotes { get; set; }

            public List<NoteProbability> ChordProbabilities { get; set; }
        }


        public class NoteProbability
        {
            public decimal Position { get; set; }

            public double OnOffChance { get; set; }

            public Dictionary<int, int> Notes { get; set; }
        }

        public class SourceFilter
        {
            public string SeedSection { get; set; }
            public string SeedArtist { get; set; }
            public string ArtistFilter { get; set; }
        }

        public class GeneratorSettings
        {
            public int SourceCount { get; set; }

            public int RandomSourceCount { get; set; }

            public decimal Bpm { get; set; }

            public string Scale { get; set; }

            public List<Channel> Channels { get; set; }
            public string LibraryFolder { get; set; }

            public string SecondaryLibraryFolder { get; set; }

            public decimal SecondaryLibraryLengthMultiplier { get; set; }

            public class Channel
            {
                public Channel(string name, int midiChannel, MidiInstrument instrument, string fileEnding, int defaultStepDifference = 0, bool primaryChannel = false, string velocityStrategy = "")
                {
                    Name = name;
                    MidiChannel = midiChannel;
                    Instrument = instrument;
                    FileEnding = fileEnding;
                    DefaultStepDifference = defaultStepDifference;
                    IsPrimaryRiff = primaryChannel;
                    VelocityStrategy = velocityStrategy;
                }

                public string Name { get; set; }

                public int MidiChannel { get; set; }

                public MidiInstrument Instrument { get; set; }

                public string FileEnding { get; set; }

                public int DefaultStepDifference { get; set; }

                public bool IsDrums => MidiChannel == 10;

                public bool IsPrimaryRiff { get; set; }

                public string VelocityStrategy { get; set; }
            }
        }
    }
}
