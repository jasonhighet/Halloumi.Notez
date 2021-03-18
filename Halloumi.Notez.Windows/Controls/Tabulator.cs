using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Halloumi.Notez.Engine.Midi;
using Halloumi.Notez.Engine.Notes;
using Halloumi.Notez.Engine.Tabs;

namespace Halloumi.Notez.Windows.Controls
{
    public partial class Tabulator : UserControl
    {
        private string _currentFile = "";

        public Tabulator()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open Midi File",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "mid",
                Filter = "Midi files (*.mid)|*.mid",
                FilterIndex = 2,
                RestoreDirectory = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true,
                InitialDirectory = string.IsNullOrEmpty(_currentFile) ? "" : Path.GetDirectoryName(_currentFile)
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadTab(dialog.FileName);
            }
        }

        private void LoadTab(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            _currentFile = fileName;
            FilenameTextbox.Text = Path.GetFileName(fileName);

            var transposeSteps = Convert.ToInt32(TransposeBox.Items[TransposeBox.SelectedIndex]);
            var oneLine = OneLineBox.Items[OneLineBox.SelectedIndex].ToString() == "Yes";
            var tuningName = TuningBox.Items[TuningBox.SelectedIndex].ToString();

            var tuning = "";
            if(tuningName == "Drop D")
                tuning = "E,B,G,D,A,D";
            else if (tuningName == "E Standard")
                tuning = "E,B,G,D,A,E";
            else if (tuningName == "Drop C#")
                tuning = "D#,A#,F#,C#,G#,C#";
            else if (tuningName == "Drop C")
                tuning = "D,A,F,C,G,C";

            var section = MidiHelper.ReadMidi(_currentFile);
            var phrase = section.Phrases[0];
            NoteHelper.ShiftNotesDirect(phrase, transposeSteps, Interval.Step);

            TabBox.Text = TabHelper.GenerateTab(phrase, tuning, oneLine);
            
        }

        public void Initialise()
        {
            TuningBox.Items.Clear();
            TuningBox.Items.Add("Drop D");
            TuningBox.Items.Add("E Standard");
            TuningBox.Items.Add("Drop C#");
            TuningBox.Items.Add("Drop C");
            TuningBox.SelectedIndex = 3;

            OneLineBox.Items.Clear();
            OneLineBox.Items.Add("Yes");
            OneLineBox.Items.Add("No");
            OneLineBox.SelectedIndex = 1;

            TransposeBox.Items.Clear();
            for (int i = -7; i < 7; i++)
            {
                TransposeBox.Items.Add(i.ToString());
            }
            TransposeBox.SelectedIndex = 0;

        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFile))
                return;

            var files = Directory.GetFiles(Path.GetDirectoryName(_currentFile)).ToList().Where(x => Path.GetExtension(x) == ".mid").ToList();
            var index = files.IndexOf(_currentFile);
            if (index < 0) return;

            index++;
            if (index >= files.Count)
                index = 0;

            LoadTab(files[index]);
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFile))
                return;

            var files = Directory.GetFiles(Path.GetDirectoryName(_currentFile)).ToList().Where(x => Path.GetExtension(x) == ".mid").ToList();
            var index = files.IndexOf(_currentFile);
            if (index < 0) return;

            index--;
            if (index < 0)
                index = files.Count - 1;

            LoadTab(files[index]);
        }

        private void OneLineBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTab(_currentFile);
        }

        private void TransposeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTab(_currentFile);
        }

        private void TuningBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTab(_currentFile);
        }
    }
}
