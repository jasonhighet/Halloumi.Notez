using Halloumi.Notez.Engine.Notes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine.Generator
{
    public static class RepeatingElementsFinder
    {
        public static List<WindowMatch> FindRepeatingElements(Phrase phrase)
        {
            var sequenceLength = Convert.ToInt32(phrase.PhraseLength);
            var minWindowSize = phrase.Elements.Min(x => x.Duration);
            var windowMatches = new List<WindowMatch>();

            for (var windowSize = sequenceLength / 2; windowSize >= minWindowSize; windowSize--)
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
                        if (result == MatchResult.PerfectMatch || result == MatchResult.TimingMatch)
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
                    .Where(x => (match.MatchType == MatchResult.PerfectMatch && x.MatchType == MatchResult.TimingMatch) || match.MatchType == x.MatchType)
                    .Where(x => x.MatchWindowStart >= match.MatchWindowStart && x.WindowSize <= match.WindowSize)
                    .ToList();

                matchesToRemove.AddRange(subMatches);
            }

            windowMatches.RemoveAll(x => matchesToRemove.Contains(x));

            return windowMatches;
        }

        private static MatchResult Compare(Phrase phrase, int windowStart, int compareWindowStart, int windowSize)
        {
            var matches = new List<MatchResult>();

            for (var i = 0; i < windowSize; i++)
            {
                var element1 = phrase.Elements.FirstOrDefault(x => x.Position == windowStart + i);
                var element2 = phrase.Elements.FirstOrDefault(x => x.Position == compareWindowStart + i);

                if(!(element1== null && element2 == null))
                    matches.Add(CompareElements(element1, element2));
            }

            if (matches.All(x => x == MatchResult.PerfectMatch))
                return MatchResult.PerfectMatch;

            if (matches.All(x => x == MatchResult.PerfectMatch || x == MatchResult.PitchMatch))
                return MatchResult.PitchMatch;

            if (matches.All(x => x == MatchResult.PerfectMatch || x == MatchResult.TimingMatch))
                return MatchResult.TimingMatch;

            if (matches.All(x => x == MatchResult.BlankMatch))
                return MatchResult.BlankMatch;

            return MatchResult.NoMatch;
        }

        private static MatchResult CompareElements(PhraseElement element1, PhraseElement element2)
        {
            if (element1 == null && element2 == null)
                return MatchResult.BlankMatch;

            if (element1 == null || element2 == null)
                return MatchResult.NoMatch;

            if (element1.Duration == element2.Duration && element1.Note == element2.Note)
                return MatchResult.PerfectMatch;

            if (element1.Duration == element2.Duration && element1.Note != element2.Note)
                return MatchResult.TimingMatch;

            if (element1.Duration != element2.Duration && element1.Note == element2.Note)
                return MatchResult.TimingMatch;

            return MatchResult.NoMatch;
        }

        private static bool NotesStartAndEndOnWindowDivision(Phrase phrase, int windowStart, int windowEnd)
        {
            var start = phrase.Elements.Exists(x => x.Position == Convert.ToDecimal(windowStart));
            var end = phrase.Elements.Exists(x => x.OffPosition == Convert.ToDecimal(windowEnd));

            return start && end;
        }

        public enum MatchResult
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
            public MatchResult MatchType { get; set; }
        }

    }
}
