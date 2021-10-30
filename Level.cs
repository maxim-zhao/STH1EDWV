using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace sth1edwv
{
    public class Level: IDataItem
    {
        //   SP FW FW FH  FH LX LX ??  LW LY LY XH  LH SX SY FL 
        //   FL FS FS BM  BM LA LA 09  SA SA IP CS  CC CP OL OL 
        //   SR UW TL 00  MU
        private byte[] _header;
        private readonly byte   _solidityIndex;
        private readonly ushort _floorWidth;
        private readonly ushort _floorHeight;
        public readonly int   floorAddress;
        public ushort floorSize;
        private readonly int   _blockMappingAddress;
        private readonly ushort _levelXOffset;
        private readonly byte   _levelWidth;
        private readonly ushort _levelYOffset;
        private readonly byte   _levelExtHeight;
        private readonly byte   _levelHeight;
        private readonly ushort _offsetArt;
        public readonly ushort offsetObjectLayout;
        private readonly byte   _initPalette;
        public TileSet TileSet { get; }
        public Floor Floor { get; }
        public LevelObjectSet Objects { get; }

        public BlockMapping BlockMapping { get; }
        private readonly string _label;
        private int _blockSize;

        public Level(Cartridge cartridge, int offset, int artBanksTableOffset, IList<Palette> palettes, string label)
        {
            _label = label;
            var address = BitConverter.ToUInt16(cartridge.Memory, offset);
            Offset = address + 0x15580;
            LengthConsumed = 37;
            _header = new byte[37];
            Array.Copy(cartridge.Memory, address + 0x15580, _header, 0, _header.Length);


            _solidityIndex = _header[0];
            _floorWidth = BitConverter.ToUInt16(_header, 1);
            _floorHeight = BitConverter.ToUInt16(_header, 3);
            if (address == 666)
                _floorHeight /= 2;
            floorAddress = BitConverter.ToUInt16(_header, 15) + 0x14000;
            floorSize = BitConverter.ToUInt16(_header, 17);
            _blockMappingAddress = BitConverter.ToUInt16(_header, 19) + 0x10000;
            _levelXOffset = BitConverter.ToUInt16(_header, 5);
            _levelWidth = _header[8];
            _levelYOffset = BitConverter.ToUInt16(_header, 9);
            _levelExtHeight = _header[11];
            _levelHeight = _header[12];
            _offsetArt = BitConverter.ToUInt16(_header, 21);
            offsetObjectLayout = BitConverter.ToUInt16(_header, 30);
            _initPalette = _header[26];
            if (artBanksTableOffset > 0)
            {
                var levelIndex = (offset - 0x15580) / 2;
                var artBank = cartridge.Memory[artBanksTableOffset + levelIndex];
                TileSet = cartridge.GetTileSet(_offsetArt + artBank * 0x4000, palettes[_initPalette]);
            }
            else
            {
                TileSet = cartridge.GetTileSet(_offsetArt + 0x30000, palettes[_initPalette]);
            }
            Floor = cartridge.GetFloor(floorAddress, floorSize, _floorWidth);
            BlockMapping = cartridge.GetBlockMapping(_blockMappingAddress, _solidityIndex, TileSet);
            Objects = new LevelObjectSet(cartridge, 0x15580 + offsetObjectLayout);
        }

        public override string ToString()
        {
            return _label;
        }

        public TreeNode ToNode()
        {
            var result = new TreeNode("Header")
            {
                Nodes =
                {
                    new TreeNode($"Floor Size           = ({_floorWidth} x {_floorHeight})"),
                    new TreeNode($"Floor Data           = (@0x{floorAddress:X} Size: 0x{floorSize:X})"),
                    new TreeNode($"Level Size           = ({_levelWidth} x {_levelHeight})"),
                    new TreeNode($"Level Offset         = (dx:{_levelXOffset} dy:{_levelYOffset})"),
                    new TreeNode($"Extended Height      = {_levelExtHeight}"),
                    new TreeNode($"Offset Art           = 0x{_offsetArt:X8}"),
                    new TreeNode($"Offset Object Layout = 0x{offsetObjectLayout:X8}"),
                    new TreeNode($"Initial Palette      = {_initPalette}"),
                    Objects.ToNode()
                }
            };
            result.Expand();
            return result;
        }

        public Bitmap Render(bool withObjects, bool blockGaps, bool tileGaps, bool blockLabels)
        {
            _blockSize = 32;
            var tileSize = 8;
            if (tileGaps)
            {
                _blockSize += 4;
                ++tileSize;
            }

            if (blockGaps)
            {
                ++_blockSize;
            }

            const int labelWidth = 13;
            const int labelHeight = 9;

            var result = new Bitmap(_floorWidth * _blockSize, _floorHeight * _blockSize);
            using var f = new Font("Consolas", 8, FontStyle.Bold);
            using var g = Graphics.FromImage(result);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.White);

            for (var blockX = 0; blockX < _floorWidth; ++blockX)
            for (var blockY = 0; blockY < _floorHeight; ++blockY)
            {
                var blockIndex = Floor.BlockIndices[blockX + blockY * _floorWidth];
                var block = BlockMapping.Blocks[blockIndex];
                for (var tileX = 0; tileX < 4; ++tileX)
                for (var tileY = 0; tileY < 4; ++tileY)
                {
                    var tileIndex = block.TileIndices[tileX + tileY * 4];
                    var tile = TileSet.Tiles[tileIndex];
                    var x = blockX * _blockSize + tileX * tileSize;
                    var y = blockY * _blockSize + tileY * tileSize;
                    g.DrawImageUnscaled(tile.Image, x, y);
                }

                if (blockLabels)
                {
                    g.DrawString(blockIndex.ToString("X2"), f, Brushes.Black, blockX * _blockSize, blockY * _blockSize - 1);
                    g.DrawString(blockIndex.ToString("X2"), f, Brushes.White, blockX * _blockSize - 1, blockY * _blockSize - 2);
                }
            }

            if (withObjects)
            {
                var image = Properties.Resources.package;
                // Draw objects
                foreach (var levelObject in Objects)
                {
                    var x = levelObject.X * _blockSize;
                    var y = levelObject.Y * _blockSize;
                    g.DrawRectangle(Pens.Blue, x, y, _blockSize, _blockSize);
                    g.DrawImageUnscaled(image, x + _blockSize / 2 - image.Width / 2, y + _blockSize / 2 - image.Height / 2);

                    x += _blockSize - labelWidth;
                    y += _blockSize - labelHeight;
                    g.FillRectangle(Brushes.Blue, x, y, labelWidth, labelHeight);
                    g.DrawString(levelObject.Type.ToString("X2"), f, Brushes.White, x - 1, y - 2);
                }
            }

            return result;
        }

        public void AdjustPixelsToTile(ref int x, ref int y)
        {
            x /= _blockSize;
            y /= _blockSize;
        }

        public int Offset { get; }
        public int LengthConsumed { get; }
        public IList<byte> GetData()
        {
            throw new NotImplementedException();
        }
    }
}
