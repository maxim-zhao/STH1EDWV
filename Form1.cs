using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Be.Windows.Forms;
using Equin.ApplicationFramework;
using Microsoft.VisualBasic;

namespace sth1edwv
{
    public partial class Form1 : Form
    {
        private int _lastX, _lastY;
        private Cartridge _cartridge;
        private readonly ImageList _solidityImages;

        public Form1()
        {
            InitializeComponent();
            _solidityImages = new ImageList
            {
                ColorDepth = ColorDepth.Depth16Bit,
                ImageSize = new Size(32, 32),
            };
            _solidityImages.Images.AddStrip(Properties.Resources.SolidityImages);
        }

        private void openROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog{Filter = "*.sms|*.sms"};
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                _cartridge?.Dispose();
                LoadFile(d.FileName);
            }
        }

        private void LoadFile(string filename)
        {
            _cartridge = new Cartridge(filename);
            _richTextBoxGeneralSummary.Text = _cartridge.MakeSummary();
            RefreshAll();
        }

        private void RefreshAll()
        {
            hexViewer.ByteProvider = new DynamicByteProvider(_cartridge.Memory);
            listBoxMemoryLocations.Items.Clear();
            listBoxMemoryLocations.Items.AddRange(_cartridge.Labels.ToArray<object>());
            listBoxLevels.Items.Clear();
            listBoxLevels.Items.AddRange(_cartridge.Levels.ToArray<object>());
            listBoxPalettes.Items.Clear();
            listBoxPalettes.Items.AddRange(_cartridge.Palettes.ToArray<object>());
            listBoxGameText.Items.Clear();
            listBoxGameText.Items.AddRange(_cartridge.GameText.ToArray<object>());
        }

        private void ListBoxMemoryLocationsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxMemoryLocations.SelectedItem is not Cartridge.MemMapEntry label)
            {
                return;
            }
            hexViewer.SelectionStart = label.Offset;
            hexViewer.SelectionLength = 1;
            hexViewer.ScrollByteIntoView();
        }

        private void ListBoxPalettesSelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxPalette.Image?.Dispose();
            if (listBoxPalettes.SelectedItem is not Palette palette)
            {
                pictureBoxPalette.Image = null;
                return;
            }

            pictureBoxPalette.Image = palette.ToImage(256, 128);
        }

        private void SelectedLevelChanged(object sender, EventArgs e)
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }

            RenderLevel();

            tilePicker1.TileSet = level.TileSet;

            LoadLevelData();

            dataGridViewBlocks.DataSource = new BindingListView<Block>(level.BlockMapping.Blocks);
        }

        private void LoadLevelData()
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }
            treeViewLevelData.Nodes.Clear();
            var t = new TreeNode($"{level}");
            t.Nodes.Add(level.ToNode());
            t.Nodes.Add(level.TileSet.ToNode());
            t.Expand();
            treeViewLevelData.Nodes.Add(t);
        }

        private void LevelRenderModeChanged(object sender, EventArgs e)
        {
            RenderLevel();
        }

        private void RenderLevel()
        {
            if (tabControlLevel.SelectedTab == tabPageLayout && listBoxLevels.SelectedItem is Level level)
            {
                pictureBoxRenderedLevel.Image?.Dispose();
                pictureBoxRenderedLevel.Image = level.Render(buttonShowObjects.Checked, buttonBlockGaps.Checked,
                    buttonTileGaps.Checked, buttonBlockNumbers.Checked);
            }
            else
            {
                pictureBoxRenderedLevel.Image = null;
            }
        }

        private void tilePicker1_SelectionChanged(object sender, Tile tile)
        {
            pictureBoxTilePreview.Image?.Dispose();
            if (tile == null)
            {
                pictureBoxTilePreview.Image = null;
                return;
            }
            var zoom = pictureBoxTilePreview.Width / 8.0f;
            var bmp = new Bitmap(pictureBoxTilePreview.Width, pictureBoxTilePreview.Width);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(zoom, zoom);
                g.DrawImageUnscaled(tile.Image, 0, 0);
            }

            pictureBoxTilePreview.Image = bmp;
        }

        private Block GetSelectedBlock()
        {
            return dataGridViewBlocks.SelectedRows.Count == 0 
                ? null 
                : (dataGridViewBlocks.SelectedRows[0].DataBoundItem as ObjectView<Block>)?.Object;
        }

        private void BlockGridCellEdited(object sender, DataGridViewCellEventArgs e)
        {
            var block = GetSelectedBlock();
            if (block == null)
            {
                return;
            }

            _cartridge.Memory[block.SolidityOffset] = block.Data;
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

            var scale = pictureBoxBlockEditor.Width / 32;
            var bmp = new Bitmap(32 * scale, 32 * scale);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(scale, scale);
                g.DrawImageUnscaled(block.Image, 0, 0);
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
            var scale = pictureBoxBlockEditor.Image.Width / 4;
            var x = e.X / scale;
            var y = e.Y / scale;
            var subBlockIndex = x + y * 4;
            if (subBlockIndex > 15)
            {
                return;
            }
            var tileIndex = block.TileIndices[subBlockIndex];
            using var tc = new TileChooser(block.TileSet, tileIndex);
            tc.ShowDialog(this);
            // Apply to the block object
            block.TileIndices[subBlockIndex] = (byte)tc.SelectedTile.Index;
            // Invalidate its cached image
            block.ResetImage();
            // Trigger a redraw of the editor
            DrawBlockEditor();
            // And the grid
            dataGridViewBlocks.InvalidateRow(dataGridViewBlocks.SelectedRows[0].Index);
            // And the rendered level
            RenderLevel();
            // Finally apply the data to the cartridge
            Array.Copy(block.TileIndices, 0, _cartridge.Memory, block.Offset, block.TileIndices.Length);
        }

        private void saveROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog { Filter = "*.sms|*.sms" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllBytes(dialog.FileName, _cartridge.Memory);
                MessageBox.Show(this, "Done");
            }
        }

        private void TreeViewLevelDataItemSelected(object sender, TreeViewEventArgs e)
        {
            var node = treeViewLevelData.SelectedNode;
            if (node.Tag is not LevelObject levelObject)
            {
                return;
            }

            using var chooser = new ObjectChooser(levelObject);
            if (chooser.ShowDialog(this) == DialogResult.OK)
            {
                levelObject.X = Convert.ToByte(chooser.textBoxX.Text);
                levelObject.Y = Convert.ToByte(chooser.textBoxY.Text);
                levelObject.Type = Convert.ToByte(chooser.textBoxType.Text);

                // Apply to the cartridge
                levelObject.GetData().CopyTo(_cartridge.Memory, levelObject.Offset);

                // Refresh the level data
                LoadLevelData();

                // And the level map may be different
                RenderLevel();
            }
        }

        private void GameTextDoubleClicked(object sender, EventArgs e)
        {
            if (listBoxGameText.SelectedItem is not GameText text)
            {
                return;
            }
            string input = Interaction.InputBox(
                "Please enter new game text, format X;Y;TEXT\n(TEXT can be 'A'-'Z', ' ' and '©')", "Edit game text",
                text.AsSerialized);
            if (input != "")
            {
                try
                {
                    text.Deserialize(input.ToUpperInvariant());
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, exception.Message);
                }

                // This makes the listbox re-get the text
                listBoxGameText.Items[listBoxGameText.SelectedIndex] = text;
            }
        }

        private void toolStripButtonSaveRenderedLevel_Click(object sender, EventArgs e)
        {
            if (pictureBoxRenderedLevel.Image == null)
            {
                return;
            }

            using var d = new SaveFileDialog() { Filter = "*.png|*.png" };
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                pictureBoxRenderedLevel.Image.Save(d.FileName);
            }
        }

        private void LevelMapMouseDown(object sender, MouseEventArgs e)
        {
            _lastX = e.X;
            _lastY = e.Y;
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
            g.DrawString($"{index}", SystemFonts.DefaultFont, SystemBrushes.WindowText, rect.Left + _solidityImages.ImageSize.Width + 4, rect.Top + 8);
        }

        private void dataGridViewBlocks_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox combo)
            {
                combo.DrawMode = DrawMode.OwnerDrawFixed;
                combo.ItemHeight = _solidityImages.ImageSize.Height + 1;
                combo.DrawItem -= DrawSolidityComboItem;
                combo.DrawItem += DrawSolidityComboItem;
            }
        }

        private void DrawSolidityComboItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                DrawSolidityItem(e.Graphics, e.Index, e.Bounds);
            }
        }

        private void dataGridViewBlocks_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void tabControlLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlLevel.SelectedTab == tabPageLayout &&
                pictureBoxRenderedLevel.Image == null)
            {
                RenderLevel();
            }
        }

        private void pb3_MouseUp(object sender, MouseEventArgs e)
        {
            if (listBoxLevels.SelectedItem is not Level level)
            {
                return;
            }

            var x = e.X;
            var y = e.Y;
            level.AdjustPixelsToTile(ref x, ref y);
            level.AdjustPixelsToTile(ref _lastX, ref _lastY);

            var minX = Math.Min(x, _lastX);
            var maxX = Math.Max(x, _lastX);
            var minY = Math.Min(y, _lastY);
            var maxY = Math.Max(y, _lastY);
            using var bc = new BlockChooser(level);
            bc.SelectedBlockIndex = level.Floor.BlockIndices[x + y * level.Floor.Width];
            if (bc.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Make a backup
            var temp = (byte[])level.Floor.BlockIndices.Clone();

            // Apply the change
            for (var row = minY; row <= maxY; row++)
            for (var col = minX; col <= maxX; col++)
            {
                level.Floor.BlockIndices[col + row * level.Floor.Width] = (byte)bc.SelectedBlockIndex;
            }

            // Try to insert it
            var newData = level.Floor.GetData();
            if (newData.Count > level.Floor.LengthConsumed)
            {
                MessageBox.Show(this, "Cannot compress level enough to fit into ROM.");
                level.Floor.BlockIndices = temp;
                return;
            }

            // Apply to the ROM
            newData.CopyTo(_cartridge.Memory, level.Floor.Offset);
            // We also have to change the level header to match
            _cartridge.Memory[level.Offset + 17] = (byte)(newData.Count & 0xff);
            _cartridge.Memory[level.Offset + 17] = (byte)(newData.Count >> 8);

            // Redraw the level
            RenderLevel();
        }
    }
}