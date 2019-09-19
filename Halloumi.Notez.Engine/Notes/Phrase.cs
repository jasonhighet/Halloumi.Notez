using Halloumi.Notez.Engine.Midi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloumi.Notez.Engine.Notes
{
    [Serializable]
    public class Phrase
    {
        public Phrase()
        {
            Elements = new List<PhraseElement>();
            Instrument = MidiInstrument.AcousticGrandPiano;
        }

        public List<PhraseElement> Elements { get; set; }

        public decimal PhraseLength { get; set; }

        public decimal Bpm { get; set; }

        public string Description { get; internal set; }

        public MidiInstrument Instrument { get; set; }

        public bool IsDrums { get; set; }

        public Phrase Clone()
        {
            return new Phrase()
            {
                Elements = Elements.Select(x => x.Clone()).OrderBy(x=>x.Position).ThenBy(x=>x.Note).ToList(),
                PhraseLength = PhraseLength,
                Description = Description,
                Bpm = Bpm,
                Instrument = Instrument,
                IsDrums = IsDrums
            };
        }
    }
}
