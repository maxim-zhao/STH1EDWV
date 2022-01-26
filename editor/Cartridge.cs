using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using sth1edwv.GameObjects;

namespace sth1edwv
{
    public interface IDataItem
    {
        /// <summary>
        /// Offset data was read from
        /// </summary>
        int Offset { get; set; }

        /// <summary>
        /// Get raw data from the item
        /// </summary>
        IList<byte> GetData();
    }

    public class Cartridge: IDisposable
    {
        private readonly Action<string> _logger;

        public class Game
        {
            public class LevelHeader
            {
                public string Name { get; set; }
                public int Offset { get; set; }
            }
            public List<LevelHeader> Levels { get; set; }

            public class Reference
            {
                protected bool Equals(Reference other)
                {
                    return Offset == other.Offset && Type == other.Type && Delta == other.Delta;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((Reference)obj);
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(Offset, (int)Type, Delta);
                }

                public int Offset { get; init; }
                public enum Types
                {
                    Absolute,
                    Slot1,
                    PageNumber,
                    Size,
                    Size8
                }
                public Types Type { get; init; }
                public int Delta { get; init; }
                public override string ToString()
                {
                    return $"{Type}@{Offset:X} ({Delta:X})";
                }
            }

            public class LocationRestriction
            {
                public int MinimumOffset { get; set; }
                public int MaximumOffset { get; set; } = 1024*1024;
                public bool CanCrossBanks { get; set; }
                public string MustFollow { get; set; }
            }

            public class Asset
            {
                public override string ToString()
                {
                    return $"{Type} from {OriginalOffset:X} ({OriginalSize}B)";
                }

                public enum Types { TileSet, Palette, TileMap, SpriteTileSet, ForegroundTileMap, Unused, Misc, TileMapData, RawValue }
                public Types Type { get; init; }
                public List<Reference> References { get; init; }
                public LocationRestriction Restrictions { get; init; } = new(); // Default to defaults...
                public int FixedSize { get; init; }
                public int BitPlanes { get; init; }
                public List<Point> TileGrouping { get; init; }
                public int TilesPerRow { get; init; } = 16; // 16 is often the best default
                public bool Hidden { get; init; }
                public RawValue.Encodings Encoding { get; init; }

                // These are in the original ROM, not where we loaded from
                public int OriginalOffset { get; init; }
                public int OriginalSize { get; init; }

                public int GetOffset(Memory memory)
                {
                    // Raw values are not relocatable
                    if (Type == Types.RawValue)
                    {
                        return OriginalOffset;
                    }

                    // The offset is implied by the references
                    // First see if there is an absolute one
                    var absoluteReference = References.FirstOrDefault(x => x.Type == Reference.Types.Absolute);
                    if (absoluteReference != null)
                    {
                        return memory.Word(absoluteReference.Offset) - absoluteReference.Delta;
                    }
                    // Next try for a paged one
                    var pageNumberReference = References.FirstOrDefault(x => x.Type == Reference.Types.PageNumber);
                    if (pageNumberReference == null)
                    {
                        throw new Exception("Unable to compute offset");
                    }
                    var pagedReference = References.FirstOrDefault(x => x.Type == Reference.Types.Slot1);
                    if (pagedReference == null)
                    {
                        throw new Exception("Unable to compute offset");
                    }

                    var offset = memory.Word(pagedReference.Offset) - pagedReference.Delta;
                    var page = memory[pageNumberReference.Offset] - pageNumberReference.Delta;
                    if (pagedReference.Type == Reference.Types.Slot1)
                    {
                        page -= 1;
                    }

                    return page * 0x4000 + offset;
                }

                public int GetLength(Memory memory)
                {
                    if (FixedSize > 0)
                    {
                        return FixedSize;
                    }
                    // We must have a reference to our length
                    var reference = References.FirstOrDefault(x => x.Type == Reference.Types.Size);
                    if (reference == null)
                    {
                        throw new Exception("No length reference");
                    }

                    return memory.Word(reference.Offset) - reference.Delta;
                }
            }

            // Here we have a list of assets in the original ROM, each holding all its references. All of these can be "cleared"...
            public Dictionary<string, Asset> Assets { get; init; }

            // And we link them into groups here. This lets us display things together and discard unused things.
            public Dictionary<string, IEnumerable<string>> AssetGroups { get; init; }

            // HOWEVER: this means all sharing of assets is fixed. We want to instead define "grouped assets" with distinct references.
            // Thus each "asset group" is a collection of the assets referenced and the references in this scope.
            // Assets may be shared between asset groups, or not used at all.
            public class AssetGroupItem
            {
                public List<Reference> References { get; init; }
                public string AssetName { get; init; }
            }
            public Dictionary<string, List<AssetGroupItem>> AssetGroups2 = new();
        }

        private static readonly Game Sonic1MasterSystem = new()
        {
            // These are in ROM order to help me keep track
            Assets = new Dictionary<string, Game.Asset> {
                {
                    "Start of ROM gap 1", new Game.Asset { OriginalOffset = 0xc, OriginalSize = 0xc, Type = Game.Asset.Types.Unused}
                }, {
                    "Start of ROM gap 2", new Game.Asset { OriginalOffset = 0x1b, OriginalSize = 0x5, Type = Game.Asset.Types.Unused}
                }, {
                    "Start of ROM gap 3", new Game.Asset { OriginalOffset = 0x23, OriginalSize = 0x5, Type = Game.Asset.Types.Unused}
                }, {
                    "Start of ROM gap 4", new Game.Asset { OriginalOffset = 0x2b, OriginalSize = 0x5, Type = Game.Asset.Types.Unused}
                }, {
                    "Start of ROM gap 5", new Game.Asset { OriginalOffset = 0x3b, OriginalSize = 0x2b, Type = Game.Asset.Types.Unused}
                }, {
                    // Palettes scattered in the low ROM area
                    "Underwater palette", new Game.Asset { 
                        OriginalOffset = 0x024B,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x01C4 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$024b ; 0001C4 21 4B 02 
                            new() {Offset = 0x0223 + 1, Type = Game.Reference.Types.Absolute}  // ld hl,$024b ; 000223 21 4B 02 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 } // Must be in low 16KB
                    }
                }, {
                    // Palettes scattered in the low ROM area
                    "Underwater boss palette", new Game.Asset { 
                        OriginalOffset = 0x026B,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x01cd + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$026b ; 0001CD 21 6B 02 
                            new() {Offset = 0x022C + 1, Type = Game.Reference.Types.Absolute}  // ld hl,$026b ; 00022C 21 6B 02 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    // Palettes scattered in the low ROM area
                    "Map screen 1 palette", new Game.Asset { 
                        OriginalOffset = 0x0f0e,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0cd4 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$0f0e ; 000CD4 21 0E 0F 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Map screen 2 palette", new Game.Asset { 
                        OriginalOffset = 0xf2e,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0d36 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$0f2e ; 000D36 21 2E 0F 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Map screen text: Green Hill", new Game.Asset { 
                        OriginalOffset = 0x122d,
                        OriginalSize = 15,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1209, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x120b, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x120d, Type = Game.Reference.Types.Absolute},
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Map screen text: Bridge", new Game.Asset { 
                        OriginalOffset = 0x123c,
                        OriginalSize = 15,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x120f, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x1211, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x1213, Type = Game.Reference.Types.Absolute},
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Map screen text: Jungle", new Game.Asset { 
                        OriginalOffset = 0x124b,
                        OriginalSize = 15,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1215, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x1217, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x1219, Type = Game.Reference.Types.Absolute},
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Map screen text: Labyrinth", new Game.Asset { 
                        OriginalOffset = 0x125a,
                        OriginalSize = 15,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x121b, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x121d, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x121f, Type = Game.Reference.Types.Absolute},
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Map screen text: Scrap Brain", new Game.Asset { 
                        OriginalOffset = 0x1269,
                        OriginalSize = 15,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1221, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x1223, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x1225, Type = Game.Reference.Types.Absolute},
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Map screen text: Sky Base", new Game.Asset { 
                        OriginalOffset = 0x1278,
                        OriginalSize = 15,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1227, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x1229, Type = Game.Reference.Types.Absolute},
                            new() {Offset = 0x122b, Type = Game.Reference.Types.Absolute},
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Title screen music", new Game.Asset {
                        OriginalOffset = 0x12d8 + 1, // ld a,$06 ; 0012D8 3E 06 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Byte
                    }
                }, {
                    "Title screen \"PRESS BUTTON\" flash time", new Game.Asset {
                        OriginalOffset = 0x1308 + 1, // cp $40 ; 001308 FE 40 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Byte
                    }
                }, {
                    "Title screen \"PRESS BUTTON\" total time", new Game.Asset {
                        OriginalOffset = 0x12fd + 1, // cp $64 ; 0012FD FE 64 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Byte
                    }
                }, {
                    "Title screen hand X", new Game.Asset {
                        OriginalOffset = 0x133b + 1, // ld hl,$0080 ; 00133B 21 80 00 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Byte
                    }
                }, {
                    "Title screen hand Y", new Game.Asset {
                        OriginalOffset = 0x133e + 1, // ld de,$0018 ; 00133E 11 18 00 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Byte
                    }
                }, {
                    "Title screen palette", new Game.Asset { 
                        OriginalOffset = 0x13e1,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x12cc + 1, Type = Game.Reference.Types.Absolute} // ld hl,$13e1 ; 0012CC 21 E1 13 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Game Over: Continue top", new Game.Asset { 
                        OriginalOffset = 0x14e6,
                        OriginalSize = 11,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x147f + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$14e6 ; 00147F 21 E6 14 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Game Over: Continue bottom", new Game.Asset { 
                        OriginalOffset = 0x14f1,
                        OriginalSize = 11,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1485 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$14f1 ; 001485 21 F1 14 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Game Over palette", new Game.Asset { 
                        OriginalOffset = 0x14fc,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x143c + 1, Type = Game.Reference.Types.Absolute} // ld hl,$14fc ; 00143C 21 FC 14 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 1", new Game.Asset { 
                        OriginalOffset = 0x1907,
                        OriginalSize = 21,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1785 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1907 ; 001785 21 07 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 2", new Game.Asset { 
                        OriginalOffset = 0x191c,
                        OriginalSize = 21,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x178B + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$191c ; 00178B 21 1C 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 3", new Game.Asset { 
                        OriginalOffset = 0x1931,
                        OriginalSize = 21,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1791 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1931 ; 001791 21 31 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 4", new Game.Asset { 
                        OriginalOffset = 0x1946,
                        OriginalSize = 13,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1797 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1946 ; 001797 21 46 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 5", new Game.Asset { 
                        OriginalOffset = 0x1953,
                        OriginalSize = 13,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x179D + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1953 ; 00179D 21 53 19
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 6", new Game.Asset { 
                        OriginalOffset = 0x1960,
                        OriginalSize = 13,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x17A3 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1960 ; 0017A3 21 60 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 7", new Game.Asset { 
                        OriginalOffset = 0x196d,
                        OriginalSize = 13,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x17A9 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$196d ; 0017A9 21 6D 19
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: box 8", new Game.Asset { 
                        OriginalOffset = 0x197a,
                        OriginalSize = 4,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1823 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$197a ; 001823 21 7A 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: Chaos Emerald", new Game.Asset { 
                        OriginalOffset = 0x197e,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x17af + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$197e ; 0017AF 21 7E 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: Sonic Left", new Game.Asset { 
                        OriginalOffset = 0x198e,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x17e8 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$198e ; 0017E8 21 8E 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Ending text: Special Bonus", new Game.Asset { 
                        OriginalOffset = 0x199e,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x181d + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$199e ; 00181D 21 9E 19 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Act Complete palette", new Game.Asset { 
                        OriginalOffset = 0x1b8d,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32, 
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1604 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$1b8d ; 001604 21 8D 1B 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Starting lives count", new Game.Asset
                    {
                        OriginalOffset = 0x1c4e + 1, // ld a,$03 ; 001C4E 3E 03 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Byte
                    }
                }, {
                    "Extra life at n x 10,000 points", new Game.Asset
                    {
                        OriginalOffset = 0x1c53 + 1, // ld a,$05 ; 001C53 3E 05 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Bcd
                    }
                }, {
                    "Ending palette", new Game.Asset {
                        OriginalOffset = 0x2828,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() { Offset = 0x25a1 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$2828 ; 0025A1 21 28 28 
                            new() { Offset = 0x268d + 1, Type = Game.Reference.Types.Absolute}  // ld hl,$2828 ; 00268D 21 28 28 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, { "Credits eyebrow X", new Game.Asset { OriginalOffset = 0x289f, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits eyebrow Y", new Game.Asset { OriginalOffset = 0x28a0, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits mouth 1 X", new Game.Asset { OriginalOffset = 0x28a8, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits mouth 1 Y", new Game.Asset { OriginalOffset = 0x28a9, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits mouth 2 X", new Game.Asset { OriginalOffset = 0x28b1, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits mouth 2 Y", new Game.Asset { OriginalOffset = 0x28b2, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits mouth 3 X", new Game.Asset { OriginalOffset = 0x28ba, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits mouth 3 Y", new Game.Asset { OriginalOffset = 0x28bb, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits foot 1 X", new Game.Asset { OriginalOffset = 0x28c3, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits foot 1 Y", new Game.Asset { OriginalOffset = 0x28c4, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits foot 2 X", new Game.Asset { OriginalOffset = 0x28cc, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits foot 2 Y", new Game.Asset { OriginalOffset = 0x28cd, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits arm 1 X", new Game.Asset { OriginalOffset = 0x28d5, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits arm 1 Y", new Game.Asset { OriginalOffset = 0x28d6, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits arm 2 X", new Game.Asset { OriginalOffset = 0x28e4, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits arm 2 Y", new Game.Asset { OriginalOffset = 0x28e5, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits arm 3 X", new Game.Asset { OriginalOffset = 0x28f3, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, { "Credits arm 3 Y", new Game.Asset { OriginalOffset = 0x28f4, OriginalSize = 1, Type = Game.Asset.Types.RawValue, Encoding = RawValue.Encodings.Byte }
                }, {
                    "Credits palette", new Game.Asset {
                        OriginalOffset = 0x2ad6,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x2702 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$2ad6 ; 002702 21 D6 2A 
                        }, 
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Extra life every n x 10,000 subsequent points", new Game.Asset
                    {
                        OriginalOffset = 0x39f6 + 1, // ld a,$05 ; 0039F6 3E 05 
                        OriginalSize = 1,
                        Type = Game.Asset.Types.RawValue,
                        Encoding = RawValue.Encodings.Bcd
                    }
                }, {
                    "End sign palette", new Game.Asset {
                        OriginalOffset = 0x626c,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 16, // Sprite palette only
                        References = new List<Game.Reference> {
                            new() {Offset = 0x5F38 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$626c ; 005F38 21 6C 62
                        }, 
                        Restrictions = { MaximumOffset = 0x8000 }
                    }
                }, {
                    "Green Hill palette", 
                    new Game.Asset { OriginalOffset = 0x629e, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, Hidden = true, References = new List<Game.Reference> { new() { Offset = 0x627C, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Green Hill cycling palette", 
                    new Game.Asset { OriginalOffset = 0x62be, OriginalSize = 48, Type = Game.Asset.Types.Palette, FixedSize = 48, References = new List<Game.Reference> { new() { Offset = 0x628C, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Bridge palette", 
                    new Game.Asset { OriginalOffset = 0x62ee, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, Hidden = true, References = new List<Game.Reference> { new() { Offset = 0x627E, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Bridge cycling palette", 
                    new Game.Asset { OriginalOffset = 0x630e, OriginalSize = 48, Type = Game.Asset.Types.Palette, FixedSize = 48, References = new List<Game.Reference> { new() { Offset = 0x628E, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Jungle palette", 
                    new Game.Asset { OriginalOffset = 0x633e, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, Hidden = true, References = new List<Game.Reference> { new() { Offset = 0x6280, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Jungle cycling palette", 
                    new Game.Asset { OriginalOffset = 0x635e, OriginalSize = 48, Type = Game.Asset.Types.Palette, FixedSize = 48, References = new List<Game.Reference> { new() { Offset = 0x6290, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Labyrinth palette", 
                    new Game.Asset { OriginalOffset = 0x638e, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, Hidden = true, References = new List<Game.Reference>
                    {
                        new() { Offset = 0x6282, Type = Game.Reference.Types.Absolute},
                        new() { Offset = 0x01e9 + 1, Type = Game.Reference.Types.Absolute, Delta = +16 } // ld hl,$639e ; 0001E9 21 9E 63 
                    }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Labyrinth cycling palette", 
                    new Game.Asset { OriginalOffset = 0x63ae, OriginalSize = 48, Type = Game.Asset.Types.Palette, FixedSize = 48, References = new List<Game.Reference> { new() { Offset = 0x6292, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Scrap Brain palette", 
                    new Game.Asset { OriginalOffset = 0x63de, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, Hidden = true, References = new List<Game.Reference> { new() { Offset = 0x6284, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Scrap Brain cycling palette", 
                    new Game.Asset { OriginalOffset = 0x63fe, OriginalSize = 64, Type = Game.Asset.Types.Palette, FixedSize = 64, References = new List<Game.Reference> { new() { Offset = 0x6294, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Sky Base exterior palette", 
                    new Game.Asset { OriginalOffset = 0x643e, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, Hidden = true, References = new List<Game.Reference> { new() { Offset = 0x6286, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Sky Base cycling palette", 
                    new Game.Asset { OriginalOffset = 0x645e, OriginalSize = 64, Type = Game.Asset.Types.Palette, FixedSize = 64, References = new List<Game.Reference>
                    {
                        new() { Offset = 0x6296, Type = Game.Reference.Types.Absolute},
                        new() { Offset = 0x1f9f, Type = Game.Reference.Types.Absolute}
                    }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Sky Base lightning palette 1", new Game.Asset { OriginalOffset = 0x649E, OriginalSize = 64, Type = Game.Asset.Types.Palette, FixedSize = 64, References = new List<Game.Reference> { new() {Offset = 0x1FA3, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 }
                    }
                }, {
                    "Sky Base lightning palette 2", new Game.Asset { OriginalOffset = 0x64DE, OriginalSize = 64, Type = Game.Asset.Types.Palette, FixedSize = 64, References = new List<Game.Reference> { new() {Offset = 0x1FA7, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 }
                    }
                }, {
                    "Sky Base exterior cycling palette", 
                    new Game.Asset { OriginalOffset = 0x651e, OriginalSize = 64, Type = Game.Asset.Types.Palette, FixedSize = 64, References = new List<Game.Reference> { new() { Offset = 0x629c, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Special stage palette", 
                    new Game.Asset { OriginalOffset = 0x655e, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, References = new List<Game.Reference> { new() { Offset = 0x628a, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Special stage cycling palette", 
                    new Game.Asset { OriginalOffset = 0x657e, OriginalSize = 16, Type = Game.Asset.Types.Palette, FixedSize = 16, References = new List<Game.Reference> { new() { Offset = 0x629a, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Sky Base interior palette", 
                    new Game.Asset { OriginalOffset = 0x658e, OriginalSize = 32, Type = Game.Asset.Types.Palette, FixedSize = 32, References = new List<Game.Reference> { new() { Offset = 0x6288, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } }
                }, {
                    "Sky Base interior cycling palette", 
                    new Game.Asset { OriginalOffset = 0x65ae, OriginalSize = 64, Type = Game.Asset.Types.Palette, FixedSize = 64, References = new List<Game.Reference> { new() { Offset = 0x6298, Type = Game.Reference.Types.Absolute} }, Restrictions = { MaximumOffset = 0x8000 } } // TODO: all these references and addresses need to be checked
                }, {
                    "Boss sprites palette", new Game.Asset {
                        OriginalOffset = 0x731c,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 16,
                        References = new List<Game.Reference> {
                            // One reference in each boss loader. Capsule depends on these.
                            new() {Offset = 0x703C + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00703C 21 1C 73
                            new() {Offset = 0x807F + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00807F 21 1C 73
                            new() {Offset = 0x84C7 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 0084C7 21 1C 73
                            new() {Offset = 0x929C + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00929C 21 1C 73
                            new() {Offset = 0xA821 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00A821 21 1C 73
                            new() {Offset = 0xBE07 + 1, Type = Game.Reference.Types.Absolute}  // ld hl,$731c ; 00BE07 21 1C 73
                        }, 
                        Restrictions = { MaximumOffset = 0x8000 }
                    }
                }, {
                    "Unused space bank 2", new Game.Asset { OriginalOffset = 0x07fdb, OriginalSize = 0x15, Type = Game.Asset.Types.Unused }
                }, {
                    "Unused space bank 3", new Game.Asset { OriginalOffset = 0x0ffb1, OriginalSize = 0x4f, Type = Game.Asset.Types.Unused }
                }, {
                    "Monitor Art", new Game.Asset { 
                        OriginalOffset = 0x15180, 
                        OriginalSize = 0x400,
                        Type = Game.Asset.Types.SpriteTileSet, 
                        FixedSize = 0x400,
                        BitPlanes = 4,
                        TileGrouping = TileSet.Groupings.Monitor,
                        TilesPerRow = 8,
                        References = new List<Game.Reference> {
                            new() { Offset = 0x5B31 + 1, Type = Game.Reference.Types.Slot1 }, // ld hl, $5180 ; 005B31 21 80 51 
                            new() { Offset = 0x5F09 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5400 - 0x5180 }, // ld hl, $5400 ; 005F09 21 00 54
                            new() { Offset = 0xBF50 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5400 - 0x5180 }, // ld hl, $5400 ; 00BF50 21 00 54
                            new() { Offset = 0x5BFF + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5200 - 0x5180 }, // ld hl, $5200 ; 005BFF 21 00 52
                            new() { Offset = 0x5C6D + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5280 - 0x5180 }, // ld hl, $5280 ; 005C6D 21 80 52
                            new() { Offset = 0x5CA7 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5180 - 0x5180 }, // ld hl, $5100 ; 005CA7 21 80 51
                            new() { Offset = 0x5CB2 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5280 - 0x5180 }, // ld hl, $5200 ; 005CB2 21 80 52
                            new() { Offset = 0x5CF9 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5300 - 0x5180 }, // ld hl, $5300 ; 005CF9 21 00 53
                            new() { Offset = 0x5D29 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5380 - 0x5180 }, // ld hl, $5380 ; 005D29 21 80 53
                            new() { Offset = 0x5D7A + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5480 - 0x5180 }, // ld hl, $5480 ; 005D7A 21 80 54
                            new() { Offset = 0x5DA2 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5500 - 0x5180 }, // ld hl, $5500 ; 005DA2 21 00 55
                            new() { Offset = 0x0c1e + 1, Type = Game.Reference.Types.PageNumber } // ld a,$05 ; 000C1E 3E 05 
                        }
                    }
                }, {
                    // We just clear space for these
                    "Level object lists", new Game.Asset { OriginalOffset = 0x15AB4, OriginalSize = 0x15fc4 - 0x15AB4, Type = Game.Asset.Types.Misc}
                }, {
                    "Unused space bank 5", new Game.Asset { OriginalOffset = 0x15fc4, OriginalSize = 0x3c, Type = Game.Asset.Types.Unused }
                }, {
                    "Title screen tilemap", new Game.Asset { 
                        OriginalOffset = 0x16000,
                        OriginalSize = 0x1612E - 0x16000,
                        Type = Game.Asset.Types.TileMap,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x12ac + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 0012AC 3E 05 
                            new() {Offset = 0x12b4 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$6000 ; 0012B4 21 00 60
                            new() {Offset = 0x12ba + 1, Type = Game.Reference.Types.Size}        // ld bc,$012e ; 0012BA 01 2E 01
                        }
                    }
                }, {
                    "Act Complete tilemap", new Game.Asset { 
                        OriginalOffset = 0x1612E,
                        OriginalSize = 0x161E9 - 0x1612E,
                        Type = Game.Asset.Types.TileMap,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x158B + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 00158B 3E 05 
                            new() {Offset = 0x1593 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$612e ; 001593 21 2E 61
                            new() {Offset = 0x1596 + 1, Type = Game.Reference.Types.Size}        // ld bc,$00bb ; 001596 01 BB 00
                        }
                    }
                }, {
                    "Special Stage Complete tilemap", new Game.Asset { 
                        OriginalOffset = 0x161E9,
                        OriginalSize = 0x1627E - 0x161E9,
                        Type = Game.Asset.Types.TileMap,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x158B + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 00158B 3E 05 // Shared with Act Complete tilemap
                            new() {Offset = 0x15A3 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$61e9 ; 0015A3 21 E9 61 
                            new() {Offset = 0x15A6 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0095 ; 0015A6 01 95 00 
                        },
                        Restrictions = { MustFollow = "Act Complete tilemap" }
                    }
                }, {
                    "Map screen 1 tilemap 1", new Game.Asset { 
                        OriginalOffset = 0x1627E,
                        OriginalSize = 0x163F6 - 0x1627E,
                        Type = Game.Asset.Types.ForegroundTileMap, 
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0caa + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 000CAA 3E 05 
                            new() {Offset = 0x0cb2 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$627e ; 000CB2 21 7E 62
                            new() {Offset = 0x0cb5 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0178 ; 000CB5 01 78 01
                        }
                    }
                }, {
                    "Map screen 1 tilemap 2", new Game.Asset { 
                        OriginalOffset = 0x163F6,
                        OriginalSize = 0x1653B - 0x163F6,
                        Type = Game.Asset.Types.TileMap, 
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0caa + 1, Type = Game.Reference.Types.PageNumber}, // We duplicate this to read it in
                            new() {Offset = 0x0cc3 + 1, Type = Game.Reference.Types.Slot1}, // ld hl,$63f6 ; 000CC3 21 F6 63 
                            new() {Offset = 0x0cc6 + 1, Type = Game.Reference.Types.Size}   // ld bc,$0145 ; 000CC6 01 45 01 
                        },
                        Restrictions = { MustFollow = "Map screen 1 tilemap 1" } // Same page as the above
                    }
                }, {
                    "Map screen 2 tilemap 1", new Game.Asset { 
                        OriginalOffset = 0x1653B,
                        OriginalSize = 0x166AB - 0x1653B,
                        Type = Game.Asset.Types.ForegroundTileMap, 
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0d0c + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05     ; 000D0C 3E 05 
                            new() {Offset = 0x0d14 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$653b  ; 000D14 21 3B 65
                            new() {Offset = 0x0d17 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0170  ; 000D17 01 70 01
                        }
                    }
                }, {
                    "Map screen 2 tilemap 2", new Game.Asset { 
                        OriginalOffset = 0x166AB,
                        OriginalSize = 0x167FE - 0x166AB,
                        Type = Game.Asset.Types.TileMap, 
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0d0c + 1, Type = Game.Reference.Types.PageNumber},
                            new() {Offset = 0x0d25 + 1, Type = Game.Reference.Types.Slot1}, // ld hl,$66ab ; 000D25 21 AB 66 
                            new() {Offset = 0x0d28 + 1, Type = Game.Reference.Types.Size}   // ld bc,$0153 ; 000D28 01 53 01 
                        },
                        Restrictions = { MustFollow = "Map screen 2 tilemap 1" } // Same page as the above
                    }
                }, {
                    "Game Over tilemap", new Game.Asset { 
                        OriginalOffset = 0x167FE,
                        OriginalSize = 0x16830 - 0x167FE,
                        Type = Game.Asset.Types.TileMap,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x141c + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 00141C 3E 05 
                            new() {Offset = 0x1424 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$67fe ; 001424 21 FE 67
                            new() {Offset = 0x1427 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0032 ; 001427 01 32 00
                        }
                    }
                }, {
                    "Ending 1 tilemap", new Game.Asset {
                        OriginalOffset = 0x16830,
                        OriginalSize = 0x169A9 - 0x16830,
                        Type = Game.Asset.Types.TileMap,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x25B4 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 0025B4 3E 05 
                            new() {Offset = 0x25BC + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$6830 ; 0025BC 21 30 68
                            new() {Offset = 0x25BF + 1, Type = Game.Reference.Types.Size}        // ld bc,$0179 ; 0025BF 01 79 01
                        }
                    }
                }, {
                    "Ending 2 tilemap", new Game.Asset {
                        OriginalOffset = 0x169A9,
                        OriginalSize = 0x16AED - 0x169A9,
                        Type = Game.Asset.Types.TileMap,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x2675 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 002675 3E 05 
                            new() {Offset = 0x267D + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$69a9 ; 00267D 21 A9 69
                            new() {Offset = 0x2680 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0145 ; 002680 01 45 01
                        }
                    }
                }, {
                    "Unused credits tilemap", new Game.Asset {
                        OriginalOffset = 0x16AED,
                        OriginalSize = 0x16C61 - 0x16AED,
                        Type = Game.Asset.Types.TileMap
                        // Unused in-game, can be removed
                    }
                }, {
                    "Credits tilemap", new Game.Asset {
                        OriginalOffset = 0x16c61,
                        OriginalSize = 0x16dea - 0x16c61,
                        Type = Game.Asset.Types.TileMap,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x26C1 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 0026C1 3E 05 
                            new() {Offset = 0x26C9 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$6c61 ; 0026C9 21 61 6C
                            new() {Offset = 0x26CC + 1, Type = Game.Reference.Types.Size}        // ld bc,$0189 ; 0026CC 01 89 01
                        }
                    }
                }, {
                    "Floors area", new Game.Asset { OriginalOffset = 0x16dea, OriginalSize = 0x1FBA1 - 0x16dea, Type = Game.Asset.Types.Misc}
                }, {
                    "Unused space bank 7", new Game.Asset { OriginalOffset = 0x1FBA1, OriginalSize = 0x45f, Type = Game.Asset.Types.Unused }
                }, {
                    "Sonic (right)", new Game.Asset { 
                        OriginalOffset = 0x20000, 
                        OriginalSize = 0x3000, // The original pads it, this allows us to reclaim it
                        Type = Game.Asset.Types.SpriteTileSet, 
                        BitPlanes = 3,
                        TileGrouping = TileSet.Groupings.Sonic,
                        FixedSize = 39 * 24 * 32 * 3/8, // 42 frames, each 24x32, each pixel is 3 bits. The last 3 frames are unused so we ignore them.
                        TilesPerRow = 8,
                        References = new List<Game.Reference> {
                            new() { Offset = 0x4c84 + 1, Type = Game.Reference.Types.Slot1 }, // ld bc,$4000 ; 004C84 01 00 40 
                            new() { Offset = 0x012d + 1, Type = Game.Reference.Types.PageNumber }, // ld a,$08 ; 00012D 3E 08 
                            new() { Offset = 0x0135 + 1, Type = Game.Reference.Types.PageNumber, Delta = 1 }, // ld a,$09 ; 000135 3E 09 
                            // We put this one at the end so it won't be used for reading but will be written.
                            // This allows us to make sure the left-facing art pointer is in the right place while removing the padding.
                            // We want this sprite set plus the left-facing ones to be in the same 32KB window, and to have the two pointers
                            // relative to the same base. This isn't currently explicitly handled.
                            new() { Offset = 0x4c8d + 1, Type = Game.Reference.Types.Slot1, Delta = 39 * 24 * 32 * 3/8}, // ld bc,$7000 ; 004C8D 01 00 70
                            new() { Offset = 0x4E49 + 1, Type = Game.Reference.Types.Slot1, Delta = 39 * 24 * 32 * 3/8}  // ld bc,$7000 ; 004E49 01 00 70 ; Needed for dropped rings
                        },
                        Restrictions = { CanCrossBanks = true, MinimumOffset = 0x20000 } // This minimum cajoles the code into picking a working location. It's a hack.
                    }
                }, {
                    "Sonic (left)", new Game.Asset { 
                        OriginalOffset = 0x23000,
                        OriginalSize = 0x3000, // Similarly this is padded
                        Type = Game.Asset.Types.SpriteTileSet, 
                        BitPlanes = 3,
                        TileGrouping = TileSet.Groupings.Sonic,
                        FixedSize = 39 * 24 * 32 * 3/8, // 39 frames, each 24x32, each pixel is 3 bits
                        TilesPerRow = 8,
                        References = new List<Game.Reference> {
                            new() { Offset = 0x4c8d + 1, Type = Game.Reference.Types.Slot1 }, // ld bc,$7000 ; 004C8D 01 00 70
                            new() { Offset = 0x012d + 1, Type = Game.Reference.Types.PageNumber }, // ld a,$08 ; 00012D 3E 08 
                            new() { Offset = 0x0135 + 1, Type = Game.Reference.Types.PageNumber, Delta = 1 } // ld a,$09 ; 000135 3E 09 
                        },
                        Restrictions = { MustFollow = "Sonic (right)", CanCrossBanks = true } // This has to be in the same 32KB window as the above
                    }
                }, {
                    "Title screen tiles", new Game.Asset { 
                        OriginalOffset = 0x26000,
                        OriginalSize = 0x2751f - 0x26000,
                        Type = Game.Asset.Types.TileSet,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1296 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$2000 ; 001296 21 00 20 
                            new() {Offset = 0x129c + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 00129C 3E 09 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Title screen press button text 1", new Game.Asset { 
                        OriginalOffset = 0x1352,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1305 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1352 ; 001305 21 52 13 
                        },
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Title screen press button text 2", new Game.Asset { 
                        OriginalOffset = 0x1362,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.TileMapData,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x130c + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1362 ; 00130C 21 62 13
                        },
                        Restrictions = { MaximumOffset = 0x4000 }
                    }
                }, {
                    "Act Complete tiles", new Game.Asset { 
                        OriginalOffset = 0x2751f,
                        OriginalSize = 0x28294 - 0x2751f,
                        Type = Game.Asset.Types.TileSet,
                        References = new List<Game.Reference> {
                            // Game Over
                            new() {Offset = 0x1411 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$351f ; 001411 21 1F 35 
                            new() {Offset = 0x1417 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 001417 3E 09 
                            // Act Complete
                            new() {Offset = 0x1580 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$351f ; 001580 21 1F 35 
                            new() {Offset = 0x1586 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 001586 3E 09 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "End sign tileset", new Game.Asset
                    {
                        OriginalOffset = 0x28294,
                        OriginalSize = 0x28b0a - 0x28294,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference>
                        {
                            new() {Offset = 0x5f2d + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$4294 ; 005F2D 21 94 42 // This is actually into page 10
                            new() {Offset = 0x5f33 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 005F33 3E 09 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Title screen sprites", new Game.Asset 
                    { 
                        OriginalOffset = 0x28b0a,
                        OriginalSize = 0x2926b - 0x28b0a,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference>
                        {
                            new() {Offset = 0x12a1 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$4b0a ; 0012A1 21 0A 4B 
                            new() {Offset = 0x12a7 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 0012A7 3E 09 
                            new() {Offset = 0x26B6 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$4b0a ; 0026B6 21 0A 4B 
                            new() {Offset = 0x26BC + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 0026BC 3E 09 
                    },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Map screen 1 sprite tiles", new Game.Asset { 
                        OriginalOffset = 0x2926b,
                        OriginalSize = 0x29942 - 0x2926b,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0c94 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$526b ; 000C94 21 6B 52 
                            new() {Offset = 0x0c9a + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 000C9A 3E 09 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Map screen 2 sprite tiles", new Game.Asset { 
                        OriginalOffset = 0x29942,
                        OriginalSize = 0x2a12a - 0x29942,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0cf6 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$5942 ; 000CF6 21 42 59 
                            new() {Offset = 0x0cfc + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 000CFC 3E 09 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, { "Green Hill sprite tiles",    new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x2A12A, OriginalSize = 0x2AC3D - 0x2A12A }
                }, { "Bridge sprite tiles",        new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x2AC3D, OriginalSize = 0x2B7CD - 0x2AC3D }
                }, { "Jungle sprite tiles",        new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x2B7CD, OriginalSize = 0x2C3B6 - 0x2B7CD }
                }, { "Labyrinth sprite tiles",     new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x2C3B6, OriginalSize = 0x2CF75 - 0x2C3B6 }
                }, { "Scrap Brain sprite tiles",   new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x2CF75, OriginalSize = 0x2D9E0 - 0x2CF75 }
                }, { "Sky Base sprite tiles",      new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x2D9E0, OriginalSize = 0x2E511 - 0x2D9E0 }
                }, { "Special Stage sprite tiles", new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x2E511, OriginalSize = 0x2EEB1 - 0x2E511 }
                }, {
                    "Boss sprites 1", new Game.Asset {
                        OriginalOffset = 0x2eeb1,
                        OriginalSize = 2685,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x7031 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$aeb1 ; 007031 21 B1 AE 
                            new() {Offset = 0x7037 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 007037 3E 09 
                            new() {Offset = 0x8074 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$aeb1 ; 008074 21 B1 AE 
                            new() {Offset = 0x807A + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 00807A 3E 09 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "HUD sprite tiles", new Game.Asset { 
                        OriginalOffset = 0x2f92e,
                        OriginalSize = 0x2fcf0 - 0x2f92e,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference>
                        {
                            // Map screen 1
                            new() {Offset = 0x0C9F + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 000C9F 21 2E B9 
                            new() {Offset = 0x0CA5 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 000CA5 3E 09 
                            // Map screen 2
                            new() {Offset = 0x0D01 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 000D01 21 2E B9 
                            new() {Offset = 0x0D07 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 000D07 3E 09 
                            // Act Complete
                            new() {Offset = 0x1575 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 001575 21 2E B9 
                            new() {Offset = 0x157B + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 00157B 3E 09 
                            // Game levels
                            new() {Offset = 0x2172 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 002172 21 2E B9 
                            new() {Offset = 0x2178 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 002178 3E 09 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Rings", new Game.Asset {
                        OriginalOffset = 0x2fcf0,
                        OriginalSize = 768,
                        Type = Game.Asset.Types.TileSet,
                        TileGrouping = TileSet.Groupings.Ring,
                        TilesPerRow = 6,
                        BitPlanes = 4,
                        FixedSize = 6 * 16 * 16 * 4/8, // 6 16x16 frames at 4bpp
                        References = new List<Game.Reference> {
                            new() {Offset = 0x23AD + 1, Type = Game.Reference.Types.Slot1},      // ld de,$7cf0 ; 0023AD 11 F0 7C 
                            new() {Offset = 0x1D55 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$0b ; 001D55 3E 0B 
                            new() {Offset = 0x1DB5 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$0b ; 001DB5 3E 0B 
                            new() {Offset = 0x1eb1 + 1, Type = Game.Reference.Types.PageNumber}  // ld a,$0b ; 001eb1 3E 0B 
                        }
                    }
                }, {
                    "Unused space bank 9", new Game.Asset { OriginalOffset = 0x2fff0, OriginalSize = 0x10, Type = Game.Asset.Types.Unused }
                }, {
                    "Map screen 1 tileset", new Game.Asset { 
                        OriginalOffset = 0x30000,
                        OriginalSize = 0x1801,
                        Type = Game.Asset.Types.TileSet, 
                        References = new List<Game.Reference> 
                        {
                            // Map screen
                            new() {Offset = 0x0c89+1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$0000 ; 000C89 21 00 00
                            new() {Offset = 0x0c8f+1, Type = Game.Reference.Types.PageNumber},             // ld a,$0c    ; 000C8F 3E 0C 
                            // Ending screens
                            new() {Offset = 0x25a9+1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$0000 ; 0025A9 21 00 00
                            new() {Offset = 0x25af+1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 0025AF 3E 0C 
                        },
                        Restrictions = { CanCrossBanks = true } // Compressed art may cross banks
                    }
                }, {
                    "Map screen 2 tileset", new Game.Asset { 
                        OriginalOffset = 0x31801,
                        OriginalSize = 0x32fe6 - 0x31801,
                        Type = Game.Asset.Types.TileSet, 
                        References = new List<Game.Reference> 
                        {
                            // Map screen
                            new() {Offset = 0x0ceb+1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$1801 ; 000CEB 21 01 18 
                            new() {Offset = 0x0cf1+1, Type = Game.Reference.Types.PageNumber},             // ld a,$0c    ; 000CF1 3E 0C 
                            // Credits
                            new() {Offset = 0x26ab+1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$1801 ; 0026AB 21 01 18
                            new() {Offset = 0x26b1+1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 0026B1 3E 0C 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, { "Green Hill tiles",    new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x32FE6, OriginalSize = 0x34578 - 0x32FE6 }
                }, { "Bridge tiles",        new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x34578, OriginalSize = 0x35b00 - 0x34578 }
                }, { "Jungle tiles",        new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x35b00, OriginalSize = 0x371bf - 0x35b00 }
                }, { "Labyrinth tiles",     new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x371bf, OriginalSize = 0x3884B - 0x371bf }
                }, { "Scrap Brain tiles",   new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x3884B, OriginalSize = 0x39CEE - 0x3884B }
                }, { "Sky Base tiles 1",    new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x39CEE, OriginalSize = 0x3B3B5 - 0x39CEE }
                }, { "Sky Base tiles 2",    new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x3B3B5, OriginalSize = 0x3C7FE - 0x3B3B5 }
                }, { "Special Stage tiles", new Game.Asset { Type = Game.Asset.Types.TileSet, OriginalOffset = 0x3C7FE, OriginalSize = 0x3DA28 - 0x3C7FE }
                }, {
                    "Capsule sprites", new Game.Asset {
                        OriginalOffset = 0x3da28,
                        OriginalSize = 2784,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x7916 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$da28 ; 007916 21 28 DA 
                            new() {Offset = 0x791C + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 00791C 3E 0C 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Boss sprites 2", new Game.Asset {
                        OriginalOffset = 0x3E508,
                        OriginalSize = 0x3ef3f - 0x3E508,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x84BC + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$e508 ; 0084BC 21 08 E5 
                            new() {Offset = 0x84C2 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$0c    ; 0084C2 3E 0C 
                            new() {Offset = 0x9291 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$e508 ; 009291 21 08 E5 
                            new() {Offset = 0x9297 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 009297 3E 0C 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Boss sprites 3", new Game.Asset {
                        OriginalOffset = 0x3ef3f,
                        OriginalSize = 0x3f9ed - 0x3ef3f,
                        Type = Game.Asset.Types.SpriteTileSet,
                        TileGrouping = TileSet.Groupings.Sprite,
                        References = new List<Game.Reference> {
                            new() {Offset = 0xA816 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$ef3f ; 00A816 21 3F EF 
                            new() {Offset = 0xA81C + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$0c    ; 00A81C 3E 0C 
                            new() {Offset = 0xBB94 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$ef3f ; 00BB94 21 3F EF 
                            new() {Offset = 0xBB9A + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 00BB9A 3E 0C 
                        },
                        Restrictions = { CanCrossBanks = true }
                    }
                }, {
                    "Unused space bank 15", new Game.Asset { OriginalOffset = 0x3FF21, OriginalSize = 0xdf, Type = Game.Asset.Types.Unused }
                }
            },
            // These all need to match strings above. This is a bit nasty but I don't see a better way.
            AssetGroups = new Dictionary<string, IEnumerable<string>>
            {
                { "Map screen 1", new [] { "Map screen 1 tileset", "Map screen 1 tilemap 1", "Map screen 1 tilemap 2", "Map screen 1 palette", "Map screen 1 sprite tiles", "HUD sprite tiles", "Map screen text: Green Hill", "Map screen text: Bridge", "Map screen text: Jungle" } }, // HUD sprites only used for life counter
                { "Map screen 2", new [] { "Map screen 2 tileset", "Map screen 2 tilemap 1", "Map screen 2 tilemap 2", "Map screen 2 palette", "Map screen 2 sprite tiles", "HUD sprite tiles", "Map screen text: Labyrinth", "Map screen text: Scrap Brain", "Map screen text: Sky Base" } },
                { "Sonic", new[] { "Sonic (right)", "Sonic (left)", "HUD sprite tiles", "Green Hill palette" } }, // HUD sprites contain the spring jump toes, the ones in the art seem unused...
                { "Monitors", new [] { "Monitor Art", "HUD sprite tiles", "Green Hill palette"  } }, // Monitor bases are in the HUD sprites
                { "Title screen", new [] { "Title screen tiles", "Title screen sprites", "Title screen palette", "Title screen tilemap", "Title screen press button text 1", "Title screen press button text 2", "Title screen music", "Title screen \"PRESS BUTTON\" flash time", "Title screen \"PRESS BUTTON\" total time", "Starting lives count", "Title screen hand X", "Title screen hand Y" } },
                { "Game Over", new [] { "Act Complete tiles", "Game Over palette", "Game Over tilemap", "Game Over: Continue top", "Game Over: Continue bottom" } },
                { "Act Complete", new [] { "Act Complete tiles", "Act Complete palette", "Act Complete tilemap", "HUD sprite tiles", "Extra life at n x 10,000 points", "Extra life every n x 10,000 subsequent points" } },
                { "Special Stage Complete", new [] { "Act Complete tiles", "Act Complete palette", "Special Stage Complete tilemap", "HUD sprite tiles" } },
                { "Ending 1", new [] { "Map screen 1 tileset", "Ending palette", "Ending 1 tilemap" } },
                { "Ending 2", new [] { "Map screen 1 tileset", "Ending palette", "Ending 2 tilemap", "Ending text: box 1", "Ending text: box 2", "Ending text: box 3", "Ending text: box 4", "Ending text: box 5", "Ending text: box 6", "Ending text: box 7", "Ending text: box 8", "Ending text: Chaos Emerald", "Ending text: Sonic Left", "Ending text: Special Bonus" } },
                { "Credits", new [] { "Map screen 2 tileset", "Title screen sprites", "Credits tilemap", "Credits palette",  "Credits eyebrow X", "Credits eyebrow Y", "Credits mouth 1 X", "Credits mouth 1 Y", "Credits mouth 2 X", "Credits mouth 2 Y", "Credits mouth 3 X", "Credits mouth 3 Y", "Credits foot 1 X", "Credits foot 1 Y", "Credits foot 2 X", "Credits foot 2 Y", "Credits arm 1 X", "Credits arm 1 Y", "Credits arm 2 X", "Credits arm 2 Y", "Credits arm 3 X", "Credits arm 3 Y",  } },
                { "End sign", new [] { "End sign tileset", "End sign palette" } },
                { "Rings", new [] { "Rings", "Green Hill palette" } },
                { "Dr. Robotnik", new [] { "Boss sprites 1", "Boss sprites 2", "Boss sprites 3", "Boss sprites palette" } },
                { "Capsule", new [] { "Capsule sprites", "Boss sprites palette" } }
            },
            AssetGroups2 = new Dictionary<string, List<Game.AssetGroupItem>>
            {
                { "Sonic", new() {
                    new() {
                        AssetName = "Sonic (right)", 
                        References = new() {
                            new() { Offset = 0x4c84 + 1, Type = Game.Reference.Types.Slot1 }, // ld bc,$4000 ; 004C84 01 00 40 
                            new() { Offset = 0x012d + 1, Type = Game.Reference.Types.PageNumber }, // ld a,$08 ; 00012D 3E 08 
                            new() { Offset = 0x0135 + 1, Type = Game.Reference.Types.PageNumber, Delta = 1 }, // ld a,$09 ; 000135 3E 09 
                            // We put this one at the end so it won't be used for reading but will be written.
                            // This allows us to make sure the left-facing art pointer is in the right place while removing the padding.
                            // We want this sprite set plus the left-facing ones to be in the same 32KB window, and to have the two pointers
                            // relative to the same base. This isn't currently explicitly handled.
                            new() { Offset = 0x4c8d + 1, Type = Game.Reference.Types.Slot1, Delta = 39 * 24 * 32 * 3/8}, // ld bc,$7000 ; 004C8D 01 00 70
                            new() { Offset = 0x4E49 + 1, Type = Game.Reference.Types.Slot1, Delta = 39 * 24 * 32 * 3/8}  // ld bc,$7000 ; 004E49 01 00 70 ; Needed for dropped rings
                    }}, new() {
                        AssetName = "Sonic (left)",
                        References = new()
                        {
                            new() { Offset = 0x4c8d + 1, Type = Game.Reference.Types.Slot1 }, // ld bc,$7000 ; 004C8D 01 00 70
                            new() { Offset = 0x012d + 1, Type = Game.Reference.Types.PageNumber }, // ld a,$08 ; 00012D 3E 08 
                            new() { Offset = 0x0135 + 1, Type = Game.Reference.Types.PageNumber, Delta = 1 } // ld a,$09 ; 000135 3E 09 
                    }}, new() {
                        AssetName = "HUD sprite tiles", // HUD sprites contain the spring jump toes, the ones in the art are unused...
                        References = new()
                        {
                            // Level loader
                            new() {Offset = 0x2172 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 002172 21 2E B9 
                            new() {Offset = 0x2178 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 002178 3E 09 
                        }
                    }, new() {
                        AssetName = "Green Hill palette"
                        // No explicit references, Sonic takes the level sprite palette
                    }
                } }, 
                { "Map screen 1", new() {
                    new() {
                        AssetName = "Map screen 1 tileset", 
                        References = new() {
                            new() { Offset = 0x0c89 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000 }, // ld hl,$0000 ; 000C89 21 00 00
                            new() { Offset = 0x0c8f + 1, Type = Game.Reference.Types.PageNumber }, // ld a,$0c    ; 000C8F 3E 0C 
                    }},
                    new() { 
                        AssetName = "Map screen 1 tilemap 1", 
                        References = new()
                        {
                            new() {Offset = 0x0caa + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 000CAA 3E 05 
                            new() {Offset = 0x0cb2 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$627e ; 000CB2 21 7E 62
                            new() {Offset = 0x0cb5 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0178 ; 000CB5 01 78 01
                    }},
                    new() { 
                        AssetName = "Map screen 1 tilemap 2", 
                        References = new() {
                            new() {Offset = 0x0caa + 1, Type = Game.Reference.Types.PageNumber}, // We duplicate this to read it in
                            new() {Offset = 0x0cc3 + 1, Type = Game.Reference.Types.Slot1}, // ld hl,$63f6 ; 000CC3 21 F6 63 
                            new() {Offset = 0x0cc6 + 1, Type = Game.Reference.Types.Size}   // ld bc,$0145 ; 000CC6 01 45 01 
                    }},
                    new() { 
                        AssetName = "Map screen 1 palette", 
                        References = new() {
                            new() {Offset = 0x0cd4 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$0f0e ; 000CD4 21 0E 0F 
                    }},
                    new() { 
                        AssetName = "Map screen 1 sprite tiles", 
                        References = new() {
                            new() {Offset = 0x0c94 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$526b ; 000C94 21 6B 52 
                            new() {Offset = 0x0c9a + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 000C9A 3E 09 
                    }},
                    new() { AssetName = "HUD sprite tiles", References = new()
                    {
                        new() {Offset = 0x0C9F + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 000C9F 21 2E B9 
                        new() {Offset = 0x0CA5 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 000CA5 3E 09 
                    }},
                    new() { AssetName = "Map screen text: Green Hill", References = new()
                    {
                        // TODO: these references are per level and could be split up
                        new() {Offset = 0x1209, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x120b, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x120d, Type = Game.Reference.Types.Absolute},
                    }},
                    new() { AssetName = "Map screen text: Bridge", References = new()
                    {
                        new() {Offset = 0x120f, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x1211, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x1213, Type = Game.Reference.Types.Absolute},
                    }},
                    new() { AssetName = "Map screen text: Jungle", References = new()
                    {
                        new() {Offset = 0x1215, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x1217, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x1219, Type = Game.Reference.Types.Absolute},
                    }}
                } }, // HUD sprites only used for life counter
                { "Map screen 2", new() {
                    new() { AssetName = "Map screen 2 tileset", References = new()
                    {
                        new() {Offset = 0x0ceb+1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$1801 ; 000CEB 21 01 18 
                        new() {Offset = 0x0cf1+1, Type = Game.Reference.Types.PageNumber},             // ld a,$0c    ; 000CF1 3E 0C 
                    }}, 
                    new() { AssetName = "Map screen 2 tilemap 1", References = new() {
                        new() {Offset = 0x0d0c + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05     ; 000D0C 3E 05 
                        new() {Offset = 0x0d14 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$653b  ; 000D14 21 3B 65
                        new() {Offset = 0x0d17 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0170  ; 000D17 01 70 01
                    }}, 
                    new() { AssetName = "Map screen 2 tilemap 2", References = new() {
                        new() {Offset = 0x0d0c + 1, Type = Game.Reference.Types.PageNumber}, // Same as above
                        new() {Offset = 0x0d25 + 1, Type = Game.Reference.Types.Slot1}, // ld hl,$66ab ; 000D25 21 AB 66 
                        new() {Offset = 0x0d28 + 1, Type = Game.Reference.Types.Size}   // ld bc,$0153 ; 000D28 01 53 01 
                    }}, 
                    new() { AssetName = "Map screen 2 palette", References = new() {
                        new() {Offset = 0x0d36 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$0f2e ; 000D36 21 2E 0F 
                    }}, 
                    new() { AssetName = "Map screen 2 sprite tiles", References = new() {
                        new() {Offset = 0x0cf6 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$5942 ; 000CF6 21 42 59 
                        new() {Offset = 0x0cfc + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 000CFC 3E 09 
                    }}, 
                    new() { AssetName = "HUD sprite tiles", References = new() { // Used for life counter only
                        new() {Offset = 0x0D01 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 000D01 21 2E B9 
                        new() {Offset = 0x0D07 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 000D07 3E 09 
                    }}, 
                    new() { AssetName = "Map screen text: Labyrinth", References = new() {
                        // TODO: these references are per level and could be split up
                        new() {Offset = 0x121b, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x121d, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x121f, Type = Game.Reference.Types.Absolute},
                    }}, 
                    new() { AssetName = "Map screen text: Scrap Brain", References = new() {
                        new() {Offset = 0x1221, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x1223, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x1225, Type = Game.Reference.Types.Absolute},
                    }}, 
                    new() { AssetName = "Map screen text: Sky Base", References = new() {
                        new() {Offset = 0x1227, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x1229, Type = Game.Reference.Types.Absolute},
                        new() {Offset = 0x122b, Type = Game.Reference.Types.Absolute},
                    }}
                } },
                { "Monitors", new() {
                    new() { AssetName = "Monitor Art", References = new List<Game.Reference> {
                            new() { Offset = 0x5B31 + 1, Type = Game.Reference.Types.Slot1 }, // ld hl, $5180 ; 005B31 21 80 51 
                            new() { Offset = 0x5F09 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5400 - 0x5180 }, // ld hl, $5400 ; 005F09 21 00 54
                            new() { Offset = 0xBF50 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5400 - 0x5180 }, // ld hl, $5400 ; 00BF50 21 00 54
                            new() { Offset = 0x5BFF + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5200 - 0x5180 }, // ld hl, $5200 ; 005BFF 21 00 52
                            new() { Offset = 0x5C6D + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5280 - 0x5180 }, // ld hl, $5280 ; 005C6D 21 80 52
                            new() { Offset = 0x5CA7 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5180 - 0x5180 }, // ld hl, $5100 ; 005CA7 21 80 51
                            new() { Offset = 0x5CB2 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5280 - 0x5180 }, // ld hl, $5200 ; 005CB2 21 80 52
                            new() { Offset = 0x5CF9 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5300 - 0x5180 }, // ld hl, $5300 ; 005CF9 21 00 53
                            new() { Offset = 0x5D29 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5380 - 0x5180 }, // ld hl, $5380 ; 005D29 21 80 53
                            new() { Offset = 0x5D7A + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5480 - 0x5180 }, // ld hl, $5480 ; 005D7A 21 80 54
                            new() { Offset = 0x5DA2 + 1, Type = Game.Reference.Types.Slot1, Delta = 0x5500 - 0x5180 }, // ld hl, $5500 ; 005DA2 21 00 55
                            new() { Offset = 0x0c1e + 1, Type = Game.Reference.Types.PageNumber } // ld a,$05 ; 000C1E 3E 05 
                        }
                    }, 
                    new() { AssetName = "HUD sprite tiles"}, // Monitor bases are in the HUD sprites, no references here (?)
                    new() { AssetName = "Green Hill palette"} // Arbitrary choice of palette, no references here
                } },
                { "Title screen", new() {
                    new() { AssetName = "Title screen tiles", References = new() {
                        new() {Offset = 0x1296 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$2000 ; 001296 21 00 20 
                        new() {Offset = 0x129c + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 00129C 3E 09 
                    }}, 
                    new() { AssetName = "Title screen sprites", References = new() {
                        new() {Offset = 0x12a1 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$4b0a ; 0012A1 21 0A 4B 
                        new() {Offset = 0x12a7 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 0012A7 3E 09 
                    }}, 
                    new() { AssetName = "Title screen palette", References = new() {
                        new() {Offset = 0x12cc + 1, Type = Game.Reference.Types.Absolute} // ld hl,$13e1 ; 0012CC 21 E1 13 
                    }}, 
                    new() { AssetName = "Title screen tilemap", References = new() {
                            new() {Offset = 0x12ac + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 0012AC 3E 05 
                            new() {Offset = 0x12b4 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$6000 ; 0012B4 21 00 60
                            new() {Offset = 0x12ba + 1, Type = Game.Reference.Types.Size}        // ld bc,$012e ; 0012BA 01 2E 01
                    }}, 
                    new() { AssetName = "Title screen press button text 1", References = new() {
                        new() {Offset = 0x1305 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1352 ; 001305 21 52 13 
                    }}, 
                    new() { AssetName = "Title screen press button text 2", References = new() {
                        new() {Offset = 0x130c + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1362 ; 00130C 21 62 13
                    }}, 
                    new() { AssetName = "Title screen music" }, 
                    new() { AssetName = "Title screen \"PRESS BUTTON\" flash time" }, 
                    new() { AssetName = "Title screen \"PRESS BUTTON\" total time" }, 
                    new() { AssetName = "Starting lives count" }, 
                    new() { AssetName = "Title screen hand X" }, 
                    new() { AssetName = "Title screen hand Y" }
                } },
                { "Game Over", new() {
                    new() { AssetName = "Act Complete tiles", References = new()
                    {
                        new() {Offset = 0x1411 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$351f ; 001411 21 1F 35 
                        new() {Offset = 0x1417 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 001417 3E 09 
                    }}, 
                    new() { AssetName = "Game Over palette", References = new()
                    {
                        new() {Offset = 0x143c + 1, Type = Game.Reference.Types.Absolute} // ld hl,$14fc ; 00143C 21 FC 14 
                    }}, 
                    new() { AssetName = "Game Over tilemap", References = new()
                    {
                        new() {Offset = 0x141c + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 00141C 3E 05 
                        new() {Offset = 0x1424 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$67fe ; 001424 21 FE 67
                        new() {Offset = 0x1427 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0032 ; 001427 01 32 00
                    }}, 
                    new() { AssetName = "Game Over: Continue top", References = new()
                    {
                        new() {Offset = 0x1485 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$14f1 ; 001485 21 F1 14 
                    }}, 
                    new() { AssetName = "Game Over: Continue bottom", References =  new()
                    {
                        new() {Offset = 0x147f + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$14e6 ; 00147F 21 E6 14 
                    }}
                }}, { "Act/Special Stage Complete", new()
                {
                    new() { AssetName = "Act Complete tiles", References = new()
                    {
                        new() {Offset = 0x1580 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$351f ; 001580 21 1F 35 
                        new() {Offset = 0x1586 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 001586 3E 09 
                    }}, 
                    new() { AssetName = "Act Complete palette", References = new()
                    {
                        new() {Offset = 0x1604 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$1b8d ; 001604 21 8D 1B 
                    }}, 
                    new() { AssetName = "Act Complete tilemap", References = new()
                    {
                        new() {Offset = 0x158B + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 00158B 3E 05 
                        new() {Offset = 0x1593 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$612e ; 001593 21 2E 61
                        new() {Offset = 0x1596 + 1, Type = Game.Reference.Types.Size}        // ld bc,$00bb ; 001596 01 BB 00
                    }},
                    new() { AssetName = "Special Stage Complete tilemap", References = new()
                    {
                        new() {Offset = 0x158B + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 00158B 3E 05 // Shared with Act Complete tilemap
                        new() {Offset = 0x15A3 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$61e9 ; 0015A3 21 E9 61 
                        new() {Offset = 0x15A6 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0095 ; 0015A6 01 95 00 
                    }}, 
                    new() { AssetName = "HUD sprite tiles", References = new()
                    {
                        new() {Offset = 0x1575 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 001575 21 2E B9 
                        new() {Offset = 0x157B + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 00157B 3E 09 
                    }},
                    new() { AssetName = "Extra life at n x 10,000 points"},
                    new() { AssetName = "Extra life every n x 10,000 subsequent points" },
                }}, { "Ending", new() {
                    new() { AssetName = "Map screen 1 tileset", References =  new() {
                        new() {Offset = 0x25a9+1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$0000 ; 0025A9 21 00 00
                        new() {Offset = 0x25af+1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 0025AF 3E 0C 
                    }}, 
                    new() { AssetName = "Ending palette", References =  new() {
                        new() { Offset = 0x25a1 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$2828 ; 0025A1 21 28 28 // Palette load
                        new() { Offset = 0x268d + 1, Type = Game.Reference.Types.Absolute}  // ld hl,$2828 ; 00268D 21 28 28 // Fade out
                    }}, 
                    new() { AssetName = "Ending 1 tilemap", References = new() {
                        new() {Offset = 0x25B4 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 0025B4 3E 05 
                        new() {Offset = 0x25BC + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$6830 ; 0025BC 21 30 68
                        new() {Offset = 0x25BF + 1, Type = Game.Reference.Types.Size}        // ld bc,$0179 ; 0025BF 01 79 01
                    }},
                    new() { AssetName = "Ending 2 tilemap", References = new() {
                        new() {Offset = 0x2675 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 002675 3E 05 
                        new() {Offset = 0x267D + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$69a9 ; 00267D 21 A9 69
                        new() {Offset = 0x2680 + 1, Type = Game.Reference.Types.Size}        // ld bc,$0145 ; 002680 01 45 01
                    }}, 
                    new() { AssetName = "Ending text: box 1", References = new() {
                        new() {Offset = 0x1785 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1907 ; 001785 21 07 19 
                    }}, 
                    new() { AssetName = "Ending text: box 2", References = new() {
                        new() {Offset = 0x178B + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$191c ; 00178B 21 1C 19 
                    }}, 
                    new() { AssetName = "Ending text: box 3", References = new() {
                        new() {Offset = 0x1791 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1931 ; 001791 21 31 19 
                    }}, 
                    new() { AssetName = "Ending text: box 4", References = new() {
                        new() {Offset = 0x1797 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1946 ; 001797 21 46 19 
                    }}, 
                    new() { AssetName = "Ending text: box 5", References = new() {
                        new() {Offset = 0x179D + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1953 ; 00179D 21 53 19
                    }}, 
                    new() { AssetName = "Ending text: box 6", References = new() {
                        new() {Offset = 0x17A3 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$1960 ; 0017A3 21 60 19 
                    }}, 
                    new() { AssetName = "Ending text: box 7", References = new() {
                        new() {Offset = 0x17A9 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$196d ; 0017A9 21 6D 19
                    }}, 
                    new() { AssetName = "Ending text: box 8", References = new() {
                        new() {Offset = 0x1823 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$197a ; 001823 21 7A 19 
                    }}, 
                    new() { AssetName = "Ending text: Chaos Emerald", References = new() {
                        new() {Offset = 0x17af + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$197e ; 0017AF 21 7E 19 
                    }}, 
                    new() { AssetName = "Ending text: Sonic Left", References = new() {
                        new() {Offset = 0x17e8 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$198e ; 0017E8 21 8E 19 
                    }}, 
                    new() { AssetName = "Ending text: Special Bonus", References = new() {
                        new() {Offset = 0x181d + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$199e ; 00181D 21 9E 19 
                    }} 
                } }, { "Credits", new() {
                    new() { AssetName = "Map screen 2 tileset", References = new() {
                        new() {Offset = 0x26ab+1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$1801 ; 0026AB 21 01 18
                        new() {Offset = 0x26b1+1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 0026B1 3E 0C 
                    }},
                    new() { AssetName = "Title screen sprites", References = new() {
                        new() {Offset = 0x26B6 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$4b0a ; 0026B6 21 0A 4B 
                        new() {Offset = 0x26BC + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 0026BC 3E 09 
                    }},
                    new() { AssetName = "Credits tilemap", References = new() {
                        new() {Offset = 0x26C1 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$05    ; 0026C1 3E 05 
                        new() {Offset = 0x26C9 + 1, Type = Game.Reference.Types.Slot1},      // ld hl,$6c61 ; 0026C9 21 61 6C
                        new() {Offset = 0x26CC + 1, Type = Game.Reference.Types.Size}        // ld bc,$0189 ; 0026CC 01 89 01
                    }},
                    new() { AssetName = "Credits palette", References = new() {
                        new() {Offset = 0x2702 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$2ad6 ; 002702 21 D6 2A 
                    }},
                    new() { AssetName = "Credits eyebrow X"},
                    new() { AssetName = "Credits eyebrow Y"},
                    new() { AssetName = "Credits mouth 1 X"},
                    new() { AssetName = "Credits mouth 1 Y"},
                    new() { AssetName = "Credits mouth 2 X"},
                    new() { AssetName = "Credits mouth 2 Y"},
                    new() { AssetName = "Credits mouth 3 X"},
                    new() { AssetName = "Credits mouth 3 Y"},
                    new() { AssetName = "Credits foot 1 X"},
                    new() { AssetName = "Credits foot 1 Y"},
                    new() { AssetName = "Credits foot 2 X"},
                    new() { AssetName = "Credits foot 2 Y"},
                    new() { AssetName = "Credits arm 1 X"},
                    new() { AssetName = "Credits arm 1 Y"},
                    new() { AssetName = "Credits arm 2 X"},
                    new() { AssetName = "Credits arm 2 Y"},
                    new() { AssetName = "Credits arm 3 X"},
                    new() { AssetName = "Credits arm 3 Y"}
                }}, { "End sign", new() {
                    new() { AssetName = "End sign tileset", References =  new() {
                        new() { Offset = 0x5f2d + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000 }, // ld hl,$4294 ; 005F2D 21 94 42 // This is actually into page 10
                        new() { Offset = 0x5f33 + 1, Type = Game.Reference.Types.PageNumber } // ld a,$09    ; 005F33 3E 09 
                    }}, 
                    new() { AssetName = "End sign palette", References =  new() {
                        new() { Offset = 0x5F38 + 1, Type = Game.Reference.Types.Absolute } // ld hl,$626c ; 005F38 21 6C 62
                    }}
                }}, { "Rings", new()
                {
                    new(){AssetName = "Rings", References = new() {
                        new() {Offset = 0x23AD + 1, Type = Game.Reference.Types.Slot1},      // ld de,$7cf0 ; 0023AD 11 F0 7C 
                        new() {Offset = 0x1D55 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$0b ; 001D55 3E 0B 
                        new() {Offset = 0x1DB5 + 1, Type = Game.Reference.Types.PageNumber}, // ld a,$0b ; 001DB5 3E 0B 
                        new() {Offset = 0x1eb1 + 1, Type = Game.Reference.Types.PageNumber}  // ld a,$0b ; 001eb1 3E 0B 
                    }}, new() {AssetName = "Green Hill palette"} // No references here
                } },
                // TODO: join Dr. Robotniks to relevant stages?
                { "Dr. Robotnik #1", new() {
                    new() {
                        AssetName = "Boss sprites 1", References = new() {
                            new() {Offset = 0x7031 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$aeb1 ; 007031 21 B1 AE 
                            new() {Offset = 0x7037 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 007037 3E 09 
                        }
                    }, new() {
                        AssetName = "Boss sprites palette", References = new() {
                            new() {Offset = 0x703C + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00703C 21 1C 73
                        }
                    }
                } },
                { "Dr. Robotnik #2", new() {
                    new() {
                        AssetName = "Boss sprites 1", References = new() {
                            new() {Offset = 0x8074 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$aeb1 ; 008074 21 B1 AE 
                            new() {Offset = 0x807A + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 00807A 3E 09 
                        }
                    }, new() {
                        AssetName = "Boss sprites palette", References = new() {
                            new() {Offset = 0x807F + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00807F 21 1C 73
                        }
                    }
                } },
                { "Dr. Robotnik #3", new() {
                    new() {
                        AssetName = "Boss sprites 2", References = new() {
                            new() {Offset = 0x84BC + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$e508 ; 0084BC 21 08 E5 
                            new() {Offset = 0x84C2 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$0c    ; 0084C2 3E 0C 
                        }
                    }, new() {
                        AssetName = "Boss sprites palette", References = new() {
                            new() {Offset = 0x84C7 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 0084C7 21 1C 73
                        }
                    }
                } },
                { "Dr. Robotnik #4", new() {
                    new() {
                        AssetName = "Boss sprites 2", References = new() {
                            new() {Offset = 0x9291 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$e508 ; 009291 21 08 E5 
                            new() {Offset = 0x9297 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 009297 3E 0C 
                        }
                    }, new() {
                        AssetName = "Boss sprites palette", References = new() {
                            new() {Offset = 0x929C + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00929C 21 1C 73
                        }
                    }
                } },
                { "Dr. Robotnik #5", new() {
                    new() {
                        AssetName = "Boss sprites 3", References = new() {
                            new() {Offset = 0xA816 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$ef3f ; 00A816 21 3F EF 
                            new() {Offset = 0xA81C + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$0c    ; 00A81C 3E 0C 
                        }
                    }, new() {
                        AssetName = "Boss sprites palette", References = new() {
                            new() {Offset = 0xA821 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00A821 21 1C 73
                        }
                    }
                } },
                { "Dr. Robotnik #6", new() {
                    new() {
                        AssetName = "Boss sprites 3", References = new() {
                            new() {Offset = 0xBB94 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$ef3f ; 00BB94 21 3F EF 
                            new() {Offset = 0xBB9A + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$0c    ; 00BB9A 3E 0C 
                        }
                    }, new() {
                        AssetName = "Boss sprites palette", References = new() {
                            new() {Offset = 0xBE07 + 1, Type = Game.Reference.Types.Absolute}  // ld hl,$731c ; 00BE07 21 1C 73
                        }
                    }
                } },
                { "Capsule", new() {
                        new() {
                            AssetName = "Capsule sprites", References =  new() {
                                new() { Offset = 0x7916 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000 }, // ld hl,$da28 ; 007916 21 28 DA 
                                new() { Offset = 0x791C + 1, Type = Game.Reference.Types.PageNumber }              // ld a,$0c    ; 00791C 3E 0C 
                            }
                        }, new() {
                            AssetName = "Boss sprites palette" // Dependent on palette left over from boss
                        }
                    }
                }
            },
            Levels = new List<Game.LevelHeader>
            {
                new() { Name = "Green Hill Act 1", Offset = 0x15580 + 0x4a },
                new() { Name = "Green Hill Act 2", Offset = 0x15580 + 0x6f },
                new() { Name = "Green Hill Act 3", Offset = 0x15580 + 0x94 },
                new() { Name = "Bridge Act 1", Offset = 0x15580 + 0xde },
                new() { Name = "Bridge Act 2", Offset = 0x15580 + 0x103 },
                new() { Name = "Bridge Act 3", Offset = 0x15580 + 0x128 },
                new() { Name = "Jungle Act 1", Offset = 0x15580 + 0x14d },
                new() { Name = "Jungle Act 2", Offset = 0x15580 + 0x172 },
                new() { Name = "Jungle Act 3", Offset = 0x15580 + 0x197 },
                new() { Name = "Labyrinth Act 1", Offset = 0x15580 + 0x1bc },
                new() { Name = "Labyrinth Act 2", Offset = 0x15580 + 0x1e1 },
                new() { Name = "Labyrinth Act 3", Offset = 0x15580 + 0x206 },
                new() { Name = "Scrap Brain Act 1", Offset = 0x15580 + 0x22b },
                new() { Name = "Scrap Brain Act 2", Offset = 0x15580 + 0x250 },
                new() { Name = "Scrap Brain Act 2, from Emerald Maze", Offset = 0x15580 + 0x2e4 },
                new() { Name = "Scrap Brain Act 2, from Ball Hog Area", Offset = 0x15580 + 0x309 },
                new() { Name = "Scrap Brain Act 2, from transporter", Offset = 0x15580 + 0x32e },
                new() { Name = "Scrap Brain Act 2 (Ball Hog Area)", Offset = 0x15580 + 0x29a },
                new() { Name = "Scrap Brain Act 2 (Emerald Maze), from corridor", Offset = 0x15580 + 0x275 },
                new() { Name = "Scrap Brain Act 2 (Emerald Maze), from transporter", Offset = 0x15580 + 0x353 },
                new() { Name = "Scrap Brain Act 3", Offset = 0x15580 + 0x2bf },
                new() { Name = "Sky Base Act 1", Offset = 0x15580 + 0x378 },
                new() { Name = "Sky Base Act 2", Offset = 0x15580 + 0x39d },
                new() { Name = "Sky Base Act 2 (Interior)", Offset = 0x15580 + 0x3e7 },
                new() { Name = "Sky Base Act 3", Offset = 0x15580 + 0x3c2 },
                new() { Name = "Ending Sequence", Offset = 0x15580 + 0xb9 },
                new() { Name = "Special Stage 1", Offset = 0x15580 + 0x40c },
                new() { Name = "Special Stage 2", Offset = 0x15580 + 0x431 },
                new() { Name = "Special Stage 3", Offset = 0x15580 + 0x456 },
                new() { Name = "Special Stage 4", Offset = 0x15580 + 0x47b },
                new() { Name = "Special Stage 5", Offset = 0x15580 + 0x4a0 },
                new() { Name = "Special Stage 6", Offset = 0x15580 + 0x4c5 },
                new() { Name = "Special Stage 7", Offset = 0x15580 + 0x4ea },
                new() { Name = "Special Stage 8", Offset = 0x15580 + 0x50f }
            }
        };

        public Memory Memory { get; }
        public List<Level> Levels { get; } = new();
        public List<ArtItem> Art { get; } = new();
        public FreeSpace LastFreeSpace { get; private set; }
        public SdscTag SdscTag { get; set; }

        private readonly Dictionary<int, TileSet> _tileSets = new();
        private readonly Dictionary<int, Floor> _floors = new();
        private readonly Dictionary<int, BlockMapping> _blockMappings = new();
        private readonly Dictionary<int, Palette> _palettes = new();
        private readonly Dictionary<Game.Asset, IDataItem> _assetsLookup = new();
        private readonly Dictionary<int, LevelObjectSet> _levelObjects = new();

        public Cartridge(string path, Action<string> logger)
        {
            _logger = logger;
            logger($"Loading {path}...");
            var sw = Stopwatch.StartNew();
            Memory = new Memory(File.ReadAllBytes(path));
            DisposeAll(_blockMappings);
            DisposeAll(_tileSets);

            ReadAssets();
            ReadLevels();
            ReadSdscTag();

            // Apply rings to level tile sets
            var rings = Art.Find(x => x.Name == "Rings");
            if (rings != null)
            {
                foreach (var tileSet in Levels.Select(x => x.TileSet).Distinct())
                {
                    tileSet.SetRings(rings.TileSet.Tiles[0]);
                }
            }

            _logger($"Load complete in {sw.Elapsed}");
        }

        private void ReadSdscTag()
        {
            if (Memory.String(0x7fe0, 4) != "SDSC")
            {
                SdscTag = null;
                return;
            }

            SdscTag = new SdscTag(Memory);
        }

        private void ReadAssets()
        {
            _assetsLookup.Clear();

            _logger("Loading art...");

            if (!Sonic1MasterSystem.AssetGroups.ContainsKey("All palettes"))
            {
                Sonic1MasterSystem.AssetGroups.Add("All palettes", Sonic1MasterSystem.Assets
                    .Where(x => x.Value.Type == Game.Asset.Types.Palette)
                    .Select(x => x.Key)); // TODO something better with these. Maybe have the level loader write the palette tables?
            }

            // TODO change this to AssetGroups2
            foreach (var (name, assets) in Sonic1MasterSystem.AssetGroups)
            {
                var item = new ArtItem{Name = name};
                foreach (var part in assets.Select(x => new { Name = x, Asset = Sonic1MasterSystem.Assets[x]}))
                {
                    var asset = part.Asset;
                    item.Assets.Add(asset);

                    var offset = asset.GetOffset(Memory);

                    _logger($"- Loading {asset.Type} \"{part.Name}\" from ${offset:X}");

                    switch (asset.Type)
                    {
                        case Game.Asset.Types.TileSet:
                            _assetsLookup[asset] = item.TileSet = asset.BitPlanes > 0 
                                ? GetTileSet(offset, asset.GetLength(Memory), asset.BitPlanes, asset.TileGrouping, asset.TilesPerRow) 
                                : GetTileSet(offset, asset.TileGrouping, asset.TilesPerRow);
                            break;
                        case Game.Asset.Types.Palette:
                            _assetsLookup[asset] = item.Palette = GetPalette(offset, asset.FixedSize / 16);
                            item.PaletteEditable = !asset.Hidden; // Hidden only applies to palettes for now...
                            // TODO we don't handle multiple palettes here yet
                            break;
                        case Game.Asset.Types.ForegroundTileMap:
                            // We assume these are set first
                            _assetsLookup[asset] = item.TileMap = new TileMap(Memory, offset, asset.GetLength(Memory));
                            item.TileMap.SetAllForeground();
                            // TODO we don't support editing foreground/background yet
                            break;
                        case Game.Asset.Types.TileMap:
                        {
                            // We assume these are set second so we have to check if it's a set or overlay
                            var tileMap = new TileMap(Memory, offset, asset.GetLength(Memory));
                            if (tileMap.IsOverlay() && item.TileMap != null)
                            {
                                item.TileMap.OverlayWith(tileMap);
                                _assetsLookup[asset] = item.TileMap; // Point at the same object for both
                            }
                            else
                            {
                                _assetsLookup[asset] = item.TileMap = tileMap;
                            }
                            break;
                        }
                        case Game.Asset.Types.TileMapData:
                            var tileMapData = new TileMapData(Memory, offset, part.Name);
                            _assetsLookup[asset] = tileMapData;
                            item.TileMapData.Add(tileMapData);
                            break;
                        case Game.Asset.Types.SpriteTileSet:
                            var tileSet = asset.BitPlanes > 0 
                                ? GetTileSet(offset, asset.GetLength(Memory), asset.BitPlanes, asset.TileGrouping, asset.TilesPerRow) 
                                : GetTileSet(offset, asset.TileGrouping, asset.TilesPerRow);
                            _assetsLookup[asset] = tileSet;
                            item.SpriteTileSets.Add(tileSet);
                            break;
                        case Game.Asset.Types.RawValue:
                            var rawValue = new RawValue(Memory, part.Asset.OriginalOffset, part.Asset.OriginalSize, part.Asset.Encoding, part.Name);
                            _assetsLookup[asset] = rawValue;
                            item.RawValues.Add(rawValue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                Art.Add(item);
            }
        }

        private void ReadLevels()
        {
            _logger("Loading levels...");
            Levels.Clear();
            foreach (var level in Sonic1MasterSystem.Levels)
            {
                _logger($"- Loading level {level.Name} at offset ${level.Offset:X}...");
                Levels.Add(new Level(this, level.Offset, level.Name));
            }
        }

        public TileSet GetTileSet(int offset, List<Point> grouping, int tilesPerRow)
        {
            return GetItem(_tileSets, offset, () => new TileSet(Memory, offset, grouping, tilesPerRow));
        }

        private TileSet GetTileSet(int offset, int length, int bitPlanes, List<Point> tileGrouping, int tilesPerRow)
        {
            return GetItem(_tileSets, offset, () => new TileSet(Memory, offset, length, bitPlanes, tileGrouping, tilesPerRow));
        }

        public Floor GetFloor(int offset, int compressedSize, int width)
        {
            return GetItem(_floors, offset, () => new Floor(this, offset, compressedSize, width));
        }

        public BlockMapping GetBlockMapping(int offset, int blockCount, int solidityIndex, TileSet tileSet)
        {
            return GetItem(_blockMappings, offset, () => new BlockMapping(this, offset, blockCount, solidityIndex, tileSet));
        }

        public Palette GetPalette(int offset, int count)
        {
            return GetItem(_palettes, offset, () => new Palette(Memory, offset, count));
        }

        public LevelObjectSet GetLevelObjectSet(int offset)
        {
            return GetItem(_levelObjects, offset, () => new LevelObjectSet(this, offset));
        }


        private static T GetItem<T>(IDictionary<int, T> dictionary, int offset, Func<T> generatorFunc) 
        {
            if (!dictionary.TryGetValue(offset, out var result))
            {
                result = generatorFunc();
                dictionary.Add(offset, result);
            }

            return result;
        }

        public void Dispose()
        {
            DisposeAll(_tileSets);
            DisposeAll(_blockMappings);
        }

        private static void DisposeAll<T>(Dictionary<int, T> collection) where T: IDisposable
        {
            foreach (var item in collection.Values)
            {
                item.Dispose();
            }
            collection.Clear();
        }

        // This holds the useful parts about an asset we want to pack, pre-serialized so we avoid doing that more than once.
        private class AssetToPack
        {
            public string Name { get; }
            public Game.Asset Asset { get; }
            public IDataItem DataItem { get; }
            public IList<byte> Data { get; }

            public AssetToPack(string name, Game.Asset asset, IDataItem dataItem, IList<byte> data)
            {
                Name = name;
                Asset = asset;
                DataItem = dataItem;
                Data = data;
            }

            public override string ToString()
            {
                return $"{Name}: {Asset}";
            }
        }

        private class RawAsset : IDataItem
        {
            public int Offset { get; set; }
            public IList<byte> GetData()
            {
                throw new NotImplementedException();
            }
        }

        public byte[] MakeRom(bool log = true)
        {
            if (log)
            {
                _logger("Building ROM...");
            }
            var sw = Stopwatch.StartNew();
            // We clone the memory to a memory stream
            var memory = new byte[512 * 1024];
            Memory.GetStream(0, Memory.Count).ToArray().CopyTo(memory, 0);

            // Here is what we do not relocate...
            // Start    End     Description                         Repacking?
            // -----------------------------------------------------------------------------
            // 0627c    0679d   Level palette lookups
            // 15580    155c9   Level header pointers               Untouched
            // 155ca    15ab3   Level headers                       In-place
            // 3e9fd            Solidity data start?                Not planning on moving this...

            // We work through the data types...

            // We start from the asset groups here so we don't pick up any unused or blank parts
            // TODO change this to AssetGroups2
            // TODO We also need to change the way references to the asset are found as they are no longer static per asset (we may split shared references)
            var assetsToPack = new HashSet<AssetToPack>(
                Sonic1MasterSystem.AssetGroups.Values
                    .SelectMany(x => x) // Flatten all the groups
                    .Distinct() // Remove duplicates (by reference)
                    .Select(x => new AssetToPack(x, Sonic1MasterSystem.Assets[x], _assetsLookup[Sonic1MasterSystem.Assets[x]], _assetsLookup[Sonic1MasterSystem.Assets[x]].GetData())) // Select the asset name, details and serialized data
                    .Where(x => x.Asset.OriginalOffset != 0)); // Exclude any not yet configured with a source location

            // Raw values must go to their original locations
            foreach (var assetToPack in assetsToPack.Where(x => x.Asset.Type == Game.Asset.Types.RawValue))
            {
                assetToPack.Asset.Restrictions.MinimumOffset = assetToPack.Asset.OriginalOffset;
                assetToPack.Asset.Restrictions.MaximumOffset = assetToPack.Asset.OriginalOffset + assetToPack.Data.Count;
            }

            // First we build a list of "free space". We include all the "original assets" so we will overwrite unused space. Missing "original" data makes us ignore it.
            var freeSpace = new FreeSpace();
            foreach (var asset in Sonic1MasterSystem.Assets.Values.Where(x => x.OriginalOffset != 0))
            {
                freeSpace.Add(asset.OriginalOffset, asset.OriginalOffset + asset.OriginalSize);
            }
            
            // Expand to 512KB here
            freeSpace.Add(256*1024, 512*1024);
            freeSpace.Maximum = 512 * 1024;

            // We do this after adding all the free space spans
            freeSpace.Consolidate();

            InitialFreeSpace = freeSpace.Clone();

            // Then log the state
            if (log) _logger($"Initial free space: {freeSpace}");

            // - SDSC tag
            var sdscParts = new Dictionary<string, AssetToPack>();
            if (SdscTag != null)
            {
                // Add parts for the SDSC header
                void AddString(string name, string value, int referenceOffset)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return;
                    }
                    sdscParts.Add(
                        name,
                        new AssetToPack(
                            name,
                            new Game.Asset
                            {
                                Type = Game.Asset.Types.Misc,
                                Restrictions = new Game.LocationRestriction { MaximumOffset = 0x10000 },
                                References = new List<Game.Reference>
                                {
                                    new() { Offset = referenceOffset, Type = Game.Reference.Types.Absolute }
                                }
                            },
                            new RawAsset(),
                            Encoding.UTF8.GetBytes(value + "\0")));
                }

                AddString("SDSC author", SdscTag.Author, 0x7fea);
                AddString("SDSC title", SdscTag.Title, 0x7fec);
                AddString("SDSC notes", SdscTag.Notes, 0x7fee);

                if (sdscParts.Count > 0)
                {
                    sdscParts.Add("Header", new AssetToPack("SDSC header", new Game.Asset{Type = Game.Asset.Types.Misc, Restrictions = new Game.LocationRestriction { MinimumOffset = 0x7fe0, MaximumOffset = 0x7fea }}, SdscTag, SdscTag.GetData()));
                    // Set all the pointers to the "unused" value
                    freeSpace.Remove(0x7fea, 6);
                    for (var i = 0x7fea; i < 0x7ff0; ++i)
                    {
                        memory[i] = 0xff; // Unused pointers are $ffff
                    }
                }

                assetsToPack.UnionWith(sdscParts.Values);
            }

            // Add level data to assets list
            // - Palettes
            AddAssets(assetsToPack, Levels.GroupBy(l => l.Palette), "Palette", new Game.LocationRestriction { CanCrossBanks = true, MaximumOffset = 0x8000 });
            AddAssets(assetsToPack, Levels.GroupBy(l => l.CyclingPalette), "Cycling palette", new Game.LocationRestriction { CanCrossBanks = true, MaximumOffset = 0x8000 });
            // - Floors
            // Must be in the range 14000..23fff
            AddAssets(assetsToPack, Levels.GroupBy(l => l.Floor), "Floor", new Game.LocationRestriction { CanCrossBanks = true, MinimumOffset = 0x14000, MaximumOffset = 0x24000 });

            // - Sprite tile sets
            // Game engine expects data in the range 24000..33fff
            AddAssets(assetsToPack, Levels.GroupBy(l => l.SpriteTileSet), "Sprite tiles", new Game.LocationRestriction { CanCrossBanks = true, MinimumOffset = 0x24000, MaximumOffset = 0x34000 });

            // - Level background art
            // Game engine expects data in the range 30000..3ffff
            AddAssets(assetsToPack, Levels.GroupBy(l => l.TileSet), "Sprite tiles", new Game.LocationRestriction { CanCrossBanks = true, MinimumOffset = 0x30000, MaximumOffset = 0x40000 });

            // - Block mappings (at original offsets)
            // TODO make these flexible and make UI to make the lengths flexible. Problem: how to determine the length?
            foreach (var group in Levels.GroupBy(l => l.BlockMapping))
            {
                // We need to place both the block data and solidity data
                foreach (var block in group.Key.Blocks)
                {
                    block.GetData().CopyTo(memory, block.Offset);

                    memory[block.SolidityOffset] = block.Data;
                }

                if (log) _logger($"- Wrote block mapping for level(s) {string.Join(", ", group)} at offset ${group.Key.Blocks[0].Offset:X}");
            }

            // - Level objects
            AddAssets(assetsToPack, Levels.GroupBy(l => l.Objects), "Objects", new Game.LocationRestriction { MinimumOffset = 0x15580, MaximumOffset = 0x16000 }); // Object lists are in the same bank as the headers

            // We avoid writing the same item twice by different routes...
            var writtenItems = new HashSet<IDataItem>();
            var writtenReferences = new HashSet<int>();

            // We look for the ones that "must follow" each other and combine them together
            var followers = assetsToPack
                .Where(x => !string.IsNullOrEmpty(x.Asset.Restrictions.MustFollow))
                .ToDictionary(x => x.Asset.Restrictions.MustFollow);

            // Then we remove them from the list as we will get to them inside the loop when we get to their "precedent"
            assetsToPack.ExceptWith(followers.Values);

            // We write the assets ordered by urgency (in the restricted space) and then by size
            while (assetsToPack.Count > 0)
            {
                // We continuously re-order as the "urgency" changes over time
                var item = assetsToPack
                    .OrderBy(x => freeSpace.GetEaseOfPlacing(x.Data.Count, x.Asset.Restrictions.MinimumOffset,
                        x.Asset.Restrictions.MaximumOffset))
                    .ThenByDescending(x => x.Data.Count)
                    .First();
                try
                {
                    WriteAsset(item, writtenItems, writtenReferences, freeSpace, memory, followers, log);
                    assetsToPack.Remove(item);
                }
                catch (Exception ex)
                {
                    _logger(ex.Message);
                    _logger(freeSpace.ToString());
                    throw new Exception($"{ex.Message} while packing {item.Name}");
                }
            }

            // - Level headers (at original offsets). We do these last so they pick up info from the contained objects.
            foreach (var level in Levels)
            {
                level.GetData().CopyTo(memory, level.Offset);
                if (log)
                {
                    _logger($"- Wrote level header for {level} at offset ${level.Offset:X}");
                }
            }

            // Next we round to a multiple of 64KB. This is needed for Everdrive compatibility.
            var trimmedSize = (int)Math.Ceiling(freeSpace.MaximumUsed / (64.0 * 1024)) * 64 * 1024;
            if (trimmedSize < freeSpace.Maximum)
            {
                var trimmed = new byte[trimmedSize];
                Array.Copy(memory, 0, trimmed, 0, trimmedSize);
                memory = trimmed;
                freeSpace.Remove(trimmedSize, freeSpace.Maximum - trimmedSize);
                freeSpace.Maximum = trimmedSize;
            }

            _logger($"Built ROM image in {sw.Elapsed}");

            LastFreeSpace = freeSpace;

            return memory;
        }

        public FreeSpace InitialFreeSpace { get; private set; }

        private static void AddAssets(ISet<AssetToPack> assetsToPack, IEnumerable<IGrouping<IDataItem, Level>> items, string prefix, Game.LocationRestriction restriction)
        {
            assetsToPack.UnionWith(items.Select(group => new AssetToPack(
                $"{prefix} for {string.Join(", ", group)}",
                new Game.Asset
                {
                    Type = Game.Asset.Types.Palette,
                    Restrictions = restriction
                },
                group.Key,
                group.Key.GetData())));
        }

        private void WriteAsset(AssetToPack item, ISet<IDataItem> writtenItems, HashSet<int> writtenReferences, FreeSpace freeSpace, byte[] memory, Dictionary<string, AssetToPack> followers, bool log)
        {
            int offset;

            // This is a bit of a hack. We want to ensure that we find a space big enough to fit the followers, but we'd like each item to maintain its restrictions.
            // Instead of doing that, we just flip this flag if needed.
            var sizeNeeded = GetSizeNeeded(item, followers);
            if (sizeNeeded > 0x4000)
            {
                item.Asset.Restrictions.CanCrossBanks = true;
            }

            if (writtenItems.Contains(item.DataItem))
            {
                if (log) _logger($"- Data for asset {item.Name} was already written");
                offset = item.DataItem.Offset;
            }
            else
            {
                offset = freeSpace.FindSpace(sizeNeeded, item.Asset.Restrictions);
                item.DataItem.Offset = offset;
                item.Data.CopyTo(memory, offset);
                if (log) _logger($"- Wrote data for asset {item.Name} at ${offset:X}, length {item.Data.Count} bytes");
                freeSpace.Remove(offset, item.Data.Count);

                writtenItems.Add(item.DataItem);
            }

            var size = item.Data.Count;

            // Tilemaps may have two parts. If so, the foreground data comes first and the background tiles second.
            // We fix up the offsets and sizes we write if this is the case.
            if (item.DataItem is TileMap tileMap)
            {
                switch (item.Asset.Type)
                {
                    case Game.Asset.Types.TileMap:
                        offset += tileMap.ForegroundTileMapSize;
                        size = tileMap.BackgroundTileMapSize;
                        break;
                    case Game.Asset.Types.ForegroundTileMap:
                        size = tileMap.ForegroundTileMapSize;
                        break;
                }
            }

            // Then we fix up the references
            if (item.Asset.References != null)
            {
                foreach (var reference in item.Asset.References)
                {
                    if (writtenReferences.Contains(reference.Offset))
                    {
                        if (log) _logger($" - Reference at {reference.Offset:X} was already written");
                        continue;
                    }

                    writtenReferences.Add(reference.Offset);
                    switch (reference.Type)
                    {
                        case Game.Reference.Types.Absolute:
                        {
                            var value = offset + reference.Delta;
                            if (value >= 0x10000)
                            {
                                throw new Exception($"Can't write absolute address for offset {offset:X}: {value:X} is >= 0x10000");
                            }
                            memory[reference.Offset + 0] = (byte)(value & 0xff);
                            memory[reference.Offset + 1] = (byte)(value >> 8);
                            if (log) _logger($" - Wrote location ${value:X} for offset ${offset:X} + {reference.Delta} at reference at {reference.Offset:X}");
                            break;
                        }
                        case Game.Reference.Types.PageNumber:
                        {
                            // Delta applies to the page number, not the offset
                            var value = (byte)(offset / 0x4000 + reference.Delta);
                            memory[reference.Offset] = value;
                            if (log) _logger($" - Wrote page number ${value:X} for offset {offset:X} + {reference.Delta} at reference at {reference.Offset:X}");
                            break;
                        }
                        case Game.Reference.Types.Size:
                        {
                            var value = size + reference.Delta;
                            memory[reference.Offset + 0] = (byte)(value & 0xff);
                            memory[reference.Offset + 1] = (byte)(value >> 8);
                            if (log) _logger($" - Wrote ${value:X} for size {size:X} + {reference.Delta} at reference at {reference.Offset:X}");
                            break;
                        }
                        case Game.Reference.Types.Size8:
                        {
                            var value = size + reference.Delta;
                            if (value > 255)
                            {
                                throw new Exception($"Cannot write size {value} because it exceeds 8 bits");
                            }
                            memory[reference.Offset] = (byte)(value & 0xff);
                            if (log) _logger($" - Wrote ${value:X} for size {size:X} + {reference.Delta} at reference at {reference.Offset:X}");
                            break;
                        }
                        case Game.Reference.Types.Slot1:
                        {
                            var value = (uint)(offset % 0x4000 + 0x4000 + reference.Delta);
                            memory[reference.Offset + 0] = (byte)(value & 0xff);
                            memory[reference.Offset + 1] = (byte)(value >> 8);
                            if (log) _logger($" - Wrote location ${value:X} for offset {offset:X} + {reference.Delta} at reference at {reference.Offset:X}");
                            break;
                        }
                    }
                }
            }

            // If there are any followers, write them now
            if (followers.TryGetValue(item.Name, out var follower))
            {
                // We modify its restrictions to require it to be exactly after this one
                follower.Asset.Restrictions.MinimumOffset = item.DataItem.Offset + item.Data.Count;
                follower.Asset.Restrictions.MaximumOffset = follower.Asset.Restrictions.MinimumOffset + follower.Data.Count;
                WriteAsset(follower, writtenItems, writtenReferences, freeSpace, memory, followers, log);
            }
        }

        private static int GetSizeNeeded(AssetToPack item, Dictionary<string, AssetToPack> followers)
        {
            var size = item.Data.Count;
            if (followers.TryGetValue(item.Name, out var follower))
            {
                // Recurse into followers
                return size + GetSizeNeeded(follower, followers);
            }

            return size;
        }

        public void ChangeTileSet(ArtItem item, TileSet value)
        {
            // We want to replace the object only in the context of this art item.
            item.TileSet = value;
            // The tricky part is this lookup...
            var asset = item.Assets.Find(x => x.Type == Game.Asset.Types.TileSet);
            if (asset != null)
            {
                _assetsLookup[asset] = value;
            }
        }
    }
}
