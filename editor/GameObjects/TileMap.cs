using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace sth1edwv.GameObjects
{
    public class TileMap: IDataItem, IDisposable
    {
        private List<ushort> _data;

        public TileMap(Memory memory, int offset, int size)
        {
            Offset = offset;
            _data = Compression.DecompressRle(memory, offset, size)
                .Select(b => (ushort)b)
                .ToList();
        }

        public void SetAllForeground()
        {
            for (var i = 0; i < _data.Count; i++)
            {
                _data[i] |= 0x1000;
            }
        }

        public void OverlayWith(TileMap other)
        {
            for (var i = 0; i < _data.Count; i++)
            {
                if (other._data[i] != 0xff)
                {
                    _data[i] = other._data[i];
                }
            }
        }

        public Bitmap GetImage(TileSet tileSet, Palette palette)
        {
            var image = new Bitmap(256, 192, PixelFormat.Format8bppIndexed);
            // We work in 8bpp again...
            image.Palette = palette.ImagePalette;
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            try
            {
                for (var i = 0; i < _data.Count; ++i)
                {
                    var x = i % 32 * 8;
                    var y = i / 32 * 8;
                    var index = _data[i];
                    // Tile 0xff is unusable
                    if (index != 0xff)
                    {
                        // Mask off high bits
                        index &= 0xff;
                        if (index < tileSet.Tiles.Count)
                        {
                            tileSet.Tiles[index].Draw8Bpp(data, x, y);
                        }
                    }
                }

                return image; // Caller must dispose
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }


        public int Offset { get; set; }
        public int ForegroundTileMapSize { get; private set; }
        public int BackgroundTileMapSize { get; private set; }

        public IList<byte> GetData()
        {
            if (!_data.Any(x => x > 0xff))
            {
                // Single tilemap mode
                var tileMap = Compression.CompressRle(_data.Select(x => (byte)x).ToArray());
                BackgroundTileMapSize = tileMap.Length;
                ForegroundTileMapSize = 0;
                return tileMap;
            }
            // Two tilemaps, first is the foreground tiles
            var foregroundTileMap = Compression.CompressRle(_data.Select(x => x > 0xff ? (byte)(x & 0xff) : (byte)0xff).ToArray());
            var backgroundTileMap = Compression.CompressRle(_data.Select(x => x > 0xff ? (byte)0xff : (byte)(x & 0xff)).ToArray());
            ForegroundTileMapSize = foregroundTileMap.Length;
            BackgroundTileMapSize = backgroundTileMap.Length;
            return foregroundTileMap.Concat(backgroundTileMap).ToList();
        }

        public bool IsOverlay()
        {
            return _data.Any(x => (x & 0xff) == 0xff);
        }

        public void FromImage(Bitmap image, TileSet tileSet)
        {
            // First check dimensions
            if (image.Width != 256 || image.Height != 192)
            {
                throw new Exception($"Image is {image.Width}x{image.Height}, must be 256x192");
            }

            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                throw new Exception($"Image must be 8bpp with no transparency");
            }

            // Next we need to get the data into tiles...
            var data = image.LockBits(new Rectangle(0, 0, 256, 192), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            try
            {
                var indices = new List<ushort>();
                for (var y = 0; y < 192; y += 8)
                for (var x = 0; x < 256; x += 8)
                {
                    var buffer = new byte[8 * 8];

                    for (var row = 0; row < 8; ++row)
                    {
                        Marshal.Copy(
                            data.Scan0 + (y + row) * data.Stride + x,
                            buffer,
                            row * 8,
                            8);
                    }

                    var tileIndex = tileSet.Tiles.FindIndex(x => x.Matches(buffer));
                    if (tileIndex < 0)
                    {
                        throw new Exception($"Tile mismatch: tile in source image at ({x}, {y}) not found in tiles");
                    }

                    if (tileIndex >= 255)
                    {
                        throw new Exception($"Tile index too high: tile in source image at ({x}, {y}) has tile index {tileIndex}; this must be less than 255");
                    }

                    indices.Add((ushort)tileIndex);
                }

                // If we get here then all is well
                _data = indices;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }

        public void Dispose()
        {
        }
    }
}