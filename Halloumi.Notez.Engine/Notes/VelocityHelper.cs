using System;
using System.Linq;

namespace Halloumi.Notez.Engine.Notes
{
    public static class VelocityHelper
    {
        public static void ApplyVelocityStrategy(Phrase phrase, string strategy)
        {
            switch (strategy)
            {
                case "Shreddage":
                    ApplyShreddage(phrase);
                    break;
                case "Highest":
                    ApplyHighest(phrase);
                    break;
                default:
                    ApplyDefault(phrase);
                    break;
            }
        }

        private static void ApplyDefault(Phrase phrase)
        {
            foreach (var element in phrase.Elements)
            {
                element.Velocity = 96;
            }
        }

        private static void ApplyHighest(Phrase phrase)
        {
            foreach (var element in phrase.Elements)
            {
                element.Velocity = 127;
            }
        }

        private static void ApplyShreddage(Phrase phrase)
        {
            const decimal minVelocity = 60M;
            const decimal maxVelocity = 120M;
            const decimal velocityRange = maxVelocity - minVelocity;

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
