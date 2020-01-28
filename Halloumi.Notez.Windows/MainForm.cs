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
            generator1.Initialise();
        }
    }
}
