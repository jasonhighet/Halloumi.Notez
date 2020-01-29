using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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
            var midiFiles = new List<string>();
            foreach (var item in FilesListBox.Items)
            {
                midiFiles.Add(item.ToString());
            }

            MagentaHelper.Interpolate(midiFiles);
        }
    }
}
