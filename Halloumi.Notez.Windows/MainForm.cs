using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Halloumi.Notez.Engine;

namespace Halloumi.Notez.Windows
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var tab = new TabParser();
            tab.LoadTabFromTabText(textBoxTab.Text);

            var notes = tab.TabNotes.Aggregate("", (current, note) => current + (note.Note) + "\t");

            textBoxNotes.Text = notes;

        }
    }
}
