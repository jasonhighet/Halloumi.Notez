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
            this.label1 = new System.Windows.Forms.Label();
            this.libraryDropdown = new System.Windows.Forms.ComboBox();
            this.mergeButton = new System.Windows.Forms.Button();
            this.reloadButton = new System.Windows.Forms.Button();
            this.generateButton = new System.Windows.Forms.Button();
            this.countDropdown = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.riffGenerator1 = new Halloumi.Notez.Windows.RiffGenerator();
            this.seedArtistDropdown = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.filterArtistDropdown = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.seedSectionDropdown = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "Library";
            // 
            // libraryDropdown
            // 
            this.libraryDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.libraryDropdown.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.libraryDropdown.FormattingEnabled = true;
            this.libraryDropdown.Location = new System.Drawing.Point(109, 22);
            this.libraryDropdown.Name = "libraryDropdown";
            this.libraryDropdown.Size = new System.Drawing.Size(144, 25);
            this.libraryDropdown.Sorted = true;
            this.libraryDropdown.TabIndex = 2;
            this.libraryDropdown.SelectedIndexChanged += new System.EventHandler(this.libraryDropdown_SelectedIndexChanged);
            // 
            // mergeButton
            // 
            this.mergeButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mergeButton.Location = new System.Drawing.Point(397, 21);
            this.mergeButton.Name = "mergeButton";
            this.mergeButton.Size = new System.Drawing.Size(140, 28);
            this.mergeButton.TabIndex = 3;
            this.mergeButton.Text = "Merge Source";
            this.mergeButton.UseVisualStyleBackColor = true;
            this.mergeButton.Click += new System.EventHandler(this.mergeButton_Click);
            // 
            // reloadButton
            // 
            this.reloadButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reloadButton.Location = new System.Drawing.Point(260, 20);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Size = new System.Drawing.Size(131, 28);
            this.reloadButton.TabIndex = 4;
            this.reloadButton.Text = "Reload";
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.Click += new System.EventHandler(this.reloadButton_Click);
            // 
            // generateButton
            // 
            this.generateButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generateButton.Location = new System.Drawing.Point(260, 179);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(131, 28);
            this.generateButton.TabIndex = 7;
            this.generateButton.Text = "Generate!";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // countDropdown
            // 
            this.countDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.countDropdown.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.countDropdown.FormattingEnabled = true;
            this.countDropdown.Location = new System.Drawing.Point(109, 180);
            this.countDropdown.Name = "countDropdown";
            this.countDropdown.Size = new System.Drawing.Size(144, 25);
            this.countDropdown.Sorted = true;
            this.countDropdown.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 181);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 19);
            this.label2.TabIndex = 5;
            this.label2.Text = "Count";
            // 
            // riffGenerator1
            // 
            this.riffGenerator1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.riffGenerator1.Location = new System.Drawing.Point(0, 0);
            this.riffGenerator1.Name = "riffGenerator1";
            this.riffGenerator1.Size = new System.Drawing.Size(603, 231);
            this.riffGenerator1.TabIndex = 0;
            // 
            // seedArtistDropdown
            // 
            this.seedArtistDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.seedArtistDropdown.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.seedArtistDropdown.FormattingEnabled = true;
            this.seedArtistDropdown.Location = new System.Drawing.Point(109, 98);
            this.seedArtistDropdown.Name = "seedArtistDropdown";
            this.seedArtistDropdown.Size = new System.Drawing.Size(144, 25);
            this.seedArtistDropdown.Sorted = true;
            this.seedArtistDropdown.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 19);
            this.label3.TabIndex = 8;
            this.label3.Text = "Seed Artist";
            // 
            // filterArtistDropdown
            // 
            this.filterArtistDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterArtistDropdown.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filterArtistDropdown.FormattingEnabled = true;
            this.filterArtistDropdown.Location = new System.Drawing.Point(109, 138);
            this.filterArtistDropdown.Name = "filterArtistDropdown";
            this.filterArtistDropdown.Size = new System.Drawing.Size(144, 25);
            this.filterArtistDropdown.Sorted = true;
            this.filterArtistDropdown.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 19);
            this.label4.TabIndex = 10;
            this.label4.Text = "Filter Artist";
            // 
            // seedSectionDropdown
            // 
            this.seedSectionDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.seedSectionDropdown.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.seedSectionDropdown.FormattingEnabled = true;
            this.seedSectionDropdown.Location = new System.Drawing.Point(109, 60);
            this.seedSectionDropdown.Name = "seedSectionDropdown";
            this.seedSectionDropdown.Size = new System.Drawing.Size(144, 25);
            this.seedSectionDropdown.Sorted = true;
            this.seedSectionDropdown.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 19);
            this.label5.TabIndex = 12;
            this.label5.Text = "Seed Section";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 231);
            this.Controls.Add(this.seedSectionDropdown);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.filterArtistDropdown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.seedArtistDropdown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.countDropdown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.reloadButton);
            this.Controls.Add(this.mergeButton);
            this.Controls.Add(this.libraryDropdown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.riffGenerator1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Notez";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RiffGenerator riffGenerator1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox libraryDropdown;
        private System.Windows.Forms.Button mergeButton;
        private System.Windows.Forms.Button reloadButton;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.ComboBox countDropdown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox seedArtistDropdown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox filterArtistDropdown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox seedSectionDropdown;
        private System.Windows.Forms.Label label5;
    }
}

