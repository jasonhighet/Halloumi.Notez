namespace Halloumi.Notez.Windows
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewGeneratorButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMagentaButton = new System.Windows.Forms.ToolStripMenuItem();
            this.generator = new Halloumi.Notez.Windows.Controls.Generator();
            this.magenta = new Halloumi.Notez.Windows.Controls.Magenta();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(692, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewGeneratorButton,
            this.ViewMagentaButton});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // ViewGeneratorButton
            // 
            this.ViewGeneratorButton.Checked = true;
            this.ViewGeneratorButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewGeneratorButton.Name = "ViewGeneratorButton";
            this.ViewGeneratorButton.Size = new System.Drawing.Size(158, 26);
            this.ViewGeneratorButton.Text = "&Generator";
            this.ViewGeneratorButton.Click += new System.EventHandler(this.ViewGeneratorButton_Click);
            // 
            // ViewMagentaButton
            // 
            this.ViewMagentaButton.Name = "ViewMagentaButton";
            this.ViewMagentaButton.Size = new System.Drawing.Size(158, 26);
            this.ViewMagentaButton.Text = "&Magenta";
            this.ViewMagentaButton.Click += new System.EventHandler(this.ViewMagentaButton_Click);
            // 
            // generator
            // 
            this.generator.Location = new System.Drawing.Point(0, 273);
            this.generator.Name = "generator";
            this.generator.Size = new System.Drawing.Size(692, 259);
            this.generator.TabIndex = 2;
            // 
            // magenta
            // 
            this.magenta.AllowDrop = true;
            this.magenta.Dock = System.Windows.Forms.DockStyle.Top;
            this.magenta.Location = new System.Drawing.Point(0, 28);
            this.magenta.Name = "magenta";
            this.magenta.Size = new System.Drawing.Size(692, 239);
            this.magenta.TabIndex = 4;
            this.magenta.Visible = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 532);
            this.Controls.Add(this.magenta);
            this.Controls.Add(this.generator);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Notez";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewGeneratorButton;
        private System.Windows.Forms.ToolStripMenuItem ViewMagentaButton;
        private Controls.Generator generator;
        private Controls.Magenta magenta;
    }
}

