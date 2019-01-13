using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine
{
    public class PhraseGenerator
    {
        private int _minNotes;
        private int _maxNotes;
        private readonly Random _random;
        private List<NoteProbability> _probabilities;
        private const int RiffLength = 32;

        public PhraseGenerator()
        {
            _random = new Random();
            LoadTrainingData();
        }

        private void LoadTrainingData()
        {
            var riffs = Directory.GetFiles("TestMidi", "*.mid")
                .Select(MidiHelper.ReadMidi)
                .Where(riff => NoteHelper.GetTotalDuration(riff) == RiffLength)
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
        }

        private void RemoveNotes(Phrase phrase, int startPosition, int endPosition)
        {
            phrase.Elements.RemoveAll(x => x.Position >= startPosition && x.Position <= endPosition);
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

        private int GetNumberOfNotes()
        {
            var range = _maxNotes - _minNotes;
            return _minNotes + Convert.ToInt32(Math.Round(range * GetBellCurvedRandom()));
        }

        private bool GetRandomBool(double chanceOfTrue)
        {
            var randomNumber = _random.NextDouble();
            return randomNumber <= chanceOfTrue;
        }

        private double GetBellCurvedRandom()
        {
            return (Math.Pow(2 * _random.NextDouble() - 1, 3) / 2) + .5;
        }

        public class NoteProbability
        {
            public decimal Position { get; set; }

            public double OnOffChance { get; set; }

            public Dictionary<int, int> Notes { get; set; }
        }
    }
}
