using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace sth1edwv.GameObjects
{
    public class Palette: IDataItem
    {
        private readonly List<Color> _colors = [];
        private ColorPalette _imagePalette;

        public ColorPalette ImagePalette
        {
            get
            {
                if (_imagePalette != null)
                {
                    return _imagePalette;
                }
                // We have to extract a ColorPalette from an image as it blocks us from constructing it...
                using var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
                _imagePalette = bmp.Palette;
                // We have to clear the whole palette to non-transparent colours to avoid (1) the default palette and (2) an image that GDI+ transforms to 32bpp on load
                for (var i = 0; i < 256; ++i)
                {
                    _imagePalette.Entries[i] = Color.Magenta;
                }

                // Then we apply the real palette to the start
                _colors.CopyTo(_imagePalette.Entries, 0);
                return _imagePalette;
            }
        }

        public Palette(IReadOnlyList<byte> mem, int offset, int count)
        {
            Offset = offset;
            for (var i = 0; i < 16*count; i++)
            {
                var color = mem[offset + i];
                var r = ScaleColor((color >> 0) & 0b11);
                var g = ScaleColor((color >> 2) & 0b11);
                var b = ScaleColor((color >> 4) & 0b11);
                _colors.Add(Color.FromArgb(r, g, b));
            }
        }

        private Palette(IEnumerable<Color> colors, int offset, int count)
        {
            _colors.AddRange(colors.Skip(offset).Take(count));
        }

        public override string ToString()
        {
            return Offset > 0 
                ? $"{_colors.Count} colours @ {Offset:X}" 
                : $"{_colors.Count} colours (copy, do not save this)";
        }

        private static int ScaleColor(int c) => c switch
        {
            0 => 0x00,
            1 => 0x55,
            2 => 0xaa,
            _ => 0xff
        };

        /*
        public static IEnumerable<Palette> ReadPalettes(Memory mem, int offset, int count)
        {
            var counts = new[]
            {
                2, 2, 2, 2, 2, 2, 2, 2, // Regular tile+sprite palettes
                3, 3, 3, 3, 4, 12, 4, 1, 4 // Cycling palette 
            };

            for (var i = 0; i < count; i++)
            {
                var address = mem.Word(offset);
                var palette = new Palette(mem, address, counts[i]);
                yield return palette;
                offset += 2;
            }
        }*/

        public Image ToImage(int width)
        {
            var rows = _colors.Count / 16;
            var rectSize = width / 16;
            var bmp = new Bitmap(width, rows * rectSize);
            using var g = Graphics.FromImage(bmp);
            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    var index = x + y * 16;
                    using var b = new SolidBrush(_colors[index]);
                    g.FillRectangle(b, rectSize*x, rectSize*y, rectSize, rectSize);
                }
            }

            return bmp;
        }

        public Palette GetSubPalette(int start, int count)
        {
            return new Palette(_colors, start, count);
        }

        public void SaveAsText(string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("JASC-PAL")
                .AppendLine("0100")
                .AppendLine("256");
            foreach (var color in _colors)
            {
                sb.AppendLine($"{color.R} {color.G} {color.B}");
            }
            // Pad to 256 colours
            for (var i = 0; i < 256 - _colors.Count; ++i)
            {
                sb.AppendLine("255 0 255");
            }
            File.WriteAllText(fileName, sb.ToString());
        }

        public void SaveAsImage(string fileName)
        {
            using var image = ToImage(16);
            image.Save(fileName);
        }

        public void LoadFromText(string fileName)
        {
            var lines = File.ReadLines(fileName).ToList();
            // We are not very tolerant...
            if (lines.Count != 256 + 3 ||
                lines[0] != "JASC-PAL" ||
                lines[1] != "0100" ||
                lines[2] != "256")
            {
                throw new Exception("Incorrect palette file");
            }

            for (var i = 0; i < _colors.Count; ++i)
            {
                var match = Regex.Match(lines[i + 3], "(?<R>\\d+) (?<G>\\d+) (?<B>\\d+)");
                if (!match.Success)
                {
                    throw new Exception($"Failed to parse line: {lines[i + 3]}");
                }

                // Local function...
                int GetIndex(string s)
                {
                    var x = Convert.ToInt32(match.Groups[s].Value);
                    if (x is < 0 or > 255)
                    {
                        throw new Exception($"Value out of range: {x}");
                    }

                    return x;
                }

                _colors[i] = Color.FromArgb(GetIndex("R"), GetIndex("G"), GetIndex("B"));
            }
            // Reset the image palette
            _imagePalette = null;
        }

        public void LoadFromImage(string fileName)
        {
            using var image = Image.FromFile(fileName);
            if (image is not Bitmap bitmap)
            {
                throw new Exception("Unsupported image format");
            }

            // Rounds a colour to the SMS palette
            Color ToSms(Color color) =>
                Color.FromArgb(
                    (color.R + 42) / 0x55 * 0x55,
                    (color.G + 42) / 0x55 * 0x55,
                    (color.B + 42) / 0x55 * 0x55);

            // If it's an 8-bit paletted image, we copy its palette.
            // If it's anything else, we expect a 16px wide image.
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // We read the colours out of its palette
                var palette = bitmap.Palette;
                if (palette.Entries.Length < _colors.Count)
                {
                    throw new Exception($"Image palette has {palette.Entries.Length} colours, we need at least {_colors.Count}");
                }
                for (var i = 0; i < _colors.Count; ++i)
                {
                    _colors[i] = ToSms(palette.Entries[i]);
                }
            }
            else
            {
                // We read the colours straight out of the bitmap
                if (image.Width * image.Height < _colors.Count)
                {
                    throw new Exception($"Image is too small to define {_colors.Count} colours");
                }
                for (var i = 0; i < _colors.Count; ++i)
                {
                    _colors[i] = ToSms(bitmap.GetPixel(i % 16, i / 16));
                }
            }

            // Reset the image palette
            _imagePalette = null;
        }

        public int Offset { get; set; } = -1;
        public IList<byte> GetData()
        {
            // We truncate each colour to its top two bits and merge...
            return _colors.Select(color => 
                (byte)(
                    ((color.R >> 6) << 0) | 
                    ((color.G >> 6) << 2) | 
                    ((color.B >> 6) << 4)))
                .ToList();
        }
    }
}