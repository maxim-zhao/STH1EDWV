﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using sth1edwv.Controls;
using sth1edwv.GameObjects;
using sth1edwv.Properties;

namespace sth1edwv
{
    sealed partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quickTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.listBoxLevels = new System.Windows.Forms.ListBox();
            this.tabControlLevel = new System.Windows.Forms.TabControl();
            this.tabPageMetadata = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.treeViewLevelData = new System.Windows.Forms.TreeView();
            this.propertyGridLevel = new System.Windows.Forms.PropertyGrid();
            this.tabPagePalettes = new System.Windows.Forms.TabPage();
            this.PalettesLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPageTiles = new System.Windows.Forms.TabPage();
            this.tileSetViewer = new sth1edwv.Controls.TileSetViewer();
            this.tabPageSprites = new System.Windows.Forms.TabPage();
            this.spriteTileSetViewer = new sth1edwv.Controls.TileSetViewer();
            this.tabPageBlocks = new System.Windows.Forms.TabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewBlocks = new System.Windows.Forms.DataGridView();
            this.Image = new System.Windows.Forms.DataGridViewImageColumn();
            this.Index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Solidity = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Foreground = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Used = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UsedGlobal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pictureBoxBlockEditor = new System.Windows.Forms.PictureBox();
            this.tabPageLayout = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.floorEditor1 = new sth1edwv.Controls.FloorEditor();
            this.layoutBlockChooser = new sth1edwv.Controls.ItemPicker();
            this.levelEditorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectBlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripLayout = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.buttonShowObjects = new System.Windows.Forms.ToolStripButton();
            this.buttonBlockNumbers = new System.Windows.Forms.ToolStripButton();
            this.buttonBlockGaps = new System.Windows.Forms.ToolStripButton();
            this.buttonTileGaps = new System.Windows.Forms.ToolStripButton();
            this.buttonLevelBounds = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSaveRenderedLevel = new System.Windows.Forms.ToolStripButton();
            this.buttonCopyFloor = new System.Windows.Forms.ToolStripButton();
            this.buttonPasteFloor = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.buttonDraw = new System.Windows.Forms.ToolStripButton();
            this.buttonSelect = new System.Windows.Forms.ToolStripButton();
            this.buttonFloodFill = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonResizeFloor = new System.Windows.Forms.ToolStripButton();
            this.SharingButton = new System.Windows.Forms.ToolStripButton();
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.listBoxGameText = new System.Windows.Forms.ListBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.listBoxArt = new System.Windows.Forms.ListBox();
            this.tabControlArt = new System.Windows.Forms.TabControl();
            this.tabPageArtLayout = new System.Windows.Forms.TabPage();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.pictureBoxArtLayout = new System.Windows.Forms.PictureBox();
            this.tabPageArtTiles = new System.Windows.Forms.TabPage();
            this.otherArtTileSetViewer = new sth1edwv.Controls.TileSetViewer();
            this.tabPageArtPalette = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.floorStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tileSetStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.spriteTileSetStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.tabControlLevel.SuspendLayout();
            this.tabPageMetadata.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabPagePalettes.SuspendLayout();
            this.tabPageTiles.SuspendLayout();
            this.tabPageSprites.SuspendLayout();
            this.tabPageBlocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBlocks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBlockEditor)).BeginInit();
            this.tabPageLayout.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).BeginInit();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            this.levelEditorContextMenu.SuspendLayout();
            this.toolStripLayout.SuspendLayout();
            this.tabPage9.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabControlArt.SuspendLayout();
            this.tabPageArtLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxArtLayout)).BeginInit();
            this.tabPageArtTiles.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1331, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openROMToolStripMenuItem,
            this.saveROMToolStripMenuItem,
            this.quickTestToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openROMToolStripMenuItem
            // 
            this.openROMToolStripMenuItem.Name = "openROMToolStripMenuItem";
            this.openROMToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openROMToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.openROMToolStripMenuItem.Text = "Open ROM...";
            this.openROMToolStripMenuItem.Click += new System.EventHandler(this.openROMToolStripMenuItem_Click);
            // 
            // saveROMToolStripMenuItem
            // 
            this.saveROMToolStripMenuItem.Name = "saveROMToolStripMenuItem";
            this.saveROMToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveROMToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.saveROMToolStripMenuItem.Text = "Save as...";
            this.saveROMToolStripMenuItem.Click += new System.EventHandler(this.saveROMToolStripMenuItem_Click);
            // 
            // quickTestToolStripMenuItem
            // 
            this.quickTestToolStripMenuItem.Name = "quickTestToolStripMenuItem";
            this.quickTestToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.quickTestToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.quickTestToolStripMenuItem.Text = "Quick test";
            this.quickTestToolStripMenuItem.Click += new System.EventHandler(this.quickTestToolStripMenuItem_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.splitContainer6);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1323, 537);
            this.tabPage5.TabIndex = 5;
            this.tabPage5.Text = "Levels";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(3, 3);
            this.splitContainer6.Name = "splitContainer6";
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.listBoxLevels);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.tabControlLevel);
            this.splitContainer6.Size = new System.Drawing.Size(1317, 531);
            this.splitContainer6.SplitterDistance = 302;
            this.splitContainer6.TabIndex = 0;
            // 
            // listBoxLevels
            // 
            this.listBoxLevels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLevels.FormattingEnabled = true;
            this.listBoxLevels.IntegralHeight = false;
            this.listBoxLevels.Location = new System.Drawing.Point(0, 0);
            this.listBoxLevels.Name = "listBoxLevels";
            this.listBoxLevels.Size = new System.Drawing.Size(302, 531);
            this.listBoxLevels.TabIndex = 1;
            this.listBoxLevels.SelectedIndexChanged += new System.EventHandler(this.SelectedLevelChanged);
            // 
            // tabControlLevel
            // 
            this.tabControlLevel.Controls.Add(this.tabPageMetadata);
            this.tabControlLevel.Controls.Add(this.tabPagePalettes);
            this.tabControlLevel.Controls.Add(this.tabPageTiles);
            this.tabControlLevel.Controls.Add(this.tabPageSprites);
            this.tabControlLevel.Controls.Add(this.tabPageBlocks);
            this.tabControlLevel.Controls.Add(this.tabPageLayout);
            this.tabControlLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlLevel.Location = new System.Drawing.Point(0, 0);
            this.tabControlLevel.Name = "tabControlLevel";
            this.tabControlLevel.SelectedIndex = 0;
            this.tabControlLevel.Size = new System.Drawing.Size(1011, 531);
            this.tabControlLevel.TabIndex = 1;
            // 
            // tabPageMetadata
            // 
            this.tabPageMetadata.Controls.Add(this.splitContainer2);
            this.tabPageMetadata.Location = new System.Drawing.Point(4, 22);
            this.tabPageMetadata.Name = "tabPageMetadata";
            this.tabPageMetadata.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMetadata.Size = new System.Drawing.Size(1003, 505);
            this.tabPageMetadata.TabIndex = 0;
            this.tabPageMetadata.Text = "Level metadata";
            this.tabPageMetadata.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.treeViewLevelData);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.propertyGridLevel);
            this.splitContainer2.Size = new System.Drawing.Size(997, 499);
            this.splitContainer2.SplitterDistance = 332;
            this.splitContainer2.TabIndex = 3;
            // 
            // treeViewLevelData
            // 
            this.treeViewLevelData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLevelData.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.treeViewLevelData.Location = new System.Drawing.Point(0, 0);
            this.treeViewLevelData.Name = "treeViewLevelData";
            this.treeViewLevelData.Size = new System.Drawing.Size(332, 499);
            this.treeViewLevelData.TabIndex = 1;
            this.treeViewLevelData.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewLevelDataItemSelected);
            // 
            // propertyGridLevel
            // 
            this.propertyGridLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridLevel.Location = new System.Drawing.Point(0, 0);
            this.propertyGridLevel.Name = "propertyGridLevel";
            this.propertyGridLevel.Size = new System.Drawing.Size(661, 499);
            this.propertyGridLevel.TabIndex = 2;
            this.propertyGridLevel.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridLevel_PropertyValueChanged);
            // 
            // tabPagePalettes
            // 
            this.tabPagePalettes.Controls.Add(this.PalettesLayout);
            this.tabPagePalettes.Location = new System.Drawing.Point(4, 22);
            this.tabPagePalettes.Name = "tabPagePalettes";
            this.tabPagePalettes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePalettes.Size = new System.Drawing.Size(1003, 505);
            this.tabPagePalettes.TabIndex = 4;
            this.tabPagePalettes.Text = "Palettes";
            this.tabPagePalettes.UseVisualStyleBackColor = true;
            // 
            // PalettesLayout
            // 
            this.PalettesLayout.AutoSize = true;
            this.PalettesLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PalettesLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PalettesLayout.Location = new System.Drawing.Point(3, 3);
            this.PalettesLayout.Name = "PalettesLayout";
            this.PalettesLayout.Size = new System.Drawing.Size(997, 499);
            this.PalettesLayout.TabIndex = 0;
            // 
            // tabPageTiles
            // 
            this.tabPageTiles.Controls.Add(this.tileSetViewer);
            this.tabPageTiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageTiles.Name = "tabPageTiles";
            this.tabPageTiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTiles.Size = new System.Drawing.Size(1003, 505);
            this.tabPageTiles.TabIndex = 1;
            this.tabPageTiles.Text = "Tiles";
            this.tabPageTiles.UseVisualStyleBackColor = true;
            // 
            // tileSetViewer
            // 
            this.tileSetViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tileSetViewer.Location = new System.Drawing.Point(3, 3);
            this.tileSetViewer.Name = "tileSetViewer";
            this.tileSetViewer.Size = new System.Drawing.Size(997, 499);
            this.tileSetViewer.TabIndex = 1;
            this.tileSetViewer.TilesPerRow = 16;
            this.tileSetViewer.Changed += new System.Action<sth1edwv.GameObjects.TileSet>(this.tileSetViewer_Changed);
            // 
            // tabPageSprites
            // 
            this.tabPageSprites.Controls.Add(this.spriteTileSetViewer);
            this.tabPageSprites.Location = new System.Drawing.Point(4, 22);
            this.tabPageSprites.Name = "tabPageSprites";
            this.tabPageSprites.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSprites.Size = new System.Drawing.Size(1003, 505);
            this.tabPageSprites.TabIndex = 5;
            this.tabPageSprites.Text = "Sprite tiles";
            this.tabPageSprites.UseVisualStyleBackColor = true;
            // 
            // spriteTileSetViewer
            // 
            this.spriteTileSetViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spriteTileSetViewer.Location = new System.Drawing.Point(3, 3);
            this.spriteTileSetViewer.Name = "spriteTileSetViewer";
            this.spriteTileSetViewer.Size = new System.Drawing.Size(997, 499);
            this.spriteTileSetViewer.TabIndex = 0;
            this.spriteTileSetViewer.TilesPerRow = 16;
            this.spriteTileSetViewer.Changed += new System.Action<sth1edwv.GameObjects.TileSet>(this.spriteTileSetViewer_Changed);
            // 
            // tabPageBlocks
            // 
            this.tabPageBlocks.Controls.Add(this.splitContainer4);
            this.tabPageBlocks.Location = new System.Drawing.Point(4, 22);
            this.tabPageBlocks.Name = "tabPageBlocks";
            this.tabPageBlocks.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBlocks.Size = new System.Drawing.Size(1003, 505);
            this.tabPageBlocks.TabIndex = 3;
            this.tabPageBlocks.Text = "Blocks";
            this.tabPageBlocks.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(3, 3);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.dataGridViewBlocks);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.AutoScroll = true;
            this.splitContainer4.Panel2.Controls.Add(this.pictureBoxBlockEditor);
            this.splitContainer4.Size = new System.Drawing.Size(997, 499);
            this.splitContainer4.SplitterDistance = 614;
            this.splitContainer4.TabIndex = 2;
            // 
            // dataGridViewBlocks
            // 
            this.dataGridViewBlocks.AllowUserToAddRows = false;
            this.dataGridViewBlocks.AllowUserToDeleteRows = false;
            this.dataGridViewBlocks.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridViewBlocks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewBlocks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewBlocks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Image,
            this.Index,
            this.Solidity,
            this.Foreground,
            this.Used,
            this.UsedGlobal});
            this.dataGridViewBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewBlocks.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewBlocks.MultiSelect = false;
            this.dataGridViewBlocks.Name = "dataGridViewBlocks";
            this.dataGridViewBlocks.RowHeadersVisible = false;
            this.dataGridViewBlocks.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridViewBlocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewBlocks.Size = new System.Drawing.Size(614, 499);
            this.dataGridViewBlocks.TabIndex = 2;
            this.dataGridViewBlocks.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridViewBlocks_CellPainting);
            this.dataGridViewBlocks.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewBlocks_DataError);
            this.dataGridViewBlocks.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridViewBlocks_EditingControlShowing);
            this.dataGridViewBlocks.SelectionChanged += new System.EventHandler(this.SelectedBlockChanged);
            // 
            // Image
            // 
            this.Image.DataPropertyName = "Image";
            this.Image.HeaderText = "Image";
            this.Image.Name = "Image";
            this.Image.ReadOnly = true;
            // 
            // Index
            // 
            this.Index.DataPropertyName = "Index";
            dataGridViewCellStyle1.Format = "X2";
            this.Index.DefaultCellStyle = dataGridViewCellStyle1;
            this.Index.HeaderText = "Index";
            this.Index.Name = "Index";
            this.Index.ReadOnly = true;
            // 
            // Solidity
            // 
            this.Solidity.DataPropertyName = "SolidityIndex";
            this.Solidity.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.Solidity.HeaderText = "Solidity";
            this.Solidity.Name = "Solidity";
            this.Solidity.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // Foreground
            // 
            this.Foreground.DataPropertyName = "IsForeground";
            this.Foreground.HeaderText = "Foreground";
            this.Foreground.Name = "Foreground";
            // 
            // Used
            // 
            this.Used.DataPropertyName = "UsageCount";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Used.DefaultCellStyle = dataGridViewCellStyle2;
            this.Used.HeaderText = "Used";
            this.Used.Name = "Used";
            this.Used.ReadOnly = true;
            // 
            // UsedGlobal
            // 
            this.UsedGlobal.DataPropertyName = "GlobalUsageCount";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.UsedGlobal.DefaultCellStyle = dataGridViewCellStyle3;
            this.UsedGlobal.HeaderText = "Total used";
            this.UsedGlobal.Name = "UsedGlobal";
            this.UsedGlobal.ReadOnly = true;
            // 
            // pictureBoxBlockEditor
            // 
            this.pictureBoxBlockEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxBlockEditor.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxBlockEditor.Name = "pictureBoxBlockEditor";
            this.pictureBoxBlockEditor.Size = new System.Drawing.Size(379, 499);
            this.pictureBoxBlockEditor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxBlockEditor.TabIndex = 3;
            this.pictureBoxBlockEditor.TabStop = false;
            this.pictureBoxBlockEditor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.BlockEditorMouseClick);
            // 
            // tabPageLayout
            // 
            this.tabPageLayout.Controls.Add(this.panel1);
            this.tabPageLayout.Controls.Add(this.toolStripLayout);
            this.tabPageLayout.Location = new System.Drawing.Point(4, 22);
            this.tabPageLayout.Name = "tabPageLayout";
            this.tabPageLayout.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLayout.Size = new System.Drawing.Size(1003, 505);
            this.tabPageLayout.TabIndex = 2;
            this.tabPageLayout.Text = "Layout";
            this.tabPageLayout.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.splitContainer7);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 49);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(997, 453);
            this.panel1.TabIndex = 4;
            // 
            // splitContainer7
            // 
            this.splitContainer7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer7.Location = new System.Drawing.Point(0, 0);
            this.splitContainer7.Name = "splitContainer7";
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.AutoScroll = true;
            this.splitContainer7.Panel1.Controls.Add(this.floorEditor1);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.layoutBlockChooser);
            this.splitContainer7.Size = new System.Drawing.Size(997, 453);
            this.splitContainer7.SplitterDistance = 705;
            this.splitContainer7.TabIndex = 3;
            // 
            // floorEditor1
            // 
            this.floorEditor1.BlockChooser = this.layoutBlockChooser;
            this.floorEditor1.BlockGaps = false;
            this.floorEditor1.BlockNumbers = true;
            this.floorEditor1.ContextMenuStrip = this.levelEditorContextMenu;
            this.floorEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.floorEditor1.DrawingMode = sth1edwv.Controls.FloorEditor.Modes.Draw;
            this.floorEditor1.LastClickedBlockIndex = 0;
            this.floorEditor1.LevelBounds = false;
            this.floorEditor1.Location = new System.Drawing.Point(0, 0);
            this.floorEditor1.Name = "floorEditor1";
            this.floorEditor1.Size = new System.Drawing.Size(705, 453);
            this.floorEditor1.TabIndex = 0;
            this.floorEditor1.Text = "floorEditor1";
            this.floorEditor1.TileGaps = false;
            this.floorEditor1.WithObjects = true;
            this.floorEditor1.FloorChanged += new System.Action(this.floorEditor1_FloorChanged);
            // 
            // layoutBlockChooser
            // 
            this.layoutBlockChooser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutBlockChooser.FixedItemsPerRow = false;
            this.layoutBlockChooser.ItemsPerRow = 0;
            this.layoutBlockChooser.Location = new System.Drawing.Point(0, 0);
            this.layoutBlockChooser.Name = "layoutBlockChooser";
            this.layoutBlockChooser.Scaling = 1;
            this.layoutBlockChooser.SelectedIndex = -1;
            this.layoutBlockChooser.ShowTransparency = false;
            this.layoutBlockChooser.Size = new System.Drawing.Size(288, 453);
            this.layoutBlockChooser.TabIndex = 0;
            // 
            // levelEditorContextMenu
            // 
            this.levelEditorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectBlockToolStripMenuItem,
            this.editObjectToolStripMenuItem});
            this.levelEditorContextMenu.Name = "levelEditorContextMenu";
            this.levelEditorContextMenu.Size = new System.Drawing.Size(140, 48);
            // 
            // selectBlockToolStripMenuItem
            // 
            this.selectBlockToolStripMenuItem.Name = "selectBlockToolStripMenuItem";
            this.selectBlockToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.selectBlockToolStripMenuItem.Text = "Select block";
            this.selectBlockToolStripMenuItem.Click += new System.EventHandler(this.selectBlockToolStripMenuItem_Click);
            // 
            // editObjectToolStripMenuItem
            // 
            this.editObjectToolStripMenuItem.Name = "editObjectToolStripMenuItem";
            this.editObjectToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.editObjectToolStripMenuItem.Text = "Edit object...";
            this.editObjectToolStripMenuItem.Click += new System.EventHandler(this.editObjectToolStripMenuItem_Click);
            // 
            // toolStripLayout
            // 
            this.toolStripLayout.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.buttonShowObjects,
            this.buttonBlockNumbers,
            this.buttonBlockGaps,
            this.buttonTileGaps,
            this.buttonLevelBounds,
            this.toolStripSeparator1,
            this.toolStripButtonSaveRenderedLevel,
            this.buttonCopyFloor,
            this.buttonPasteFloor,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.buttonDraw,
            this.buttonSelect,
            this.buttonFloodFill,
            this.toolStripSeparator3,
            this.buttonResizeFloor,
            this.SharingButton});
            this.toolStripLayout.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStripLayout.Location = new System.Drawing.Point(3, 3);
            this.toolStripLayout.Name = "toolStripLayout";
            this.toolStripLayout.Size = new System.Drawing.Size(997, 46);
            this.toolStripLayout.TabIndex = 3;
            this.toolStripLayout.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(41, 15);
            this.toolStripLabel1.Text = "Show:";
            // 
            // buttonShowObjects
            // 
            this.buttonShowObjects.Checked = true;
            this.buttonShowObjects.CheckOnClick = true;
            this.buttonShowObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.buttonShowObjects.Image = global::sth1edwv.Properties.Resources.package;
            this.buttonShowObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonShowObjects.Name = "buttonShowObjects";
            this.buttonShowObjects.Size = new System.Drawing.Size(67, 20);
            this.buttonShowObjects.Text = "Objects";
            this.buttonShowObjects.CheckedChanged += new System.EventHandler(this.LevelRenderModeChanged);
            // 
            // buttonBlockNumbers
            // 
            this.buttonBlockNumbers.Checked = true;
            this.buttonBlockNumbers.CheckOnClick = true;
            this.buttonBlockNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.buttonBlockNumbers.Image = ((System.Drawing.Image)(resources.GetObject("buttonBlockNumbers.Image")));
            this.buttonBlockNumbers.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonBlockNumbers.Name = "buttonBlockNumbers";
            this.buttonBlockNumbers.Size = new System.Drawing.Size(106, 20);
            this.buttonBlockNumbers.Text = "Block numbers";
            this.buttonBlockNumbers.CheckedChanged += new System.EventHandler(this.LevelRenderModeChanged);
            // 
            // buttonBlockGaps
            // 
            this.buttonBlockGaps.CheckOnClick = true;
            this.buttonBlockGaps.Image = ((System.Drawing.Image)(resources.GetObject("buttonBlockGaps.Image")));
            this.buttonBlockGaps.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonBlockGaps.Name = "buttonBlockGaps";
            this.buttonBlockGaps.Size = new System.Drawing.Size(84, 20);
            this.buttonBlockGaps.Text = "Block gaps";
            this.buttonBlockGaps.CheckedChanged += new System.EventHandler(this.LevelRenderModeChanged);
            // 
            // buttonTileGaps
            // 
            this.buttonTileGaps.CheckOnClick = true;
            this.buttonTileGaps.Image = ((System.Drawing.Image)(resources.GetObject("buttonTileGaps.Image")));
            this.buttonTileGaps.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonTileGaps.Name = "buttonTileGaps";
            this.buttonTileGaps.Size = new System.Drawing.Size(73, 20);
            this.buttonTileGaps.Text = "Tile gaps";
            this.buttonTileGaps.CheckedChanged += new System.EventHandler(this.LevelRenderModeChanged);
            // 
            // buttonLevelBounds
            // 
            this.buttonLevelBounds.CheckOnClick = true;
            this.buttonLevelBounds.Image = ((System.Drawing.Image)(resources.GetObject("buttonLevelBounds.Image")));
            this.buttonLevelBounds.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonLevelBounds.Name = "buttonLevelBounds";
            this.buttonLevelBounds.Size = new System.Drawing.Size(97, 20);
            this.buttonLevelBounds.Text = "Level bounds";
            this.buttonLevelBounds.CheckedChanged += new System.EventHandler(this.LevelRenderModeChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonSaveRenderedLevel
            // 
            this.toolStripButtonSaveRenderedLevel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSaveRenderedLevel.Image")));
            this.toolStripButtonSaveRenderedLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSaveRenderedLevel.Name = "toolStripButtonSaveRenderedLevel";
            this.toolStripButtonSaveRenderedLevel.Size = new System.Drawing.Size(60, 20);
            this.toolStripButtonSaveRenderedLevel.Text = "Save...";
            this.toolStripButtonSaveRenderedLevel.Click += new System.EventHandler(this.toolStripButtonSaveRenderedLevel_Click);
            // 
            // buttonCopyFloor
            // 
            this.buttonCopyFloor.Image = ((System.Drawing.Image)(resources.GetObject("buttonCopyFloor.Image")));
            this.buttonCopyFloor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonCopyFloor.Name = "buttonCopyFloor";
            this.buttonCopyFloor.Size = new System.Drawing.Size(55, 20);
            this.buttonCopyFloor.Text = "Copy";
            this.buttonCopyFloor.Click += new System.EventHandler(this.buttonCopyFloor_Click);
            // 
            // buttonPasteFloor
            // 
            this.buttonPasteFloor.Image = ((System.Drawing.Image)(resources.GetObject("buttonPasteFloor.Image")));
            this.buttonPasteFloor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPasteFloor.Name = "buttonPasteFloor";
            this.buttonPasteFloor.Size = new System.Drawing.Size(55, 20);
            this.buttonPasteFloor.Text = "Paste";
            this.buttonPasteFloor.Click += new System.EventHandler(this.buttonPasteFloor_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(57, 15);
            this.toolStripLabel2.Text = "Drawing:";
            // 
            // buttonDraw
            // 
            this.buttonDraw.Checked = true;
            this.buttonDraw.CheckState = System.Windows.Forms.CheckState.Checked;
            this.buttonDraw.Image = ((System.Drawing.Image)(resources.GetObject("buttonDraw.Image")));
            this.buttonDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonDraw.Name = "buttonDraw";
            this.buttonDraw.Size = new System.Drawing.Size(54, 20);
            this.buttonDraw.Text = "Draw";
            this.buttonDraw.Click += new System.EventHandler(this.DrawingButtonCheckedChanged);
            // 
            // buttonSelect
            // 
            this.buttonSelect.Image = ((System.Drawing.Image)(resources.GetObject("buttonSelect.Image")));
            this.buttonSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(58, 20);
            this.buttonSelect.Text = "Select";
            this.buttonSelect.Click += new System.EventHandler(this.DrawingButtonCheckedChanged);
            // 
            // buttonFloodFill
            // 
            this.buttonFloodFill.Image = ((System.Drawing.Image)(resources.GetObject("buttonFloodFill.Image")));
            this.buttonFloodFill.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonFloodFill.Name = "buttonFloodFill";
            this.buttonFloodFill.Size = new System.Drawing.Size(73, 20);
            this.buttonFloodFill.Text = "Flood fill";
            this.buttonFloodFill.Click += new System.EventHandler(this.DrawingButtonCheckedChanged);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
            // 
            // buttonResizeFloor
            // 
            this.buttonResizeFloor.Image = ((System.Drawing.Image)(resources.GetObject("buttonResizeFloor.Image")));
            this.buttonResizeFloor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonResizeFloor.Name = "buttonResizeFloor";
            this.buttonResizeFloor.Size = new System.Drawing.Size(68, 20);
            this.buttonResizeFloor.Text = "Resize...";
            this.buttonResizeFloor.Click += new System.EventHandler(this.ResizeFloorButtonClick);
            // 
            // SharingButton
            // 
            this.SharingButton.Image = ((System.Drawing.Image)(resources.GetObject("SharingButton.Image")));
            this.SharingButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SharingButton.Name = "SharingButton";
            this.SharingButton.Size = new System.Drawing.Size(76, 20);
            this.SharingButton.Text = "Sharing...";
            this.SharingButton.Click += new System.EventHandler(this.SharingButton_Click);
            // 
            // tabPage9
            // 
            this.tabPage9.Controls.Add(this.listBoxGameText);
            this.tabPage9.Location = new System.Drawing.Point(4, 22);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage9.Size = new System.Drawing.Size(1323, 537);
            this.tabPage9.TabIndex = 7;
            this.tabPage9.Text = "Game Text";
            this.tabPage9.UseVisualStyleBackColor = true;
            // 
            // listBoxGameText
            // 
            this.listBoxGameText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxGameText.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.listBoxGameText.FormattingEnabled = true;
            this.listBoxGameText.IntegralHeight = false;
            this.listBoxGameText.Location = new System.Drawing.Point(3, 3);
            this.listBoxGameText.Name = "listBoxGameText";
            this.listBoxGameText.Size = new System.Drawing.Size(1317, 531);
            this.listBoxGameText.TabIndex = 1;
            this.listBoxGameText.DoubleClick += new System.EventHandler(this.GameTextDoubleClicked);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage9);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1331, 563);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1323, 537);
            this.tabPage1.TabIndex = 9;
            this.tabPage1.Text = "Other art";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(3, 3);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.listBoxArt);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tabControlArt);
            this.splitContainer3.Size = new System.Drawing.Size(1317, 531);
            this.splitContainer3.SplitterDistance = 261;
            this.splitContainer3.TabIndex = 1;
            // 
            // listBoxArt
            // 
            this.listBoxArt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxArt.FormattingEnabled = true;
            this.listBoxArt.Location = new System.Drawing.Point(0, 0);
            this.listBoxArt.Name = "listBoxArt";
            this.listBoxArt.Size = new System.Drawing.Size(261, 531);
            this.listBoxArt.TabIndex = 0;
            this.listBoxArt.SelectedIndexChanged += new System.EventHandler(this.listBoxArt_SelectedIndexChanged);
            // 
            // tabControlArt
            // 
            this.tabControlArt.Controls.Add(this.tabPageArtLayout);
            this.tabControlArt.Controls.Add(this.tabPageArtTiles);
            this.tabControlArt.Controls.Add(this.tabPageArtPalette);
            this.tabControlArt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlArt.Location = new System.Drawing.Point(0, 0);
            this.tabControlArt.Name = "tabControlArt";
            this.tabControlArt.SelectedIndex = 0;
            this.tabControlArt.Size = new System.Drawing.Size(1052, 531);
            this.tabControlArt.TabIndex = 1;
            // 
            // tabPageArtLayout
            // 
            this.tabPageArtLayout.Controls.Add(this.propertyGrid1);
            this.tabPageArtLayout.Controls.Add(this.pictureBoxArtLayout);
            this.tabPageArtLayout.Location = new System.Drawing.Point(4, 22);
            this.tabPageArtLayout.Name = "tabPageArtLayout";
            this.tabPageArtLayout.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageArtLayout.Size = new System.Drawing.Size(1044, 505);
            this.tabPageArtLayout.TabIndex = 2;
            this.tabPageArtLayout.Text = "Layout";
            this.tabPageArtLayout.UseVisualStyleBackColor = true;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
            this.propertyGrid1.Location = new System.Drawing.Point(737, 3);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(304, 499);
            this.propertyGrid1.TabIndex = 1;
            // 
            // pictureBoxArtLayout
            // 
            this.pictureBoxArtLayout.Location = new System.Drawing.Point(6, 6);
            this.pictureBoxArtLayout.Name = "pictureBoxArtLayout";
            this.pictureBoxArtLayout.Size = new System.Drawing.Size(317, 194);
            this.pictureBoxArtLayout.TabIndex = 0;
            this.pictureBoxArtLayout.TabStop = false;
            // 
            // tabPageArtTiles
            // 
            this.tabPageArtTiles.Controls.Add(this.otherArtTileSetViewer);
            this.tabPageArtTiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageArtTiles.Name = "tabPageArtTiles";
            this.tabPageArtTiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageArtTiles.Size = new System.Drawing.Size(1044, 505);
            this.tabPageArtTiles.TabIndex = 0;
            this.tabPageArtTiles.Text = "Tiles";
            this.tabPageArtTiles.UseVisualStyleBackColor = true;
            // 
            // otherArtTileSetViewer
            // 
            this.otherArtTileSetViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.otherArtTileSetViewer.Location = new System.Drawing.Point(3, 3);
            this.otherArtTileSetViewer.Name = "otherArtTileSetViewer";
            this.otherArtTileSetViewer.Size = new System.Drawing.Size(1038, 499);
            this.otherArtTileSetViewer.TabIndex = 0;
            this.otherArtTileSetViewer.TilesPerRow = 4;
            // 
            // tabPageArtPalette
            // 
            this.tabPageArtPalette.Location = new System.Drawing.Point(4, 22);
            this.tabPageArtPalette.Name = "tabPageArtPalette";
            this.tabPageArtPalette.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageArtPalette.Size = new System.Drawing.Size(1044, 505);
            this.tabPageArtPalette.TabIndex = 1;
            this.tabPageArtPalette.Text = "Palette";
            this.tabPageArtPalette.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.logTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1323, 537);
            this.tabPage2.TabIndex = 10;
            this.tabPage2.Text = "Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Location = new System.Drawing.Point(3, 3);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logTextBox.Size = new System.Drawing.Size(1317, 531);
            this.logTextBox.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.floorStatus,
            this.tileSetStatus,
            this.spriteTileSetStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 587);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1331, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // floorStatus
            // 
            this.floorStatus.Name = "floorStatus";
            this.floorStatus.Size = new System.Drawing.Size(70, 17);
            this.floorStatus.Text = "Floor space:";
            // 
            // tileSetStatus
            // 
            this.tileSetStatus.Name = "tileSetStatus";
            this.tileSetStatus.Size = new System.Drawing.Size(76, 17);
            this.tileSetStatus.Text = "Tileset space:";
            // 
            // spriteTileSetStatus
            // 
            this.spriteTileSetStatus.Name = "spriteTileSetStatus";
            this.spriteTileSetStatus.Size = new System.Drawing.Size(107, 17);
            this.spriteTileSetStatus.Text = "Sprite tileset space:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1331, 609);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "STH1 Editor by WV :: extended by Maxim";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            this.tabControlLevel.ResumeLayout(false);
            this.tabPageMetadata.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabPagePalettes.ResumeLayout(false);
            this.tabPagePalettes.PerformLayout();
            this.tabPageTiles.ResumeLayout(false);
            this.tabPageSprites.ResumeLayout(false);
            this.tabPageBlocks.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBlocks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBlockEditor)).EndInit();
            this.tabPageLayout.ResumeLayout(false);
            this.tabPageLayout.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).EndInit();
            this.splitContainer7.ResumeLayout(false);
            this.levelEditorContextMenu.ResumeLayout(false);
            this.toolStripLayout.ResumeLayout(false);
            this.toolStripLayout.PerformLayout();
            this.tabPage9.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.tabControlArt.ResumeLayout(false);
            this.tabPageArtLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxArtLayout)).EndInit();
            this.tabPageArtTiles.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openROMToolStripMenuItem;
        private ToolStripMenuItem saveROMToolStripMenuItem;
        private ToolStripMenuItem quickTestToolStripMenuItem;
        private TabPage tabPage5;
        private SplitContainer splitContainer6;
        private ListBox listBoxLevels;
        private TabControl tabControlLevel;
        private TabPage tabPageMetadata;
        private PropertyGrid propertyGridLevel;
        private TreeView treeViewLevelData;
        private TabPage tabPagePalettes;
        private TabPage tabPageTiles;
        private TabPage tabPageBlocks;
        private SplitContainer splitContainer4;
        private DataGridView dataGridViewBlocks;
        private DataGridViewImageColumn Image;
        private DataGridViewTextBoxColumn Index;
        private DataGridViewComboBoxColumn Solidity;
        private DataGridViewCheckBoxColumn Foreground;
        private DataGridViewTextBoxColumn Used;
        private DataGridViewTextBoxColumn UsedGlobal;
        private PictureBox pictureBoxBlockEditor;
        private TabPage tabPageLayout;
        private Panel panel1;
        private ToolStrip toolStripLayout;
        private ToolStripButton buttonShowObjects;
        private ToolStripButton buttonBlockNumbers;
        private ToolStripButton buttonBlockGaps;
        private ToolStripButton buttonTileGaps;
        private ToolStripButton buttonLevelBounds;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButtonSaveRenderedLevel;
        private ToolStripButton buttonCopyFloor;
        private ToolStripButton buttonPasteFloor;
        private TabPage tabPage9;
        private ListBox listBoxGameText;
        private TabControl tabControl1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel floorStatus;
        private ToolStripStatusLabel tileSetStatus;
        private SplitContainer splitContainer7;
        private ItemPicker layoutBlockChooser;
        private ToolStripLabel toolStripLabel1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripLabel toolStripLabel2;
        private ToolStripButton buttonDraw;
        private ToolStripButton buttonSelect;
        private FloorEditor floorEditor1;
        private TabPage tabPageSprites;
        private TileSetViewer spriteTileSetViewer;
        private TileSetViewer tileSetViewer;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton buttonResizeFloor;
        private ToolStripButton buttonFloodFill;
        private ToolStripButton SharingButton;
        private FlowLayoutPanel PalettesLayout;
        private SplitContainer splitContainer2;
        private ContextMenuStrip levelEditorContextMenu;
        private ToolStripMenuItem selectBlockToolStripMenuItem;
        private ToolStripMenuItem editObjectToolStripMenuItem;
        private TabPage tabPage1;
        private TileSetViewer otherArtTileSetViewer;
        private SplitContainer splitContainer3;
        private ListBox listBoxArt;
        private TabControl tabControlArt;
        private TabPage tabPageArtTiles;
        private TabPage tabPageArtPalette;
        private TabPage tabPageArtLayout;
        private PictureBox pictureBoxArtLayout;
        private PropertyGrid propertyGrid1;
        private ToolStripStatusLabel spriteTileSetStatus;
        private TabPage tabPage2;
        private TextBox logTextBox;
    }
}

