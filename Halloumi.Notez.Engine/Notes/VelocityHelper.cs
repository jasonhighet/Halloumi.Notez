using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Notes
{
    public static class VelocityHelper
    {
        public static void ApplyVelocityStrategy(Phrase phrase, string strategy)
        {
            if (strategy != "Shreddage") return;

            if (strategy == "FlatHigh")
                ApplyShreddage(phrase);

        }

        private static void ApplyFlatHigh(Phrase phrase)
        {
            foreach (var element in phrase.Elements)
            {
                element.Velocity = 96;
            }
        }

        private static void ApplyShreddage(Phrase phrase)
        {
            var minVelocity = 60M;
            var maxVelocity = 120M;
            var velocityRange = maxVelocity - minVelocity;

            var minLength = phrase.Elements.Min(x => x.Duration);
            var maxLength = phrase.Elements.Max(x => x.Duration);
            var lengthRange = maxLength - minLength;

            var minNote = phrase.Elements.Min(x => x.Note);
            var maxNote = phrase.Elements.Max(x => x.Note);
            var noteRange = maxNote - minNote;

            foreach (var element in phrase.Elements)
            {
                var velocityByLength = minVelocity + (velocityRange / 2);
                if (lengthRange > 0)
                {
                    var adjustedLength = element.Duration - minLength;
                    var lengthAsPercent = adjustedLength / lengthRange;
                    velocityByLength = (lengthAsPercent * velocityRange) + minVelocity;
                }

                var velocityByNote = minVelocity + (velocityRange / 2);
                if (noteRange > 0)
                {
                    var adjustedNote = element.Note - minNote;
                    var noteAsPercent = adjustedNote / noteRange;
                    velocityByNote = (noteAsPercent * velocityRange) + minVelocity;
                }

                element.Velocity = Convert.ToInt32((velocityByLength + velocityByLength + velocityByLength + velocityByNote) / 4);
            }
        }
    }
}
