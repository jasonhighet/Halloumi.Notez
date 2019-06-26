using Halloumi.Notez.Engine.Notes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine.Generator
{
    public static class PatternFinder
    {
        public enum PatternType
        {
            Perfect,
            Tempo
        }

        public class Window
        {
            public Window(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start { get; set; }
            public int End { get; set; }
        }

        public class Pattern : Dictionary<string, Window>
        {
            public PatternType PatternType { get; set; }
        }

        public class Patterns : Dictionary<string, Pattern>
        {
            public void AddPattern(string patternKey, int windowStart, int windowEnd, int compareWindowStart,
                int compareWindowEnd, PatternType patternType)
            {
                var windowKey = $"{windowStart},{windowEnd}";
                var compareWindowKey = $"{compareWindowStart},{compareWindowEnd}";

                if (!ContainsKey(patternKey))
                    Add(patternKey, new Pattern() { PatternType = patternType });

                var pattern = this[patternKey];
                if (!pattern.ContainsKey(windowKey))
                    pattern.Add(windowKey, new Window(windowStart, windowEnd));

                if (!pattern.ContainsKey(compareWindowKey))
                    pattern.Add(compareWindowKey, new Window(compareWindowStart, compareWindowEnd));
            }

        }

        public static Patterns FindPatterns(List<Phrase> phrases)
        {
            var patterns = new Patterns();

            foreach (var phrase in phrases)
            {
                var phrasePatterns = FindPatterns(phrase);

                foreach (var pattern in phrasePatterns)
                {
                    if (!patterns.ContainsKey(pattern.Key))
                        patterns.Add(pattern.Key, pattern.Value);
                }
            }
            return patterns;
        }

        public static Patterns FindPatterns(Phrase phrase)
        {
            var patterns = FindPatterns(phrase, PatternType.Perfect);
            var tempoPatterns = FindPatterns(phrase, PatternType.Tempo);

            foreach (var pattern in tempoPatterns)
            {
                patterns.Add(pattern.Key, pattern.Value);
            }
            RemoverOverlapsWithinEachPattern(patterns);
            RemoveOverlappingPatterns(patterns);

            return patterns;
        }

        public static Patterns FindPatterns(Phrase phrase, PatternType patternType)
        {
            var elements = phrase.Clone().Elements;
            var sequenceLength = elements.Count;

            if (patternType == PatternType.Tempo)
                elements.ForEach(x => x.Note = 1);

            var patterns = new Patterns();

            for (var windowSize = sequenceLength / 2; windowSize >= 1; windowSize--)
            {
                var lastWindowStart = sequenceLength - (windowSize * 2);

                for (var windowStart = 0; windowStart <= lastWindowStart; windowStart++)
                {
                    var windowEnd = windowStart + windowSize - 1;
                    for (var compareWindowStart = windowStart + windowSize; compareWindowStart + windowSize <= sequenceLength; compareWindowStart++)
                    {
                        var compareWindowEnd = compareWindowStart + windowSize - 1;
                        if (!Compare(elements, windowStart, windowEnd, compareWindowStart)) continue;

                        var patternKey = GetPatternKey(elements, windowStart, windowEnd, patternType);
                        patterns.AddPattern(patternKey, windowStart, windowEnd, compareWindowStart, compareWindowEnd, patternType);
                    }
                }
            }

            RemoverOverlapsWithinEachPattern(patterns);
            RemoveOverlappingPatterns(patterns);

            return patterns;
        }

        private static void RemoverOverlapsWithinEachPattern(Patterns patterns)
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

                        var overlap = currentWindow.Value.Start <= compareWindow.Value.End && compareWindow.Value.Start <= currentWindow.Value.End;
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
        private static void RemoveOverlappingPatterns(Patterns patterns)
        {
            var patternsToRemove = new List<string>();
            foreach (var pattern in patterns
                .OrderBy(x => GetPatternTotalLength(x.Value))
                .ThenBy(x => GetPatternWindowLength(x.Value))
                .ThenByDescending(x => x.Value.PatternType))
            {
                if (patternsToRemove.Contains(pattern.Key))
                    continue;

                var otherPatterns = patterns
                    .Where(x => x.Key != pattern.Key && GetPatternWindowLength(x.Value) >= GetPatternWindowLength(pattern.Value))
                    .OrderBy(x => GetPatternTotalLength(x.Value))
                    .ThenBy(x => GetPatternWindowLength(x.Value))
                    .ToList();
                foreach (var otherPattern in otherPatterns)
                {
                    if (pattern.Value.PatternType == PatternType.Perfect
                        && otherPattern.Value.PatternType == PatternType.Tempo)
                        continue;

                    if (patternsToRemove.Contains(otherPattern.Key))
                        continue;

                    if (IsPatternOverlappingOtherPattern(pattern.Value, otherPattern.Value))
                    {
                        var comparePatterns = new List<KeyValuePair<string, Pattern>>()
                            {
                                pattern,
                                otherPattern
                            }
                            .OrderBy(x => x.Value.PatternType)
                            .ThenBy(x => GetPatternTotalLength(x.Value))
                            .ThenBy(x => GetPatternWindowLength(x.Value))
                            .ToList();

                        var patternToRemove = comparePatterns.First();

                        if (!patternsToRemove.Contains(patternToRemove.Key))
                            patternsToRemove.Add(patternToRemove.Key);
                        //Console.WriteLine(pattern.Key + " inside " + otherPattern.Key);
                    }
                }
            }
            patternsToRemove.Distinct().ToList().ForEach(x => patterns.Remove(x));
        }

        private static bool IsPatternOverlappingOtherPattern(Pattern pattern, Pattern otherPattern)
        {
            foreach (var currentWindow in pattern)
            {
                var windowOverlapping = false;
                foreach (var compareWindow in otherPattern)
                {
                    if (currentWindow.Value.Start <= compareWindow.Value.End && compareWindow.Value.Start <= currentWindow.Value.End)
                    {
                        windowOverlapping = true;
                        break;
                    }
                }
                if (!windowOverlapping)
                    return false;
            }
            return true;
        }

        private static bool IsPatternWholyLocatedInsideOtherPattern(Pattern pattern, Pattern otherPattern)
        {
            foreach (var currentWindow in pattern)
            {
                var windowContained = false;
                foreach (var compareWindow in otherPattern)
                {
                    if (currentWindow.Value.Start >= compareWindow.Value.Start && currentWindow.Value.End <= compareWindow.Value.End)
                    {
                        windowContained = true;
                        break;
                    }
                }
                if (!windowContained)
                    return false;
            }
            return true;
        }

        private static int GetPatternWindowLength(Pattern pattern)
        {
            return pattern.First().Value.End - pattern.First().Value.Start;
        }

        private static int GetPatternTotalLength(Pattern pattern)
        {
            return GetPatternWindowLength(pattern) * pattern.Count();
        }


        private static string GetPatternKey(IReadOnlyList<PhraseElement> elements, int windowStart, int windowEnd, PatternType patternType)
        {
            var key = patternType.ToString();
            for (var i = windowStart; i <= windowEnd; i++)
            {
                key += ","
                    + elements[i].Note
                    + "_"
                    + elements[i].Duration;
            }
            return key;
        }

        private static bool Compare(IReadOnlyList<PhraseElement> elements, int windowStart, int windowEnd, int compareWindowStart)
        {
            var windowLength = windowEnd - windowStart + 1;

            for (var i = 0; i < windowLength; i++)
            {
                var sourceElementIndex = (windowStart + i);
                var compareWindowIndex = (compareWindowStart + i);

                var sourceElement = elements[sourceElementIndex];
                var compareElement = elements[compareWindowIndex];

                var isMatch = sourceElement.Duration == compareElement.Duration
                              && sourceElement.Note == compareElement.Note;
                //&& sourceElement.IsChord == compareElement.IsChord
                //&& sourceElement.RepeatDuration == compareElement.RepeatDuration;

                if (!isMatch)
                    return false;
            }

            return true;
        }
    }
}
