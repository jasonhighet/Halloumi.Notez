using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Notes
{
    public class Section
    {
        public Section()
        {
            Phrases = new List<Phrase>();   
        }

        public decimal Length => Phrases == null || Phrases.Count == 0 ? 32 : Phrases[0].PhraseLength;

        public decimal Bpm => Phrases == null || Phrases.Count == 0 ? 120 : Phrases[0].Bpm;

        public List<Phrase> Phrases { get; set; }
    }
}
