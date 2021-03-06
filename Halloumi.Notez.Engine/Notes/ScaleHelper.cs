﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine.Notes
{
    public static class ScaleHelper
    {
        private static List<Scale> _scales;

        public static List<Scale> GetScales()
        {
            if (_scales != null) return _scales;

            _scales = new List<Scale>();

            var chromaticNotes = NoteHelper.GetNoteNames().Select(NoteHelper.NoteToNumber).ToList();
            chromaticNotes.AddRange(NoteHelper.GetNoteNames().Select(NoteHelper.NoteToNumber).ToList());

            var scaleDefinitions = new Dictionary<string, int[]>
            {
                {"Natural Minor", new[] {0, 2, 1, 2, 2, 1, 2}},
                {"Harmonic Minor", new[] {0, 2, 1, 2, 2, 1, 3}},
                {"Major", new[] {0, 2, 2, 1, 2, 2, 2}}
            };


            foreach (var scaleDefinition in scaleDefinitions)
            {
                var increments = scaleDefinition.Value.ToList();
                for (var i = 0; i < 12; i++)
                {
                    var noteIndex = i;
                    var notes = new List<int>();
                    foreach (var increment in increments)
                    {
                        noteIndex += increment;
                        notes.Add(chromaticNotes[noteIndex]);
                    }
                    var scale = new Scale()
                    {
                        Name = NoteHelper.NumberToNoteOnly(chromaticNotes[i]) + " " + scaleDefinition.Key,
                        Notes = notes

                    };
                    _scales.Add(scale);
                }

            }

            return _scales;
        }


        public static void MashNotesToScale(Section section, string toScale)
        {
            foreach (var phrase in section.Phrases.Where(x => !x.IsDrums))
            {
                MashNotesToScaleDirect(phrase, toScale);
            }
        }

        public static void MashNotesToScaleDirect(Phrase phrase, string toScale)
        {
            var scale = GetScaleByName(toScale);
            if (scale == null)
                throw new ApplicationException("Invalid scale name");

            foreach (var element in phrase.Elements)
            {
                element.Note = MashNoteToScale(scale, element.Note);

                if (!element.IsChord) continue;

                var newChordNotes = new List<int>();
                foreach (var chordNote in element.ChordNotes)
                {
                    newChordNotes.Add(MashNoteToScale(scale, chordNote));
                }

                element.ChordNotes = newChordNotes;

            }
        }

        public static Phrase MashNotesToScale(Phrase phrase, string toScale)
        {
            var mashedPhrase = phrase.Clone();
            MashNotesToScaleDirect(mashedPhrase, toScale);
            return mashedPhrase;
        }

        private static int MashNoteToScale(Scale scale, int note)
        {
            if (ScaleContainsNote(scale, note))
                return note;

            var difference = 1;
            while (true)
            {
                if (ScaleContainsNote(scale, note + difference))
                    return note + difference;

                if (ScaleContainsNote(scale, note - difference))
                    return note + difference;

                difference++;
            }
        }

        private static bool ScaleContainsNote(Scale scale, int note)
        {
            return scale.Notes.Contains(NoteHelper.RemoveOctave(note));
        }

        public static Phrase TransposeToScale(Phrase phrase, string fromScale, string toScale)
        {
            var fromRoot = GetScaleRootNote(fromScale);
            var toRoot = GetScaleRootNote(toScale);

            var fromType = GetScaleType(fromScale);
            var toType = GetScaleType(toScale);

            Phrase transposedPhrase;
            if (fromType == toType)
            {
                transposedPhrase = phrase.Clone();
            }
            else
            {
                var intermediateScale = NoteHelper.NumberToNoteOnly(fromRoot) + " " + toType;
                transposedPhrase = MashNotesToScale(phrase, intermediateScale);
            }

            var distance = NoteHelper.GetDistanceBetweenNotes(fromRoot, toRoot);
            if (distance == 0) return transposedPhrase;

            var direction = distance < 0 ? Direction.Down : Direction.Up;
            transposedPhrase = NoteHelper.ShiftNotes(transposedPhrase, Math.Abs(distance), Interval.Step, direction);
            transposedPhrase = MashNotesToScale(transposedPhrase, toScale);

            return transposedPhrase;
        }

        private static int GetScaleRootNote(string scaleName)
        {
            var root = GetScaleByName(scaleName)?.Notes[0];

            if (!root.HasValue)
                throw new ApplicationException("Scale not found");

            return root.Value;
        }

        private static Scale GetScaleByName(string scaleName)
        {
            return GetScales()
                .FirstOrDefault(x => string.Equals(x.Name, scaleName, StringComparison.CurrentCultureIgnoreCase));
        }

        private static string GetScaleType(string scaleName)
        {
            if (!scaleName.Contains(" ") || scaleName.Length < 4)
                return scaleName;

            return scaleName.Substring(scaleName.Substring(1, 1) == "#" ? 3 : 2).Trim();
        }

        public static List<ScaleMatch> FindMatchingScales(Phrase phrase)
        {
            var notes = phrase.Elements.Where(x=>!x.IsChord)
                .Select(x => x.Note)
                .Union(phrase.Elements.Where(x => x.IsChord)
                    .SelectMany(x => x.ChordNotes))
                .Distinct()
                .ToList();

            return FindMatchingScales(notes);
        }

        public static List<ScaleMatch> FindMatchingScales(List<int> notes)
        {
            var noteNumbers = notes
                .Select(NoteHelper.NumberToNoteOnly)
                .Select(NoteHelper.NoteToNumber)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            
            var matches = new List<ScaleMatch>();
            foreach (var scale in GetScales())
            {
                var match = MatchNotesToScale(noteNumbers, scale);
                matches.Add(match);
            }

            return matches.OrderBy(x => x.DistanceFromScale)
                .ThenBy(x=> Math.Abs(NoteHelper.GetDistanceBetweenNotes(notes[0], x.Scale.Notes[0])))
                .ToList();
        }

        //public static ScaleMatch MatchPhraseToScale(Phrase phrase, string scaleName)
        //{
        //    var notes = phrase.Elements.Select(x => x.Note).Distinct().ToList();
        //    var scale = GetScaleByName(scaleName);
        //    var noteNumbers = notes
        //        .Select(NoteHelper.NumberToNoteOnly)
        //        .Select(NoteHelper.NoteToNumber)
        //        .Distinct()
        //        .OrderBy(x => x)
        //        .ToList();

        //    return MatchNotesToScale(noteNumbers, scale);
        //}

        private static ScaleMatch MatchNotesToScale(IEnumerable<int> noteNumbers, Scale scale)
        {
            var match = new ScaleMatch
            {
                Scale = scale,
                DistanceFromScale = 0
            };

            foreach (var note in noteNumbers)
            {
                var distanceFromScale = DistanceFromScale(note, scale);
                if (distanceFromScale != 0)
                    match.NotInScale.Add(note);

                match.DistanceFromScale += distanceFromScale;
            }

            match.NotInScale = match.NotInScale.Distinct().OrderBy(x => x).ToList();
            return match;
        }

        private static int DistanceFromScale(int note, Scale scale)
        {
            return scale.Notes.Select(x => Math.Abs(NoteHelper.GetDistanceBetweenNotes(note, x))).Min();
        }

        [Serializable]
        public class ScaleMatch
        {
            public ScaleMatch()
            {
                NotInScale = new List<int>();
            }
            public Scale Scale { get; set; }

            public int DistanceFromScale { get; set; }

            public List<int> NotInScale { get; set; }
        }

        [Serializable]
        public class Scale
        {
            public string Name { get; set; }
            public List<int> Notes { get; set; }
        }
    }
}
