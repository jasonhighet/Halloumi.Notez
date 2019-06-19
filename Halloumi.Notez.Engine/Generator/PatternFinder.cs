using Halloumi.Notez.Engine.Notes;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine.Generator
{
    public static class PatternFinder
    {
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
        }

        public class Patterns : Dictionary<string, Pattern>
        {
            public void AddPattern(string patternKey, int windowStart, int windowEnd, int compareWindowStart,
                int compareWindowEnd)
            {
                var windowKey = $"{windowStart},{windowEnd}";
                var compareWindowKey = $"{compareWindowStart},{compareWindowEnd}";

                if (!ContainsKey(patternKey))
                    Add(patternKey, new Pattern());

                var pattern = this[patternKey];
                if (!pattern.ContainsKey(windowKey))
                    pattern.Add(windowKey, new Window(windowStart, windowEnd));

                if (!pattern.ContainsKey(compareWindowKey))
                    pattern.Add(compareWindowKey, new Window(compareWindowStart, compareWindowEnd));
            }

        }

        public static Patterns FindPatterns(Phrase phrase, bool ignorePitch = false)
        {
            var elements = phrase.Clone().Elements;
            var sequenceLength = elements.Count;

            if (ignorePitch)
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

                        var patternKey = GetPatternKey(elements, windowStart, windowEnd);
                        patterns.AddPattern(patternKey, windowStart, windowEnd, compareWindowStart, compareWindowEnd);
                    }
                }
            }

            RemoverOverlaps(patterns);

            return patterns;
        }

        private static void RemoverOverlaps(Patterns patterns)
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

        private static string GetPatternKey(IReadOnlyList<PhraseElement> elements, int windowStart, int windowEnd)
        {
            var key = "";
            for (var i = windowStart; i <= windowEnd; i++)
            {
                if (key != "") key += ",";
                key += elements[i].Note
                       + "_" + elements[i].Duration;
                //+ "_" + elements[i].IsChord
                //+ "_" + elements[i].RepeatDuration;
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
