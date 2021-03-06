﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Halloumi.Notez.Engine.Notes
{
    public class NoteHelper
    {
        public static int NoteToNumber(string note)
        {
            var octave = GetOctaveFromNote(note);
            var noteIndex = GetNoteIndexFromNote(note);
            return (octave * 12) + noteIndex;
        }

        public static string NumberToNote(int number)
        {
            var octave = GetOctaveFromNumber(number);
            var noteIndex = GetNoteIndexFromNumber(number);

            return FormatNote(octave, noteIndex);
        }

        public static string NumberToNoteOnly(int number)
        {
            var noteIndex = GetNoteIndexFromNumber(number);
            return GetNoteNames()[noteIndex];
        }

        public static int RemoveOctave(int number)
        {
            var note = NumberToNoteOnly(number);
            return NoteToNumber(note);
        }

        public static int GetDistanceBetweenNotes(int note1, int note2)
        {
            if (note1 == note2) return 0;
            const int noteCount = 12;

            int diffForward;
            int diffBack;
            if (note1 < note2)
            {

                diffForward = note2 - note1;
                diffBack = (noteCount + note1 - note2) * -1;
            }
            else
            {
                diffForward = noteCount - note1 - note2;
                diffBack = note2 - note1;
            }

            return Math.Abs(diffForward) < Math.Abs(diffBack) ? diffForward : diffBack;
        }

        public static int ShiftNote(int note, int amount, Interval interval, Direction direction = Direction.Up)
        {
            return note + (amount * (int)direction * (int)interval);
        }

        public static Phrase ShiftNotes(Phrase phrase, int amount, Interval step, Direction direction = Direction.Up)
        {
            if (amount == 0)
                return phrase.Clone();

            var shiftedPhrase = phrase.Clone();

            ShiftNotesDirect(shiftedPhrase, amount,step, direction);

            return shiftedPhrase;
        }

        public static void ShiftNotesDirect(Phrase phrase, int amount, Interval step, Direction direction = Direction.Up)
        {
            foreach (var element in phrase.Elements)
            {
                element.Note = ShiftNote(element.Note, amount, step, direction);

                if (!element.IsChord) continue;

                var newChord = new List<int>();
                foreach (var chordNote in element.ChordNotes)
                {
                    newChord.Add(ShiftNote(chordNote, amount, step, direction));
                }

                element.ChordNotes = newChord;
            }
        }


        private static string FormatNote(int octave, int noteIndex)
        {
            var noteName = GetNoteNames()[noteIndex];
            return noteName + octave;
        }

        private static int GetNoteIndexFromNote(string note)
        {
            note = note.ToUpper().Trim();
            var regex = new Regex("[^A-Z#]");
            note = regex.Replace(note, "");
            return GetNoteNames().IndexOf(note);
        }

        private static int GetOctaveFromNote(string note)
        {
            note = note.ToUpper().Trim();
            var regex = new Regex("[^0-9-]");
            note = regex.Replace(note, "");
            return note == "" ? 0 : int.Parse(note);
        }

        private static int GetNoteIndexFromNumber(int number)
        {
            return number - (GetOctaveFromNumber(number) * 12);
        }

        private static int GetOctaveFromNumber(int number)
        {
            return Convert.ToInt32(Math.Floor(Convert.ToDecimal(number) / Convert.ToDecimal(12)));
        }


        public static List<string> GetNoteNames()
        {
            return "C,C#,D,D#,E,F,F#,G,G#,A,A#,B".Split(',').ToList();
        }

        public static decimal GetTotalDuration(Phrase phrase)
        {
            return phrase.Elements.Count == 0 ? 0M : phrase.Elements.Max(x => x.OffPosition);
        }
    }

    public enum Interval
    {
        Step = 1,
        Note = 2,
        Octave = 12
    }

    public enum Direction
    {
        Up = 1,
        Down = -1
    }
}
