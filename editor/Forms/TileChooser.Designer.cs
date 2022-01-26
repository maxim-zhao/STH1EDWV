using System.ComponentModel;
using sth1edwv.Controls;
using sth1edwv.GameObjects;

namespace sth1edwv.Forms
{
    partial class TileChooser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.tilePicker1 = new sth1edwv.Controls.ItemPicker();
            this.SuspendLayout();
            // 
            // tilePicker1
            // 
            this.tilePicker1.FixedItemsPerRow = true;
            this.tilePicker1.ItemsPerRow = 16;
            this.tilePicker1.Location = new System.Drawing.Point(4, 3);
            this.tilePicker1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tilePicker1.Name = "tilePicker1";
            this.tilePicker1.Scaling = 1;
            this.tilePicker1.SelectedIndex = -1;
            this.tilePicker1.ShowTransparency = false;
            this.tilePicker1.Size = new System.Drawing.Size(609, 605);
            this.tilePicker1.TabIndex = 1;
            this.tilePicker1.SelectionChanged += new System.EventHandler<sth1edwv.GameObjects.IDrawableBlock>(this.tilePicker1_SelectionChanged);
            // 
            // TileChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 608);
            this.Controls.Add(this.tilePicker1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TileChooser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tile Chooser";
            this.ResumeLayout(false);

        }

        #endregion

        private ItemPicker tilePicker1;
    }
}