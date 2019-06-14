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

            for (var windowSize = sequenceLength / 2; windowSize >= 1; windowSize--)
            {
                Console.WriteLine("Window Size " + windowSize);

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
                            Console.WriteLine("\t Match found: " + windowStart + "," + windowEnd + " to " + compareWindowStart + "," + compareWindowEnd + "\t" + patternKey);
                        }
                    }
                }
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

            for(var i = 0; i < windowLength; i++)
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
