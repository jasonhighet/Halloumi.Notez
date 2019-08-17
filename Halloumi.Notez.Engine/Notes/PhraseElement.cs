using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Notes
{
    public class PhraseElement
    {
        public PhraseElement()
        {
            ChordNotes = new List<int>();
            Velocity = 95;
        }

        public int Note { get; set; }
        public decimal Duration { get; set; }
        public decimal Position { get; set; }
        public List<int> ChordNotes { get; set; }

        public decimal OffPosition => Position + Duration;
        public bool IsChord => ChordNotes != null && ChordNotes.Count > 0;


        public bool HasRepeatingNotes => RepeatDuration != 0;
        public decimal RepeatDuration { get; set; }
        public decimal RepeatCount => RepeatDuration == 0 ? 0 : Duration / RepeatDuration;

        public int Velocity { get; set; }

        public PhraseElement Clone()
        {
            return new PhraseElement()
            {
                Note = Note,
                Duration = Duration,
                Position = Position,
                ChordNotes = new List<int>(ChordNotes),
                RepeatDuration = RepeatDuration,
                Velocity = Velocity
            };
        }

    }
}
