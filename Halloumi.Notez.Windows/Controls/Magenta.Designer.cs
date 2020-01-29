namespace Halloumi.Notez.Windows.Controls
{
    partial class Magenta
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FilesListBox = new System.Windows.Forms.ListBox();
            this.InterpolateButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.ResultsListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // FilesListBox
            // 
            this.FilesListBox.AllowDrop = true;
            this.FilesListBox.FormattingEnabled = true;
            this.FilesListBox.ItemHeight = 16;
            this.FilesListBox.Location = new System.Drawing.Point(15, 14);
            this.FilesListBox.Name = "FilesListBox";
            this.FilesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.FilesListBox.Size = new System.Drawing.Size(205, 276);
            this.FilesListBox.TabIndex = 19;
            this.FilesListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilesListBox_DragDrop);
            this.FilesListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilesListBox_DragEnter);
            // 
            // InterpolateButton
            // 
            this.InterpolateButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InterpolateButton.Location = new System.Drawing.Point(226, 14);
            this.InterpolateButton.Name = "InterpolateButton";
            this.InterpolateButton.Size = new System.Drawing.Size(131, 28);
            this.InterpolateButton.TabIndex = 24;
            this.InterpolateButton.Text = "Interpolate";
            this.InterpolateButton.UseVisualStyleBackColor = true;
            this.InterpolateButton.Click += new System.EventHandler(this.InterpolateButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClearButton.Location = new System.Drawing.Point(226, 48);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(131, 28);
            this.ClearButton.TabIndex = 25;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // ResultsListBox
            // 
            this.ResultsListBox.AllowDrop = true;
            this.ResultsListBox.FormattingEnabled = true;
            this.ResultsListBox.ItemHeight = 16;
            this.ResultsListBox.Location = new System.Drawing.Point(363, 14);
            this.ResultsListBox.Name = "ResultsListBox";
            this.ResultsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.ResultsListBox.Size = new System.Drawing.Size(205, 276);
            this.ResultsListBox.TabIndex = 26;
            this.ResultsListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ResultsListBox_MouseDown);
            // 
            // Magenta
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ResultsListBox);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.InterpolateButton);
            this.Controls.Add(this.FilesListBox);
            this.Name = "Magenta";
            this.Size = new System.Drawing.Size(638, 459);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox FilesListBox;
        private System.Windows.Forms.Button InterpolateButton;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.ListBox ResultsListBox;
    }
}
