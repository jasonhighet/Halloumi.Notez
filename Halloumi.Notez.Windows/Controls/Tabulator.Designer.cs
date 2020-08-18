namespace Halloumi.Notez.Windows.Controls
{
    partial class Tabulator
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
            this.LoadButton = new System.Windows.Forms.Button();
            this.FilenameTextbox = new System.Windows.Forms.TextBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.PreviousButton = new System.Windows.Forms.Button();
            this.TuningBox = new System.Windows.Forms.ComboBox();
            this.TransposeBox = new System.Windows.Forms.ComboBox();
            this.OneLineBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TabBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // LoadButton
            // 
            this.LoadButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold);
            this.LoadButton.Location = new System.Drawing.Point(95, 14);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(75, 29);
            this.LoadButton.TabIndex = 0;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // FilenameTextbox
            // 
            this.FilenameTextbox.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FilenameTextbox.Location = new System.Drawing.Point(179, 17);
            this.FilenameTextbox.Name = "FilenameTextbox";
            this.FilenameTextbox.ReadOnly = true;
            this.FilenameTextbox.Size = new System.Drawing.Size(327, 25);
            this.FilenameTextbox.TabIndex = 1;
            // 
            // NextButton
            // 
            this.NextButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold);
            this.NextButton.Location = new System.Drawing.Point(593, 17);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 28);
            this.NextButton.TabIndex = 2;
            this.NextButton.Text = "Next";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // PreviousButton
            // 
            this.PreviousButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold);
            this.PreviousButton.Location = new System.Drawing.Point(512, 17);
            this.PreviousButton.Name = "PreviousButton";
            this.PreviousButton.Size = new System.Drawing.Size(75, 29);
            this.PreviousButton.TabIndex = 3;
            this.PreviousButton.Text = "Previous";
            this.PreviousButton.UseVisualStyleBackColor = true;
            this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
            // 
            // TuningBox
            // 
            this.TuningBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TuningBox.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.TuningBox.FormattingEnabled = true;
            this.TuningBox.Location = new System.Drawing.Point(95, 59);
            this.TuningBox.Name = "TuningBox";
            this.TuningBox.Size = new System.Drawing.Size(161, 25);
            this.TuningBox.TabIndex = 5;
            this.TuningBox.SelectedIndexChanged += new System.EventHandler(this.TuningBox_SelectedIndexChanged);
            // 
            // TransposeBox
            // 
            this.TransposeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TransposeBox.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.TransposeBox.FormattingEnabled = true;
            this.TransposeBox.Location = new System.Drawing.Point(348, 59);
            this.TransposeBox.Name = "TransposeBox";
            this.TransposeBox.Size = new System.Drawing.Size(78, 25);
            this.TransposeBox.TabIndex = 6;
            this.TransposeBox.SelectedIndexChanged += new System.EventHandler(this.TransposeBox_SelectedIndexChanged);
            // 
            // OneLineBox
            // 
            this.OneLineBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OneLineBox.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.OneLineBox.FormattingEnabled = true;
            this.OneLineBox.Location = new System.Drawing.Point(512, 59);
            this.OneLineBox.Name = "OneLineBox";
            this.OneLineBox.Size = new System.Drawing.Size(75, 25);
            this.OneLineBox.TabIndex = 7;
            this.OneLineBox.SelectedIndexChanged += new System.EventHandler(this.OneLineBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(19, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 19);
            this.label5.TabIndex = 13;
            this.label5.Text = "Tuning";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(271, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 19);
            this.label1.TabIndex = 14;
            this.label1.Text = "Transpose";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(441, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 19);
            this.label2.TabIndex = 15;
            this.label2.Text = "One Line";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(19, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 19);
            this.label3.TabIndex = 16;
            this.label3.Text = "File";
            // 
            // TabBox
            // 
            this.TabBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabBox.Location = new System.Drawing.Point(23, 104);
            this.TabBox.Multiline = true;
            this.TabBox.Name = "TabBox";
            this.TabBox.ReadOnly = true;
            this.TabBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TabBox.Size = new System.Drawing.Size(642, 305);
            this.TabBox.TabIndex = 17;
            this.TabBox.WordWrap = false;
            // 
            // Tabulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TabBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.OneLineBox);
            this.Controls.Add(this.TransposeBox);
            this.Controls.Add(this.TuningBox);
            this.Controls.Add(this.PreviousButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.FilenameTextbox);
            this.Controls.Add(this.LoadButton);
            this.Name = "Tabulator";
            this.Size = new System.Drawing.Size(689, 427);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.TextBox FilenameTextbox;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button PreviousButton;
        private System.Windows.Forms.ComboBox TuningBox;
        private System.Windows.Forms.ComboBox TransposeBox;
        private System.Windows.Forms.ComboBox OneLineBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TabBox;
    }
}
