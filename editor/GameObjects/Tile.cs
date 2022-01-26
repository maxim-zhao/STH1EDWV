using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace sth1edwv.GameObjects
{
    public class Tile: IDisposable, IDrawableBlock
    {
        private readonly byte[] _data;
        private readonly List<Point> _grouping;

        public int Index { get; }
        public int Height { get; }
        public int Width { get; }

        // Images are per-palette
        private readonly Dictionary<Palette, Bitmap> _images = new();
        
        public Bitmap GetImage(Palette palette)
        {
            if (_images.TryGetValue(palette, out var image))
            {
                return image;
            }

            // We render only when needed. We do stick to paletted images though..
            image = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            image.Palette = palette.ImagePalette;
            var data = image.LockBits(
                new Rectangle(0, 0, 8, Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);
            int offset = 0;
            foreach (var point in _grouping)
            {
                for (var row = 0; row < 8; ++row)
                {
                    // Copy indices one row at a time
                    Marshal.Copy(_data, offset + row * 8, data.Scan0 + (point.Y + row) * data.Stride + point.X, 8);
                }

                offset += 8 * 8;
            }

            image.UnlockBits(data);
            _images.Add(palette, image);
            return image;
        }

        public Tile(byte[] data, List<Point> grouping, int index)
        {
            Index = index;
            Width = grouping.Max(p => p.X) + 8;
            Height = grouping.Max(p => p.Y) + 8;
            _data = data;
            _grouping = grouping;
        }

        public void WriteTo(MemoryStream ms)
        {
            ms.Write(_data, 0, _data.Length);
        }

        public void Dispose()
        {
            ResetImages();
        }

        public void ResetImages()
        {
            foreach (var image in _images.Values)
            {
                image.Dispose();
            }
            _images.Clear();
        }

        public void SetData(byte[] data)
        {
            // The incoming data is a rectangle. We want to "wind" it back to the game data format.
            var index = 0;
            foreach (var point in _grouping)
            {
                // We copy the data at the given point to the given index's position in the data
                for (int row = 0; row < 8; ++row)
                {
                    var sourceOffset = (row + point.Y) * Width + point.X;
                    var destinationOffset = index * 64 + row * 8;
                    Array.Copy(data, sourceOffset, _data, destinationOffset, 8);
                }

                ++index;
            }

            ResetImages();
        }

        public void Blank()
        {
            Array.Clear(_data, 0, _data.Length);
            ResetImages();
        }

        public bool Matches(byte[] buffer)
        {
            return _data.SequenceEqual(buffer);
        }

        public void Draw8Bpp(BitmapData destination, int x, int y)
        {
            // We use our data array directly...
            for (var row = 0; row < 8; ++row)
            {
                Marshal.Copy(
                    _data,
                    row*8,
                    destination.Scan0 + (row + y) * destination.Stride + x,
                    8);
            }
        }
    }
}