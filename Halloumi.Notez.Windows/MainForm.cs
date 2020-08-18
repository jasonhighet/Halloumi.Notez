using Halloumi.Notez.Engine.Generator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Halloumi.Notez.Engine.Magenta;
using Halloumi.Notez.Engine.Midi;

namespace Halloumi.Notez.Windows
{
    public partial class MainForm : Form
    {
        private bool _magentaInitialised = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            generator.Dock = DockStyle.Fill;
            magenta.Dock = DockStyle.Fill;
            tabulator.Dock = DockStyle.Fill;

            generator.Initialise();
            tabulator.Initialise();

            ViewGeneratorButton_Click(null, null);
        }

        private void ViewGeneratorButton_Click(object sender, EventArgs e)
        {
            ViewMagentaButton.Checked = false;
            ViewGeneratorButton.Checked = true;
            ViewTabulatorButton.Checked = false;
            magenta.Visible = false;
            generator.Visible = true;
            tabulator.Visible = false;
        }

        private void ViewMagentaButton_Click(object sender, EventArgs e)
        {
            ViewMagentaButton.Checked = true;
            ViewGeneratorButton.Checked = false;
            ViewTabulatorButton.Checked = false;

            magenta.Visible = true;
            generator.Visible = false;
            tabulator.Visible = false;

            if (!_magentaInitialised)
            {
                magenta.Inititalise();
                _magentaInitialised = true;
            }
            
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MagentaHelper.Stop();
        }

        private void ViewTabulatorButton_Click(object sender, EventArgs e)
        {
            ViewMagentaButton.Checked = false;
            ViewGeneratorButton.Checked = false;
            ViewTabulatorButton.Checked = true;

            magenta.Visible = false;
            generator.Visible = false;
            tabulator.Visible = true;
        }
    }
}
