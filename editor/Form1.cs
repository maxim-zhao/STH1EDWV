using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using sth1edwv.BindingListView;
using sth1edwv.Controls;
using sth1edwv.Forms;
using sth1edwv.GameObjects;
using sth1edwv.Properties;

namespace sth1edwv
{
    public sealed partial class Form1 : Form
    {
        private Cartridge _cartridge;
        private readonly ImageList _solidityImages;
        private int _blockEditorTileSize;
        private byte[] _lastSaved;

        public Form1()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Font = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;

            InitializeComponent();

            Text += $" :: v{Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";

            _solidityImages = new ImageList
            {
                ColorDepth = ColorDepth.Depth16Bit,
                ImageSize = new Size(32, 32)
            };
            _solidityImages.Images.AddStrip(Resources.SolidityImages);

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                dataGridViewBlocks,
                [true]);
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                extraDataLayoutPanel,
                [true]);
        }

        private void openROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserWantsToSaveModifications())
            {
                return;
            }

            using var d = new OpenFileDialog();
            d.Filter = "*.sms|*.sms";
            if (d.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _cartridge?.Dispose();
            LoadFile(d.FileName);
        }

        private void LoadFile(string filename)
        {
            _loading = true;
            _cartridge = new Cartridge(filename, Log);
            listBoxLevels.Items.Clear();
            listBoxLevels.Items.AddRange(_cartridge.Levels.ToArray<object>());
            listBoxArt.Items.Clear();
            listBoxArt.Items.AddRange(_cartridge.Art.ToArray<object>());
            listBoxArt.Sorted = true;

            if (_cartridge.SdscTag != null)
            {
                textBoxSDSCTitle.Text = _cartridge.SdscTag.Title ?? "";
                textBoxSDSCAuthor.Text = _cartridge.SdscTag.Author ?? "";
                textBoxSDSCNotes.Text = (_cartridge.SdscTag.Notes ?? "").ReplaceLineEndings();
                udSDSCVersion.Value = _cartridge.SdscTag.Version;
                dateTimePickerSDSCDate.Value = _cartridge.SdscTag.Date;
            }

            // Add or replace filename in title bar
            Text = $"{Regex.Replace(Text, " \\[.+\\]$", "")} [{Path.GetFileName(filename)}]";

            // Trigger the selected level changed event
            SelectedLevelChanged(null, null);

            // Take an image at the point of loading. This isn't what we loaded, but should be equivalent.
            _lastSaved = _cartridge.MakeRom(false);

            spaceVisualizer1.SetData(_cartridge.LastFreeSpace, _cartridge.InitialFreeSpace);
            _loading = false;
        }

        private void SelectedLevelChanged(object sender, EventArgs e)
        {
            var level = listBoxLevels.SelectedItem as Level;

            floorEditor1.SetData(level);
            LevelRenderModeChanged(null, EventArgs.Empty);

            tileSetViewer.SetData(level?.TileSet, level?.TilePalette, GetTileUsedInBlocks);
            spriteTileSetViewer.SetData(level?.SpriteTileSet, level?.SpritePalette, null, true);

            foreach (Control control in PalettesLayout.Controls)
            {
                control.Dispose();
            }
            PalettesLayout.Controls.Clear();
            if (level != null)
            {
                PalettesLayout.Controls.Add(new PaletteEditor(level.Palette, "Base palette (only sprites part is used)", OnPaletteChanged));
                PalettesLayout.Controls.Add(new PaletteEditor(level.CyclingPalette, "Colour cycling", OnPaletteChanged));
            }

            propertyGridLevel.SelectedObject = level;

            level?.BlockMapping.UpdateUsageForLevel(level);
            level?.BlockMapping.UpdateGlobalUsage(_cartridge.Levels);

            dataGridViewBlocks.DataSource = level == null
                ? null
                : new BindingListView<BlockRow>(level.BlockMapping.Blocks.Select(x => new BlockRow(x, level.TilePalette)).ToList());
            dataGridViewBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            layoutBlockChooser.SetData(level?.BlockMapping.Blocks, level?.TilePalette);
        }

        private void OnPaletteChanged(Palette palette)
        {
            // We invalidate everything based on it...
            foreach (var level in _cartridge.Levels.Where(x => x.Palette == palette))
            {
                level.UpdateRenderingPalettes();
            }

            if (listBoxLevels.SelectedItem is Level selectedLevel)
            {
                tileSetViewer.SetData(selectedLevel.TileSet, selectedLevel.TilePalette, GetTileUsedInBlocks);
                spriteTileSetViewer.SetData(selectedLevel.SpriteTileSet, selectedLevel.SpritePalette);
            }
        }

        private IList<Block> GetTileUsedInBlocks(int index)
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return new List<Block>();
            }

            return level.BlockMapping.Blocks
                .Where(block => block.TileIndices.Contains((byte)index))
                .ToList();
        }

        private void LevelRenderModeChanged(object sender, EventArgs e)
        {
            floorEditor1.TileGaps = buttonTileGaps.Checked;
            floorEditor1.BlockGaps = buttonBlockGaps.Checked;
            floorEditor1.LevelBounds = buttonLevelBounds.Checked;
            floorEditor1.BlockNumbers = buttonBlockNumbers.Checked;
            floorEditor1.WithObjects = buttonShowObjects.Checked;
            floorEditor1.Zoom = comboZoom.SelectedIndex + 1;
        }

        private Block GetSelectedBlock()
        {
            return dataGridViewBlocks.SelectedRows.Count == 0
                ? null
                : (dataGridViewBlocks.SelectedRows[0].DataBoundItem as ObjectView<BlockRow>)?.Object.Block;
        }

        private void SelectedBlockChanged(object sender, EventArgs e)
        {
            DrawBlockEditor();
        }

        private void DrawBlockEditor()
        {
            pictureBoxBlockEditor.Image?.Dispose();
            pictureBoxBlockEditor.Image = null;
            var block = GetSelectedBlock();
            if (block == null)
            {
                return;
            }

            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }

            var scale = (pictureBoxBlockEditor.Width - 3) / 32;
            var bmp = new Bitmap(32 * scale + 3, 32 * scale + 3);
            _blockEditorTileSize = scale * 8 + 1;
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                for (var i = 0; i < 16; ++i)
                {
                    var x = i % 4 * _blockEditorTileSize - 1;
                    var y = i / 4 * _blockEditorTileSize - 1;
                    g.DrawImage(block.TileSet.GetImageWithRings(block.TileIndices[i], level.TilePalette), x, y, _blockEditorTileSize - 1, _blockEditorTileSize - 1);
                }
            }

            pictureBoxBlockEditor.Image = bmp;
        }

        private void BlockEditorMouseClick(object sender, MouseEventArgs e)
        {
            var block = GetSelectedBlock();
            if (block == null)
            {
                return;
            }
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }

            var x = e.X / _blockEditorTileSize;
            var y = e.Y / _blockEditorTileSize;
            if (x > 3 || y > 3)
            {
                return;
            }
            var subBlockIndex = x + y * 4;
            var tileIndex = block.TileIndices[subBlockIndex];
            using var tc = new TileChooser(block.TileSet, tileIndex, level.TilePalette);
            if (tc.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            // Apply to the block object
            block.TileIndices[subBlockIndex] = (byte)tc.SelectedTile.Index;
            // Invalidate its cached image
            block.ResetImages();
            // Trigger a redraw of the editor
            DrawBlockEditor();
            // And the grid
            dataGridViewBlocks.InvalidateRow(dataGridViewBlocks.SelectedRows[0].Index);
            // And the rendered level
            floorEditor1.Invalidate();
        }

        private void saveROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog();
            dialog.Filter = "*.sms|*.sms";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    var data = _cartridge.MakeRom();
                    File.WriteAllBytes(dialog.FileName, data);
                    _lastSaved = data;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"There was an error while saving:\n\n{ex.Message}");
                }
            }
        }

        private void toolStripButtonSaveRenderedLevel_Click(object sender, EventArgs e)
        {
            using var bmp = new Bitmap(floorEditor1.AutoScrollMinSize.Width, floorEditor1.AutoScrollMinSize.Height);
            using var g = Graphics.FromImage(bmp);
            floorEditor1.Draw(g, new Rectangle(0, 0, bmp.Width, bmp.Height));
            using var d = new SaveFileDialog();
            d.Filter = "*.png|*.png";
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                bmp.Save(d.FileName);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2 && File.Exists(args[1]))
            {
                LoadFile(args[1]);
            }

            // We do some grid setup here...
            dataGridViewBlocks.AutoGenerateColumns = false;
            foreach (DataGridViewColumn column in dataGridViewBlocks.Columns)
            {
                switch (column.DataPropertyName)
                {
                    case nameof(Block.SolidityIndex):
                        {
                            column.SortMode = DataGridViewColumnSortMode.Automatic;
                            if (column is DataGridViewComboBoxColumn c)
                            {
                                c.DataSource = Enumerable
                                    .Range(0, _solidityImages.Images.Count)
                                    .ToList();
                            }
                            break;
                        }
                }
            }
        }

        private void dataGridViewBlocks_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (dataGridViewBlocks.Columns[e.ColumnIndex].DataPropertyName == nameof(Block.SolidityIndex) && e.RowIndex >= 0 && e.Value != null)
            {
                var rect = dataGridViewBlocks.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

                e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ContentForeground);

                var index = (int)e.Value;
                DrawSolidityItem(e.Graphics, index, rect);

                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void DrawSolidityItem(Graphics g, int index, Rectangle rect)
        {
            g.DrawImageUnscaled(_solidityImages.Images[index], rect);
            g.DrawString($"{index}", Font, SystemBrushes.WindowText, rect.Left + _solidityImages.ImageSize.Width + 4, rect.Top + 8);
        }

        private void dataGridViewBlocks_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is not ComboBox combo)
            {
                return;
            }
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.ItemHeight = _solidityImages.ImageSize.Height + 1;
            combo.DrawItem -= DrawSolidityComboItem;
            combo.DrawItem += DrawSolidityComboItem;
        }

        private void DrawSolidityComboItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }
            e.DrawBackground();
            DrawSolidityItem(e.Graphics, e.Index, e.Bounds);
        }

        private void dataGridViewBlocks_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void quickTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var filename = Path.Combine(Path.GetTempPath(), "test.sms");
                File.WriteAllBytes(filename, _cartridge.MakeRom());
                new Process { StartInfo = new ProcessStartInfo(filename) { UseShellExecute = true } }.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"There was an error while saving:\n\n{ex.Message}");
            }
        }

        private void buttonCopyFloor_Click(object sender, EventArgs e)
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }
            Clipboard.SetText(Convert.ToBase64String(level.Floor.BlockIndices));
        }

        private void buttonPasteFloor_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxLevels.SelectedItem is not Level level)
                {
                    return;
                }

                var text = Clipboard.GetText();
                var data = Convert.FromBase64String(text);
                if (data.Length != level.Floor.BlockIndices.Length)
                {
                    return;
                }
                data.CopyTo(level.Floor.BlockIndices, 0);
                // Need to redraw
                floorEditor1.Invalidate();
                UpdateSpace();
            }
            catch (Exception)
            {
                // Ignore it
            }
        }

        private void propertyGridLevel_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // Invalidate the level map
            floorEditor1.Invalidate();
        }

        private void DrawingButtonCheckedChanged(object sender, EventArgs e)
        {
            foreach (var button in new[] { buttonDraw, buttonSelect, buttonFloodFill })
            {
                button.Checked = button == sender;
            }

            floorEditor1.DrawingMode = buttonDraw.Checked ? FloorEditor.Modes.Draw
                : buttonSelect.Checked ? FloorEditor.Modes.Select
                : FloorEditor.Modes.FloodFill;
        }

        private void tileSetViewer_Changed(TileSet tileSet)
        {
            // We need to reset any cached images in block mappings using this tile set
            foreach (var block in _cartridge.Levels
                .Where(x => x.TileSet == tileSet)
                .Select(x => x.BlockMapping)
                .Distinct()
                .SelectMany(x => x.Blocks))
            {
                block.ResetImages();
            }

            UpdateSpace();
        }

        private void ResizeFloorButtonClick(object sender, EventArgs e)
        {
            if (listBoxLevels.SelectedItem is not Level selectedLevel)
            {
                return;
            }

            using var form = new FloorSizeEditor(selectedLevel);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Floor size changed!
            // We need to change stuff for every level using this floor...
            foreach (var level in _cartridge.Levels.Where(x => x.Floor == selectedLevel.Floor))
            {
                // - The level header
                level.FloorWidth += form.Result.Horizontal;
                level.FloorHeight += form.Result.Vertical;
                // - The level bounds - top/left only
                level.LeftPixels = Math.Max(0, level.LeftPixels + form.Result.Left * 32);
                level.TopPixels = Math.Max(0, level.TopPixels + form.Result.Top * 32);
                // - Start position
                level.StartX += form.Result.Left;
                level.StartY += form.Result.Top;
                // - Object positions
                foreach (var levelObject in level.Objects)
                {
                    levelObject.X = (byte)Math.Min(level.FloorWidth - 1, Math.Max(0, levelObject.X + form.Result.Left));
                    levelObject.Y = (byte)Math.Min(level.FloorHeight - 1, Math.Max(0, levelObject.Y + form.Result.Top));
                }
            }
            // Then the floor itself
            selectedLevel.Floor.Resize(form.Result);

            // Reset the floor editor so it picks up the new size
            floorEditor1.SetData(selectedLevel);
        }

        private void SharingButton_Click(object sender, EventArgs e)
        {
            if (listBoxLevels.SelectedItem is not Level selectedLevel)
            {
                return;
            }

            using var d = new FloorSharingEditor(selectedLevel, _cartridge);
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                // Invalidate stuff
                floorEditor1.SetData(selectedLevel);
            }
        }

        private void selectBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }
            layoutBlockChooser.SelectedIndex = level.Floor.BlockIndices[floorEditor1.LastClickedBlockIndex];
        }

        private LevelObject GetClickedLevelObject()
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return null;
            }
            var index = floorEditor1.LastClickedBlockIndex;
            var x = index % level.FloorWidth;
            var y = index / level.FloorWidth;
            // See if we have an object here
            return level.Objects.FirstOrDefault(o => o.X == x && o.Y == y);
        }

        private void editObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var levelObject = GetClickedLevelObject();
            if (levelObject != null)
            {
                using var editor = new ObjectEditor(levelObject);
                if (editor.ShowDialog(this) == DialogResult.OK)
                {
                    levelObject.X = editor.X;
                    levelObject.Y = editor.Y;
                    levelObject.Type = editor.Type;

                    // And the level map may be different
                    if (floorEditor1.WithObjects)
                    {
                        floorEditor1.Invalidate();
                    }
                }
            }
        }

        private void listBoxArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxArt.SelectedItem is not ArtItem artItem)
            {
                return;
            }

            // We want to hide irrelevant tabs. This is a bit of a pain because we can't just hide them, so we remove them all and re-add...
            // Sprite pages are created dynamically, so we also dispose any of those
            foreach (var tabPage in tabControlArt.TabPages.Cast<TabPage>().Where(x => x.Tag is TileSet))
            {
                tabPage.Dispose();
            }

            foreach (Control control in extraDataLayoutPanel.Controls)
            {
                control.Dispose();
            }
            extraDataLayoutPanel.Controls.Clear();

            tabControlArt.TabPages.Clear();

            var firstPalette = artItem.Palettes.First().Value;
            if (artItem.TileMap != null)
            {
                pictureBoxArtLayout.Image?.Dispose();
                pictureBoxArtLayout.Image = artItem.TileMap.GetImage(artItem.TileSet, firstPalette.GetSubPalette(0, 16));
                tabControlArt.TabPages.Add(tabPageArtLayout);
            }

            if (artItem.TileSet != null)
            {
                otherArtTileSetViewer.TilesPerRow = artItem.TileSet.TilesPerRow;
                otherArtTileSetViewer.SetData(artItem.TileSet, firstPalette.GetSubPalette(0, 16));
                tabControlArt.TabPages.Add(tabPageArtTiles);
            }

            foreach (var kvp in artItem.SpriteTileSets)
            {
                var tileSet = kvp.Value;
                var page = new TabPage(kvp.Key) { Tag = tileSet, Padding = new Padding(3), UseVisualStyleBackColor = true };
                var viewer = new TileSetViewer { Dock = DockStyle.Fill, TilesPerRow = tileSet.TilesPerRow };
                page.Controls.Add(viewer);
                var palette = firstPalette.GetData().Count >= 32
                    ? firstPalette.GetSubPalette(16, 16)
                    : firstPalette;
                viewer.SetData(tileSet, palette, null, true);
                viewer.Changed += _ => UpdateSpace();
                tabControlArt.TabPages.Add(page);
            }

            if (artItem.PaletteEditable)
            {
                foreach (Control control in palettesLayoutPanel.Controls)
                {
                    control.Dispose();
                    palettesLayoutPanel.Controls.Clear();
                }

                foreach (var kvp in artItem.Palettes.OrderBy(x => x.Key))
                {
                    var paletteEditor = new PaletteEditor(kvp.Value, kvp.Key, _ =>
                    {
                        foreach (var tile in artItem.TileSet.Tiles)
                        {
                            tile.ResetImages();
                        }

                        otherArtTileSetViewer.Invalidate();
                    });
                    palettesLayoutPanel.Controls.Add(paletteEditor);
                }

                tabControlArt.TabPages.Add(tabPageArtPalette);
            }

            bool shownExtraDataPage = false;

            void CheckExtraDataPage()
            {
                if (shownExtraDataPage) return;
                tabControlArt.TabPages.Add(tabPageExtraData);
                shownExtraDataPage = true;
            }
            foreach (var asset in artItem.TileMapData)
            {
                CheckExtraDataPage();

                // Make an editor for it
                extraDataLayoutPanel.Controls.Add(new Label { Text = asset.Name, AutoSize = true });
                var editor = new TileMapDataEditor(asset, artItem.TileSet, firstPalette);
                editor.DataChanged += _ => UpdateSpace();
                extraDataLayoutPanel.Controls.Add(editor);
            }

            foreach (var asset in artItem.RawValues)
            {
                CheckExtraDataPage();

                // Make an editor for it
                extraDataLayoutPanel.Controls.Add(new Label { Text = asset.Name, AutoSize = true });
                extraDataLayoutPanel.Controls.Add(new RawValueEditor(asset));
            }

            // The tab control steals focus, we give it back
            listBoxArt.Focus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = e.CloseReason == CloseReason.UserClosing && UserWantsToSaveModifications();
        }

        // Returns true if there are modifications and the user does not want to discard them.
        private bool UserWantsToSaveModifications()
        {
            if (_lastSaved == null)
            {
                return false;
            }

            try
            {
                var current = _cartridge.MakeRom(false);
                if (!current.SequenceEqual(_lastSaved))
                {
                    if (MessageBox.Show(
                            this,
                            "You have unsaved changes. Do you want to discard them?",
                            "Unsaved changes",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(
                        this,
                        $"There may be unsaved changes, however there is currently an error:\n\n{ex.Message}\n\nDo you want to discard them?",
                        "Unsaved changes",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return true;
                }
            }

            return false;
        }

        private void Log(string text)
        {
            logTextBox.BeginInvoke(() =>
            {
                logTextBox.AppendText(text);
                logTextBox.AppendText("\r\n");
            });
        }

        private void buttonSaveScreen_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog();
            sfd.Filter = "*.png|*.png";
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                pictureBoxArtLayout.Image.Save(sfd.FileName);
            }
        }

        private void buttonLoadTileset_Click(object sender, EventArgs e)
        {
            if (listBoxArt.SelectedItem is not ArtItem artItem)
            {
                return;
            }

            try
            {
                using var ofd = new OpenFileDialog();
                ofd.Filter = "*.png|*.png";
                if (ofd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                using var image = (Bitmap)System.Drawing.Image.FromFile(ofd.FileName);
                try
                {
                    artItem.TileMap.FromImage(image, artItem.TileSet);
                }
                catch (Exception ex)
                {
                    // Didn't work, offer a tileset replacement
                    if (MessageBox.Show(
                        this,
                        $"Failed to load image: {ex.Message}. Do you want to try again, replacing the tiles?",
                        "Art mismatch",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // Make the tileset
                        var tileSet = new TileSet(image, artItem.TileSet);
                        // Try to use it
                        try
                        {
                            // Change the art item to use it
                            _cartridge.ChangeTileSet(artItem, tileSet);
                            // Send it to the relevant control
                            otherArtTileSetViewer.SetData(tileSet, artItem.Palettes.First().Value);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(this, $"Failed: {exception.Message}");
                        }
                    }
                }
                pictureBoxArtLayout.Image?.Dispose();
                pictureBoxArtLayout.Image = artItem.TileMap.GetImage(artItem.TileSet, artItem.Palettes.First().Value);

                UpdateSpace();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void otherArtTileSetViewer_Changed(TileSet obj)
        {
            if (listBoxArt.SelectedItem is not ArtItem artItem)
            {
                return;
            }

            if (artItem.TileMap != null)
            {
                pictureBoxArtLayout.Image?.Dispose();
                pictureBoxArtLayout.Image = artItem.TileMap.GetImage(artItem.TileSet, artItem.Palettes.First().Value);
            }

            UpdateSpace();
        }

        private bool _awaitingUpdate;
        private Task _updateTask;
        private bool _loading;

        private void UpdateSpace()
        {
            if (_updateTask is { IsCompleted: false })
            {
                _awaitingUpdate = true;
                return;
            }

            _updateTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    _cartridge.MakeRom(false);
                    BeginInvoke(() => { spaceVisualizer1.SetData(_cartridge.LastFreeSpace, _cartridge.InitialFreeSpace); });
                }
                catch (Exception ex)
                {
                    BeginInvoke(() =>
                    {
                        spaceVisualizer1.SetError(ex.Message);
                    });
                }
                finally
                {
                    BeginInvoke(() =>
                    {
                        if (_awaitingUpdate)
                        {
                            _awaitingUpdate = false;
                            UpdateSpace();
                        }
                    });
                }
            });
        }

        private void spriteTileSetViewer_Changed(TileSet obj)
        {
            UpdateSpace();
        }

        private void floorEditor1_FloorChanged()
        {
            UpdateSpace();
            comboZoom.SelectedIndex = floorEditor1.Zoom - 1;
        }

        private void addObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }
            var index = floorEditor1.LastClickedBlockIndex;
            var x = index % level.FloorWidth;
            var y = index / level.FloorWidth;
            // Make an object
            var levelObject = new LevelObject { X = x, Y = y, Type = LevelObject.Names[0].Type };
            using var editor = new ObjectEditor(levelObject);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                levelObject.X = editor.X;
                levelObject.Y = editor.Y;
                levelObject.Type = editor.Type;

                level.Objects.Add(levelObject);

                // And the level map
                if (floorEditor1.WithObjects)
                {
                    floorEditor1.Invalidate();
                }

                UpdateSpace();
            }
        }

        private void deleteObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var levelObject = GetClickedLevelObject();
            if (levelObject != null && listBoxLevels.SelectedItem is Level level)
            {
                level.Objects.Remove(levelObject);
            }

            // And the level map
            if (floorEditor1.WithObjects)
            {
                floorEditor1.Invalidate();
            }

            UpdateSpace();
        }

        private void textBoxSDSCTitle_TextChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }
            _cartridge.SdscTag = new SdscTag
            {
                Author = textBoxSDSCAuthor.Text,
                Title = textBoxSDSCTitle.Text,
                Notes = textBoxSDSCNotes.Text,
                Date = dateTimePickerSDSCDate.Value.Date,
                Version = udSDSCVersion.Value
            };
            UpdateSpace();
        }

        private void levelEditorContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Disable object items if no object is selected
            editObjectToolStripMenuItem.Enabled = deleteObjectToolStripMenuItem.Enabled = GetClickedLevelObject() != null;
        }
    }
}