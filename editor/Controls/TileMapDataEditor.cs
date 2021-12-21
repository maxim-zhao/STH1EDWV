using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using sth1edwv.Forms;
using sth1edwv.GameObjects;

namespace sth1edwv.Controls
{
    public sealed partial class TileMapDataEditor : UserControl
    {
        private readonly TileMapData _data;
        private readonly TileSet _tileSet;
        private readonly Palette _palette;
        private readonly int _tileSize;

        private const int TileScale = 4;

        public TileMapDataEditor(TileMapData data, TileSet tileSet, Palette palette)
        {
            AutoSize = true;
            DoubleBuffered = true;
            InitializeComponent();

            _data = data;
            _tileSet = tileSet;
            _palette = palette;
            _tileSize = tileSet.Tiles[0].Height * TileScale;
            udX.Value = data.X;
            udY.Value = data.Y;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(
                panel1.Width + _data.Values.Count * (_tileSize + 1),
                panel1.Height);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(SystemColors.Control);
            var x = 0;
            var y = (Height - _tileSize) / 2;
            foreach (var tile in _data.Values.Select(index => _tileSet.Tiles[index]))
            {
                e.Graphics.DrawImage(tile.GetImage(_palette), new Rectangle(x, y, _tileSize, _tileSize));
                x += _tileSize + 1;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            _data.Values.RemoveAt(_data.Values.Count - 1);
            Size = PreferredSize;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            _data.Values.Add(0);
            Size = PreferredSize;
        }

        private void panel2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.Clicks != 1)
            {
                return;
            }

            // Check if it's on a tile
            var x = e.X / (_tileSize + 1);
            if (x < 0 || x >= _data.Values.Count)
            {
                return;
            }

            var yOffset = (Height - _tileSize) / 2;
            var y = (e.Y - yOffset) / _tileSize;
            if (y != 0)
            {
                return;
            }
            // It is on a tile...
            using var picker = new TileChooser(_tileSet, _data.Values[x], _palette);
            if (picker.ShowDialog(this) == DialogResult.OK)
            {
                // Change it
                var index = picker.SelectedTile.Index;
                if (index >= 255)
                {
                    MessageBox.Show(this, "This tile cannot be used. Index must be less than 255");
                }
                _data.Values[x] = (byte)index;
                Invalidate();
            }
        }

        private void udX_ValueChanged(object sender, EventArgs e)
        {
            _data.X = (byte)udX.Value;
        }

        private void udY_ValueChanged(object sender, EventArgs e)
        {
            _data.Y = (byte)udY.Value;
        }
    }
}
