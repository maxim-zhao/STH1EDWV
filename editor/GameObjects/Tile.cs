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
            Draw8Bpp(data, 0, 0);
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
            // The incoming data is a rectangle. We want to "unwind" it back to the game data format.
            var index = 0;
            foreach (var point in _grouping)
            {
                var sourceOffset = point.Y * Width + point.X;
                var destinationOffset = index * 8 * 8;
                // We copy the 8x8 square of data at the given point to the given index's position in the data
                for (int row = 0; row < 8; ++row)
                {
                    Array.Copy(data, sourceOffset, _data, destinationOffset, 8);
                    sourceOffset += Width;
                    destinationOffset += 8;
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
            // We draw it in 8x8 chunks...
            var index = 0;
            foreach (var point in _grouping)
            {
                var sourceOffset = index * 8 * 8;
                var destOffset = (y + point.Y) * destination.Stride + x + point.X;
                for (var row = 0; row < 8; ++row)
                {
                    Marshal.Copy(
                        _data,
                        sourceOffset,
                        destination.Scan0 + destOffset,
                        8);
                    sourceOffset += 8;
                    destOffset += destination.Stride;
                }
                ++index;
            }
        }
    }
}