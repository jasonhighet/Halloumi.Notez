﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using Newtonsoft.Json;

namespace Halloumi.Notez.Engine.Generator
{
    public class SectionGenerator
    {
        private readonly string _folder;

        private readonly Random _random = new Random();

        private GeneratorSettings _generatorSettings;

        public SectionGenerator(string folder)
        {
            _folder = folder;
        }

        private List<Clip> Clips { get; set; }

        public List<string> GetLibraries()
        {
            return Directory.GetFiles(_folder, "*.generatorSettings.json")
                .ToList()
                .Select(Path.GetFileNameWithoutExtension)
                .Select(x => x.Replace(".generatorSettings", ""))
                .ToList();
        }

        public List<string> GetArtists()
        {
            return Clips.Where(x => !x.IsSecondary).Select(x => x.Artist).Distinct().ToList();
        }

        public List<string> GetSections()
        {
            return Clips.Where(x => !x.IsSecondary).Select(x => x.Section).Distinct().ToList();
        }

        public List<string> GetDrumPatterns()
        {
            return Clips.Where(x => !x.IsSecondary)
                .Select(x =>
                    Math.Round(x.AvgDistanceBetweenKicks, 0, MidpointRounding.AwayFromZero).ToString("00") + "," +
                    Math.Round(x.AvgDistanceBetweenSnares, 0, MidpointRounding.AwayFromZero).ToString("00"))
                .Distinct()
                .ToList();
        }


        public void LoadLibrary(string library, bool clearCache)
        {
            var settingFile = Path.Combine(_folder, library + ".generatorSettings.json");
            _generatorSettings = JsonConvert.DeserializeObject<GeneratorSettings>(File.ReadAllText(settingFile));

            if (!clearCache && LoadCache()) return;

            Clips = LoadMidi();
            CalculateScales();
            MashToScale();
            MergeChords();
            MergeRepeatedNotes();
            CalculateLengths();
            ProcessChords();
            CalculateDrumAverages();
            CalculateBasePhrases();
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

            for (var i = 0; i < count; i++) GenerateSection(name + i, filter);
        }

        public void MergeSourceClips()
        {
            var clips = LoadMidiInFolder(GetLibraryFolder());
            var sections = clips
                .Where(x => !x.IsSecondary)
                .Where(x=> IsSingleChannelMidiFile(x.Filename))
                .Select(x => x.Section)
                .Distinct().ToList();

            foreach (var sectionName in sections)
            {
                var section = new Section(sectionName);
                foreach (var channel in _generatorSettings.Channels)
                {
                    var channelClip = clips.FirstOrDefault(x => x.ClipType == channel.Name && x.Section == sectionName);
                    if (channelClip == null)
                    {
                        Console.WriteLine(sectionName + " is missing " + channel.Name);
                        continue;
                    }

                    section.Phrases.Add(channelClip.Phrase.Clone());
                }

                if (section.Phrases.Count != _generatorSettings.Channels.Count) continue;

                if (!ApplyStrategiesToSection(section))
                {
                    Console.WriteLine("Can't apply strategies to "  + sectionName);
                    continue;
                }

                var filepath = Path.Combine(GetLibraryFolder(), sectionName + ".mid");
                MidiHelper.SaveToMidi(section, filepath);

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
            var randomSourceCount = GetBellCurvedRandom(_generatorSettings.RandomSourceCount - 1,
                _generatorSettings.RandomSourceCount + 1);

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


                clips.Add(new Clip
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
                    clips.Add(new Clip
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

        private Phrase ApplyPatterns(IReadOnlyCollection<KeyValuePair<string, PatternFinder.Pattern>> patterns,
            Phrase sourcePhrase, PhraseProbabilities probabilities)
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

                        var probability =
                            probabilities.NoteProbabilities.FirstOrDefault(x => x.Position == element.Position);
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
            var phrase = new Phrase { PhraseLength = phraseLength };

            var selectedNotes =
                (from onOffProbability in probabilities.NoteProbabilities
                 let noteOn = GetRandomBool(onOffProbability.OnOffChance)
                 where noteOn
                 select onOffProbability).ToList();

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
            return Math.Pow(2 * _random.NextDouble() - 1, 3) / 2 + .5;
        }

        private int GetRandomNote(Dictionary<int, int> noteNumbers)
        {
            var numbers = new List<int>();
            foreach (var noteNumber in noteNumbers)
                for (var i = 0; i < noteNumber.Value; i++)
                    numbers.Add(noteNumber.Key);

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
                .Select(x => new NoteProbability
                {
                    Position = x,
                    OnOffChance = allNotes.Where(y => y.Position == x).Sum(y => y.Count) /
                                  Convert.ToDouble(phrases.Count)
                })
                .ToList();
            foreach (var prob in probabilities.NoteProbabilities)
                prob.Notes = allNotes.Where(y => y.Position == prob.Position).ToDictionary(y => y.Note, y => y.Count);

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
            if (!filename.EndsWith(".mid"))
                filename += ".mid";

            var sourceCount =
                GetBellCurvedRandom(_generatorSettings.SourceCount - 1, _generatorSettings.SourceCount + 1);
            if (sourceCount < 2) sourceCount = 2;
            var sourceBaseClips = LoadSourceBasePhraseClips(sourceCount, filter);

            var section = GenerateSection(sourceBaseClips, Path.GetFileNameWithoutExtension(filename));
            if (!ApplyStrategiesToSection(section))
                throw new ApplicationException("Cannot apply strategies to generated section");

            MidiHelper.SaveToMidi(section, filename);
        }

        private Section GenerateSection(List<Clip> sourceBaseClips, string name)
        {
            var randomClips = GenerateRandomClips(sourceBaseClips);
            sourceBaseClips.AddRange(randomClips.Where(x => x.ClipType == "BasePhrase"));

            var mergedPhrase = MergePhrases(sourceBaseClips.Select(x => x.Phrase).ToList());
            mergedPhrase.Phrase.Bpm = _generatorSettings.Bpm;

            var section = new Section(name);
            foreach (var channel in _generatorSettings.Channels)
            {
                Phrase channelPhrase;
                if (channel.IsDrums)
                {
                    channelPhrase = sourceBaseClips
                        .Where(x => _generatorSettings.SecondaryLibraryIncludeDrums ||
                                    !_generatorSettings.SecondaryLibraryIncludeDrums && !x.IsSecondary)
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
                }

                section.Phrases.Add(channelPhrase ?? new Phrase());
            }

            return section;
        }

        private void ProcessChords()
        {
            foreach (var channel in _generatorSettings.Channels.Where(x => !x.IsDrums))
                if (channel.MaximumNotesInChord > 0 || channel.ConvertChordsToNotesCoverage > 0)
                {
                    var channelClips = Clips.Where(x => x.ClipType == channel.Name).ToList();
                    foreach (var clip in channelClips) ProcessChords(channel, clip.Phrase);
                }
        }

        private static void ProcessChords(GeneratorSettings.Channel channel, Phrase phrase)
        {
            PhraseHelper.MergeChords(phrase);

            if (channel.MaximumNotesInChord > 0)
            {
                var chordElements = phrase.Elements
                    .Where(x => x.IsChord && x.ChordNotes.Count > channel.MaximumNotesInChord).ToList();

                if (chordElements.Count > 0)
                    foreach (var element in chordElements)
                        element.ChordNotes.RemoveAll(x => element.ChordNotes.IndexOf(x) >= channel.MaximumNotesInChord);
            }

            if (channel.ConvertChordsToNotesCoverage == 0) return;
            if (!phrase.Elements.Any(x => x.IsChord)) return;

            var chordDuration = phrase.Elements.Where(x => x.IsChord).Sum(x => x.Duration);
            var totalCoverage = chordDuration / phrase.PhraseLength;
            var durationCoverage = chordDuration / phrase.Elements.Sum(x => x.Duration);
            var countCoverage = Convert.ToDecimal(phrase.Elements.Count(x => x.IsChord)) /
                                Convert.ToDecimal(phrase.Elements.Count);

            if (totalCoverage < channel.ConvertChordsToNotesCoverage
                && durationCoverage < channel.ConvertChordsToNotesCoverage
                && countCoverage < channel.ConvertChordsToNotesCoverage) return;

            foreach (var element in phrase.Elements)
                element.ChordNotes.Clear();

            phrase.Clone();
        }

        private List<Clip> LoadSourceBasePhraseClips(int count, SourceFilter filter)
        {
            var clips = new List<Clip>();
            if (filter == null) filter = new SourceFilter();

            var secondaryCount = _generatorSettings.SecondaryLibrarySourceCount;

            clips.AddRange(Clips
                .Where(x => x.ClipType == "BasePhrase" && !x.IsSecondary)
                .Where(x => string.IsNullOrEmpty(filter.SeedArtist) || x.Artist == filter.SeedArtist)
                .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                .Where(x => string.IsNullOrEmpty(filter.SeedSection) || x.Section == filter.SeedSection)
                .Where(x => filter.AvgDistanceBetweenKicks == 0 ||
                            Math.Round(x.AvgDistanceBetweenKicks, 0, MidpointRounding.AwayFromZero) ==
                            filter.AvgDistanceBetweenKicks)
                .Where(x => filter.AvgDistanceBetweenSnares == 0 ||
                            Math.Round(x.AvgDistanceBetweenSnares, 0, MidpointRounding.AwayFromZero) ==
                            filter.AvgDistanceBetweenSnares)
                .OrderBy(x => _random.Next())
                .Take(1));


            if (clips.Count == 0)
                clips.AddRange(Clips
                    .Where(x => x.ClipType == "BasePhrase" && !x.IsSecondary)
                    .OrderBy(x => _random.Next())
                    .Take(1));

            var initialClip = clips[0];
            var minDuration = initialClip.Phrase.Elements.Min(x => x.Duration);

            if (secondaryCount > 0)
            {
                clips.AddRange(Clips.Where(x => x.ClipType == "BasePhrase" && !x.IsSecondary)
                    .Where(x => x != initialClip)
                    .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                    .Where(x => x.Phrase.Elements.Min(y => y.Duration) == minDuration)
                    .OrderBy(x => Math.Abs(x.AvgNotePitch - initialClip.AvgNotePitch))
                    .ThenBy(x => Math.Abs(x.AvgNoteDuration - initialClip.AvgNoteDuration))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenSnares - initialClip.AvgDistanceBetweenSnares))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenKicks - initialClip.AvgDistanceBetweenKicks))
                    .Take(10)
                    .OrderBy(x => _random.Next())
                    .Take(count - 1)
                    .ToList());

                clips.AddRange(Clips.Where(x => x.ClipType == "BasePhrase" && x.IsSecondary)
                    .Where(x => x != initialClip)
                    .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                    .Where(x => x.Phrase.Elements.Min(y => y.Duration) == minDuration)
                    .OrderBy(x => Math.Abs(x.AvgNotePitch - initialClip.AvgNotePitch))
                    .ThenBy(x => Math.Abs(x.AvgNoteDuration - initialClip.AvgNoteDuration))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenSnares - initialClip.AvgDistanceBetweenSnares))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenKicks - initialClip.AvgDistanceBetweenKicks))
                    .Take(10)
                    .OrderBy(x => _random.Next())
                    .Take(secondaryCount)
                    .ToList());
            }
            else
            {
                clips.AddRange(Clips.Where(x => x.ClipType == "BasePhrase")
                    .Where(x => x != initialClip)
                    .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                    .Where(x => x.Phrase.Elements.Min(y => y.Duration) == minDuration)
                    .OrderBy(x => Math.Abs(x.AvgNotePitch - initialClip.AvgNotePitch))
                    .ThenBy(x => Math.Abs(x.AvgNoteDuration - initialClip.AvgNoteDuration))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenSnares - initialClip.AvgDistanceBetweenSnares))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenKicks - initialClip.AvgDistanceBetweenKicks))
                    .Take(10)
                    .OrderBy(x => _random.Next())
                    .Take(count - 1)
                    .ToList());
            }



            var missing = count - clips.Count;
            if (missing > 0)
                clips.AddRange(Clips.Where(x => x.ClipType == "BasePhrase")
                    .Where(x => x != initialClip)
                    .Where(x => string.IsNullOrEmpty(filter.ArtistFilter) || x.Artist == filter.ArtistFilter)
                    .Where(x => x.Phrase.Elements.Min(y => y.Duration) > minDuration)
                    .OrderBy(x => Math.Abs(x.AvgNotePitch - initialClip.AvgNotePitch))
                    .ThenBy(x => Math.Abs(x.AvgNoteDuration - initialClip.AvgNoteDuration))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenSnares - initialClip.AvgDistanceBetweenSnares))
                    .ThenBy(x => Math.Abs(x.AvgDistanceBetweenKicks - initialClip.AvgDistanceBetweenKicks))
                    .Take(10)
                    .OrderBy(x => _random.Next())
                    .Take(missing)
                    .ToList());
            return clips;
        }

        private static Phrase GeneratePhraseFromBasePhrase(MergedPhrase mergedPhrase, IEnumerable<Clip> sourceBaseClips,
            IReadOnlyCollection<Clip> sourceInstrumentClips)
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
                NoteHelper.ShiftNotesDirect(instrumentPhrases.Last(), instrumentClip.BaseIntervalDiff * -1,
                    Interval.Step);
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
                var drumClip = DrumClips().FirstOrDefault(x => x.Section == section);

                var primaryClip = clips.Where(x => GetGeneratorSettingsByClip(x) != null
                                                   && GetGeneratorSettingsByClip(x).IsPrimaryRiff)
                    .OrderBy(x => PhraseHelper.GetAverageNote(x.Phrase))
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

                    var diff = (int)RoundToNearestMultiple(
                        PhraseHelper.GetAverageNote(channelClip.Phrase) -
                        PhraseHelper.GetAverageNote(primaryClip.Phrase), 12);
                    channelClip.BaseIntervalDiff = diff;

                    var shiftedPhrase = NoteHelper.ShiftNotes(channelClip.Phrase, diff * -1, Interval.Step,
                        diff < 0 ? Direction.Up : Direction.Down);
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
                    IsSecondary = primaryClip.IsSecondary,
                    AvgNotePitch = PhraseHelper.GetAverageNote(basePhrase),
                    AvgNoteDuration = basePhrase.Elements.Average(x => x.Duration),
                    AvgDistanceBetweenSnares = drumClip?.AvgDistanceBetweenSnares ?? 0,
                    AvgDistanceBetweenKicks = drumClip?.AvgDistanceBetweenKicks ?? 0
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
            var mergedPhrase = new MergedPhrase
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
                foreach (var element in phrase.Elements) element.RepeatDuration = 0;
                while (phrase.PhraseLength < length) PhraseHelper.DuplicatePhrase(phrase);
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
                    .Select(x => new PhraseElement { Duration = x.Duration, Note = x.Note, Position = position })
                    .FirstOrDefault();

                if (newElement == null)
                    throw new ApplicationException("no new element");

                var sourceElement = distinctElements
                    .FirstOrDefault(x => x.Note == newElement.Note && x.Duration == newElement.Duration);

                if (sourceElement == null)
                    throw new ApplicationException("no source element");

                foreach (var phrase in sourcePhrases)
                    if (phrase.Elements.Contains(sourceElement))
                        mergedPhrase.SourceIndexes.Add(
                            new Tuple<string, decimal, decimal>(phrase.Description, position, sourceElement.Note));

                newElement.Duration = 0.1M;
                newPhrase.Elements.Add(newElement);
            }

            if (newPhrase.Elements[0] != null && newPhrase.Elements[0].Position != 0)
                newPhrase.Elements[0].Position = 0;

            PhraseHelper.UpdateDurationsFromPositions(newPhrase, newPhrase.PhraseLength);

            mergedPhrase.Phrase = newPhrase;

            return mergedPhrase;
        }

        private static decimal RoundToNearestMultiple(decimal value, decimal factor)
        {
            return Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
        }

        private void MergeRepeatedNotes()
        {
            foreach (var clip in InstrumentClips()) PhraseHelper.MergeRepeatedNotes(clip.Phrase);
        }

        private void MergeChords()
        {
            foreach (var clip in InstrumentClips()) PhraseHelper.MergeChords(clip.Phrase);
        }

        private void CalculateLengths()
        {
            var invalidClips = Clips.Where(x => !ValidLength(x.Phrase.PhraseLength)).ToList();
            foreach (var clip in invalidClips)
                foreach (var validLength in new List<int> { 2, 4, 8, 16, 32, 64, 128, 256 })
                {
                    var diff = validLength - clip.Phrase.PhraseLength;
                    if (diff > 0 && diff / validLength <= .25M) clip.Phrase.PhraseLength = validLength;
                }


            invalidClips = Clips.Where(x => !ValidLength(x.Phrase.PhraseLength)).ToList();
            foreach (var clip in invalidClips)
                Console.WriteLine("Invalid Length:" + clip.Name + " " + clip.Phrase.PhraseLength);

            var validClips = Clips.Where(x => ValidLength(x.Phrase.PhraseLength)).ToList();
            foreach (var clip in validClips)
                while (PhraseHelper.IsPhraseDuplicated(clip.Phrase))
                {
                    var halfLength = clip.Phrase.Elements.Count / 2;
                    clip.Phrase.Elements.RemoveAll(x => clip.Phrase.Elements.IndexOf(x) >= halfLength);
                    clip.Phrase.PhraseLength /= 2M;
                }

            Clips.RemoveAll(x => !ValidLength(x.Phrase.PhraseLength));


            invalidClips = Clips.Where(x => x.Phrase.Elements.Any(y => y.Note < -24)).ToList();
            foreach (var clip in invalidClips) Console.WriteLine("Invalid Notes:" + clip.Name);

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
                ScaleHelper.MashNotesToScaleDirect(clip.Phrase, clip.Scale);
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

        private static ScaleHelper.ScaleMatch CalculateScale(Section section, string preferredScale)
        {
            var newPhrase = new Phrase();
            newPhrase = section.Phrases.Where(x => !x.IsDrums)
                .Aggregate(newPhrase, PhraseHelper.Join);

            var mostCommonNote = NoteHelper.NumberToNoteOnly(PhraseHelper.GetMostCommonNote(newPhrase));

            var scales = ScaleHelper.FindMatchingScales(newPhrase);
            var scaleMatch = scales
                .OrderBy(x => x.DistanceFromScale)
                .ThenByDescending(x => string.IsNullOrEmpty(preferredScale) || x.Scale.Name == preferredScale)
                .ThenByDescending(x => x.Scale.Name.StartsWith(mostCommonNote) ? 1 : 0)
                .ThenByDescending(x => x.Scale.Name.EndsWith("Minor") ? 1 : 0)
                .ToList();

            return scaleMatch.FirstOrDefault();
        }


        private void CalculateScales()
        {
            var sectionNames = InstrumentClips().Select(x => x.Section).Distinct().ToList();

            foreach (var sectionName in sectionNames)
            {
                var section = GetSectionFromClips(sectionName);
                var scale = CalculateScale(section, _generatorSettings.Scale);
                if (scale == null)
                    throw new ApplicationException("no scale");

                var clips = Clips.Where(x => x.Section == sectionName).ToList();
                foreach (var clip in clips) clip.Scale = scale.Scale.Name;
            }
        }

        private List<Clip> LoadMidi()
        {
            var clips = LoadMidiInFolder(GetLibraryFolder());

            if (!string.IsNullOrEmpty(_generatorSettings.SecondaryLibraryFolder))
                clips.AddRange(LoadMidiInFolder(GetSecondaryLibraryFolder(), true,
                    _generatorSettings.SecondaryLibraryLengthMultiplier));

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


            foreach (var clip in clips.Where(x => GetGeneratorSettingsByClip(x).IsDrums)) clip.Phrase.IsDrums = true;

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

                    if (!clips.Exists(x =>
                        x.Song == clip.Song && x.Section == clip.Section && x.Artist == clip.Artist &&
                        x.ClipType == clip.ClipType))
                        clips.Add(clip);
                }
            }

            if (lengthModifier == 0) return clips;

            foreach (var clip in clips)
            {
                PhraseHelper.ChangeLength(clip.Phrase, lengthModifier);
                if (clip.Phrase.PhraseLength > 256)
                {
                    PhraseHelper.TrimPhrase(clip.Phrase, 256);
                }
            }
               

            return clips;
        }

        private string GetArtistNameFromFilename(string filename)
        {
            filename = GetSectionNameFromFilename(filename);
            filename = filename.Split('-')[0];

            return filename.ToUpper();
        }

        private string RemoveFileEnding(string filename)
        {
            foreach (var channel in _generatorSettings.Channels)
                if (!string.IsNullOrEmpty(channel.FileEnding) && filename.EndsWith(channel.FileEnding + ".mid"))
                    filename = filename.Replace(channel.FileEnding + ".mid", "");

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

            if(filename.Contains("-"))
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
                var duration = drumClip.Phrase.PhraseLength;

                var kickCount =
                    drumClip.Phrase.Elements.Count(x => DrumHelper.IsBassDrum(x.Note) && x.Position < duration);
                drumClip.AvgDistanceBetweenKicks = kickCount == 0 ? 0 : duration / kickCount;

                var snareCount =
                    drumClip.Phrase.Elements.Count(x => DrumHelper.IsSnareDrum(x.Note) && x.Position < duration);
                drumClip.AvgDistanceBetweenSnares = snareCount == 0 ? 0 : duration / snareCount;
            }
        }

        private string GetClipTypeByFilename(string filename)
        {
            foreach (var channel in _generatorSettings.Channels)
                if (filename.EndsWith(channel.FileEnding + ".mid"))
                    return channel.Name;

            throw new ApplicationException("Unknown midi extension");
        }

        public void ExportDrums(string folder)
        {
            foreach (var clip in DrumClips().Where(x => !x.IsSecondary))
            {
                var path = Path.Combine(folder, clip.Section + " Drums.mid");
                var section = new Section(clip.Section + " Drums");
                var phrase = clip.Phrase.Clone();
                phrase.IsDrums = true;
                phrase.Bpm = _generatorSettings.Bpm;

                section.Phrases.Add(phrase);

                MidiHelper.SaveToMidi(section, path);
            }
        }

        public void ExportBass(string folder)
        {
            var sections = Clips.Where(x => !x.IsSecondary).Select(x => x.Section).Distinct().ToList();

            foreach (var sectionName in sections)
            {
                var section = GetSectionFromClips(sectionName);

                if (!ApplyStrategiesToSection(section)) continue;

                var bassPhrase = section.Phrases.FirstOrDefault(x => x.Instrument == MidiInstrument.ElectricBassFinger);


                var newSection = new Section(section.Description);
                var phrase = bassPhrase.Clone();

                NoteHelper.ShiftNotesDirect(phrase, 1, Interval.Octave);
                VelocityHelper.ApplyVelocityStrategy(phrase, "Shreddage");

                phrase.Description = sectionName;
                phrase.Bpm = _generatorSettings.Bpm;

                newSection.Phrases.Add(phrase);


                var path = Path.Combine(folder, sectionName + ".mid");
                MidiHelper.SaveToMidi(newSection, path);
            }
        }

        public void ExportSections(string folder)
        {
            var sections = Clips.Where(x => !x.IsSecondary).Select(x => x.Section).Distinct().ToList();

            foreach (var sectionName in sections)
            {
                var section = GetSectionFromClips(sectionName);

                if (!ApplyStrategiesToSection(section)) continue;

                var path = Path.Combine(folder, sectionName + ".mid");
                MidiHelper.SaveToMidi(section, path);
            }
        }

        public void ApplyStrategiesToMidiFiles(List<string> midiFiles)
        {
            foreach (var midiFile in midiFiles)
            {
                var section = MidiHelper.ReadMidi(midiFile);

                if (!ApplyStrategiesToSection(section)) continue;

                MidiHelper.SaveToMidi(section, midiFile);
            }
        }

        private bool ApplyStrategiesToSection(Section section)
        {
            if (section.Phrases.Count != _generatorSettings.Channels.Count)
            {
                Console.WriteLine(section.Description + " -  Phrase count does not equal channel count");
                return false;
            }

            foreach (var channel in _generatorSettings.Channels)
            {
                var channelPhrase = section.Phrases[_generatorSettings.Channels.IndexOf(channel)];
                if (!channelPhrase.IsDrums)
                {
                    foreach (var element in channelPhrase.Elements.Where(x => x.Note < -24)) element.Note += 12;
                    PhraseHelper.MergeChords(channelPhrase);
                    ProcessChords(channel, channelPhrase);
                    VelocityHelper.ApplyVelocityStrategy(channelPhrase, channel.VelocityStrategy);
                }

                channelPhrase.IsDrums = channel.IsDrums;
                channelPhrase.Description = channel.Name;
                channelPhrase.Instrument = channel.Instrument;
                channelPhrase.Bpm = _generatorSettings.Bpm;
                channelPhrase.Panning = channel.Panning;
            }

            var scale = CalculateScale(section, _generatorSettings.Scale);
            if (scale.Scale.Name != _generatorSettings.Scale)
            {
                ScaleHelper.MashNotesToScale(section, _generatorSettings.Scale);
                scale = CalculateScale(section, _generatorSettings.Scale);
                if (scale.Scale.Name != _generatorSettings.Scale) throw new ApplicationException("does not scale");
            }

            PhraseHelper.EnsureLengthsAreEqual(section.Phrases);

            return true;
        }

        private Section GetSectionFromClips(string sectionName)
        {
            var section = new Section(sectionName);
            foreach (var channel in _generatorSettings.Channels)
            {
                Phrase channelPhrase;
                if (channel.IsDrums)
                    channelPhrase = DrumClips().FirstOrDefault(x => x.Section == sectionName)?.Phrase.Clone();
                else
                    channelPhrase = Clips
                        .FirstOrDefault(x => x.ClipType == channel.Name && x.Section == sectionName)?.Phrase.Clone();
                section.Phrases.Add(channelPhrase ?? new Phrase());
            }

            return section;
        }

        [Serializable]
        private class Clip
        {
            public string Name { get; set; }
            public string Song { get; set; }
            public string Artist { get; set; }
            public string Section { get; set; }
            public Phrase Phrase { get; set; }
            public string Scale { get; set; }
            public int BaseIntervalDiff { get; set; }

            public string ClipType { get; set; }
            public decimal AvgDistanceBetweenKicks { get; set; }
            public decimal AvgDistanceBetweenSnares { get; set; }
            public string Filename { get; internal set; }

            public bool IsSecondary { get; set; }
            public decimal AvgNotePitch { get; set; }
            public decimal AvgNoteDuration { get; set; }
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
            public decimal AvgDistanceBetweenKicks { get; set; }

            public decimal AvgDistanceBetweenSnares { get; set; }
        }

        public class GeneratorSettings
        {
            /// <summary>
            /// the number of source files to merge together into a new one
            /// </summary>
            public int SourceCount { get; set; }

            public int RandomSourceCount { get; set; }

            public decimal Bpm { get; set; }

            public string Scale { get; set; }

            public List<Channel> Channels { get; set; }
            public string LibraryFolder { get; set; }

            public string SecondaryLibraryFolder { get; set; }

            public decimal SecondaryLibraryLengthMultiplier { get; set; }

            public bool SecondaryLibraryIncludeDrums { get; set; }

            public int SecondaryLibrarySourceCount { get; set; }

            public class Channel
            {
                public string Name { get; set; }

                public int MidiChannel { get; set; }

                public MidiInstrument Instrument { get; set; }

                public string FileEnding { get; set; }

                public int DefaultStepDifference { get; set; }

                public bool IsDrums => MidiChannel == 10;

                public bool IsPrimaryRiff { get; set; }

                public string VelocityStrategy { get; set; }

                public int MaximumNotesInChord { get; set; }

                public decimal ConvertChordsToNotesCoverage { get; set; }

                public decimal Panning { get; set; }
            }
        }
    }
}