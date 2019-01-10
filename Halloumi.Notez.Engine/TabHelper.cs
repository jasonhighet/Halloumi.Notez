using System.IO;
using System.Linq;

namespace Halloumi.Notez.Engine
{
    public static class TabHelper
    {
        public static Phrase ParseTab(string tabText)
        {

            var tab = new TabParser();
            tab.LoadTabFromTabText(tabText);

            var phrase = new Phrase
            {
                Elements = tab
                    .TabNotes
                    .Select(x => new PhraseElement { Note = x.Number, Duration = x.Length })
                    .ToList()
            };

            PhraseHelper.UpdatePositionsFromDurations(phrase);

            return phrase;
        }

        public static Phrase ParseTabFile(string filepath)
        {
            var tabText = File.ReadAllLines(filepath);

                var tab = new TabParser();
                tab.LoadTabFromTabText(tabText);

                var phrase = new Phrase
                {
                    Elements = tab
                        .TabNotes
                        .Select(x => new PhraseElement {Note = x.Number, Duration = x.Length})
                        .ToList()
                };

            PhraseHelper.UpdatePositionsFromDurations(phrase);

            return phrase;

        }
    }
}
