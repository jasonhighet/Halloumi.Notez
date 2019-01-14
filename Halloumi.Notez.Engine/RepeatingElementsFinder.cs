using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine
{
    public static class RepeatingElementsFinder
    {
        public static List<WindowMatch> FindRepeatingElements(Phrase phrase)
        {
            var sequenceLength = Convert.ToInt32(phrase.PhraseLength);
            var windowMatches = new List<WindowMatch>();

            for (var windowSize = sequenceLength / 2; windowSize > 1; windowSize--)
            {
                var lastWindowStart = sequenceLength - (windowSize * 2);

                for (var windowStart = 0; windowStart <= lastWindowStart; windowStart++)
                {
                    var windowEnd = windowStart + windowSize - 1;

                    if (!NotesStartAndEndOnWindowDivision(phrase, windowStart, windowEnd))
                        continue;

                    for (var compareWindowStart = windowStart + windowSize; compareWindowStart + windowSize <= sequenceLength; compareWindowStart++)
                    {
                        var compareWindowEnd = compareWindowStart + windowSize - 1;

                        if (!NotesStartAndEndOnWindowDivision(phrase, compareWindowStart, compareWindowEnd))
                            continue;

                        var result = Compare(phrase, windowStart, compareWindowStart, windowSize);
                        if (result == CompareResult.PerfectMatch || result == CompareResult.TimingMatch)
                        {
                            windowMatches.Add(new WindowMatch
                            {
                                WindowSize = windowSize,
                                WindowStart = windowStart,
                                MatchWindowStart = compareWindowStart,
                                MatchType = result
                            });
                        }
                    }
                }

            }

            var matchesToRemove = new List<WindowMatch>();
            foreach (var match in windowMatches)
            {
                var subMatches = windowMatches
                    .Where(x => x != match)
                    .Where(x => (match.MatchType == CompareResult.PerfectMatch && x.MatchType == CompareResult.TimingMatch) || match.MatchType == x.MatchType)
                    .Where(x => x.MatchWindowStart >= match.MatchWindowStart && x.WindowSize <= match.WindowSize)
                    .ToList();

                matchesToRemove.AddRange(subMatches);
            }

            windowMatches.RemoveAll(x => matchesToRemove.Contains(x));

            foreach (var match in windowMatches)
            {
                Console.WriteLine(match.MatchType
                                  + "match! "
                                  + match.WindowStart
                                  + " to "
                                  + (match.WindowStart + match.WindowSize - 1)
                                  + " matches "
                                  + match.MatchWindowStart
                                  + " to "
                                  + (match.MatchWindowStart + match.WindowSize - 1));
            }
            

            return windowMatches;
        }

        private static CompareResult Compare(Phrase phrase, int windowStart, int compareWindowStart, int windowSize)
        {
            var matches = new List<CompareResult>();

            for (var i = 0; i < windowSize; i++)
            {
                var element1 = phrase.Elements.FirstOrDefault(x => x.Position == windowStart + i);
                var element2 = phrase.Elements.FirstOrDefault(x => x.Position == compareWindowStart + i);

                matches.Add(CompareElements(element1, element2));
            }

            if (matches.All(x => x == CompareResult.PerfectMatch))
                return CompareResult.PerfectMatch;

            if (matches.All(x => x == CompareResult.PerfectMatch || x == CompareResult.PitchMatch))
                return CompareResult.PitchMatch;

            if (matches.All(x => x == CompareResult.PerfectMatch || x == CompareResult.TimingMatch))
                return CompareResult.TimingMatch;

            if (matches.All(x => x == CompareResult.BlankMatch))
                return CompareResult.BlankMatch;

            return CompareResult.NoMatch;
        }

        private static CompareResult CompareElements(PhraseElement element1, PhraseElement element2)
        {
            if (element1 == null && element2 == null)
                return CompareResult.BlankMatch;

            if (element1 == null || element2 == null)
                return CompareResult.NoMatch;

            if (element1.Duration == element2.Duration && element1.Note == element2.Note)
                return CompareResult.PerfectMatch;

            if (element1.Duration == element2.Duration && element1.Note != element2.Note)
                return CompareResult.TimingMatch;

            if (element1.Duration != element2.Duration && element1.Note == element2.Note)
                return CompareResult.TimingMatch;

            return CompareResult.NoMatch;
        }

        private static bool NotesStartAndEndOnWindowDivision(Phrase phrase, int windowStart, int windowEnd)
        {
            var start = phrase.Elements.Exists(x => x.Position == Convert.ToDecimal(windowStart));
            var end = phrase.Elements.Exists(x => x.OffPosition == Convert.ToDecimal(windowEnd));

            return start && end;
        }

        public enum CompareResult
        {
            NoMatch,
            PerfectMatch,
            PitchMatch,
            TimingMatch,
            BlankMatch
        }

        public class WindowMatch
        {
            public int WindowSize { get; set; }
            public int WindowStart { get; set; }
            public int MatchWindowStart { get; set; }
            public CompareResult MatchType { get; set; }
        }

    }
}
