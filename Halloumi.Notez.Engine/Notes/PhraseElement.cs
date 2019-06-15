using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Notes
{
    public class PhraseElement
    {
        public int Note { get; set; }
        public decimal Duration { get; set; }
        public decimal Position { get; set; }
        public List<int> ChordNotes { get; set; }

        public decimal OffPosition => Position + Duration - 1;
        public bool IsChord => ChordNotes != null && ChordNotes.Count > 0;


        public bool HasRepeatingNotes => RepeatDuration != 0;
        public decimal RepeatDuration { get; set; }
        public decimal RepeatCount => RepeatDuration == 0 ? 0 : Duration / RepeatDuration;

        public PhraseElement Clone()
        {
            return new PhraseElement()
            {
                Note = Note,
                Duration = Duration,
                Position = Position,
                ChordNotes = ChordNotes,
                RepeatDuration = RepeatDuration
            };
        }

    }
}
