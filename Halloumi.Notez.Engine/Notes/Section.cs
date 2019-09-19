using System.Collections.Generic;

namespace Halloumi.Notez.Engine.Notes
{
    public class Section
    {
        public Section()
        {
            Phrases = new List<Phrase>();   
        }

        public decimal Bpm => Phrases == null || Phrases.Count == 0 ? 120 : Phrases[0].Bpm;

        public List<Phrase> Phrases { get; set; }
    }
}
