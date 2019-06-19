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
                        if (!Compare(phrase, windowStart, windowEnd, compareWindowStart)) continue;

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

            RemoverOverlaps(patterns);

            Console.WriteLine(phrase.Description 
                + " has "
                + sequenceLength
                + " notes and "
                + patterns.SelectMany(x => x.Value).Count() + " patterns");
        }

        private static void RemoverOverlaps(IDictionary<string, Dictionary<string, Tuple<int, int>>> patterns)
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

                        var overlap = currentWindow.Value.Item1 <= compareWindow.Value.Item2 && compareWindow.Value.Item1 <= currentWindow.Value.Item2;
                        if (!overlap) continue;

                        overlaps.Add(currentWindow.Key);
                        overlaps.Add(compareWindow.Key);
                    }

                    foreach (var overlap in overlaps.Distinct())
                    {
                        pattern.Value.Remove(overlap);
                    }
                }
            }

            var emptyPatterns = (from pattern in patterns where pattern.Value.Count == 0 select pattern.Key).ToList();

            foreach (var emptyPattern in emptyPatterns)
            {
                patterns.Remove(emptyPattern);
            }
        }

        private static string GetPatternKey(Phrase phrase, int windowStart, int windowEnd)
        {
            var key = "";
            for (var i = windowStart; i <= windowEnd; i++)
            {
                if (key != "") key += ",";
                key += phrase.Elements[i].Note 
                    + "_" + phrase.Elements[i].Duration 
                    + "_" + phrase.Elements[i].IsChord
                    + "_" + phrase.Elements[i].RepeatDuration;
            }
            return key;
        }

        private static bool Compare(Phrase phrase, int windowStart, int windowEnd, int compareWindowStart)
        {
            var windowLength = windowEnd - windowStart + 1;

            for (var i = 0; i < windowLength; i++)
            {
                var sourceElementIndex = (windowStart + i);
                var compareWindowIndex = (compareWindowStart + i);

                var sourceElement = phrase.Elements[sourceElementIndex];
                var compareElement = phrase.Elements[compareWindowIndex];

                var isMatch = sourceElement.Duration == compareElement.Duration
                    && sourceElement.Note == compareElement.Note
                    && sourceElement.IsChord == compareElement.IsChord
                    && sourceElement.RepeatDuration == compareElement.RepeatDuration;

                if (!isMatch)
                    return false;
            }

            return true;
        }
    }
}
