using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine.Notes
{
    public static class PhraseHelper
    {
        public static decimal GetAverageNote(Phrase phrase)
        {
            return phrase.Elements.Sum(x => x.Note * x.Duration) / phrase.Elements.Sum(x => x.Duration);
        }

        public static int GetMostCommonNote(Phrase phrase)
        {
            var notes = phrase.Elements
                .GroupBy(x => NoteHelper.RemoveOctave(x.Note), 
                    x => x.Duration, 
                    (key, values) => new {Note = key, Duration = values.Sum()})
                .OrderByDescending(x => x.Duration)
                .ToList();

            return notes.FirstOrDefault()?.Note ?? 0;
        }

        public static void TrimPhrase(Phrase phrase, decimal newLength)
        {
            var toRemove = phrase.Elements.Where(x => x.Position >= newLength).ToList();
            if (toRemove.Any())
            {
                phrase.Elements.RemoveAll(x => x.Position >= newLength);
            }


            foreach (var element in phrase.Elements.Where(x => x.OffPosition >= newLength))
            {
                element.Duration = newLength - element.Position;
            }

            phrase.PhraseLength = newLength;
        }

        public static void DuplicatePhrase(Phrase phrase, int count = 1)
        {
            for (var i = 0; i < count; i++)
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

        }

        public static void EnsureLengthsAreEqual(IReadOnlyCollection<Phrase> phrases, decimal length = 0)
        {
            if (length == 0) length = phrases.Max(x => x.PhraseLength);
            foreach (var phrase in phrases)
            {
                if (phrase.PhraseLength == 0)
                {
                    Console.WriteLine("phrase length is 0");
                    continue;
                }

                while (phrase.PhraseLength < length)
                {
                    DuplicatePhrase(phrase);
                }
                if (phrase.PhraseLength > length)
                {
                    TrimPhrase(phrase, length);
                }
            }
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
                if (baseElement == null) continue;

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

        public static void UnmergeChords(Phrase phrase)
        {
            var chords = phrase
                .Elements
                .Where(x => x.IsChord)
                .ToList();

            if (chords.Count == 0)
                return;

            foreach (var chord in chords)
            {
                foreach (var note in chord.ChordNotes)
                {
                    if (note == chord.Note)
                        continue;

                    var newElement = chord.Clone();
                    newElement.ChordNotes.Clear();
                    newElement.Note = note;

                    phrase.Elements.Add(newElement);
                }
                chord.ChordNotes.Clear();
            }

            phrase.Elements = phrase.Elements.OrderBy(x => x.Position).ThenBy(x => x.Note).ToList();
        }

        public static void MergeRepeatedNotes(Phrase phrase)
        {
            if (phrase.Elements.Count <= 1)
                return;

            var elementsToRemove = new List<PhraseElement>();
            for (var currentIndex = 0; currentIndex < phrase.Elements.Count - 1; currentIndex++)
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

        public static void UnmergeRepeatedNotes(Phrase phrase)
        {
            var repeatedNotes = phrase
                .Elements
                .Where(x => x.HasRepeatingNotes)
                .ToList();

            if (repeatedNotes.Count == 0)
                return;

            foreach (var repeatedNote in repeatedNotes)
            {
                var duration = repeatedNote.RepeatDuration;
                for (var i = 1; i < repeatedNote.RepeatCount; i++)
                {
                    var newElement = repeatedNote.Clone();
                    newElement.RepeatDuration = 0;
                    newElement.Duration = duration;
                    newElement.Position = repeatedNote.Position + (i * duration);
                    phrase.Elements.Add(newElement);
                }
                repeatedNote.RepeatDuration = 0;
                repeatedNote.Duration = duration;
            }

            phrase.Elements = phrase.Elements.OrderBy(x => x.Position).ThenBy(x => x.Note).ToList();
        }

        public static void MergeNotes(Phrase phrase)
        {
            if (phrase.Elements.Count <= 1)
                return;

            var elementsToRemove = new List<PhraseElement>();
            for (var currentIndex = 0; currentIndex < phrase.Elements.Count - 1; currentIndex++)
            {
                var current = phrase.Elements[currentIndex];

                var compareIndex = currentIndex + 1;
                var compare = phrase.Elements[compareIndex];

                while (compare.Note == current.Note && compare.IsChord == current.IsChord)
                {
                    elementsToRemove.Add(compare);
                    current.Duration += compare.Duration;

                    compareIndex++;

                    if (compareIndex >= phrase.Elements.Count - 1)
                    {
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

            var halfLength = (phrase.Elements.Count / 2) -1;
            for (var i = 0; i < halfLength; i++)
            {
                if (!AreTheSame(phrase.Elements[i], phrase.Elements[i + halfLength]))
                    return false;
            }

            return true;
        }

        private static bool AreTheSame(PhraseElement element1, PhraseElement element2)
        {
            return element1.Note == element2.Note
                    && element1.Duration == element2.Duration;
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
            foreach (var element in phrase.Elements)
            {
                PhraseElement nextElement;

                if (phrase.IsDrums)
                {
                    nextElement = phrase
                        .Elements
                        .Where(x => x.Position >= element.Position + element.Duration)
                        .OrderBy(x => x.Position)
                        .FirstOrDefault();
                }
                else
                {
                    nextElement = phrase
                        .Elements
                        .Where(x => phrase.Elements.IndexOf(x) > phrase.Elements.IndexOf(element) && x.Position > element.Position)
                        .OrderBy(x => x.Position)
                        .FirstOrDefault();
                }

                var nextPosition = nextElement?.Position ?? phraseLength;

                element.Duration = nextPosition - element.Position;
                if (element.Duration <= 0)
                    throw new ApplicationException("Update duration has gone rogue");
            }
        }

        public static Phrase Join(Phrase newPhrase1, Phrase newPhrase2)
        {
            var newPhrase = newPhrase1.Clone();
            newPhrase.PhraseLength = newPhrase1.PhraseLength + newPhrase2.PhraseLength;

            foreach (var element in newPhrase2.Elements)
            {
                var newElement = element.Clone();
                newElement.Position += newPhrase1.PhraseLength;
                newPhrase.Elements.Add(newElement);
            }

            return newPhrase;

        }

        public static void ChangeLength(Phrase phrase, decimal multiplier)
        {
            foreach (var element in phrase.Elements)
            {
                element.Position *= multiplier;
                element.Duration *= multiplier;
            }

            phrase.PhraseLength *= multiplier;

            phrase.Bpm *= multiplier;
        }
    }
}
