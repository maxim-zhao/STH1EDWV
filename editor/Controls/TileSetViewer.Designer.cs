using sth1edwv.GameObjects;

namespace sth1edwv.Controls
{
    partial class TileSetViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TileSetViewer));
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.buttonSaveTileset = new System.Windows.Forms.ToolStripButton();
            this.buttonLoadTileset = new System.Windows.Forms.ToolStripButton();
            this.buttonBlankUnusedTiles = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonAddTile = new System.Windows.Forms.ToolStripButton();
            this.buttonRemoveTile = new System.Windows.Forms.ToolStripButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tilePicker = new sth1edwv.Controls.ItemPicker();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.pictureBoxTilePreview = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBoxTileUsedIn = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxTransparency = new System.Windows.Forms.CheckBox();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTilePreview)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTileUsedIn)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSaveTileset,
            this.buttonLoadTileset,
            this.buttonBlankUnusedTiles,
            this.toolStripSeparator1,
            this.buttonAddTile,
            this.buttonRemoveTile});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(962, 25);
            this.toolStrip2.TabIndex = 4;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // buttonSaveTileset
            // 
            this.buttonSaveTileset.Image = ((System.Drawing.Image)(resources.GetObject("buttonSaveTileset.Image")));
            this.buttonSaveTileset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSaveTileset.Name = "buttonSaveTileset";
            this.buttonSaveTileset.Size = new System.Drawing.Size(60, 22);
            this.buttonSaveTileset.Text = "Save...";
            this.buttonSaveTileset.Click += new System.EventHandler(this.buttonSaveTileset_Click);
            // 
            // buttonLoadTileset
            // 
            this.buttonLoadTileset.Image = ((System.Drawing.Image)(resources.GetObject("buttonLoadTileset.Image")));
            this.buttonLoadTileset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonLoadTileset.Name = "buttonLoadTileset";
            this.buttonLoadTileset.Size = new System.Drawing.Size(62, 22);
            this.buttonLoadTileset.Text = "Load...";
            this.buttonLoadTileset.Click += new System.EventHandler(this.buttonLoadTileset_Click);
            // 
            // buttonBlankUnusedTiles
            // 
            this.buttonBlankUnusedTiles.Image = ((System.Drawing.Image)(resources.GetObject("buttonBlankUnusedTiles.Image")));
            this.buttonBlankUnusedTiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonBlankUnusedTiles.Name = "buttonBlankUnusedTiles";
            this.buttonBlankUnusedTiles.Size = new System.Drawing.Size(98, 22);
            this.buttonBlankUnusedTiles.Text = "Blank unused";
            this.buttonBlankUnusedTiles.Click += new System.EventHandler(this.buttonBlankUnusedTiles_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonAddTile
            // 
            this.buttonAddTile.Image = ((System.Drawing.Image)(resources.GetObject("buttonAddTile.Image")));
            this.buttonAddTile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddTile.Name = "buttonAddTile";
            this.buttonAddTile.Size = new System.Drawing.Size(77, 22);
            this.buttonAddTile.Text = "Add a tile";
            this.buttonAddTile.Click += new System.EventHandler(this.buttonAddTile_Click);
            // 
            // buttonRemoveTile
            // 
            this.buttonRemoveTile.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemoveTile.Image")));
            this.buttonRemoveTile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemoveTile.Name = "buttonRemoveTile";
            this.buttonRemoveTile.Size = new System.Drawing.Size(98, 22);
            this.buttonRemoveTile.Text = "Remove a tile";
            this.buttonRemoveTile.Click += new System.EventHandler(this.buttonRemoveTile_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 25);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tilePicker);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer2.Size = new System.Drawing.Size(962, 453);
            this.splitContainer2.SplitterDistance = 388;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 5;
            // 
            // tilePicker
            // 
            this.tilePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tilePicker.FixedItemsPerRow = true;
            this.tilePicker.ItemsPerRow = 16;
            this.tilePicker.Location = new System.Drawing.Point(0, 0);
            this.tilePicker.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tilePicker.Name = "tilePicker";
            this.tilePicker.Scaling = 1;
            this.tilePicker.SelectedIndex = -1;
            this.tilePicker.ShowTransparency = false;
            this.tilePicker.Size = new System.Drawing.Size(386, 451);
            this.tilePicker.TabIndex = 0;
            this.tilePicker.SelectionChanged += new System.EventHandler<sth1edwv.GameObjects.IDrawableBlock>(this.tilePicker_SelectionChanged);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.pictureBoxTilePreview);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.panel2);
            this.splitContainer5.Panel2.Controls.Add(this.label1);
            this.splitContainer5.Size = new System.Drawing.Size(567, 451);
            this.splitContainer5.SplitterDistance = 322;
            this.splitContainer5.SplitterWidth = 5;
            this.splitContainer5.TabIndex = 1;
            // 
            // pictureBoxTilePreview
            // 
            this.pictureBoxTilePreview.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pictureBoxTilePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxTilePreview.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxTilePreview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBoxTilePreview.Name = "pictureBoxTilePreview";
            this.pictureBoxTilePreview.Size = new System.Drawing.Size(567, 322);
            this.pictureBoxTilePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxTilePreview.TabIndex = 0;
            this.pictureBoxTilePreview.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.pictureBoxTileUsedIn);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 15);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(567, 109);
            this.panel2.TabIndex = 1;
            // 
            // pictureBoxTileUsedIn
            // 
            this.pictureBoxTileUsedIn.Location = new System.Drawing.Point(4, 3);
            this.pictureBoxTileUsedIn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBoxTileUsedIn.Name = "pictureBoxTileUsedIn";
            this.pictureBoxTileUsedIn.Size = new System.Drawing.Size(142, 82);
            this.pictureBoxTileUsedIn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxTileUsedIn.TabIndex = 0;
            this.pictureBoxTileUsedIn.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Used in blocks:";
            // 
            // checkBoxTransparency
            // 
            this.checkBoxTransparency.AutoSize = true;
            this.checkBoxTransparency.Location = new System.Drawing.Point(615, 2);
            this.checkBoxTransparency.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxTransparency.Name = "checkBoxTransparency";
            this.checkBoxTransparency.Size = new System.Drawing.Size(126, 19);
            this.checkBoxTransparency.TabIndex = 6;
            this.checkBoxTransparency.Text = "Show transparency";
            this.checkBoxTransparency.UseVisualStyleBackColor = true;
            this.checkBoxTransparency.CheckedChanged += new System.EventHandler(this.checkBoxTransparency_CheckedChanged);
            // 
            // TileSetViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBoxTransparency);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.toolStrip2);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "TileSetViewer";
            this.Size = new System.Drawing.Size(962, 478);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTilePreview)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTileUsedIn)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton buttonSaveTileset;
        private System.Windows.Forms.ToolStripButton buttonLoadTileset;
        private System.Windows.Forms.ToolStripButton buttonBlankUnusedTiles;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private ItemPicker tilePicker;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.PictureBox pictureBoxTilePreview;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBoxTileUsedIn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxTransparency;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton buttonAddTile;
        private System.Windows.Forms.ToolStripButton buttonRemoveTile;
    }
}
