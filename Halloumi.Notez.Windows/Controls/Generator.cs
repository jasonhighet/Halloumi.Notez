using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Halloumi.Notez.Engine.Generator;
using Halloumi.Notez.Engine.Midi;

namespace Halloumi.Notez.Windows.Controls
{
    public partial class Generator : UserControl
    {
        private const string Folder = @"..\..\..\Halloumi.Notez.Engine\SourceMidi\";
        private const string GoodFolder = @"..\..\Good\";

        private readonly SectionGenerator _generator = new SectionGenerator(Folder);

        public Generator()
        {
            InitializeComponent();
        }


        private void ReloadButton_Click(object sender, EventArgs e)
        {
            LoadLibrary(true);
        }

        private void MergeButton_Click(object sender, EventArgs e)
        {
            if (!LoadLibrary(true))
                return;

            if (!MergeClips())
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

        public void Initialise()
        {
            for (var i = 0; i < 5; i++)
            {
                countDropdown.Items.Add((i + 1) * 10);
            }

            countDropdown.SelectedIndex = 0;

            _generator.GetLibraries().ForEach(x => libraryDropdown.Items.Add(x));
            libraryDropdown.SelectedIndex = 0;

            LoadFilesList();
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            foreach (var midiFile in GetMidiFiles()) File.Delete(midiFile);
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
                sourceFilter.AvgDistanceBetweenSnares = Convert.ToDecimal(drumPattern.Split(',')[1]);
            }

            _generator.GenerateRiffs(now, count, sourceFilter);

            LoadFilesList();

            Cursor = Cursors.Default;
        }

        private void LoadFilesList()
        {
            FilesListBox.Items.Clear();
            foreach (var midiFile in GetMidiFiles()) FilesListBox.Items.Add(Path.GetFileName(midiFile) + "");
        }

        private void ExploreButton_Click(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            var midiFiles = GetMidiFiles();

            if (midiFiles.Count > 0)
            {
                const string playlistName = "Notez.mpl";

                File.WriteAllLines(playlistName, midiFiles);
                Process.Start(playlistName);
            }

            Cursor = Cursors.Default;

        }

        private static List<string> GetMidiFiles()
        {
            return Directory.EnumerateFiles(".", "*.mid").Select(Path.GetFullPath).ToList();
        }

        private void GoodButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            var sourceFileName = FilesListBox.SelectedItem.ToString();
            var destFileName = Path.Combine(Path.Combine(GoodFolder, CurrentLibrary()), sourceFileName);
            File.Move(sourceFileName, destFileName);

            LoadFilesList();

            Cursor = Cursors.Default;
        }

        private void ExportDrumsButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = @"Select folder to export to";
                var result = dialog.ShowDialog();

                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath)) return;

                Cursor = Cursors.WaitCursor;
                _generator.ExportDrums(dialog.SelectedPath);
                Cursor = Cursors.Default;
            }
        }

        private void ExportSectionsButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = @"Select folder to export to";
                var result = dialog.ShowDialog();

                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath)) return;

                Cursor = Cursors.WaitCursor;
                _generator.ExportSections(dialog.SelectedPath);
                Cursor = Cursors.Default;
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            var files = OpenFiles(@"mid files (*.mid)| *.mid|All files (*.*)|*.*");
            if (files == null || files.Count == 0) return;


            Cursor = Cursors.WaitCursor;
            var consoleOut = new StringWriter();
            Console.SetOut(consoleOut);

            _generator.ApplyStrategiesToMidiFiles(files);

            Console.SetOut(Console.Out);
            var message = consoleOut.ToString();
            if (message.Trim() != "")
                MessageBox.Show(message);
            Cursor = Cursors.Default;
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            var file = OpenFile(@"mpl files (*.mpl)| *.mpl|All files (*.*)|*.*");
            if (file == "") return;
            var folder = OpenFolder("Select destination");
            if (folder == "") return;

            Cursor = Cursors.WaitCursor;
            MidiFileLibraryHelper.CopyPlaylistFiles(file, folder);
            Cursor = Cursors.Default;
        }

        private static string OpenFolder(string description)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                var result = dialog.ShowDialog();

                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath)) return "";

                return dialog.SelectedPath;
            }
        }

        private static string OpenFile(string filter)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Filter = filter;
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName)) return "";
                return dialog.FileName;
            }
        }

        private static List<string> OpenFiles(string filter)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Filter = filter;
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName)) return new List<string>();
                return dialog.FileNames.ToList();
            }
        }

        private void TidyButton_Click(object sender, EventArgs e)
        {
            var files = OpenFiles(@"mid files (*.mid)| *.mid|All files (*.*)|*.*");
            if (files == null || files.Count == 0) return;
            var folder = OpenFolder("Select destination");
            if (folder == "") return;

            Cursor = Cursors.WaitCursor;
            MidiFileLibraryHelper.CopyPlaylistFiles(files, folder);
            Cursor = Cursors.Default;
        }

        private void FilesListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (FilesListBox.SelectedItems.Count == 0)
                return;

            var dataObject = new DataObject();
            var filePaths = new StringCollection();
            foreach (var item in FilesListBox.SelectedItems)
            {
                filePaths.Add(Path.GetFullPath(item.ToString()));
            }

            dataObject.SetFileDropList(filePaths);

            FilesListBox.DoDragDrop(dataObject, DragDropEffects.All);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            var sourceFileName = FilesListBox.SelectedItem.ToString();
            File.Delete(sourceFileName);

            LoadFilesList();

            Cursor = Cursors.Default;
        }
    }
}
