using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using sth1edwv.GameObjects;

namespace sth1edwv.Controls
{
    public partial class TileSetViewer : UserControl
    {
        private Palette _palette;

        public TileSetViewer()
        {
            InitializeComponent();
            toolStrip2.Items.Add(new ToolStripControlHost(checkBoxTransparency));
        }

        public event Action<TileSet> Changed;

        public int TilesPerRow
        {
            get => tilePicker.ItemsPerRow;
            set => tilePicker.ItemsPerRow = value;
        }

        public void SetData(TileSet tileSet, Palette palette,
            Func<int, IList<Block>> getUsedInBlocks = null,
            bool allowShowingTransparency = false)
        {
            _tileSet = tileSet;
            _palette = palette;
            tilePicker.SetData(tileSet?.Tiles, palette);
            tilePicker.SelectedIndex = -1;
            _getUsedInBlocks = getUsedInBlocks;
            buttonBlankUnusedTiles.Visible = getUsedInBlocks != null;
            splitContainer5.Panel2Collapsed = getUsedInBlocks == null;
            checkBoxTransparency.Visible = allowShowingTransparency;
            checkBoxTransparency.Checked = allowShowingTransparency;
            tilePicker.ShowTransparency = checkBoxTransparency.Checked;
        }

        private void tilePicker_SelectionChanged(object sender, IDrawableBlock b)
        {
            pictureBoxTilePreview.Image = null;
            pictureBoxTilePreview.Image?.Dispose();
            pictureBoxTileUsedIn.Image = null;
            pictureBoxTileUsedIn.Image?.Dispose();
            if (b == null)
            {
                return;
            }
            var zoom = Math.Min((float)pictureBoxTilePreview.Width / b.Width, (float)pictureBoxTilePreview.Height / b.Height);
            var bmp = new Bitmap((int)(b.Width * zoom), (int)(b.Height * zoom));
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(zoom, zoom);
                g.DrawImageUnscaled(b.GetImage(_palette), 0, 0);
            }

            pictureBoxTilePreview.Image = bmp;

            if (_getUsedInBlocks == null || b is not Tile tile)
            {
                return;
            }
            var blocks = _getUsedInBlocks((byte)tile.Index);
            if (blocks.Count == 0)
            {
                return;
            }

            var image = new Bitmap(blocks.Count * 33 - 1, 48);
            var x = 0;
            using (var g = Graphics.FromImage(image))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(SystemColors.Window);
                foreach (var block in blocks)
                {
                    g.DrawImageUnscaled(block.GetImage(_palette), x, 0);
                    g.DrawString(block.Index.ToString("X2"), Font, SystemBrushes.WindowText, 
                        new RectangleF(x, block.Height, block.Width, 16),
                        new StringFormat
                        {
                            Alignment = StringAlignment.Center
                        });
                    x += block.Width + 1;
                }
            }

            pictureBoxTileUsedIn.Image = image;

        }

        private Func<int, IList<Block>> _getUsedInBlocks;
        private TileSet _tileSet;

        private void buttonSaveTileset_Click(object sender, EventArgs e)
        {
            using var d = new SaveFileDialog { Filter = "PNG images|*.png" };
            if (d.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            using var image = _tileSet.ToImage(_palette, tilePicker.ItemsPerRow);
            image.Save(d.FileName, ImageFormat.Png);
        }

        private void buttonLoadTileset_Click(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog { Filter = "Images|*.png" };
            if (d.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // This way avoids locking the file
            using var image = (Bitmap) new ImageConverter().ConvertFrom(File.ReadAllBytes(d.FileName));
            if (image == null)
            {
                MessageBox.Show(this, "Failed to load image");
                return;
            }

            try
            {
                _tileSet.FromImage(image);
                tilePicker.Invalidate();
                Changed?.Invoke(_tileSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void buttonBlankUnusedTiles_Click(object sender, EventArgs e)
        {
            if (_getUsedInBlocks == null)
            {
                return;
            }
            foreach (var tile in _tileSet.Tiles.Where(t => _getUsedInBlocks(t.Index).Count == 0))
            {
                tile.Blank();
            }
            tilePicker.Invalidate();
            tilePicker_SelectionChanged(tilePicker, tilePicker.SelectedItem);
            Changed?.Invoke(_tileSet);
        }

        private void checkBoxTransparency_CheckedChanged(object sender, EventArgs e)
        {
            tilePicker.ShowTransparency = checkBoxTransparency.Checked;
        }

        private void buttonAddTile_Click(object sender, EventArgs e)
        {
            _tileSet.AddTile();
            Changed?.Invoke(_tileSet);
            tilePicker.SetData(_tileSet.Tiles, _palette);
        }

        private void buttonRemoveTile_Click(object sender, EventArgs e)
        {
            _tileSet.RemoveTile();
            Changed?.Invoke(_tileSet);
            tilePicker.SetData(_tileSet.Tiles, _palette);
        }
    }
}
