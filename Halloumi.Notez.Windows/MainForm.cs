using Halloumi.Notez.Engine.Generator;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Halloumi.Notez.Windows
{
    public partial class MainForm : Form
    {
        private const string Folder = @"..\..\..\Halloumi.Notez.Engine\SourceMidi\";

        private readonly SectionGenerator _generator = new SectionGenerator(Folder);


        public MainForm()
        {
            InitializeComponent();
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            var consoleOut = new StringWriter();
            Console.SetOut(consoleOut);

            _generator.LoadLibrary(CurrentLibrary(), true);

            Console.SetOut(Console.Out);
            var messge = consoleOut.ToString();
            if(messge.Trim() != "")
                MessageBox.Show(messge);
            Cursor = Cursors.Default;
        }

        private void mergeButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            var consoleOut = new StringWriter();
            Console.SetOut(consoleOut);
            
            _generator.MergeSourceClips();
            _generator.LoadLibrary(CurrentLibrary(), true);

            Console.SetOut(Console.Out);
            var messge = consoleOut.ToString();
            if (messge.Trim() != "")
                MessageBox.Show(messge);
            Cursor = Cursors.Default;
        }

        private void libraryDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            var consoleOut = new StringWriter();
            Console.SetOut(consoleOut);

            _generator.LoadLibrary(CurrentLibrary(), false);

            seedArtistDropdown.Items.Clear();
            seedArtistDropdown.Items.Add("");
            _generator.GetArtists().ForEach(x => seedArtistDropdown.Items.Add(x));
            seedArtistDropdown.SelectedIndex = 0;

            filterArtistDropdown.Items.Clear();
            filterArtistDropdown.Items.Add("");
            _generator.GetArtists().ForEach(x => filterArtistDropdown.Items.Add(x));
            filterArtistDropdown.SelectedIndex = 0;

            seedSectionDropdown.Items.Clear();
            seedSectionDropdown.Items.Add("");
            _generator.GetSections().ForEach(x => seedSectionDropdown.Items.Add(x));
            seedSectionDropdown.SelectedIndex = 0;

            drumPatternDropdown.Items.Clear();
            drumPatternDropdown.Items.Add("");
            _generator.GetDrumPatterns().ForEach(x => drumPatternDropdown.Items.Add(x));
            drumPatternDropdown.SelectedIndex = 0;

            Console.SetOut(Console.Out);
            var messge = consoleOut.ToString();
            if (messge.Trim() != "")
                MessageBox.Show(messge);
            Cursor = Cursors.Default;
        }

        private string CurrentLibrary()
        {
            return libraryDropdown.Items[libraryDropdown.SelectedIndex].ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            for (var i = 0; i < 5; i++)
            {
                countDropdown.Items.Add((i + 1) * 10);
            }

            countDropdown.SelectedIndex = 0;

            _generator.GetLibraries().ForEach(x => libraryDropdown.Items.Add(x));
            libraryDropdown.SelectedIndex = 0;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            foreach (var midiFile in Directory.EnumerateFiles(".", "*.mid")) File.Delete(midiFile);
            var now = DateTime.Now.ToString("yyyymmddhhss");

            var count = int.Parse(countDropdown.Items[countDropdown.SelectedIndex].ToString());



            var sourceFilter = new SectionGenerator.SourceFilter
            {
                SeedArtist = seedArtistDropdown.Items[seedArtistDropdown.SelectedIndex].ToString(),
                ArtistFilter = filterArtistDropdown.Items[filterArtistDropdown.SelectedIndex].ToString(),
                SeedSection = seedSectionDropdown.Items[seedSectionDropdown.SelectedIndex].ToString()
            };


            var drumPattern = drumPatternDropdown.Items[drumPatternDropdown.SelectedIndex].ToString();
            if (drumPattern != "")
            {
                sourceFilter.AvgDistanceBetweenKicks = Convert.ToDecimal(drumPattern.Split(',')[0]);
                sourceFilter.AvgDistanceBetweenSnares =  Convert.ToDecimal(drumPattern.Split(',')[1]);
            }

            _generator.GenerateRiffs(now, count, sourceFilter);

            Cursor = Cursors.Default;
        }

        private void exploreButton_Click(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
