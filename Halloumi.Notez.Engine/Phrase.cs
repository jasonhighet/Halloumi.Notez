using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine
{
    public class Phrase
    {
        public Phrase()
        {
            Elements = new List<PhraseElement>();
        }

        public List<PhraseElement> Elements { get; set; }

        public decimal PhraseLength { get; set; }


        public Phrase Clone()
        {
            return new Phrase()
            {
                Elements = Elements.Select(x => x.Clone()).ToList(),
                PhraseLength = PhraseLength
            };
        }
    }
}
