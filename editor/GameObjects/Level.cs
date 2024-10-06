using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace sth1edwv.GameObjects
{
    public class Level : IDataItem
    {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global

        [Category("Level bounds")]
        public int FloorWidth { get; internal set; }

        [Category("Level bounds")]
        public int FloorHeight { get; internal set; }

        [Category("Level bounds")]
        [Description("Pixel location of the left of the level")]
        public int LeftPixels { get; set; }

        [Category("Level bounds")]
        [Description("Pixel location of the top of the level")]
        public int TopPixels { get; set; }

        [Category("Level bounds")]
        [Description("Right side of the level in blocks, divided by 8. The screen shows 256px more than this.")] // TODO: encapsulate this?
        public int RightEdgeFactor { get; set; }

        [Category("Level bounds")]
        [Description("Bottom edge of the level in blocks, divided by 8. The screen shows 192px more than this.")] // TODO: encapsulate this?
        public int BottomEdgeFactor { get; set; }

        [Category("Level bounds")]
        [Description("Extra pixels to add to the level height")] // TODO: encapsulate this?
        public int ExtraHeight { get; set; }

        [Category("Start location")] 
        [Description("Block location of Sonic's start position")]
        public int StartX { get; set; }
        
        [Category("Start location")] 
        [Description("Block location of Sonic's start position")]
        public int StartY { get; set; }

        [Category("Level flags")]
        [Description("The level automatically scrolls to the right (like Bridge Act 2)")]
        public bool AutoScrollRight { get; set; }

        [Category("Level flags")]
        [Description("After a pause, the level automatically scrolls upwards! If you get caught at the bottom of the screen, you die")]
        public bool AutoScrollUp { get; set; }

        [Category("Level flags")]
        [Description("The demo play data controls Sonic")]
        public bool DemoMode { get; set; }

        [Category("Level flags")]
        [Description("Locks the screen, no scrolling occurs")]
        public bool DisableScrolling { get; set; }

        [Category("Level flags")]
        [Description("Uses the lightning effect. This overrides the level's own palette")]
        public bool HasLightning { get; set; }

        [Category("Level flags")]
        [Description("Controls the under-water effect (slow movement / water colour / drowning)")]
        public bool HasWater { get; set; }

        [Category("Level flags")]
        [Description("Screen does not scroll down (like Jungle Act 2). If you get caught at the bottom of the screen, you die")]
        public bool NoScrollDown { get; set; }

        [Category("Level flags")]
        [Description("Shows ring count in HUD and rings are displayed. When turned off, no rings are visible, but the sparkle effect still occurs when you collect them")]
        public bool ShowRings { get; set; }

        [Category("Level flags")]
        [Description("Displays the time")]
        public bool ShowTime { get; set; }

        [Category("Level flags")]
        [Description("The screen scrolls smoothly, allowing you to get ahead of it")]
        public bool SlowScroll { get; set; }

        [Category("Level flags")]
        [Description("Centers the time display when on a special stage. Outside of the special stage causes the game to switch to the special stage")]
        public bool SpecialStageTimer { get; set; }

        [Category("Level flags")]
        [Description("Slow up and down wave effect (like Sky Base Act 2)")]
        public bool WaveScrollY { get; set; }

        [Category("Palette")]
        [Description("Number of frames between palette changes, 0 to disable")]
        public int PaletteCycleRate { get; set; }

        [Category("Palette")]
        [Description("Use the boss underwater palette (like Labyrinth Act 3)")]
        public bool UseUnderwaterBossPalette { get; set; }

        [Category("General")] 
        [Description("Which music track to play. Value 7 can be used for silence.")]
        public int MusicIndex { get; set; }

        // Objects representing referenced data
        [Category("General")] public TileSet TileSet { get; }
        [Category("General")] public TileSet SpriteTileSet { get; }
        [Category("General")] public Floor Floor { get; set; }
        [Category("General")] public LevelObjectSet Objects { get; }
        [Category("General")] public BlockMapping BlockMapping { get; }
        [Category("General")] public Palette Palette { get; }
        [Category("General")] public Palette CyclingPalette { get; }

        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper restore MemberCanBePrivate.Global

        [ReadOnly(true)]
        [Category("General")] public int Offset { get; set; }

        private readonly string _label;
        private readonly int _initPalette;

        // These should be encapsulated by a cycling palette object
        private readonly int _paletteCycleCount;
        private readonly int _paletteCycleIndex;

        private readonly int _solidityIndex;

        private readonly byte _unknownByte;

        public Level(Cartridge cartridge, int offset, string label)
        {
            int offsetObjectLayout;
            int offsetArt;
            int floorSize;
            int floorAddress;
            _label = label;
            Offset = offset;
            int blockMappingOffset;
            int spriteArtPage;
            int spriteArtAddress;

            using (var stream = cartridge.Memory.GetStream(offset, 37))
            {
                using (var reader = new BinaryReader(stream))
                {
                    // SP FW FW FH  FH LX LX ??  LW LY LY XH  LH SX SY FL 
                    // FL FS FS BM  BM LA LA SP  SA SA IP CS  CC CP OL OL 
                    // SR UW TL 00  MU
                    _solidityIndex = reader.ReadByte(); // SP
                    FloorWidth = reader.ReadUInt16(); // FW FW
                    FloorHeight = reader.ReadUInt16(); // FH FH
                    LeftPixels = reader.ReadUInt16(); // LX LX
                    _unknownByte = reader.ReadByte();  // ??
                    RightEdgeFactor = reader.ReadByte();  // LW
                    TopPixels = reader.ReadUInt16(); // LY LY
                    ExtraHeight = reader.ReadByte(); // XH
                    BottomEdgeFactor = reader.ReadByte(); // LH
                    StartX = reader.ReadByte(); // SX
                    StartY = reader.ReadByte(); // SY
                    floorAddress = reader.ReadUInt16(); // FL FL: relative to 0x14000
                    floorSize = reader.ReadUInt16(); // FS FS: compressed size in bytes
                    blockMappingOffset = reader.ReadUInt16(); // BM BM: relative to 0x10000
                    offsetArt = reader.ReadUInt16(); // LA LA: Relative to 0x30000
                    spriteArtPage = reader.ReadByte(); // SP: Page for the below
                    spriteArtAddress = reader.ReadUInt16(); // SA SA: offset from start of above bank
                    _initPalette = reader.ReadByte(); // IP: Index of palette
                    PaletteCycleRate = reader.ReadByte(); // CS: Number of frames between palette cycles
                    _paletteCycleCount = reader.ReadByte(); // CC: Number of palette cycles in a loop
                    _paletteCycleIndex = reader.ReadByte(); // CP: Which cycling palette to use
                    offsetObjectLayout = reader.ReadUInt16(); // OL OL: relative to 0x15580
                    var flags = reader.ReadByte(); // SR
                    // Nothing for bit 0
                    DemoMode = (flags & (1 << 1)) != 0;
                    ShowRings = (flags & (1 << 2)) != 0;
                    AutoScrollRight = (flags & (1 << 3)) != 0;
                    AutoScrollUp = (flags & (1 << 4)) != 0;
                    SlowScroll = (flags & (1 << 5)) != 0;
                    WaveScrollY = (flags & (1 << 6)) != 0;
                    NoScrollDown = (flags & (1 << 7)) != 0;
                    flags = reader.ReadByte(); // UW
                    // Nothing for bits 0..6
                    HasWater = (flags & (1 << 7)) != 0;
                    flags = reader.ReadByte(); // TL
                    SpecialStageTimer = (flags & (1 << 0)) != 0;
                    HasLightning = (flags & (1 << 1)) != 0;
                    // Nothing for bits 2, 3
                    UseUnderwaterBossPalette = (flags & (1 << 4)) != 0;
                    ShowTime = (flags & (1 << 5)) != 0;
                    DisableScrolling = (flags & (1 << 6)) != 0;
                    // Nothing for bit 7
                    // Skip unknown byte
                    reader.ReadByte(); // 00
                    MusicIndex = reader.ReadByte(); // MU
                }       
            }

            Palette = cartridge.GetPalette(cartridge.Memory.Word(0x627C + _initPalette*2), 2);
            CyclingPalette = cartridge.GetPalette(cartridge.Memory.Word(0x628C + _paletteCycleIndex*2), _paletteCycleCount);

            TileSet = cartridge.GetTileSet(offsetArt + 0x30000, null, 16);

            SpriteTileSet = cartridge.GetTileSet(spriteArtAddress + spriteArtPage * 0x4000, TileSet.Groupings.Sprite, 16);

            Floor = cartridge.GetFloor(
                floorAddress + 0x14000, 
                floorSize, 
                FloorWidth);

            // We rewrite FloorHeight to match the data size.
            // The original has a mistake in Scrap Brain 2 (BallHog area).
            FloorHeight = Floor.BlockIndices.Length / FloorWidth;

            // Hard-coded block counts. It's hard to avoid these...
            // we could imply them from the gaps but then we'd need to know all the offsets
            // (and whatever comes after - the monitor items art by default) before loading anything.
            // If/when we support changing the mapping sizes, we will need to address this.
            var blockCount = blockMappingOffset switch
            {
                0x0000 => 184,
                0x0B80 => 144,
                0x1480 => 160,
                0x1E80 => 176,
                0x2980 => 192,
                0x3580 => 216,
                0x4300 => 104,
                0x4980 => 128,
                _ => 0
            };

            BlockMapping = cartridge.GetBlockMapping(blockMappingOffset + 0x10000, blockCount, _solidityIndex, TileSet);
            Objects = cartridge.GetLevelObjectSet(0x15580 + offsetObjectLayout);

            // We generate sub-palettes for rendering
            UpdateRenderingPalettes();
        }

        [Browsable(false)]
        public Palette SpritePalette { get; private set; }

        [Browsable(false)]
        public Palette TilePalette { get; private set; }


        public override string ToString()
        {
            return _label;
        }

        public IList<byte> GetData()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            // SP FW FW FH  FH LX LX ??  LW LY LY XH  LH SX SY FL 
            // FL FS FS BM  BM LA LA SP  SA SA IP CS  CC CP OL OL 
            // SR UW TL 00  MU
            writer.Write((byte)_solidityIndex); // SP
            writer.Write((ushort)FloorWidth); // FW FW
            writer.Write((ushort)FloorHeight); // FH FH
            writer.Write((ushort)LeftPixels); // LX LX
            writer.Write(_unknownByte); // ??
            writer.Write((byte)RightEdgeFactor); // LW
            writer.Write((ushort)TopPixels); // LY LY
            writer.Write((byte)ExtraHeight); // XH
            writer.Write((byte)BottomEdgeFactor); // LH
            writer.Write((byte)StartX); // SX
            writer.Write((byte)StartY); // SY
            writer.Write((ushort)(Floor.Offset - 0x14000)); // FL FL: relative to 0x14000
            writer.Write((ushort)Floor.GetData().Count); // FS FS: compressed size in bytes
            writer.Write((ushort)(BlockMapping.Blocks[0].Offset - 0x10000)); // BM BM: relative to 0x10000
            writer.Write((ushort)(TileSet.Offset - 0x30000)); // LA LA: Relative to 0x30000
            writer.Write((byte)(SpriteTileSet.Offset / 0x4000)); // SP: Page for the below
            writer.Write((ushort)(SpriteTileSet.Offset % 0x4000)); // SA SA: Offset from the page above
            writer.Write((byte)_initPalette); // IP: Index of palette
            writer.Write((byte)PaletteCycleRate); // CS: Number of frames between palette cycles
            writer.Write((byte)_paletteCycleCount); // CC: Number of palette cycles in a loop
            writer.Write((byte)_paletteCycleIndex); // CP: Which cycling palette to use
            writer.Write((ushort)(Objects.Offset - 0x15580)); // OL OL: relative to 0x15580
            var flags = 0;
            if (DemoMode) flags |= 1 << 1;
            if (ShowRings) flags |= 1 << 2;
            if (AutoScrollRight) flags |= 1 << 3;
            if (AutoScrollUp) flags |= 1 << 4;
            if (SlowScroll) flags |= 1 << 5;
            if (WaveScrollY) flags |= 1 << 6;
            if (NoScrollDown) flags |= 1 << 7;
            writer.Write((byte)flags); // SR
            flags = 0;
            if (HasWater) flags |= 1 << 7;
            writer.Write((byte)flags); // UW
            flags = 0;
            if (SpecialStageTimer) flags |= 1 << 0;
            if (HasLightning) flags |= 1 << 1;
            if (UseUnderwaterBossPalette) flags |= 1 << 4;
            if (ShowTime) flags |= 1 << 5;
            if (DisableScrolling) flags |= 1 << 6;
            writer.Write((byte)flags); // TL
            writer.Write((byte)0); // 00: Always 0
            writer.Write((byte)MusicIndex); // MU
            return stream.ToArray();
        }

        public void UpdateRenderingPalettes()
        {
            // The tile palette is effectively the first cycling palette entry...
            TilePalette = CyclingPalette.GetSubPalette(0, 16);
            SpritePalette = Palette.GetSubPalette(16, 16);

            foreach (var block in BlockMapping.Blocks)
            {
                block.ResetImages();
            }

            foreach (var tile in TileSet.Tiles)
            {
                tile.ResetImages();
            }

            foreach (var tile in SpriteTileSet.Tiles)
            {
                tile.ResetImages();
            }
        }
    }
}