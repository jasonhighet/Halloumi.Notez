using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Notes
{
    public class Phrase
    {
        public Phrase()
        {
            Elements = new List<PhraseElement>();
        }

        public List<PhraseElement> Elements { get; set; }

        public decimal PhraseLength { get; set; }

        public decimal Bpm { get; set; }

        public string Description { get; internal set; }

        public Phrase Clone()
        {
            return new Phrase()
            {
                Elements = Elements.Select(x => x.Clone()).OrderBy(x=>x.Position).ThenBy(x=>x.Note).ToList(),
                PhraseLength = PhraseLength,
                Description = Description,
                Bpm = Bpm
            };
        }
    }
}
