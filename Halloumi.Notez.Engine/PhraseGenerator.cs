﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Halloumi.Notez.Engine
{
    public class PhraseGenerator
    {
        private int _minNotes;
        private int _maxNotes;
        private readonly Random _random;
        private List<NoteProbability> _probabilities;
        private const int RiffLength = 32;

        private double _chanceOfTimingRepeat;
        private double _chanceOfPerfectRepeat;
        private int _minTimingRepeats;
        private int _maxTimingRepeats;
        private int _minPerfectRepeats;
        private int _maxPerfectRepeats;
        private List<RepeatingElementsFinder.WindowMatch> _timingRepeats;
        private List<RepeatingElementsFinder.WindowMatch> _perfectRepeats;


        public PhraseGenerator()
        {
            _random = new Random(DateTime.Now.Millisecond);
            LoadTrainingData();
        }

        private void LoadTrainingData()
        {
            var riffs = Directory.GetFiles("TestMidi", "*.mid")
                .Select(MidiHelper.ReadMidi)
                .Where(riff => riff.PhraseLength == RiffLength)
                .Where(riff => ScaleHelper.FindMatchingScales(riff).Select(x => x.Scale.Name).Contains("C Natural Minor"))
                .ToList();

            var allNotes = riffs.SelectMany(x => x.Elements).GroupBy(x => new
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

            _probabilities = allNotes
                .Select(x => x.Position)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new NoteProbability()
                {
                    Position = x,
                    OnOffChance = allNotes.Where(y => y.Position == x).Sum(y => y.Count) / Convert.ToDouble(riffs.Count),
                    Notes = allNotes.Where(y => y.Position == x).ToDictionary(y => y.Note, y => y.Count)
                })
                .ToList();


            _minNotes = riffs.Select(x => x.Elements.Count).Min();
            _maxNotes = riffs.Select(x => x.Elements.Count).Max();


            var repeats = riffs.Select(RepeatingElementsFinder.FindRepeatingElements).ToList();

            _chanceOfTimingRepeat = repeats.Count(x => x.Any(y => y.MatchType == RepeatingElementsFinder.MatchResult.TimingMatch)) / Convert.ToDouble(repeats.Count);
            _chanceOfPerfectRepeat = repeats.Count(x => x.Any(y => y.MatchType == RepeatingElementsFinder.MatchResult.PerfectMatch)) / Convert.ToDouble(repeats.Count);

            _maxTimingRepeats = repeats
                .Select(x => x.Count(y => y.MatchType == RepeatingElementsFinder.MatchResult.TimingMatch))
                .Max(x => x);

            _minTimingRepeats = repeats
                .Select(x => x.Count(y => y.MatchType == RepeatingElementsFinder.MatchResult.TimingMatch))
                .Where(x => x != 0)
                .Min(x => x);

            _maxPerfectRepeats = repeats
                .Select(x => x.Count(y => y.MatchType == RepeatingElementsFinder.MatchResult.PerfectMatch))
                .Max(x => x);

            _minPerfectRepeats = repeats
                .Select(x => x.Count(y => y.MatchType == RepeatingElementsFinder.MatchResult.PerfectMatch))
                .Where(x => x != 0)
                .Min(x => x);

            _timingRepeats = repeats.SelectMany(x => x).Where(x => x.MatchType == RepeatingElementsFinder.MatchResult.TimingMatch).ToList();
            _perfectRepeats = repeats.SelectMany(x => x).Where(x => x.MatchType == RepeatingElementsFinder.MatchResult.PerfectMatch).ToList();
        }


        public Phrase GeneratePhrase()
        {
            var phrase = new Phrase();

            var noteCount = GetNumberOfNotes();

            var selectedNotes =
            (from onoffProbability in _probabilities
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
                var mostPopularNote = _probabilities.Except(selectedNotes).OrderByDescending(x => x.OnOffChance).FirstOrDefault();
                selectedNotes.Add(mostPopularNote);
            }

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

            PhraseHelper.UpdateDurationsFromPositions(phrase, RiffLength);

            var perfectRepeats = GetPerfectRepeats();
            var timingRepeats = GetTimingRepeats(perfectRepeats);

            return phrase;
        }

        private List<RepeatingElementsFinder.WindowMatch> GetPerfectRepeats()
        {
            var repeats = new List<RepeatingElementsFinder.WindowMatch>();
            if (!GetRandomBool(_chanceOfPerfectRepeat))
                return repeats;

            var repeatCount = _random.Next(_minPerfectRepeats, _maxPerfectRepeats + 1);

            for (var i = 0; i < repeatCount; i++)
            {
                var availableRepeats = _perfectRepeats.Except(repeats).ToList();
                foreach (var repeat in repeats)
                {
                    availableRepeats.RemoveAll(x => AreRegionsOverlapping(repeat.MatchWindowStart, repeat.WindowSize, x.MatchWindowStart, x.WindowSize));
                }


                if (availableRepeats.Count == 0)
                    break;

                var index = _random.Next(0, availableRepeats.Count);

                repeats.Add(availableRepeats[index]);
            }

            repeats = repeats.OrderBy(x => x.WindowStart).ThenBy(x => x.WindowSize).ToList();

            return repeats;
        }

        private IEnumerable<RepeatingElementsFinder.WindowMatch> GetTimingRepeats(IReadOnlyCollection<RepeatingElementsFinder.WindowMatch> perfectRepeats)
        {
            var repeats = new List<RepeatingElementsFinder.WindowMatch>();
            if (!GetRandomBool(_chanceOfTimingRepeat))
                return repeats;

            var repeatCount = _random.Next(_minTimingRepeats, _maxTimingRepeats + 1);

            for (var i = 0; i < repeatCount; i++)
            {
                var availableRepeats = _timingRepeats.Except(repeats).ToList();
                foreach (var repeat in repeats)
                {
                    availableRepeats.RemoveAll(x => AreRegionsOverlapping(repeat.MatchWindowStart, repeat.WindowSize, x.MatchWindowStart, x.WindowSize));
                }
                foreach (var repeat in perfectRepeats)
                {
                    availableRepeats.RemoveAll(x => AreRegionsOverlapping(repeat.MatchWindowStart, repeat.WindowSize, x.MatchWindowStart, x.WindowSize));
                }


                if (availableRepeats.Count == 0)
                    break;

                var index = _random.Next(0, availableRepeats.Count);

                repeats.Add(availableRepeats[index]);
            }

            repeats = repeats.OrderBy(x => x.WindowStart).ThenBy(x => x.WindowSize).ToList();

            return repeats;
        }

        public static bool AreRegionsOverlapping(int start, int length, int compareStart, int compareLength)
        {
            var end = start + length - 1;
            var compareEnd = compareStart + compareLength - 1;
            return (start >= compareStart && start <= compareEnd) || (end >= compareStart && end <= compareEnd);
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

        private int GetNumberOfNotes()
        {
            return _random.Next(_minNotes, _maxNotes + 1);
            //return GetBellCurvedRandom(_minNotes, _maxNotes);
        }

        private bool GetRandomBool(double chanceOfTrue)
        {
            var randomNumber = _random.NextDouble();
            return randomNumber <= chanceOfTrue;
        }

        //private double GetBellCurvedRandom()
        //{
        //    return (Math.Pow(2 * _random.NextDouble() - 1, 3) / 2) + .5;
        //}

        //private int GetBellCurvedRandom(int min, int maxInclusive)
        //{
        //    var range = maxInclusive - min;
        //    return min + Convert.ToInt32(Math.Round(range * GetBellCurvedRandom()));
        //}

        public class NoteProbability
        {
            public decimal Position { get; set; }

            public double OnOffChance { get; set; }

            public Dictionary<int, int> Notes { get; set; }
        }
    }
}
