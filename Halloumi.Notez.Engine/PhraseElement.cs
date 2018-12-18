using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine
{
    public class PhraseElement
    {
        public int Note { get; set; }
        public decimal Duration { get; set; }
        public PhraseElement Clone()
        {
            return new PhraseElement()
            {
                Note = Note,
                Duration = Duration
            };
        }

    }
}
