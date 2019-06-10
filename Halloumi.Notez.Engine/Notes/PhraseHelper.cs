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
            UpdateDurationsFromPositions(phrase, newLength);
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
