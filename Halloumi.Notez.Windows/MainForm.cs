using Halloumi.Notez.Engine.Generator;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            LoadLibrary(true);
        }

        private void MergeButton_Click(object sender, EventArgs e)
        {
            if(!LoadLibrary(true))
                return;

            if(!MergeClips())
                return;

            LoadLibrary(true);
        }

        private bool MergeClips()
        {
            Cursor = Cursors.WaitCursor;
            var consoleOut = new StringWriter();
            Console.SetOut(consoleOut);

            _generator.MergeSourceClips();

            Console.SetOut(Console.Out);
            var message = consoleOut.ToString();
            if (message.Trim() != "")
                MessageBox.Show(message);
            Cursor = Cursors.Default;

            return (message == "");
        }

        private void LibraryDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadLibrary(false);
        }

        private bool LoadLibrary(bool clearCache)
        {
            Cursor = Cursors.WaitCursor;
            var consoleOut = new StringWriter();
            Console.SetOut(consoleOut);

            _generator.LoadLibrary(CurrentLibrary(), clearCache);

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
            var message = consoleOut.ToString();
            if (message.Trim() != "")
                MessageBox.Show(message);
            Cursor = Cursors.Default;

            return (message == "");
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

        private void GenerateButton_Click(object sender, EventArgs e)
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

        private void ExploreButton_Click(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            var midiFiles = Directory.EnumerateFiles(".", "*.mid").Select(Path.GetFullPath).ToList();

            if (midiFiles.Count > 0)
            {
                const string playlistName = "Notez.mpl";

                File.WriteAllLines(playlistName, midiFiles);
                Process.Start(playlistName);
                Thread.Sleep(500);
            }

            Cursor = Cursors.Default;

        }
    }
}
