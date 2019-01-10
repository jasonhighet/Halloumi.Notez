using Halloumi.Notez.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            const int riffLength = 32;

            var riffs = Directory.GetFiles("TestMidi", "*.mid")
                .Select(MidiHelper.ReadMidi)
                .Where(riff => NoteHelper.GetTotalDuration(riff) == riffLength)
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

            var probabilities = allNotes
                .Select(x => x.Position)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new
                {
                    Position = x,
                    OnOffChance = allNotes.Where(y => y.Position == x).Sum(y => y.Count) / Convert.ToDouble(riffs.Count),
                    Notes = allNotes.Where(y => y.Position == x).ToList()
                })
                .ToList();


            var minNotes = riffs.Select(x => x.Elements.Count).Min();
            var maxNotes = riffs.Select(x => x.Elements.Count).Max();

            var random = new Random();

            const int riffCount = 32;
            for (var i = 0; i < riffCount; i++)
            {
                var phrase = new Phrase();

                var noteCount = GetNumberOfNotes(random, minNotes, maxNotes);

                var selectedNotes =
                (from onoffProbability in probabilities
                 let noteOn = GetRandomBool(random, onoffProbability.OnOffChance)
                 where noteOn
                 select onoffProbability).ToList();

                while (selectedNotes.Count > noteCount)
                {
                    var leastPopularNote = selectedNotes.OrderBy(x => x.OnOffChance).FirstOrDefault();
                    selectedNotes.Remove(leastPopularNote);
                }
                while (selectedNotes.Count < noteCount)
                {
                    var mostPopularNote = probabilities.Except(selectedNotes).OrderByDescending(x => x.OnOffChance).FirstOrDefault();
                    selectedNotes.Add(mostPopularNote);
                }

                selectedNotes = selectedNotes.OrderBy(x => x.Position).ToList();

                foreach (var note in selectedNotes)
                {
                    var noteNumbers = note.Notes.ToDictionary(x => x.Note , x => x.Count);
                    var randomNote = GetRandomNote(random, noteNumbers);
                    
                    phrase.Elements.Add(new PhraseElement
                    {
                        Position = note.Position,
                        Duration = 1,
                        Note = randomNote
                    });
                }

                PhraseHelper.UpdateDurationsFromPositions(phrase, riffLength);

                MidiHelper.SaveToMidi(phrase, "Riff" + i + ".mid");
            }



            Console.WriteLine();




           // Console.ReadLine();
        }

        private static int GetRandomNote(Random random, Dictionary<int, int> noteNumbers)
        {
            var numbers = new List<int>();
            foreach (var noteNumber in noteNumbers)
            {
                for (var i = 0; i < noteNumber.Value; i++)
                {
                    numbers.Add(noteNumber.Key);
                }
            }

            var randomIndex = random.Next(0, numbers.Count);

            return numbers[randomIndex];
        }

        private static int GetNumberOfNotes(Random random, int minNotes, int maxNotes)
        {
            var range = maxNotes - minNotes;
            //return minNotes + Convert.ToInt32(Math.Round(range * GetBellCurvedRandom(random)));
            return minNotes + Convert.ToInt32(Math.Round(range * random.NextDouble()));
        }

        private static bool GetRandomBool(Random random, double chanceOfTrue)
        {
            var randomNumber = random.NextDouble();
            return randomNumber <= chanceOfTrue;
        }

        private static double GetBellCurvedRandom(Random random)
        {
            return (Math.Pow(2 * random.NextDouble() - 1, 3) / 2) + .5;
        }

        //public class NoteProbability
        //{
        //    public decimal Position { get; set; }

        //    public double Chance { get; set; }
        //}
    }
}
