using Halloumi.Notez.Engine.Generator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Halloumi.Notez.Engine.Midi;

namespace Halloumi.Notez.Windows
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            generator.Dock = DockStyle.Fill;
            magenta.Dock = DockStyle.Fill;

            generator.Initialise();

            ViewGeneratorButton_Click(null, null);
        }

        private void ViewGeneratorButton_Click(object sender, EventArgs e)
        {
            ViewMagentaButton.Checked = false;
            ViewGeneratorButton.Checked = true;
            magenta.Visible = false;
            generator.Visible = true;
        }

        private void ViewMagentaButton_Click(object sender, EventArgs e)
        {
            ViewMagentaButton.Checked = true;
            ViewGeneratorButton.Checked = false;

            magenta.Visible = true;
            generator.Visible = false;
        }
    }
}
