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

        public void LoadLibrary(string folder)
        {
            LoadClips(folder);
            CalculateScales();
            MashToScale();
            MergeChords();
            MergeRepeatedNotes();
            CalculateLengths();
            CalculateBasePhrases();

            

            for (var i = 0; i < 31; i++)
            {
                GenerateRiff("riff" + i);
                //GenerateRandomRiff("riff" + i);
            }
        }

        private void GenerateRandomRiff(string filename)
        {
            var sourceClips = LoadSourceBasePhraseClips(2);
            var sourcePhrases = sourceClips.Select(x => x.Phrase).ToList();

            EnsureLengthsAreEqual(sourcePhrases);
            foreach (var sourcePhrase in sourcePhrases)
            {
                PhraseHelper.UnmergeRepeatedNotes(sourcePhrase);
                PhraseHelper.MergeChords(sourcePhrase);
            }

            //var patterns = PatternFinder.FindPatterns(sourcePhrases);
            //Console.WriteLine(patterns.Count);

            var probabilities = GenerateProbabilities(sourcePhrases);
            decimal phraseLength = sourcePhrases[0].PhraseLength;

            var newPhrase = GenratePhraseBasic(probabilities, phraseLength);
            var patterns = PatternFinder.FindPatterns(sourcePhrases.OrderBy(x => _random.Next()).FirstOrDefault())
                .OrderByDescending(x => x.Value.PatternType)
                .ThenBy(x => x.Value.WindowSize)
                .ThenBy(x => x.Value.ToList().FirstOrDefault().Value.Start)
                .ToList();

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
                var firstWindow = pattern.Value.OrderBy(x=> _random.Next()).FirstOrDefault().Value;
                foreach (var window in pattern.Value.Select(x => x.Value))
                {
                    if (window == firstWindow)
                        continue;

                    newPhrase.Elements.RemoveAll(x => x.Position >= window.Start && x.Position <= window.End);

                    var repeatingElements = newPhrase.Elements.Where(x => x.Position >= firstWindow.Start && x.Position <= firstWindow.End)
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
            newPhrase.Bpm = 200M;



            //var chain = GenerateChain(sourcePhrases);

            //var nextPosition = 0M;
            //PhraseElement previousElement = null;
            //var newPhrase = new Phrase() { Bpm = sourcePhrases[0].Bpm, PhraseLength = sourcePhrases[0].PhraseLength };
            //foreach (var position in positions)
            //{
            //    if (position < nextPosition)
            //        continue;

            //    var element = new PhraseElement() {Position = position};

            //    var existingElement = sourcePhrases
            //                            .Select(x => x.Elements.Where(y => y.Position <= position).OrderByDescending(y => y.Position).FirstOrDefault())
            //                            .Where(x => x != null)
            //                            .OrderBy(x => _random.Next())
            //                            .FirstOrDefault();

            //    if (previousElement == null)
            //    {
            //        element.Note = existingElement.Note;
            //        element.Duration = existingElement.Duration;
            //    }
            //    else
            //    {
            //        element.Note = chain.Item1.ContainsKey(previousElement.Note)
            //            ? chain.Item1[previousElement.Note].OrderBy(x => _random.Next()).FirstOrDefault()
            //            : existingElement.Note;

            //        element.Duration = chain.Item2.ContainsKey(previousElement.Duration)
            //            ? chain.Item2[previousElement.Duration].OrderBy(x => _random.Next()).FirstOrDefault()
            //            : existingElement.Duration;
            //    }

            //    newPhrase.Elements.Add(element);

            //    // if position < next postition
            //    //  continue
            //    // if position filled
            //    //  set next position
            //    //  contine

            //    // calculate note
            //    // calculate length
            //    // calculate ischord
            //    // calculate repeating

            //    // set next position
            //    // apply patterns

            //    nextPosition = position + element.Duration;
            //    previousElement = element;
            //}

            // find bass phrase (random of source), apply to new phrase
            // find alt phrase (random of source), apply to new phrase
            // find main phrase (random of source), apply to new phrase

            var bassPhrase = newPhrase.Clone();
            var mainGuitarPhrase = newPhrase.Clone();
            var altGuitarPhrase = newPhrase.Clone();

            NoteHelper.ShiftNotes(bassPhrase, -12, Interval.Step);
            NoteHelper.ShiftNotes(altGuitarPhrase, 12, Interval.Step);

            SaveToMidiFile(filename, bassPhrase, mainGuitarPhrase, altGuitarPhrase);
        }

        private Phrase GenratePhraseBasic(PhraseProbabilities probabilities, decimal phraseLength)
        {
            int noteCount = _random.Next(probabilities.MinNotes, probabilities.MaxNotes + 1);
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


        private static Tuple<Dictionary<int, List<int>>, Dictionary<decimal, List<decimal>>> GenerateChain(IEnumerable<Phrase> sourcePhrases)
        {
            var notes = new Dictionary<int, List<int>>();
            var durations = new Dictionary<decimal, List<decimal>>();

            foreach (var sourcePhrase in sourcePhrases)
            {
                foreach (var element in sourcePhrase.Elements)
                {
                    var index = sourcePhrase.Elements.IndexOf(element);
                    var nextElement = sourcePhrase
                        .Elements
                        .FirstOrDefault(x => sourcePhrase.Elements.IndexOf(x) == index + 1);

                    if(nextElement == null)
                        continue;
                    
                    if(!notes.ContainsKey(element.Note))
                        notes.Add(element.Note, new List<int>());
                    notes[element.Note].Add(nextElement.Note);

                    if (!durations.ContainsKey(element.Duration))
                        durations.Add(element.Duration, new List<decimal>());
                    durations[element.Duration].Add(nextElement.Duration);
                }
            }

            return new Tuple<Dictionary<int, List<int>>, Dictionary<decimal, List<decimal>>>(notes, durations);
        }

        private PhraseProbabilities GenerateProbabilities(IReadOnlyCollection<Phrase> sourcePhrases)
        {
            var probabilities = new PhraseProbabilities();

            var allNotes = sourcePhrases.SelectMany(x => x.Elements).GroupBy(x => new
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
                    OnOffChance = allNotes.Where(y => y.Position == x).Sum(y => y.Count) / Convert.ToDouble(sourcePhrases.Count),
                    Notes = allNotes.Where(y => y.Position == x).ToDictionary(y => y.Note, y => y.Count)
                })
                .ToList();


            probabilities.MinNotes = sourcePhrases.Select(x => x.Elements.Count).Min();
            probabilities.MaxNotes = sourcePhrases.Select(x => x.Elements.Count).Max();
            return probabilities;

        }

        private void GenerateRiff(string filename)
        {
            var sourceClips = LoadSourceBasePhraseClips(5);

            var mergedPhrase = MergePhrases(sourceClips.Select(x => x.Phrase).ToList());
            mergedPhrase.Phrase.Bpm = 200;

            var bassPhrase = GeneratePhraseFromBasePhrase(mergedPhrase, sourceClips, ClipType.BassGuitar);
            var mainGuitarPhrase = GeneratePhraseFromBasePhrase(mergedPhrase, sourceClips, ClipType.MainGuitar);
            var altGuitarPhrase = GeneratePhraseFromBasePhrase(mergedPhrase, sourceClips, ClipType.AltGuitar);

            SaveToMidiFile(filename, bassPhrase, mainGuitarPhrase, altGuitarPhrase);
        }

        private static void SaveToMidiFile(string filename, Phrase bassPhrase, Phrase mainGuitarPhrase, Phrase altGuitarPhrase)
        {
            bassPhrase.Instrument = MidiInstrument.ElectricBassFinger;
            bassPhrase.Description = "BassGuitar";
            mainGuitarPhrase.Instrument = MidiInstrument.DistortedGuitar;
            mainGuitarPhrase.Description = "MainGuitar";
            altGuitarPhrase.Instrument = MidiInstrument.OverdrivenGuitar;
            altGuitarPhrase.Description = "AltGuitar";

            var phrases = new List<Phrase> { mainGuitarPhrase, altGuitarPhrase, bassPhrase };
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
                .OrderBy(x => _random.Next())
                .Take(count - 1)
                .ToList());

            var missing = count - clips.Count;
            if (missing > 0)
                clips.AddRange(Clips.Where(x => x.ClipType == ClipType.BasePhrase)
                    .Where(x => x != inititialClip)
                    .Where(x => x.Phrase.Elements.Min(y => y.Duration) > minDuration)
                    .OrderBy(x => _random.Next())
                    .Take(missing)
                    .ToList());
            return clips;
        }

        private Phrase GeneratePhraseFromBasePhrase(MergedPhrase mergedPhrase, List<Clip> sourceClips, ClipType clipType)
        {
            var instrumentClips = new List<Clip>();
            var instrumentPhrases = new List<Phrase>();

            foreach (var clip in sourceClips)
            {
                var instrumentClip = Clips.FirstOrDefault(x => x.ClipType == clipType && x.Section == clip.Section);
                if (instrumentClip == null)
                    throw new ApplicationException("No instrument clip");

                instrumentClips.Add(instrumentClip);
                instrumentPhrases.Add(instrumentClip.Phrase.Clone());
                instrumentPhrases.Last().Description = instrumentClip.Section;
                NoteHelper.ShiftNotesDirect(instrumentPhrases.Last(), instrumentClip.BaseIntervalDiff * -1, Interval.Step);
            }


            EnsureLengthsAreEqual(instrumentPhrases, mergedPhrase.Phrase.PhraseLength);

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
                basePhrase.Bpm = 200M;


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

        private static void EnsureLengthsAreEqual(List<Phrase> sourcePhrases, decimal length = 0)
        {
            if (length == 0) length = sourcePhrases.Max(x => x.PhraseLength);
            foreach (var phrase in sourcePhrases)
            {
                while (phrase.PhraseLength < length)
                {
                    PhraseHelper.DuplicatePhrase(phrase);
                }
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
            public PatternFinder.Patterns Patterns { get; internal set; }
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
        }

        public class NoteProbability
        {
            public decimal Position { get; set; }

            public double OnOffChance { get; set; }

            public Dictionary<int, int> Notes { get; set; }
        }
    }
}
