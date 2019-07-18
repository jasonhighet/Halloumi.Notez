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

        private readonly Random _random = new Random();

        public SourceLibrary(string folder)
        {
            LoadClips(folder);
            CalculateScales();
            MashToScale();
            MergeChords();
            MergeRepeatedNotes();
            CalculateLengths();
            CalculateBasePhrases();
            CalculateDrumAverages();
        }

        public void GenerateRiffs(string name, int count)
        {
            //Parallel.For(0, count, i =>
            //{

            //});

            for (int i = 0; i < count; i++)
            {
                GenerateRiff(name + i);
            }
        }

        public void RunTests()
        {
            foreach (var clip in Clips.OrderBy(x => x.Filename))
            {
                MidiHelper.SaveToMidi(clip.Phrase, "test.mid");
                var phrase = MidiHelper.ReadMidi("test.mid");
                if (phrase.PhraseLength != clip.Phrase.PhraseLength)
                    Console.WriteLine("Error saving " + clip.Filename);

            }
        }

        private List<Clip> GenerateRandomSection(IEnumerable<Clip> sourceBaseClips)
        {
            const decimal maxLength = 64M;

            var sourcePhrases = sourceBaseClips.Select(x => x.Phrase.Clone()).ToList();

            var phraseLength = sourcePhrases[0].PhraseLength;
            if (phraseLength > maxLength)
                phraseLength = maxLength;

            PhraseHelper.EnsureLengthsAreEqual(sourcePhrases);
            var probabilities = GenerateProbabilities(sourcePhrases);
            PhraseHelper.EnsureLengthsAreEqual(sourcePhrases, phraseLength);

            var newPhrase = GenratePhraseBasic(probabilities, phraseLength / 2);
            PhraseHelper.DuplicatePhrase(newPhrase);


            var patterns = PatternFinder.FindPatterns(sourcePhrases.OrderBy(x => _random.Next()).FirstOrDefault())
                .OrderByDescending(x => x.Value.PatternType)
                .ThenBy(x => x.Value.WindowSize)
                .ThenBy(x => x.Value.ToList().FirstOrDefault().Value.Start)
                .ToList();

            newPhrase = ApplyPatterns(patterns, newPhrase, probabilities);
            newPhrase.Bpm = 180;

            // find bass phrase (random of source), apply to new phrase
            // find alt phrase (random of source), apply to new phrase
            // find main phrase (random of source), apply to new phrase

            var bassGuitarPhrase = newPhrase.Clone();
            var mainGuitarPhrase = newPhrase.Clone();
            var altGuitarPhrase = newPhrase.Clone();

            NoteHelper.ShiftNotes(bassGuitarPhrase, -12, Interval.Step);
            NoteHelper.ShiftNotes(altGuitarPhrase, 12, Interval.Step);


            var name = Guid.NewGuid().ToString();
            var section = Guid.NewGuid().ToString();
            var artist = Guid.NewGuid().ToString();

            var baseClip = new Clip()
            {
                Phrase = newPhrase,
                ClipType = ClipType.BasePhrase,
                Section = section,
                Artist = artist,
                Name = name,
            };

            var mainGuitarClip = new Clip()
            {
                Phrase = mainGuitarPhrase,
                ClipType = ClipType.MainGuitar,
                Section = section,
                Artist = artist,
                Name = name,
            };

            var altGuitarClip = new Clip()
            {
                Phrase = altGuitarPhrase,
                ClipType = ClipType.AltGuitar,
                Section = section,
                Artist = artist,
                Name = name,
                BaseIntervalDiff = 12
            };

            var bassGuitarClip = new Clip()
            {
                Phrase = bassGuitarPhrase,
                ClipType = ClipType.BassGuitar,
                Section = section,
                Artist = artist,
                Name = name,
                BaseIntervalDiff = -12
            };


            return new List<Clip>()
            {
                baseClip,
                mainGuitarClip,
                altGuitarClip,
                bassGuitarClip
            };
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
                x.Position,
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
                    Notes = allNotes.Where(y => y.Position == x).ToDictionary(y => y.Note, y => y.Count)
                })
                .ToList();


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

        private void GenerateRiff(string filename)
        {
            var sourceCount = GetBellCurvedRandom(2, 4);
            var sourceBaseClips = LoadSourceBasePhraseClips(sourceCount);

            var drumPhrase = sourceBaseClips
                .OrderByDescending(x => _random.Next())
                .Select(drums => Clips.FirstOrDefault(x => x.ClipType == ClipType.Drums && x.Artist == drums.Artist && x.Song == drums.Song && x.Section == drums.Section))
                .Where(x => x != null)
                .Select(x => x.Phrase.Clone())
                .FirstOrDefault();

            var randomSection = GenerateRandomSection(sourceBaseClips);
            sourceBaseClips.Add(randomSection.FirstOrDefault(x => x.ClipType == ClipType.BasePhrase));

            var mergedPhrase = MergePhrases(sourceBaseClips.Select(x => x.Phrase).ToList());
            mergedPhrase.Phrase.Bpm = 180;

            var bassClips = Clips.Where(x => x.ClipType == ClipType.BassGuitar).ToList();
            bassClips.Add(randomSection.FirstOrDefault(x => x.ClipType == ClipType.BassGuitar));
            var bassPhrase = GeneratePhraseFromBasePhrase(mergedPhrase, sourceBaseClips, bassClips);

            var mainGuitarClips = Clips.Where(x => x.ClipType == ClipType.MainGuitar).ToList();
            mainGuitarClips.Add(randomSection.FirstOrDefault(x => x.ClipType == ClipType.MainGuitar));
            var mainGuitarPhrase = GeneratePhraseFromBasePhrase(mergedPhrase, sourceBaseClips, mainGuitarClips);

            var altGuitarClips = Clips.Where(x => x.ClipType == ClipType.AltGuitar).ToList();
            altGuitarClips.Add(randomSection.FirstOrDefault(x => x.ClipType == ClipType.AltGuitar));
            var altGuitarPhrase = GeneratePhraseFromBasePhrase(mergedPhrase, sourceBaseClips, altGuitarClips);



            SaveToMidiFile(filename, bassPhrase, mainGuitarPhrase, altGuitarPhrase, drumPhrase);
        }

        private static void SaveToMidiFile(string filename, Phrase bassPhrase, Phrase mainGuitarPhrase, Phrase altGuitarPhrase, Phrase drumPhrase)
        {
            bassPhrase.Instrument = MidiInstrument.ElectricBassFinger;
            bassPhrase.Description = "BassGuitar";
            mainGuitarPhrase.Instrument = MidiInstrument.DistortedGuitar;
            mainGuitarPhrase.Description = "MainGuitar";
            altGuitarPhrase.Instrument = MidiInstrument.OverdrivenGuitar;
            altGuitarPhrase.Description = "AltGuitar";

            drumPhrase.IsDrums = true;

            var phrases = new List<Phrase> { mainGuitarPhrase, altGuitarPhrase, bassPhrase, drumPhrase };

            PhraseHelper.EnsureLengthsAreEqual(phrases);

            MidiHelper.SaveToMidi(phrases, filename + ".mid");
        }

        private List<Clip> LoadSourceBasePhraseClips(int count)
        {
            var clips = Clips
                .Where(x => x.ClipType == ClipType.BasePhrase)
                .OrderBy(x => _random.Next())
                .Take(1)
                .ToList();

            var inititialClip = clips[0];
            var minDuration = inititialClip.Phrase.Elements.Min(x => x.Duration);

            clips.AddRange(Clips.Where(x => x.ClipType == ClipType.BasePhrase)
                .Where(x => x != inititialClip)
                .Where(x => x.Phrase.Elements.Min(y => y.Duration) == minDuration)
                .OrderBy(x=> Math.Abs(x.AvgDistanceBetweenSnares - inititialClip.AvgDistanceBetweenSnares))
                .ThenBy(x => Math.Abs(x.AvgDistanceBetweenKicks - inititialClip.AvgDistanceBetweenKicks))
                .Take(10)
                .OrderBy(x => _random.Next())
                .Take(count - 1)
                .ToList());

            var missing = count - clips.Count;
            if (missing > 0)
                clips.AddRange(Clips.Where(x => x.ClipType == ClipType.BasePhrase)
                    .Where(x => x != inititialClip)
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
                var sourceElement = sourcePhrase?.Elements.FirstOrDefault(x => x.Position == sourceIndex.Item3);

                if (sourceElement == null)
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
                SourcePhrases = sourcePhrases,
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

        private IEnumerable<Clip> DrumClips()
        {
            return Clips.Where(x => x.ClipType == ClipType.Drums);
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
                    Name = Path.GetFileNameWithoutExtension(x),
                    Song = Regex.Replace(Path.GetFileNameWithoutExtension(x) + "", @"[\d-]", string.Empty),
                    Section = (Path.GetFileNameWithoutExtension(x) + "").Split(' ')[0],
                    Artist = (Path.GetFileNameWithoutExtension(x) + "").Split('-')[0],
                    Phrase = MidiHelper.ReadMidi(x),
                    ClipType = GetClipType(x),
                    Filename = Path.GetFullPath(x)
                })
                .ToList();

            foreach (var phrase in Clips.Where(x => x.ClipType == ClipType.Drums).Select(x => x.Phrase).ToList())
            {
                phrase.IsDrums = true;
            }
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
            public string Filename { get; internal set; }
            public decimal AvgDistanceBetweenKicks { get; set; }
            public decimal AvgDistanceBetweenSnares { get; set; }
        }

        private class MergedPhrase
        {
            public Phrase Phrase { get; set; }
            public List<Phrase> SourcePhrases { get; set; }
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
    }
}
