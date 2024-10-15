using System;
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
            components = new Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openROMToolStripMenuItem = new ToolStripMenuItem();
            saveROMToolStripMenuItem = new ToolStripMenuItem();
            quickTestToolStripMenuItem = new ToolStripMenuItem();
            tabPage5 = new TabPage();
            splitContainer6 = new SplitContainer();
            listBoxLevels = new ListBox();
            tabControlLevel = new TabControl();
            tabPageMetadata = new TabPage();
            propertyGridLevel = new PropertyGrid();
            tabPagePalettes = new TabPage();
            PalettesLayout = new FlowLayoutPanel();
            tabPageTiles = new TabPage();
            tileSetViewer = new TileSetViewer();
            tabPageSprites = new TabPage();
            spriteTileSetViewer = new TileSetViewer();
            tabPageBlocks = new TabPage();
            splitContainer4 = new SplitContainer();
            dataGridViewBlocks = new DataGridView();
            Image = new DataGridViewImageColumn();
            Index = new DataGridViewTextBoxColumn();
            Solidity = new DataGridViewComboBoxColumn();
            Foreground = new DataGridViewCheckBoxColumn();
            Used = new DataGridViewTextBoxColumn();
            UsedGlobal = new DataGridViewTextBoxColumn();
            pictureBoxBlockEditor = new PictureBox();
            tabPageLayout = new TabPage();
            panel1 = new Panel();
            splitContainer7 = new SplitContainer();
            floorEditor1 = new FloorEditor();
            layoutBlockChooser = new ItemPicker();
            levelEditorContextMenu = new ContextMenuStrip(components);
            selectBlockToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            editObjectToolStripMenuItem = new ToolStripMenuItem();
            addObjectToolStripMenuItem = new ToolStripMenuItem();
            deleteObjectToolStripMenuItem = new ToolStripMenuItem();
            toolStripLayout = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            buttonShowObjects = new ToolStripButton();
            buttonBlockNumbers = new ToolStripButton();
            buttonBlockGaps = new ToolStripButton();
            buttonTileGaps = new ToolStripButton();
            buttonLevelBounds = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButtonSaveRenderedLevel = new ToolStripButton();
            buttonCopyFloor = new ToolStripButton();
            buttonPasteFloor = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripLabel2 = new ToolStripLabel();
            buttonDraw = new ToolStripButton();
            buttonSelect = new ToolStripButton();
            buttonFloodFill = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            buttonResizeFloor = new ToolStripButton();
            SharingButton = new ToolStripButton();
            comboZoom = new ToolStripComboBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer3 = new SplitContainer();
            listBoxArt = new ListBox();
            tabControlArt = new TabControl();
            tabPageArtLayout = new TabPage();
            toolStrip2 = new ToolStrip();
            buttonSaveScreen = new ToolStripButton();
            buttonLoadTileset = new ToolStripButton();
            pictureBoxArtLayout = new PictureBox();
            tabPageArtTiles = new TabPage();
            otherArtTileSetViewer = new TileSetViewer();
            tabPageArtPalette = new TabPage();
            palettesLayoutPanel = new FlowLayoutPanel();
            tabPageExtraData = new TabPage();
            extraDataLayoutPanel = new FlowLayoutPanel();
            tabPage3 = new TabPage();
            udSDSCVersion = new NumericUpDown();
            label5 = new Label();
            dateTimePickerSDSCDate = new DateTimePicker();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            textBoxSDSCTitle = new TextBox();
            textBoxSDSCNotes = new TextBox();
            textBoxSDSCAuthor = new TextBox();
            tabPage2 = new TabPage();
            logTextBox = new TextBox();
            spaceVisualizer1 = new SpaceVisualizer();
            splitContainer1 = new SplitContainer();
            menuStrip1.SuspendLayout();
            tabPage5.SuspendLayout();
            ((ISupportInitialize)splitContainer6).BeginInit();
            splitContainer6.Panel1.SuspendLayout();
            splitContainer6.Panel2.SuspendLayout();
            splitContainer6.SuspendLayout();
            tabControlLevel.SuspendLayout();
            tabPageMetadata.SuspendLayout();
            tabPagePalettes.SuspendLayout();
            tabPageTiles.SuspendLayout();
            tabPageSprites.SuspendLayout();
            tabPageBlocks.SuspendLayout();
            ((ISupportInitialize)splitContainer4).BeginInit();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();
            splitContainer4.SuspendLayout();
            ((ISupportInitialize)dataGridViewBlocks).BeginInit();
            ((ISupportInitialize)pictureBoxBlockEditor).BeginInit();
            tabPageLayout.SuspendLayout();
            panel1.SuspendLayout();
            ((ISupportInitialize)splitContainer7).BeginInit();
            splitContainer7.Panel1.SuspendLayout();
            splitContainer7.Panel2.SuspendLayout();
            splitContainer7.SuspendLayout();
            levelEditorContextMenu.SuspendLayout();
            toolStripLayout.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            tabControlArt.SuspendLayout();
            tabPageArtLayout.SuspendLayout();
            toolStrip2.SuspendLayout();
            ((ISupportInitialize)pictureBoxArtLayout).BeginInit();
            tabPageArtTiles.SuspendLayout();
            tabPageArtPalette.SuspendLayout();
            tabPageExtraData.SuspendLayout();
            tabPage3.SuspendLayout();
            ((ISupportInitialize)udSDSCVersion).BeginInit();
            tabPage2.SuspendLayout();
            ((ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1331, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openROMToolStripMenuItem, saveROMToolStripMenuItem, quickTestToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openROMToolStripMenuItem
            // 
            openROMToolStripMenuItem.Name = "openROMToolStripMenuItem";
            openROMToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openROMToolStripMenuItem.Size = new Size(185, 22);
            openROMToolStripMenuItem.Text = "Open ROM...";
            openROMToolStripMenuItem.Click += openROMToolStripMenuItem_Click;
            // 
            // saveROMToolStripMenuItem
            // 
            saveROMToolStripMenuItem.Name = "saveROMToolStripMenuItem";
            saveROMToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveROMToolStripMenuItem.Size = new Size(185, 22);
            saveROMToolStripMenuItem.Text = "Save as...";
            saveROMToolStripMenuItem.Click += saveROMToolStripMenuItem_Click;
            // 
            // quickTestToolStripMenuItem
            // 
            quickTestToolStripMenuItem.Name = "quickTestToolStripMenuItem";
            quickTestToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Q;
            quickTestToolStripMenuItem.Size = new Size(185, 22);
            quickTestToolStripMenuItem.Text = "Quick test";
            quickTestToolStripMenuItem.Click += quickTestToolStripMenuItem_Click;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(splitContainer6);
            tabPage5.Location = new Point(4, 22);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(1323, 530);
            tabPage5.TabIndex = 5;
            tabPage5.Text = "Levels";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // splitContainer6
            // 
            splitContainer6.Dock = DockStyle.Fill;
            splitContainer6.Location = new Point(3, 3);
            splitContainer6.Name = "splitContainer6";
            // 
            // splitContainer6.Panel1
            // 
            splitContainer6.Panel1.Controls.Add(listBoxLevels);
            // 
            // splitContainer6.Panel2
            // 
            splitContainer6.Panel2.Controls.Add(tabControlLevel);
            splitContainer6.Size = new Size(1317, 524);
            splitContainer6.SplitterDistance = 301;
            splitContainer6.TabIndex = 0;
            // 
            // listBoxLevels
            // 
            listBoxLevels.Dock = DockStyle.Fill;
            listBoxLevels.FormattingEnabled = true;
            listBoxLevels.IntegralHeight = false;
            listBoxLevels.ItemHeight = 13;
            listBoxLevels.Location = new Point(0, 0);
            listBoxLevels.Name = "listBoxLevels";
            listBoxLevels.Size = new Size(301, 524);
            listBoxLevels.TabIndex = 1;
            listBoxLevels.SelectedIndexChanged += SelectedLevelChanged;
            // 
            // tabControlLevel
            // 
            tabControlLevel.Controls.Add(tabPageMetadata);
            tabControlLevel.Controls.Add(tabPagePalettes);
            tabControlLevel.Controls.Add(tabPageTiles);
            tabControlLevel.Controls.Add(tabPageSprites);
            tabControlLevel.Controls.Add(tabPageBlocks);
            tabControlLevel.Controls.Add(tabPageLayout);
            tabControlLevel.Dock = DockStyle.Fill;
            tabControlLevel.Location = new Point(0, 0);
            tabControlLevel.Name = "tabControlLevel";
            tabControlLevel.SelectedIndex = 0;
            tabControlLevel.Size = new Size(1012, 524);
            tabControlLevel.TabIndex = 1;
            // 
            // tabPageMetadata
            // 
            tabPageMetadata.Controls.Add(propertyGridLevel);
            tabPageMetadata.Location = new Point(4, 22);
            tabPageMetadata.Name = "tabPageMetadata";
            tabPageMetadata.Padding = new Padding(3);
            tabPageMetadata.Size = new Size(1004, 498);
            tabPageMetadata.TabIndex = 0;
            tabPageMetadata.Text = "Level metadata";
            tabPageMetadata.UseVisualStyleBackColor = true;
            // 
            // propertyGridLevel
            // 
            propertyGridLevel.Dock = DockStyle.Fill;
            propertyGridLevel.Location = new Point(3, 3);
            propertyGridLevel.Name = "propertyGridLevel";
            propertyGridLevel.Size = new Size(998, 492);
            propertyGridLevel.TabIndex = 2;
            propertyGridLevel.PropertyValueChanged += propertyGridLevel_PropertyValueChanged;
            // 
            // tabPagePalettes
            // 
            tabPagePalettes.Controls.Add(PalettesLayout);
            tabPagePalettes.Location = new Point(4, 24);
            tabPagePalettes.Name = "tabPagePalettes";
            tabPagePalettes.Padding = new Padding(3);
            tabPagePalettes.Size = new Size(1004, 496);
            tabPagePalettes.TabIndex = 4;
            tabPagePalettes.Text = "Palettes";
            tabPagePalettes.UseVisualStyleBackColor = true;
            // 
            // PalettesLayout
            // 
            PalettesLayout.AutoSize = true;
            PalettesLayout.Dock = DockStyle.Fill;
            PalettesLayout.FlowDirection = FlowDirection.TopDown;
            PalettesLayout.Location = new Point(3, 3);
            PalettesLayout.Name = "PalettesLayout";
            PalettesLayout.Size = new Size(998, 490);
            PalettesLayout.TabIndex = 0;
            // 
            // tabPageTiles
            // 
            tabPageTiles.Controls.Add(tileSetViewer);
            tabPageTiles.Location = new Point(4, 24);
            tabPageTiles.Name = "tabPageTiles";
            tabPageTiles.Padding = new Padding(3);
            tabPageTiles.Size = new Size(1004, 496);
            tabPageTiles.TabIndex = 1;
            tabPageTiles.Text = "Tiles";
            tabPageTiles.UseVisualStyleBackColor = true;
            // 
            // tileSetViewer
            // 
            tileSetViewer.Dock = DockStyle.Fill;
            tileSetViewer.Location = new Point(3, 3);
            tileSetViewer.Margin = new Padding(4, 3, 4, 3);
            tileSetViewer.Name = "tileSetViewer";
            tileSetViewer.Size = new Size(998, 490);
            tileSetViewer.TabIndex = 1;
            tileSetViewer.TilesPerRow = 16;
            tileSetViewer.Changed += tileSetViewer_Changed;
            // 
            // tabPageSprites
            // 
            tabPageSprites.Controls.Add(spriteTileSetViewer);
            tabPageSprites.Location = new Point(4, 24);
            tabPageSprites.Name = "tabPageSprites";
            tabPageSprites.Padding = new Padding(3);
            tabPageSprites.Size = new Size(1004, 496);
            tabPageSprites.TabIndex = 5;
            tabPageSprites.Text = "Sprite tiles";
            tabPageSprites.UseVisualStyleBackColor = true;
            // 
            // spriteTileSetViewer
            // 
            spriteTileSetViewer.Dock = DockStyle.Fill;
            spriteTileSetViewer.Location = new Point(3, 3);
            spriteTileSetViewer.Margin = new Padding(4, 3, 4, 3);
            spriteTileSetViewer.Name = "spriteTileSetViewer";
            spriteTileSetViewer.Size = new Size(998, 490);
            spriteTileSetViewer.TabIndex = 0;
            spriteTileSetViewer.TilesPerRow = 16;
            spriteTileSetViewer.Changed += spriteTileSetViewer_Changed;
            // 
            // tabPageBlocks
            // 
            tabPageBlocks.Controls.Add(splitContainer4);
            tabPageBlocks.Location = new Point(4, 24);
            tabPageBlocks.Name = "tabPageBlocks";
            tabPageBlocks.Padding = new Padding(3);
            tabPageBlocks.Size = new Size(1004, 496);
            tabPageBlocks.TabIndex = 3;
            tabPageBlocks.Text = "Blocks";
            tabPageBlocks.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            splitContainer4.Dock = DockStyle.Fill;
            splitContainer4.Location = new Point(3, 3);
            splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            splitContainer4.Panel1.Controls.Add(dataGridViewBlocks);
            // 
            // splitContainer4.Panel2
            // 
            splitContainer4.Panel2.AutoScroll = true;
            splitContainer4.Panel2.Controls.Add(pictureBoxBlockEditor);
            splitContainer4.Size = new Size(998, 490);
            splitContainer4.SplitterDistance = 614;
            splitContainer4.TabIndex = 2;
            // 
            // dataGridViewBlocks
            // 
            dataGridViewBlocks.AllowUserToAddRows = false;
            dataGridViewBlocks.AllowUserToDeleteRows = false;
            dataGridViewBlocks.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dataGridViewBlocks.BorderStyle = BorderStyle.None;
            dataGridViewBlocks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewBlocks.Columns.AddRange(new DataGridViewColumn[] { Image, Index, Solidity, Foreground, Used, UsedGlobal });
            dataGridViewBlocks.Dock = DockStyle.Fill;
            dataGridViewBlocks.Location = new Point(0, 0);
            dataGridViewBlocks.MultiSelect = false;
            dataGridViewBlocks.Name = "dataGridViewBlocks";
            dataGridViewBlocks.RowHeadersVisible = false;
            dataGridViewBlocks.RowHeadersWidth = 82;
            dataGridViewBlocks.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewBlocks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBlocks.Size = new Size(614, 490);
            dataGridViewBlocks.TabIndex = 2;
            dataGridViewBlocks.CellPainting += dataGridViewBlocks_CellPainting;
            dataGridViewBlocks.DataError += dataGridViewBlocks_DataError;
            dataGridViewBlocks.EditingControlShowing += dataGridViewBlocks_EditingControlShowing;
            dataGridViewBlocks.SelectionChanged += SelectedBlockChanged;
            // 
            // Image
            // 
            Image.DataPropertyName = "Image";
            Image.HeaderText = "Image";
            Image.MinimumWidth = 10;
            Image.Name = "Image";
            Image.ReadOnly = true;
            Image.Width = 200;
            // 
            // Index
            // 
            Index.DataPropertyName = "Index";
            dataGridViewCellStyle1.Format = "X2";
            Index.DefaultCellStyle = dataGridViewCellStyle1;
            Index.HeaderText = "Index";
            Index.MinimumWidth = 10;
            Index.Name = "Index";
            Index.ReadOnly = true;
            Index.Width = 200;
            // 
            // Solidity
            // 
            Solidity.DataPropertyName = "SolidityIndex";
            Solidity.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
            Solidity.HeaderText = "Solidity";
            Solidity.MinimumWidth = 10;
            Solidity.Name = "Solidity";
            Solidity.SortMode = DataGridViewColumnSortMode.Automatic;
            Solidity.Width = 200;
            // 
            // Foreground
            // 
            Foreground.DataPropertyName = "IsForeground";
            Foreground.HeaderText = "Foreground";
            Foreground.MinimumWidth = 10;
            Foreground.Name = "Foreground";
            Foreground.Width = 200;
            // 
            // Used
            // 
            Used.DataPropertyName = "UsageCount";
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
            Used.DefaultCellStyle = dataGridViewCellStyle2;
            Used.HeaderText = "Used";
            Used.MinimumWidth = 10;
            Used.Name = "Used";
            Used.ReadOnly = true;
            Used.Width = 200;
            // 
            // UsedGlobal
            // 
            UsedGlobal.DataPropertyName = "GlobalUsageCount";
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleRight;
            UsedGlobal.DefaultCellStyle = dataGridViewCellStyle3;
            UsedGlobal.HeaderText = "Total used";
            UsedGlobal.MinimumWidth = 10;
            UsedGlobal.Name = "UsedGlobal";
            UsedGlobal.ReadOnly = true;
            UsedGlobal.Width = 200;
            // 
            // pictureBoxBlockEditor
            // 
            pictureBoxBlockEditor.Dock = DockStyle.Fill;
            pictureBoxBlockEditor.Location = new Point(0, 0);
            pictureBoxBlockEditor.Name = "pictureBoxBlockEditor";
            pictureBoxBlockEditor.Size = new Size(380, 490);
            pictureBoxBlockEditor.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBoxBlockEditor.TabIndex = 3;
            pictureBoxBlockEditor.TabStop = false;
            pictureBoxBlockEditor.MouseClick += BlockEditorMouseClick;
            // 
            // tabPageLayout
            // 
            tabPageLayout.Controls.Add(panel1);
            tabPageLayout.Controls.Add(toolStripLayout);
            tabPageLayout.Location = new Point(4, 22);
            tabPageLayout.Name = "tabPageLayout";
            tabPageLayout.Padding = new Padding(3);
            tabPageLayout.Size = new Size(1004, 498);
            tabPageLayout.TabIndex = 2;
            tabPageLayout.Text = "Layout";
            tabPageLayout.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(splitContainer7);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 81);
            panel1.Name = "panel1";
            panel1.Size = new Size(998, 414);
            panel1.TabIndex = 4;
            // 
            // splitContainer7
            // 
            splitContainer7.Dock = DockStyle.Fill;
            splitContainer7.FixedPanel = FixedPanel.Panel2;
            splitContainer7.Location = new Point(0, 0);
            splitContainer7.Name = "splitContainer7";
            // 
            // splitContainer7.Panel1
            // 
            splitContainer7.Panel1.AutoScroll = true;
            splitContainer7.Panel1.Controls.Add(floorEditor1);
            // 
            // splitContainer7.Panel2
            // 
            splitContainer7.Panel2.Controls.Add(layoutBlockChooser);
            splitContainer7.Size = new Size(998, 414);
            splitContainer7.SplitterDistance = 706;
            splitContainer7.TabIndex = 3;
            // 
            // floorEditor1
            // 
            floorEditor1.BlockChooser = layoutBlockChooser;
            floorEditor1.BlockGaps = false;
            floorEditor1.BlockNumbers = true;
            floorEditor1.ContextMenuStrip = levelEditorContextMenu;
            floorEditor1.Dock = DockStyle.Fill;
            floorEditor1.DrawingMode = FloorEditor.Modes.Draw;
            floorEditor1.LastClickedBlockIndex = 0;
            floorEditor1.LevelBounds = false;
            floorEditor1.Location = new Point(0, 0);
            floorEditor1.Name = "floorEditor1";
            floorEditor1.Size = new Size(706, 414);
            floorEditor1.TabIndex = 0;
            floorEditor1.Text = "floorEditor1";
            floorEditor1.TileGaps = false;
            floorEditor1.WithObjects = true;
            floorEditor1.Zoom = 1;
            floorEditor1.FloorChanged += floorEditor1_FloorChanged;
            // 
            // layoutBlockChooser
            // 
            layoutBlockChooser.Dock = DockStyle.Fill;
            layoutBlockChooser.FixedItemsPerRow = false;
            layoutBlockChooser.ItemsPerRow = 0;
            layoutBlockChooser.Location = new Point(0, 0);
            layoutBlockChooser.Name = "layoutBlockChooser";
            layoutBlockChooser.Scaling = 1;
            layoutBlockChooser.SelectedIndex = -1;
            layoutBlockChooser.ShowTransparency = false;
            layoutBlockChooser.Size = new Size(288, 414);
            layoutBlockChooser.TabIndex = 0;
            // 
            // levelEditorContextMenu
            // 
            levelEditorContextMenu.ImageScalingSize = new Size(32, 32);
            levelEditorContextMenu.Items.AddRange(new ToolStripItem[] { selectBlockToolStripMenuItem, toolStripMenuItem1, editObjectToolStripMenuItem, addObjectToolStripMenuItem, deleteObjectToolStripMenuItem });
            levelEditorContextMenu.Name = "levelEditorContextMenu";
            levelEditorContextMenu.Size = new Size(160, 162);
            levelEditorContextMenu.Opening += levelEditorContextMenu_Opening;
            // 
            // selectBlockToolStripMenuItem
            // 
            selectBlockToolStripMenuItem.Image = (Image)resources.GetObject("selectBlockToolStripMenuItem.Image");
            selectBlockToolStripMenuItem.Name = "selectBlockToolStripMenuItem";
            selectBlockToolStripMenuItem.Size = new Size(159, 38);
            selectBlockToolStripMenuItem.Text = "Select block";
            selectBlockToolStripMenuItem.Click += selectBlockToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(156, 6);
            // 
            // editObjectToolStripMenuItem
            // 
            editObjectToolStripMenuItem.Image = (Image)resources.GetObject("editObjectToolStripMenuItem.Image");
            editObjectToolStripMenuItem.Name = "editObjectToolStripMenuItem";
            editObjectToolStripMenuItem.Size = new Size(159, 38);
            editObjectToolStripMenuItem.Text = "Edit object...";
            editObjectToolStripMenuItem.Click += editObjectToolStripMenuItem_Click;
            // 
            // addObjectToolStripMenuItem
            // 
            addObjectToolStripMenuItem.Image = (Image)resources.GetObject("addObjectToolStripMenuItem.Image");
            addObjectToolStripMenuItem.Name = "addObjectToolStripMenuItem";
            addObjectToolStripMenuItem.Size = new Size(159, 38);
            addObjectToolStripMenuItem.Text = "Add object...";
            addObjectToolStripMenuItem.Click += addObjectToolStripMenuItem_Click;
            // 
            // deleteObjectToolStripMenuItem
            // 
            deleteObjectToolStripMenuItem.Image = (Image)resources.GetObject("deleteObjectToolStripMenuItem.Image");
            deleteObjectToolStripMenuItem.Name = "deleteObjectToolStripMenuItem";
            deleteObjectToolStripMenuItem.Size = new Size(159, 38);
            deleteObjectToolStripMenuItem.Text = "Delete object";
            deleteObjectToolStripMenuItem.Click += deleteObjectToolStripMenuItem_Click;
            // 
            // toolStripLayout
            // 
            toolStripLayout.ImageScalingSize = new Size(32, 32);
            toolStripLayout.Items.AddRange(new ToolStripItem[] { toolStripLabel1, buttonShowObjects, buttonBlockNumbers, buttonBlockGaps, buttonTileGaps, buttonLevelBounds, toolStripSeparator1, toolStripButtonSaveRenderedLevel, buttonCopyFloor, buttonPasteFloor, toolStripSeparator2, toolStripLabel2, buttonDraw, buttonSelect, buttonFloodFill, toolStripSeparator3, buttonResizeFloor, SharingButton, comboZoom });
            toolStripLayout.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStripLayout.Location = new Point(3, 3);
            toolStripLayout.Name = "toolStripLayout";
            toolStripLayout.Size = new Size(998, 78);
            toolStripLayout.TabIndex = 3;
            toolStripLayout.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(41, 15);
            toolStripLabel1.Text = "Show:";
            // 
            // buttonShowObjects
            // 
            buttonShowObjects.Checked = true;
            buttonShowObjects.CheckOnClick = true;
            buttonShowObjects.CheckState = CheckState.Checked;
            buttonShowObjects.Image = Resources.package;
            buttonShowObjects.ImageTransparentColor = Color.Magenta;
            buttonShowObjects.Name = "buttonShowObjects";
            buttonShowObjects.Size = new Size(83, 36);
            buttonShowObjects.Text = "Objects";
            buttonShowObjects.CheckedChanged += LevelRenderModeChanged;
            // 
            // buttonBlockNumbers
            // 
            buttonBlockNumbers.Checked = true;
            buttonBlockNumbers.CheckOnClick = true;
            buttonBlockNumbers.CheckState = CheckState.Checked;
            buttonBlockNumbers.Image = (Image)resources.GetObject("buttonBlockNumbers.Image");
            buttonBlockNumbers.ImageTransparentColor = Color.Magenta;
            buttonBlockNumbers.Name = "buttonBlockNumbers";
            buttonBlockNumbers.Size = new Size(122, 36);
            buttonBlockNumbers.Text = "Block numbers";
            buttonBlockNumbers.CheckedChanged += LevelRenderModeChanged;
            // 
            // buttonBlockGaps
            // 
            buttonBlockGaps.CheckOnClick = true;
            buttonBlockGaps.Image = (Image)resources.GetObject("buttonBlockGaps.Image");
            buttonBlockGaps.ImageTransparentColor = Color.Magenta;
            buttonBlockGaps.Name = "buttonBlockGaps";
            buttonBlockGaps.Size = new Size(100, 36);
            buttonBlockGaps.Text = "Block gaps";
            buttonBlockGaps.CheckedChanged += LevelRenderModeChanged;
            // 
            // buttonTileGaps
            // 
            buttonTileGaps.CheckOnClick = true;
            buttonTileGaps.Image = (Image)resources.GetObject("buttonTileGaps.Image");
            buttonTileGaps.ImageTransparentColor = Color.Magenta;
            buttonTileGaps.Name = "buttonTileGaps";
            buttonTileGaps.Size = new Size(89, 36);
            buttonTileGaps.Text = "Tile gaps";
            buttonTileGaps.CheckedChanged += LevelRenderModeChanged;
            // 
            // buttonLevelBounds
            // 
            buttonLevelBounds.CheckOnClick = true;
            buttonLevelBounds.Image = (Image)resources.GetObject("buttonLevelBounds.Image");
            buttonLevelBounds.ImageTransparentColor = Color.Magenta;
            buttonLevelBounds.Name = "buttonLevelBounds";
            buttonLevelBounds.Size = new Size(113, 36);
            buttonLevelBounds.Text = "Level bounds";
            buttonLevelBounds.CheckedChanged += LevelRenderModeChanged;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 23);
            // 
            // toolStripButtonSaveRenderedLevel
            // 
            toolStripButtonSaveRenderedLevel.Image = (Image)resources.GetObject("toolStripButtonSaveRenderedLevel.Image");
            toolStripButtonSaveRenderedLevel.ImageTransparentColor = Color.Magenta;
            toolStripButtonSaveRenderedLevel.Name = "toolStripButtonSaveRenderedLevel";
            toolStripButtonSaveRenderedLevel.Size = new Size(76, 36);
            toolStripButtonSaveRenderedLevel.Text = "Save...";
            toolStripButtonSaveRenderedLevel.Click += toolStripButtonSaveRenderedLevel_Click;
            // 
            // buttonCopyFloor
            // 
            buttonCopyFloor.Image = (Image)resources.GetObject("buttonCopyFloor.Image");
            buttonCopyFloor.ImageTransparentColor = Color.Magenta;
            buttonCopyFloor.Name = "buttonCopyFloor";
            buttonCopyFloor.Size = new Size(71, 36);
            buttonCopyFloor.Text = "Copy";
            buttonCopyFloor.Click += buttonCopyFloor_Click;
            // 
            // buttonPasteFloor
            // 
            buttonPasteFloor.Image = (Image)resources.GetObject("buttonPasteFloor.Image");
            buttonPasteFloor.ImageTransparentColor = Color.Magenta;
            buttonPasteFloor.Name = "buttonPasteFloor";
            buttonPasteFloor.Size = new Size(71, 36);
            buttonPasteFloor.Text = "Paste";
            buttonPasteFloor.Click += buttonPasteFloor_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 23);
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(57, 15);
            toolStripLabel2.Text = "Drawing:";
            // 
            // buttonDraw
            // 
            buttonDraw.Checked = true;
            buttonDraw.CheckState = CheckState.Checked;
            buttonDraw.Image = (Image)resources.GetObject("buttonDraw.Image");
            buttonDraw.ImageTransparentColor = Color.Magenta;
            buttonDraw.Name = "buttonDraw";
            buttonDraw.Size = new Size(70, 36);
            buttonDraw.Text = "Draw";
            buttonDraw.Click += DrawingButtonCheckedChanged;
            // 
            // buttonSelect
            // 
            buttonSelect.Image = (Image)resources.GetObject("buttonSelect.Image");
            buttonSelect.ImageTransparentColor = Color.Magenta;
            buttonSelect.Name = "buttonSelect";
            buttonSelect.Size = new Size(74, 36);
            buttonSelect.Text = "Select";
            buttonSelect.Click += DrawingButtonCheckedChanged;
            // 
            // buttonFloodFill
            // 
            buttonFloodFill.Image = (Image)resources.GetObject("buttonFloodFill.Image");
            buttonFloodFill.ImageTransparentColor = Color.Magenta;
            buttonFloodFill.Name = "buttonFloodFill";
            buttonFloodFill.Size = new Size(89, 36);
            buttonFloodFill.Text = "Flood fill";
            buttonFloodFill.Click += DrawingButtonCheckedChanged;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 23);
            // 
            // buttonResizeFloor
            // 
            buttonResizeFloor.Image = (Image)resources.GetObject("buttonResizeFloor.Image");
            buttonResizeFloor.ImageTransparentColor = Color.Magenta;
            buttonResizeFloor.Name = "buttonResizeFloor";
            buttonResizeFloor.Size = new Size(84, 36);
            buttonResizeFloor.Text = "Resize...";
            buttonResizeFloor.Click += ResizeFloorButtonClick;
            // 
            // SharingButton
            // 
            SharingButton.Image = (Image)resources.GetObject("SharingButton.Image");
            SharingButton.ImageTransparentColor = Color.Magenta;
            SharingButton.Name = "SharingButton";
            SharingButton.Size = new Size(92, 36);
            SharingButton.Text = "Sharing...";
            SharingButton.Click += SharingButton_Click;
            // 
            // comboZoom
            // 
            comboZoom.DropDownStyle = ComboBoxStyle.DropDownList;
            comboZoom.Items.AddRange(new object[] { "100%", "200%", "300%", "400%", "500%", "600%", "700%", "800%", "900%", "1000%" });
            comboZoom.Name = "comboZoom";
            comboZoom.Size = new Size(121, 23);
            comboZoom.SelectedIndexChanged += LevelRenderModeChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1331, 556);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer3);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1323, 528);
            tabPage1.TabIndex = 9;
            tabPage1.Text = "Other art";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(3, 3);
            splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(listBoxArt);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(tabControlArt);
            splitContainer3.Size = new Size(1317, 522);
            splitContainer3.SplitterDistance = 260;
            splitContainer3.TabIndex = 1;
            // 
            // listBoxArt
            // 
            listBoxArt.Dock = DockStyle.Fill;
            listBoxArt.FormattingEnabled = true;
            listBoxArt.ItemHeight = 13;
            listBoxArt.Location = new Point(0, 0);
            listBoxArt.Name = "listBoxArt";
            listBoxArt.Size = new Size(260, 522);
            listBoxArt.TabIndex = 0;
            listBoxArt.SelectedIndexChanged += listBoxArt_SelectedIndexChanged;
            // 
            // tabControlArt
            // 
            tabControlArt.Controls.Add(tabPageArtLayout);
            tabControlArt.Controls.Add(tabPageArtTiles);
            tabControlArt.Controls.Add(tabPageArtPalette);
            tabControlArt.Controls.Add(tabPageExtraData);
            tabControlArt.Dock = DockStyle.Fill;
            tabControlArt.Location = new Point(0, 0);
            tabControlArt.Name = "tabControlArt";
            tabControlArt.SelectedIndex = 0;
            tabControlArt.Size = new Size(1053, 522);
            tabControlArt.TabIndex = 1;
            // 
            // tabPageArtLayout
            // 
            tabPageArtLayout.Controls.Add(toolStrip2);
            tabPageArtLayout.Controls.Add(pictureBoxArtLayout);
            tabPageArtLayout.Location = new Point(4, 22);
            tabPageArtLayout.Name = "tabPageArtLayout";
            tabPageArtLayout.Padding = new Padding(3);
            tabPageArtLayout.Size = new Size(1045, 496);
            tabPageArtLayout.TabIndex = 2;
            tabPageArtLayout.Text = "Layout";
            tabPageArtLayout.UseVisualStyleBackColor = true;
            // 
            // toolStrip2
            // 
            toolStrip2.ImageScalingSize = new Size(32, 32);
            toolStrip2.Items.AddRange(new ToolStripItem[] { buttonSaveScreen, buttonLoadTileset });
            toolStrip2.Location = new Point(3, 3);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(1039, 39);
            toolStrip2.TabIndex = 5;
            toolStrip2.Text = "toolStrip2";
            // 
            // buttonSaveScreen
            // 
            buttonSaveScreen.Image = (Image)resources.GetObject("buttonSaveScreen.Image");
            buttonSaveScreen.ImageTransparentColor = Color.Magenta;
            buttonSaveScreen.Name = "buttonSaveScreen";
            buttonSaveScreen.Size = new Size(76, 36);
            buttonSaveScreen.Text = "Save...";
            buttonSaveScreen.Click += buttonSaveScreen_Click;
            // 
            // buttonLoadTileset
            // 
            buttonLoadTileset.Image = (Image)resources.GetObject("buttonLoadTileset.Image");
            buttonLoadTileset.ImageTransparentColor = Color.Magenta;
            buttonLoadTileset.Name = "buttonLoadTileset";
            buttonLoadTileset.Size = new Size(78, 36);
            buttonLoadTileset.Text = "Load...";
            buttonLoadTileset.Click += buttonLoadTileset_Click;
            // 
            // pictureBoxArtLayout
            // 
            pictureBoxArtLayout.Location = new Point(6, 31);
            pictureBoxArtLayout.Name = "pictureBoxArtLayout";
            pictureBoxArtLayout.Size = new Size(317, 194);
            pictureBoxArtLayout.TabIndex = 0;
            pictureBoxArtLayout.TabStop = false;
            // 
            // tabPageArtTiles
            // 
            tabPageArtTiles.Controls.Add(otherArtTileSetViewer);
            tabPageArtTiles.Location = new Point(4, 24);
            tabPageArtTiles.Name = "tabPageArtTiles";
            tabPageArtTiles.Padding = new Padding(3);
            tabPageArtTiles.Size = new Size(1045, 494);
            tabPageArtTiles.TabIndex = 0;
            tabPageArtTiles.Text = "Tiles";
            tabPageArtTiles.UseVisualStyleBackColor = true;
            // 
            // otherArtTileSetViewer
            // 
            otherArtTileSetViewer.Dock = DockStyle.Fill;
            otherArtTileSetViewer.Location = new Point(3, 3);
            otherArtTileSetViewer.Margin = new Padding(4, 3, 4, 3);
            otherArtTileSetViewer.Name = "otherArtTileSetViewer";
            otherArtTileSetViewer.Size = new Size(1039, 488);
            otherArtTileSetViewer.TabIndex = 0;
            otherArtTileSetViewer.TilesPerRow = 4;
            otherArtTileSetViewer.Changed += otherArtTileSetViewer_Changed;
            // 
            // tabPageArtPalette
            // 
            tabPageArtPalette.Controls.Add(palettesLayoutPanel);
            tabPageArtPalette.Location = new Point(4, 24);
            tabPageArtPalette.Name = "tabPageArtPalette";
            tabPageArtPalette.Padding = new Padding(3);
            tabPageArtPalette.Size = new Size(1045, 494);
            tabPageArtPalette.TabIndex = 1;
            tabPageArtPalette.Text = "Palettes";
            tabPageArtPalette.UseVisualStyleBackColor = true;
            // 
            // palettesLayoutPanel
            // 
            palettesLayoutPanel.AutoScroll = true;
            palettesLayoutPanel.AutoSize = true;
            palettesLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            palettesLayoutPanel.Dock = DockStyle.Fill;
            palettesLayoutPanel.FlowDirection = FlowDirection.TopDown;
            palettesLayoutPanel.Location = new Point(3, 3);
            palettesLayoutPanel.Name = "palettesLayoutPanel";
            palettesLayoutPanel.Size = new Size(1039, 488);
            palettesLayoutPanel.TabIndex = 1;
            palettesLayoutPanel.WrapContents = false;
            // 
            // tabPageExtraData
            // 
            tabPageExtraData.Controls.Add(extraDataLayoutPanel);
            tabPageExtraData.Location = new Point(4, 24);
            tabPageExtraData.Name = "tabPageExtraData";
            tabPageExtraData.Padding = new Padding(3);
            tabPageExtraData.Size = new Size(1045, 494);
            tabPageExtraData.TabIndex = 3;
            tabPageExtraData.Text = "Extra data";
            tabPageExtraData.UseVisualStyleBackColor = true;
            // 
            // extraDataLayoutPanel
            // 
            extraDataLayoutPanel.AutoScroll = true;
            extraDataLayoutPanel.AutoSize = true;
            extraDataLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            extraDataLayoutPanel.Dock = DockStyle.Fill;
            extraDataLayoutPanel.FlowDirection = FlowDirection.TopDown;
            extraDataLayoutPanel.Location = new Point(3, 3);
            extraDataLayoutPanel.Name = "extraDataLayoutPanel";
            extraDataLayoutPanel.Size = new Size(1039, 488);
            extraDataLayoutPanel.TabIndex = 0;
            extraDataLayoutPanel.WrapContents = false;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(udSDSCVersion);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(dateTimePickerSDSCDate);
            tabPage3.Controls.Add(label4);
            tabPage3.Controls.Add(label3);
            tabPage3.Controls.Add(label2);
            tabPage3.Controls.Add(label1);
            tabPage3.Controls.Add(textBoxSDSCTitle);
            tabPage3.Controls.Add(textBoxSDSCNotes);
            tabPage3.Controls.Add(textBoxSDSCAuthor);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(1323, 528);
            tabPage3.TabIndex = 11;
            tabPage3.Text = "SDSC tag";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // udSDSCVersion
            // 
            udSDSCVersion.DecimalPlaces = 2;
            udSDSCVersion.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            udSDSCVersion.Location = new Point(97, 34);
            udSDSCVersion.Maximum = new decimal(new int[] { 9999, 0, 0, 131072 });
            udSDSCVersion.Name = "udSDSCVersion";
            udSDSCVersion.Size = new Size(225, 22);
            udSDSCVersion.TabIndex = 2;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 95);
            label5.Name = "label5";
            label5.Size = new Size(72, 13);
            label5.TabIndex = 9;
            label5.Text = "Release date";
            // 
            // dateTimePickerSDSCDate
            // 
            dateTimePickerSDSCDate.Location = new Point(97, 90);
            dateTimePickerSDSCDate.Name = "dateTimePickerSDSCDate";
            dateTimePickerSDSCDate.Size = new Size(225, 22);
            dateTimePickerSDSCDate.TabIndex = 4;
            dateTimePickerSDSCDate.ValueChanged += textBoxSDSCTitle_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 37);
            label4.Name = "label4";
            label4.Size = new Size(45, 13);
            label4.TabIndex = 6;
            label4.Text = "Version";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 65);
            label3.Name = "label3";
            label3.Size = new Size(43, 13);
            label3.TabIndex = 4;
            label3.Text = "Author";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 125);
            label2.Name = "label2";
            label2.Size = new Size(37, 13);
            label2.TabIndex = 2;
            label2.Text = "Notes";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 9);
            label1.Name = "label1";
            label1.Size = new Size(29, 13);
            label1.TabIndex = 0;
            label1.Text = "Title";
            // 
            // textBoxSDSCTitle
            // 
            textBoxSDSCTitle.Location = new Point(97, 6);
            textBoxSDSCTitle.Name = "textBoxSDSCTitle";
            textBoxSDSCTitle.Size = new Size(225, 22);
            textBoxSDSCTitle.TabIndex = 1;
            textBoxSDSCTitle.TextChanged += textBoxSDSCTitle_TextChanged;
            // 
            // textBoxSDSCNotes
            // 
            textBoxSDSCNotes.Location = new Point(97, 122);
            textBoxSDSCNotes.Multiline = true;
            textBoxSDSCNotes.Name = "textBoxSDSCNotes";
            textBoxSDSCNotes.ScrollBars = ScrollBars.Both;
            textBoxSDSCNotes.Size = new Size(225, 95);
            textBoxSDSCNotes.TabIndex = 5;
            textBoxSDSCNotes.TextChanged += textBoxSDSCTitle_TextChanged;
            // 
            // textBoxSDSCAuthor
            // 
            textBoxSDSCAuthor.Location = new Point(97, 62);
            textBoxSDSCAuthor.Name = "textBoxSDSCAuthor";
            textBoxSDSCAuthor.Size = new Size(225, 22);
            textBoxSDSCAuthor.TabIndex = 3;
            textBoxSDSCAuthor.TextChanged += textBoxSDSCTitle_TextChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(logTextBox);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1323, 528);
            tabPage2.TabIndex = 10;
            tabPage2.Text = "Log";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // logTextBox
            // 
            logTextBox.Dock = DockStyle.Fill;
            logTextBox.Location = new Point(3, 3);
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.ScrollBars = ScrollBars.Both;
            logTextBox.Size = new Size(1317, 522);
            logTextBox.TabIndex = 0;
            // 
            // spaceVisualizer1
            // 
            spaceVisualizer1.Dock = DockStyle.Fill;
            spaceVisualizer1.Location = new Point(0, 0);
            spaceVisualizer1.Margin = new Padding(4, 3, 4, 3);
            spaceVisualizer1.Name = "spaceVisualizer1";
            spaceVisualizer1.Size = new Size(1331, 25);
            spaceVisualizer1.TabIndex = 2;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(spaceVisualizer1);
            splitContainer1.Size = new Size(1331, 585);
            splitContainer1.SplitterDistance = 556;
            splitContainer1.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1331, 609);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Font = new Font("Segoe UI", 8.25F);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "STH1 Editor by WV :: extended by Maxim";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tabPage5.ResumeLayout(false);
            splitContainer6.Panel1.ResumeLayout(false);
            splitContainer6.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer6).EndInit();
            splitContainer6.ResumeLayout(false);
            tabControlLevel.ResumeLayout(false);
            tabPageMetadata.ResumeLayout(false);
            tabPagePalettes.ResumeLayout(false);
            tabPagePalettes.PerformLayout();
            tabPageTiles.ResumeLayout(false);
            tabPageSprites.ResumeLayout(false);
            tabPageBlocks.ResumeLayout(false);
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.Panel2.ResumeLayout(false);
            splitContainer4.Panel2.PerformLayout();
            ((ISupportInitialize)splitContainer4).EndInit();
            splitContainer4.ResumeLayout(false);
            ((ISupportInitialize)dataGridViewBlocks).EndInit();
            ((ISupportInitialize)pictureBoxBlockEditor).EndInit();
            tabPageLayout.ResumeLayout(false);
            tabPageLayout.PerformLayout();
            panel1.ResumeLayout(false);
            splitContainer7.Panel1.ResumeLayout(false);
            splitContainer7.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer7).EndInit();
            splitContainer7.ResumeLayout(false);
            levelEditorContextMenu.ResumeLayout(false);
            toolStripLayout.ResumeLayout(false);
            toolStripLayout.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            tabControlArt.ResumeLayout(false);
            tabPageArtLayout.ResumeLayout(false);
            tabPageArtLayout.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            ((ISupportInitialize)pictureBoxArtLayout).EndInit();
            tabPageArtTiles.ResumeLayout(false);
            tabPageArtPalette.ResumeLayout(false);
            tabPageArtPalette.PerformLayout();
            tabPageExtraData.ResumeLayout(false);
            tabPageExtraData.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((ISupportInitialize)udSDSCVersion).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
        private TabControl tabControl1;
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
        private ContextMenuStrip levelEditorContextMenu;
        private ToolStripMenuItem selectBlockToolStripMenuItem;
        private ToolStripMenuItem editObjectToolStripMenuItem;
        private TabPage tabPage1;
        private TileSetViewer otherArtTileSetViewer;
        private SplitContainer splitContainer3;
        private ListBox listBoxArt;
        private TabControl tabControlArt;
        private TabPage tabPageArtTiles;
        private TabPage tabPageArtLayout;
        private PictureBox pictureBoxArtLayout;
        private TabPage tabPage2;
        private TextBox logTextBox;
        private ToolStrip toolStrip2;
        private ToolStripButton buttonSaveScreen;
        private ToolStripButton buttonLoadTileset;
        private SpaceVisualizer spaceVisualizer1;
        private ToolStripMenuItem addObjectToolStripMenuItem;
        private ToolStripMenuItem deleteObjectToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private SplitContainer splitContainer1;
        private TabPage tabPage3;
        private TextBox textBoxSDSCNotes;
        private Label label3;
        private TextBox textBoxSDSCAuthor;
        private Label label2;
        private TextBox textBoxSDSCTitle;
        private Label label1;
        private Label label4;
        private Label label5;
        private DateTimePicker dateTimePickerSDSCDate;
        private NumericUpDown udSDSCVersion;
        private TabPage tabPageExtraData;
        private TabPage tabPageArtPalette;
        private FlowLayoutPanel extraDataLayoutPanel;
        private FlowLayoutPanel palettesLayoutPanel;
        private ToolStripComboBox comboZoom;
    }
}

