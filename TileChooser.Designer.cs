﻿namespace sth1edwv
{
    partial class TileChooser
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
            this.tilePicker1 = new sth1edwv.TilePicker();
            this.SuspendLayout();
            // 
            // tilePicker1
            // 
            this.tilePicker1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tilePicker1.Location = new System.Drawing.Point(0, 0);
            this.tilePicker1.Name = "tilePicker1";
            this.tilePicker1.SelectedIndex = -1;
            this.tilePicker1.Size = new System.Drawing.Size(527, 527);
            this.tilePicker1.TabIndex = 1;
            this.tilePicker1.TileSet = null;
            this.tilePicker1.SelectionChanged += new System.EventHandler<sth1edwv.Tile>(this.tilePicker1_SelectionChanged);
            // 
            // TileChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 527);
            this.Controls.Add(this.tilePicker1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TileChooser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tile Chooser";
            this.ResumeLayout(false);

        }

        #endregion

        private TilePicker tilePicker1;
    }
}