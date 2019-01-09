using Halloumi.Notez.Engine;
using System;
using System.IO;
using System.Linq;

namespace Halloumi.Notez.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var riffs = Directory.GetFiles("TestMidi", "*.mid")
                .Select(MidiHelper.ReadMidi)
                .Where(riff => NoteHelper.GetTotalDuration(riff) == 32)
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

            var onoffProbabilities = allNotes
                .Select(x => x.Position)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new
                {
                    Position = x,
                    Chance = allNotes.Where(y => y.Position == x).Sum(y => y.Count) / Convert.ToDouble(riffs.Count)
                })
                .ToList();


            var minNotes = riffs.Select(x => x.Elements.Count).Min();
            var maxNotes = riffs.Select(x => x.Elements.Count).Max();

            var random = new Random();
            var noteCount = GetNumberOfNotes(random, minNotes, maxNotes);


            for (var i = 0; i < 10; i++)
            {
                var selectedNotes =
                (from onoffProbability in onoffProbabilities
                 let noteOn = GetRandomBool(random, onoffProbability.Chance)
                 where noteOn
                 select onoffProbability).ToList();

                while (selectedNotes.Count > noteCount)
                {
                    var leastPopularNote = selectedNotes.OrderBy(x => x.Chance).FirstOrDefault();
                    selectedNotes.Remove(leastPopularNote);
                }
                while (selectedNotes.Count < noteCount)
                {
                    var mostPopularNote = onoffProbabilities.Except(selectedNotes).OrderByDescending(x => x.Chance).FirstOrDefault();
                    selectedNotes.Add(mostPopularNote);
                }

                selectedNotes = selectedNotes.OrderBy(x => x.Position).ToList();

                foreach (var note in selectedNotes)
                {
                    Console.Write(note.Position + " ");
                }

                Console.WriteLine();
            }




            Console.ReadLine();
        }

        private static int GetNumberOfNotes(Random random, int minNotes, int maxNotes)
        {
            var range = maxNotes - minNotes;
            return minNotes + Convert.ToInt32(Math.Round(range * GetBellCurvedRandom(random)));
            //return minNotes + Convert.ToInt32(Math.Round(range * random.NextDouble()));

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
