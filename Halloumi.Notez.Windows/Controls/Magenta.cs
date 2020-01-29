using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Halloumi.Notez.Engine.Magenta;

namespace Halloumi.Notez.Windows.Controls
{
    public partial class Magenta : UserControl
    {
        public Magenta()
        {
            InitializeComponent();
        }

        public void Inititalise()
        {
            MagentaHelper.Start();
        }

        private void FilesListBox_DragDrop(object sender, DragEventArgs e)
        {
            var files = ((string[]) e.Data.GetData(DataFormats.FileDrop, false)).ToList();

            foreach (var file in files)
            {
                FilesListBox.Items.Add(file);
            }
        }

        private void FilesListBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) 
                ? DragDropEffects.Copy 
                : DragDropEffects.None;
        }

        private void InterpolateButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            var midiFiles = new List<string>();
            foreach (var item in FilesListBox.Items)
            {
                midiFiles.Add(item.ToString());
            }

            var results = MagentaHelper.Interpolate(midiFiles);

            ResultsListBox.Items.Clear();
            foreach (var result in results)
            {
                ResultsListBox.Items.Add(Path.GetFullPath(result));
            }

            Cursor = Cursors.Default;

        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            FilesListBox.Items.Clear();
        }

        private void ResultsListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (ResultsListBox.SelectedItems.Count == 0)
                return;

            var dataObject = new DataObject();
            var filePaths = new StringCollection();
            foreach (var item in ResultsListBox.SelectedItems)
            {
                filePaths.Add(Path.GetFullPath(item.ToString()));
            }

            dataObject.SetFileDropList(filePaths);

            ResultsListBox.DoDragDrop(dataObject, DragDropEffects.All);
        }
    }
}
