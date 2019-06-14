using Halloumi.Notez.Engine.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Generator
{
    public static class PatternFinder
    {
        public static void FindPatterns(Phrase phrase)
        {
            var sequenceLength = phrase.Elements.Count;
            Console.WriteLine("Sequence Length " + sequenceLength);

            var patterns = new Dictionary<string, Dictionary<string, Tuple<int, int>>>();

            for (var windowSize = sequenceLength / 2; windowSize >= 1; windowSize--)
            {
                var lastWindowStart = sequenceLength - (windowSize * 2);

                for (var windowStart = 0; windowStart <= lastWindowStart; windowStart++)
                {
                    var windowEnd = windowStart + windowSize - 1;
                    for (var compareWindowStart = windowStart + windowSize; compareWindowStart + windowSize <= sequenceLength; compareWindowStart++)
                    {
                        var compareWindowEnd = compareWindowStart + windowSize - 1;
                        if (Compare(phrase, windowStart, windowEnd, compareWindowStart, compareWindowEnd))
                        {
                            var patternKey = GetPatternKey(phrase, windowStart, windowEnd);
                            var windowKey = $"{windowStart},{windowEnd}";
                            var compareWindowKey = $"{compareWindowStart},{compareWindowEnd}";

                            if (!patterns.ContainsKey(patternKey))
                                patterns.Add(patternKey, new Dictionary<string, Tuple<int, int>>());

                            var pattern = patterns[patternKey];
                            if (!pattern.ContainsKey(windowKey))
                                pattern.Add(windowKey, new Tuple<int, int>(windowStart, windowEnd));

                            if (!pattern.ContainsKey(compareWindowKey))
                                pattern.Add(compareWindowKey, new Tuple<int, int>(compareWindowStart, compareWindowEnd));
                        }
                    }
                }
            }

            RemoverOverlaps(patterns);

            foreach (var pattern in patterns)
            {
                Console.WriteLine(pattern.Key);
                foreach (var window in pattern.Value.OrderBy(x => x.Value.Item1).ThenBy(x => x.Value.Item2))
                {
                    Console.WriteLine("\t" + window.Value.Item1 + " to " + window.Value.Item2);
                }
            }
        }

        private static void RemoverOverlaps(Dictionary<string, Dictionary<string, Tuple<int, int>>> patterns)
        {
            foreach (var pattern in patterns)
            {              
                var windows = pattern.Value.ToList();
                for (var currentWindowIndex = 0; currentWindowIndex < windows.Count - 1; currentWindowIndex++)
                {
                    var overlaps = new List<string>();
                    for (var compareWindowIndex = currentWindowIndex + 1; compareWindowIndex < windows.Count; compareWindowIndex++)
                    {
                        var currentWindow = windows[currentWindowIndex];
                        var compareWindow = windows[compareWindowIndex];

                        bool overlap = currentWindow.Value.Item1 <= compareWindow.Value.Item2 && compareWindow.Value.Item1 <= currentWindow.Value.Item2;
                        if (overlap)
                        {
                            overlaps.Add(currentWindow.Key);
                            overlaps.Add(compareWindow.Key);
                        }
                    }

                    foreach (var overlap in overlaps.Distinct())
                    {
                        pattern.Value.Remove(overlap);
                    }
                }
            }

            var emptyPatterns = new List<string>();
            foreach (var pattern in patterns)
            {
                if (pattern.Value.Count == 0)
                    emptyPatterns.Add(pattern.Key);
            }
            foreach (var emptyPattern in emptyPatterns)
            {
                patterns.Remove(emptyPattern);
            }
        }

        private static string GetPatternKey(Phrase phrase, int windowStart, int windowEnd)
        {
            var key = "";
            for (int i = windowStart; i <= windowEnd; i++)
            {
                if (key != "") key += ",";
                key += phrase.Elements[i].Note + "_" + phrase.Elements[i].Duration;
            }
            return key;
        }

        private static bool Compare(Phrase phrase, int windowStart, int windowEnd, int compareWindowStart, int compareWindowEnd)
        {
            var windowLength = windowEnd - windowStart + 1;

            for (var i = 0; i < windowLength; i++)
            {
                var sourceElementIndex = (windowStart + i);
                var compareWindowIndex = (compareWindowStart + i);

                var sourceElement = phrase.Elements[sourceElementIndex];
                var compareElement = phrase.Elements[compareWindowIndex];

                var isMatch = sourceElement.Duration == compareElement.Duration
                    && sourceElement.Note == compareElement.Note;

                if (!isMatch)
                    return false;
            }

            return true;
        }
    }
}
