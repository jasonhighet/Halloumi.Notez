using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Notes
{
    public static class PhraseHelper
    {
        public static void TrimPhrase(Phrase phrase, decimal newLength)
        {
            phrase.PhraseLength = newLength;
            phrase.Elements.RemoveAll(x => x.Position >= newLength);

            var lastElement = phrase.Elements.LastOrDefault();
            if(lastElement == null)
                return;

            lastElement.Duration = newLength - lastElement.Position;
        }

        public static void DuplicatePhrase(Phrase phrase)
        {
            var newLength = phrase.PhraseLength * 2;
            var newElements = phrase.Elements.Select(x => x.Clone()).ToList();
            foreach (var newElement in newElements)
            {
                newElement.Position += phrase.PhraseLength;
            }

            phrase.PhraseLength = newLength;
            phrase.Elements.AddRange(newElements);
        }

        public static void MergeChords(Phrase phrase)
        {
            var chords = phrase
                .Elements.GroupBy(x => x.Position)
                .Where(x => x.Count() > 1)
                .ToList();

            if (chords.Count == 0)
                return;

            var elementsToRemove = new List<PhraseElement>();
            foreach (var chord in chords)
            {
                var baseElement = chord.OrderBy(x => x.Note).FirstOrDefault();
                baseElement.ChordNotes = new List<int>();
                foreach (var element in chord)
                {
                    baseElement.ChordNotes.Add(element.Note);

                    if (element == baseElement)
                        continue;
                    
                    elementsToRemove.Add(element);
                }
            }

            foreach (var element in elementsToRemove)
            {
                phrase.Elements.Remove(element);
            }
        }

        public static void MergeRepeatedNotes(Phrase phrase)
        {
            if (phrase.Elements.Count <= 1)
                return;

            var elementsToRemove = new List<PhraseElement>();
            for (int currentIndex = 0; currentIndex < phrase.Elements.Count - 1; currentIndex++)
            {
                var current = phrase.Elements[currentIndex];
                var currentRepeatDuration = current.Duration;

                var compareIndex = currentIndex + 1;
                var compare = phrase.Elements[compareIndex];

                while (compare.Note == current.Note && compare.Duration == currentRepeatDuration && compare.IsChord == current.IsChord)
                {
                    elementsToRemove.Add(compare);
                    current.Duration += currentRepeatDuration;
                    current.RepeatDuration = currentRepeatDuration;

                    compareIndex++;

                    if (compareIndex >= phrase.Elements.Count - 1)
                    {
                        currentIndex = phrase.Elements.Count;
                        break;
                    }
                    compare = phrase.Elements[compareIndex];
                }

                currentIndex = compareIndex - 1;
            }

            foreach (var element in elementsToRemove)
            {
                phrase.Elements.Remove(element);
            }

        }

        public static bool IsPhraseDuplicated(Phrase phrase)
        {
            if (phrase.Elements.Count % 2 != 0)
                return false;

            var halfLength = phrase.Elements.Count / 2;
            for (var i = 0; i < halfLength; i++)
            {
                if (!AreTheSame(phrase.Elements[i], phrase.Elements[i + halfLength]))
                    return false;
            }

            return true;
        }


        public static bool AreTheSame(Phrase phrase1, Phrase phrase2)
        {
            if (phrase1.PhraseLength != phrase2.PhraseLength)
                return false;

            return !phrase1.Elements.Where((t, i) => !AreTheSame(t, phrase2.Elements[i])).Any();
        }

        private static bool AreTheSame(PhraseElement element1, PhraseElement element2)
        {
            return (element1.Note == element2.Note
                    && element1.Duration == element2.Duration);
        }

        public static void UpdatePositionsFromDurations(Phrase phrase)
        {
            foreach (var element in phrase.Elements)
            {
                var currentIndex = phrase.Elements.IndexOf(element);
                element.Position = phrase.Elements
                    .Where(x => phrase.Elements.IndexOf(x) < currentIndex)
                    .Sum(x => x.Duration);
            }
        }

        public static void UpdateDurationsFromPositions(Phrase phrase, decimal phraseLength)
        {
            for (var i = 0; i < phrase.Elements.Count; i++)
            {
                var element = phrase.Elements[i];
                var nextPosition = (i < phrase.Elements.Count - 1) ? phrase.Elements[i + 1].Position : phraseLength;
                element.Duration = nextPosition - element.Position;

                if(element.Duration <= 0)
                    throw new ApplicationException("Update duration has gone rogue");
            }
        }
    }
}
