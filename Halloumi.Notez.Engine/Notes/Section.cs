using System.Collections.Generic;

namespace Halloumi.Notez.Engine.Notes
{
    public class Section
    {
        public Section(string description)
        {
            Phrases = new List<Phrase>();

            this.Description = description;
        }

        public decimal Bpm => Phrases == null || Phrases.Count == 0 ? 120 : Phrases[0].Bpm;

        public List<Phrase> Phrases { get; set; }

        public string Description { get; set; }
    }
}
