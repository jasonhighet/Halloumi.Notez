﻿namespace Halloumi.Notez.Windows.Controls
{
    partial class Generator
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
            this.label1 = new System.Windows.Forms.Label();
            this.libraryDropdown = new System.Windows.Forms.ComboBox();
            this.mergeButton = new System.Windows.Forms.Button();
            this.reloadButton = new System.Windows.Forms.Button();
            this.generateButton = new System.Windows.Forms.Button();
            this.countDropdown = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.seedArtistDropdown = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.filterArtistDropdown = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.seedSectionDropdown = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.exploreButton = new System.Windows.Forms.Button();
            this.drumPatternDropdown = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.playButton = new System.Windows.Forms.Button();
            this.FilesListBox = new System.Windows.Forms.ListBox();
            this.GoodButton = new System.Windows.Forms.Button();
            this.ExportDrumsButton = new System.Windows.Forms.Button();
            this.ExportSectionsButton = new System.Windows.Forms.Button();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.CopyButton = new System.Windows.Forms.Button();
            this.TidyButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
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
            this.libraryDropdown.SelectedIndexChanged += new System.EventHandler(this.LibraryDropdown_SelectedIndexChanged);
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
            this.mergeButton.Click += new System.EventHandler(this.MergeButton_Click);
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
            this.reloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // generateButton
            // 
            this.generateButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generateButton.Location = new System.Drawing.Point(109, 258);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(145, 28);
            this.generateButton.TabIndex = 7;
            this.generateButton.Text = "Generate!";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // countDropdown
            // 
            this.countDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.countDropdown.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.countDropdown.FormattingEnabled = true;
            this.countDropdown.Location = new System.Drawing.Point(109, 212);
            this.countDropdown.Name = "countDropdown";
            this.countDropdown.Size = new System.Drawing.Size(144, 25);
            this.countDropdown.Sorted = true;
            this.countDropdown.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 213);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 19);
            this.label2.TabIndex = 5;
            this.label2.Text = "Count";
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
            // exploreButton
            // 
            this.exploreButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exploreButton.Location = new System.Drawing.Point(260, 258);
            this.exploreButton.Name = "exploreButton";
            this.exploreButton.Size = new System.Drawing.Size(131, 28);
            this.exploreButton.TabIndex = 14;
            this.exploreButton.Text = "Explore";
            this.exploreButton.UseVisualStyleBackColor = true;
            this.exploreButton.Click += new System.EventHandler(this.ExploreButton_Click);
            // 
            // drumPatternDropdown
            // 
            this.drumPatternDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.drumPatternDropdown.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.drumPatternDropdown.FormattingEnabled = true;
            this.drumPatternDropdown.Location = new System.Drawing.Point(109, 173);
            this.drumPatternDropdown.Name = "drumPatternDropdown";
            this.drumPatternDropdown.Size = new System.Drawing.Size(144, 25);
            this.drumPatternDropdown.Sorted = true;
            this.drumPatternDropdown.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 176);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 19);
            this.label6.TabIndex = 15;
            this.label6.Text = "Drum Pattern";
            // 
            // playButton
            // 
            this.playButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playButton.Location = new System.Drawing.Point(397, 258);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(140, 28);
            this.playButton.TabIndex = 17;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // FilesListBox
            // 
            this.FilesListBox.FormattingEnabled = true;
            this.FilesListBox.ItemHeight = 16;
            this.FilesListBox.Location = new System.Drawing.Point(109, 302);
            this.FilesListBox.Name = "FilesListBox";
            this.FilesListBox.Size = new System.Drawing.Size(282, 180);
            this.FilesListBox.TabIndex = 18;
            this.FilesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilesListBox_MouseDown);
            // 
            // GoodButton
            // 
            this.GoodButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GoodButton.Location = new System.Drawing.Point(406, 307);
            this.GoodButton.Name = "GoodButton";
            this.GoodButton.Size = new System.Drawing.Size(131, 28);
            this.GoodButton.TabIndex = 19;
            this.GoodButton.Text = "Good";
            this.GoodButton.UseVisualStyleBackColor = true;
            this.GoodButton.Click += new System.EventHandler(this.GoodButton_Click);
            // 
            // ExportDrumsButton
            // 
            this.ExportDrumsButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportDrumsButton.Location = new System.Drawing.Point(259, 173);
            this.ExportDrumsButton.Name = "ExportDrumsButton";
            this.ExportDrumsButton.Size = new System.Drawing.Size(131, 28);
            this.ExportDrumsButton.TabIndex = 20;
            this.ExportDrumsButton.Text = "Export";
            this.ExportDrumsButton.UseVisualStyleBackColor = true;
            this.ExportDrumsButton.Click += new System.EventHandler(this.ExportDrumsButton_Click);
            // 
            // ExportSectionsButton
            // 
            this.ExportSectionsButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportSectionsButton.Location = new System.Drawing.Point(260, 58);
            this.ExportSectionsButton.Name = "ExportSectionsButton";
            this.ExportSectionsButton.Size = new System.Drawing.Size(131, 28);
            this.ExportSectionsButton.TabIndex = 21;
            this.ExportSectionsButton.Text = "Export";
            this.ExportSectionsButton.UseVisualStyleBackColor = true;
            this.ExportSectionsButton.Click += new System.EventHandler(this.ExportSectionsButton_Click);
            // 
            // ApplyButton
            // 
            this.ApplyButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ApplyButton.Location = new System.Drawing.Point(543, 21);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(131, 28);
            this.ApplyButton.TabIndex = 22;
            this.ApplyButton.Text = "Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // CopyButton
            // 
            this.CopyButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CopyButton.Location = new System.Drawing.Point(406, 454);
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(131, 28);
            this.CopyButton.TabIndex = 23;
            this.CopyButton.Text = "Copy";
            this.CopyButton.UseVisualStyleBackColor = true;
            this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // TidyButton
            // 
            this.TidyButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TidyButton.Location = new System.Drawing.Point(543, 454);
            this.TidyButton.Name = "TidyButton";
            this.TidyButton.Size = new System.Drawing.Size(131, 28);
            this.TidyButton.TabIndex = 24;
            this.TidyButton.Text = "Tidy";
            this.TidyButton.UseVisualStyleBackColor = true;
            this.TidyButton.Click += new System.EventHandler(this.TidyButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeleteButton.Location = new System.Drawing.Point(406, 341);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(131, 28);
            this.DeleteButton.TabIndex = 25;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // Generator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.TidyButton);
            this.Controls.Add(this.CopyButton);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.ExportSectionsButton);
            this.Controls.Add(this.ExportDrumsButton);
            this.Controls.Add(this.GoodButton);
            this.Controls.Add(this.FilesListBox);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.drumPatternDropdown);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.exploreButton);
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
            this.Name = "Generator";
            this.Size = new System.Drawing.Size(710, 530);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

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
        private System.Windows.Forms.Button exploreButton;
        private System.Windows.Forms.ComboBox drumPatternDropdown;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.ListBox FilesListBox;
        private System.Windows.Forms.Button GoodButton;
        private System.Windows.Forms.Button ExportDrumsButton;
        private System.Windows.Forms.Button ExportSectionsButton;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button CopyButton;
        private System.Windows.Forms.Button TidyButton;
        private System.Windows.Forms.Button DeleteButton;
    }
}
