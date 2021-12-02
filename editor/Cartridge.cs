﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
                public int Offset { get; set; }
                public enum Types
                {
                    Absolute,
                    Slot1,
                    PageNumber,
                    Size,
                    Size8
                }
                public Types Type { get; set; }
                public int Delta { get; set; }
                public override string ToString()
                {
                    return $"{Type}@{Offset:X} ({Delta:X})";
                }
            }

            public class LocationRestriction
            {
                public int MinimumOffset { get; set; } = 0;
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

                public enum Types { TileSet, Palette, TileMap, SpriteTileSet, ForegroundTileMap, Unused, Misc }
                public Types Type { get; set; }
                public List<Reference> References { get; set; }
                public LocationRestriction Restrictions { get; } = new(); // Default to defaults...
                public int FixedSize { get; set; }
                public int BitPlanes { get; set; }
                public List<Point> TileGrouping { get; set; }
                public int TilesPerRow { get; set; } = 16; // 16 is often the best default
                public bool Hidden { get; set; }
                // These are in the original ROM, not where we loaded from
                public int OriginalOffset { get; set; }
                public int OriginalSize { get; set; }

                public int GetOffset(Memory memory)
                {
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

            public Dictionary<string, Asset> Assets { get; set; }

            public Dictionary<string, IEnumerable<string>> AssetGroups { get; set; }
        }

        private static readonly Game Sonic1MasterSystem = new()
        {
            // These are in ROM order to help me keep track
            Assets = new Dictionary<string, Game.Asset> {
                {
                    // Palettes scattered in the low ROM area
                    "Map screen 1 palette", new Game.Asset { 
                        OriginalOffset = 0x0f0e,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0cd4 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$0f0e ; 000CD4 21 0E 0F 
                        }
                    }
                }, {
                    "Map screen 2 palette", new Game.Asset { 
                        OriginalOffset = 0xf2e,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x0d36 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$0f2e ; 000D36 21 2E 0F 
                        }
                    }
                }, {
                    "Title screen palette", new Game.Asset { 
                        OriginalOffset = 0x13e1,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x12cc + 1, Type = Game.Reference.Types.Absolute} // ld hl,$13e1 ; 0012CC 21 E1 13 
                        }
                    }
                }, {
                    "Game Over palette", new Game.Asset { 
                        OriginalOffset = 0x14fc,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x143c + 1, Type = Game.Reference.Types.Absolute} // ld hl,$14fc ; 00143C 21 FC 14 
                        }
                    }
                }, {
                    "Act Complete palette", new Game.Asset { 
                        OriginalOffset = 0x1b8d,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32, 
                        References = new List<Game.Reference> {
                            new() {Offset = 0x1604 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$1b8d ; 001604 21 8D 1B 
                        }
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
                        }
                    }
                }, {
                    "Credits palette", new Game.Asset {
                        OriginalOffset = 0x2ad6,
                        OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x2702 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$2ad6 ; 002702 21 D6 2A 
                        }
                    }
                }, {
                    "End sign palette", new Game.Asset {
                        OriginalOffset = 0x626c,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 16, // Sprite palette only
                        References = new List<Game.Reference> {
                            new() {Offset = 0x5F38 + 1, Type = Game.Reference.Types.Absolute} // ld hl,$626c ; 005F38 21 6C 62
                        }
                    }
                }, {
                    // TODO level palettes here
                    // This one marks it as free space for placement...
                    "Level palettes area", new Game.Asset {
                        Type = Game.Asset.Types.Palette,
                        OriginalOffset = 0x629e,
                        OriginalSize = 0x65ee-0x629e

                    }
                }, {
                    "Green Hill palette", new Game.Asset {// We add this just so we can use it below
                        // We omit the offset stuff to make this not get overwritten as a regular asset
                        // TODO make level assets join these and remove this?
                        // OriginalOffset = 0x629e,
                        // OriginalSize = 32,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 32,
                        References = new List<Game.Reference> {
                            new() { Offset = 0x627C, Type = Game.Reference.Types.Absolute}
                        }
                    }
                }, /*{
                    "Green Hill cycling palette", new Game.Asset {
                        Type = Game.Asset.Types.Palette,
                        // OriginalOffset = 0x62be,
                        // OriginalSize = 48,
                        References = new List<Game.Reference> {
                            new() { Offset = 0x628c, Type = Game.Reference.Types.Absolute},
                            new() { Offset = 0x155CA + 28, Type = Game.Reference.Types.Size8} // Cycle count
                        }
                    }
                },*/ {
                    "Boss sprites palette", new Game.Asset {
                        OriginalOffset = 0x731c,
                        OriginalSize = 16,
                        Type = Game.Asset.Types.Palette,
                        FixedSize = 16,
                        References = new List<Game.Reference> {
                            new() {Offset = 0x703C + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00703C 21 1C 73
                            new() {Offset = 0x807F + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00807F 21 1C 73
                            new() {Offset = 0x84C7 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 0084C7 21 1C 73
                            new() {Offset = 0x929C + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00929C 21 1C 73
                            new() {Offset = 0xA821 + 1, Type = Game.Reference.Types.Absolute}, // ld hl,$731c ; 00A821 21 1C 73
                            new() {Offset = 0xBE07 + 1, Type = Game.Reference.Types.Absolute}  // ld hl,$731c ; 00BE07 21 1C 73
                        }
                    }
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
                        FixedSize = 42 * 24 * 32 * 3/8, // 42 frames, each 24x32, each pixel is 3 bits
                        TilesPerRow = 8,
                        References = new List<Game.Reference> {
                            new() { Offset = 0x4c84 + 1, Type = Game.Reference.Types.Slot1 }, // ld bc,$4000 ; 004C84 01 00 40 
                            new() { Offset = 0x012d + 1, Type = Game.Reference.Types.PageNumber }, // ld a,$08 ; 00012D 3E 08 
                            new() { Offset = 0x0135 + 1, Type = Game.Reference.Types.PageNumber, Delta = 1 }, // ld a,$09 ; 000135 3E 09 
                            // We put this one at the end so it won't be used for reading but will be written.
                            // This allows us to make sure the left-facing art pointer is in the right place while removing the padding.
                            new() { Offset = 0x4c8d + 1, Type = Game.Reference.Types.Slot1, Delta = 42 * 24 * 32 * 3/8}, // ld bc,$7000 ; 004C8D 01 00 70
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
                        FixedSize = 42 * 24 * 32 * 3/8, // 42 frames, each 24x32, each pixel is 3 bits
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
                            new() {Offset = 0x12a7 + 1, Type = Game.Reference.Types.PageNumber}              // ld a,$09    ; 0012A7 3E 09 
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
                            new() {Offset = 0x0C9F + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 000C9F 21 2E B9 
                            new() {Offset = 0x0CA5 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 000CA5 3E 09 
                            new() {Offset = 0x0D01 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 000D01 21 2E B9 
                            new() {Offset = 0x0D07 + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 000D07 3E 09 
                            new() {Offset = 0x1575 + 1, Type = Game.Reference.Types.Slot1, Delta = -0x4000}, // ld hl,$b92e ; 001575 21 2E B9 
                            new() {Offset = 0x157B + 1, Type = Game.Reference.Types.PageNumber},             // ld a,$09    ; 00157B 3E 09 
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
                        OriginalSize = 0x3e9fd - 0x3ef3f,
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
                { "Map screen 1", new [] { "Map screen 1 tileset", "Map screen 1 tilemap 1", "Map screen 1 tilemap 2", "Map screen 1 palette", "Map screen 1 sprite tiles", "HUD sprite tiles" } }, // HUD sprites only used for life counter
                { "Map screen 2", new [] { "Map screen 2 tileset", "Map screen 2 tilemap 1", "Map screen 2 tilemap 2", "Map screen 2 palette", "Map screen 2 sprite tiles", "HUD sprite tiles" } },
                { "Sonic", new[] { "Sonic (right)", "Sonic (left)", "HUD sprite tiles", "Green Hill palette" } }, // HUD sprites contain the spring jump toes, the ones in the art seem unused...
                { "Monitors", new [] { "Monitor Art", "HUD sprite tiles", "Green Hill palette"  } }, // Monitor bases are in the HUD sprites
                { "Title screen", new [] { "Title screen tiles", "Title screen sprites", "Title screen palette", "Title screen tilemap" } },
                { "Game Over", new [] { "Act Complete tiles", "Game Over palette", "Game Over tilemap" } },
                { "Act Complete", new [] { "Act Complete tiles", "Act Complete palette", "Act Complete tilemap", "HUD sprite tiles" } },
                { "Special Stage Complete", new [] { "Act Complete tiles", "Act Complete palette", "Special Stage Complete tilemap", "HUD sprite tiles" } },
                { "Ending 1", new [] { "Map screen 1 tileset", "Ending palette", "Ending 1 tilemap" } },
                { "Ending 2", new [] { "Map screen 1 tileset", "Ending palette", "Ending 2 tilemap" } },
                { "Credits", new [] { "Map screen 2 tileset", "Title screen sprites", "Credits tilemap", "Credits palette" } },
                { "End sign", new [] { "End sign tileset", "End sign palette" } },
                { "Rings", new [] { "Rings", "Green Hill palette" } },
                { "Dr. Robotnik", new [] { "Boss sprites 1", "Boss sprites 2", "Boss sprites 3", "Boss sprites palette" } },
                { "Capsule", new [] { "Capsule sprites", "Boss sprites palette" } }
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
                new() { Name = "Scrap Brain Act 3", Offset = 0x15580 + 0x2bf },
                new() { Name = "Sky Base Act 1", Offset = 0x15580 + 0x378 },
                new() { Name = "Sky Base Act 2", Offset = 0x15580 + 0x39d },
                new() { Name = "Sky Base Act 3", Offset = 0x15580 + 0x3c2 },
                new() { Name = "Ending Sequence", Offset = 0x15580 + 0xb9 },
                new() { Name = "Scrap Brain Act 2 (Emerald Maze), from corridor", Offset = 0x15580 + 0x275 },
                new() { Name = "Scrap Brain Act 2 (Ball Hog Area)", Offset = 0x15580 + 0x29a },
                new() { Name = "Scrap Brain Act 2, from transporter", Offset = 0x15580 + 0x32e },
                new() { Name = "Scrap Brain Act 2, from Emerald Maze", Offset = 0x15580 + 0x2e4 },
                new() { Name = "Scrap Brain Act 2, from Ball Hog Area", Offset = 0x15580 + 0x309 },
                new() { Name = "Sky Base Act 2 (Interior)", Offset = 0x15580 + 0x3e7 },
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
        public List<GameText> GameText { get; } = new();
        public List<ArtItem> Art { get; } = new();

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
            ReadLevels();
            ReadGameText();
            ReadAssets();

            // Apply rings to level tile sets
            var rings = Art.Find(x => x.Name == "Rings").TileSet;
            foreach (var tileSet in Levels.Select(x => x.TileSet).Distinct())
            {
                tileSet.SetRings(rings.Tiles[0]);
            }

            _logger($"Load complete in {sw.Elapsed}");
        }

        private void ReadAssets()
        {
            _assetsLookup.Clear();

            _logger("Loading art...");

            foreach (var kvp in Sonic1MasterSystem.AssetGroups)
            {
                var item = new ArtItem{Name = kvp.Key};
                foreach (var part in kvp.Value.Select(x => new { Name = x, Asset = Sonic1MasterSystem.Assets[x]}))
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
                            break;
                        case Game.Asset.Types.ForegroundTileMap:
                            // We assume these are set first
                            _assetsLookup[asset] = item.TileMap = new TileMap(Memory, offset, asset.GetLength(Memory));
                            item.TileMap.SetAllForeground();
                            break;
                        case Game.Asset.Types.TileMap:
                        {
                            // We assume these are set second so we have to check if it's a set or overlay
                            var tileMap = new TileMap(Memory, offset, asset.GetLength(Memory));
                            if (tileMap.IsOverlay())
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
                        case Game.Asset.Types.SpriteTileSet:
                            var tileSet = asset.BitPlanes > 0 
                                ? GetTileSet(offset, asset.GetLength(Memory), asset.BitPlanes, asset.TileGrouping, asset.TilesPerRow) 
                                : GetTileSet(offset, asset.TileGrouping, asset.TilesPerRow);
                            _assetsLookup[asset] = tileSet;
                            item.SpriteTileSets.Add(tileSet);
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
            DisposeAll(_blockMappings);
            DisposeAll(_tileSets);
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

        private void ReadGameText()
        {
            _logger("Loading game text...");
            GameText.Clear();
            for (var i = 0; i < 6; i++)
            {
                GameText.Add(new GameText(this, 0x122D + i * 0xF, i < 3));
            }
            for (var i = 0; i < 3; i++)
            {
                GameText.Add(new GameText(this, 0x197E + i * 0x10, true));
            }
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

        public byte[] MakeRom()
        {
            _logger("Building ROM...");
            var sw = Stopwatch.StartNew();
            // We clone the memory to a memory stream
            var memory = new byte[512 * 1024];
            Memory.GetStream(0, Memory.Count).ToArray().CopyTo(memory, 0);

            // Data we read/write and what we can repack...
            // Start    End     Description                         Repacking?
            // -----------------------------------------------------------------------------
            // 0627c    0679d   Level palette lookups
            // 0629e    065ed   Level palettes                      In-place, not in assets list
            // 15580    155c9   Level header pointers               Untouched
            // 155ca    15ab3   Level headers                       In-place
            // 15ab4    15fff   Object layouts (+ unused)           In-place TODO
            // 16dea    1ffff   Floors (+unused)                    In-place TODO relocate? Defaults to 14000..23fff range
            // 3e9fd            Solidity data start?                Not planning on moving this...

            // We work through the data types...

            // We start from the asset groups here so we don't pick up any unused or blank parts
            var assetsToPack = new HashSet<AssetToPack>(
                Sonic1MasterSystem.AssetGroups.Values
                    .SelectMany(x => x) // Flatten all the groups
                    .Distinct() // Remove duplicates
                    .Select(x => new AssetToPack(x, Sonic1MasterSystem.Assets[x], _assetsLookup[Sonic1MasterSystem.Assets[x]], _assetsLookup[Sonic1MasterSystem.Assets[x]].GetData())) // Select the asset name, details and serialized data
                    .Where(x => x.Asset.OriginalOffset != 0)); // Exclude any not yet configured with a source location

            // Add in the level assets... we want to de-dupe these
            // Palettes (excluding Sky Base lightning)
            // TODO do I want to add these to the AssetGroups instead?
            foreach (var group in Levels.GroupBy(x => x.Palette))
            {
                assetsToPack.Add(new AssetToPack($"{group.First()} palette", new Game.Asset
                {
                    Type = Game.Asset.Types.Palette,
                    OriginalOffset = group.Key.Offset,
                    OriginalSize = 32,
                    Restrictions = { MaximumOffset = 0x7fff },
                    References = new List<Game.Reference>{new(){Type = Game.Reference.Types.Absolute, Offset = 0}} // Tricks it into using original offsets
                }, group.Key, group.Key.GetData()));
            }
            foreach (var group in Levels.GroupBy(x => x.CyclingPalette))
            {
                var data = @group.Key.GetData();
                assetsToPack.Add(new AssetToPack($"{group.First()} cycling palette", new Game.Asset
                {
                    Type = Game.Asset.Types.Palette,
                    OriginalOffset = group.Key.Offset,
                    OriginalSize = data.Count,
                    Restrictions = { MaximumOffset = 0x7fff },
                    References = new List<Game.Reference>{new(){Type = Game.Reference.Types.Absolute, Offset = 0}} // Tricks it into using original offsets
                }, group.Key, data));
            }

            // First we build a list of "free space". We include all the "original assets" so we will overwrite unused space. Missing "original" data makes us ignore it.
            var freeSpace = new FreeSpace();
            foreach (var asset in Sonic1MasterSystem.Assets.Values.Where(x => x.OriginalOffset != 0))
            {
                freeSpace.Add(asset.OriginalOffset, asset.OriginalOffset + asset.OriginalSize);
            }
            
            // Expand to 512KB here
            freeSpace.Add(256*1024, 512*1024);

            // We do this after adding all the free space spans
            freeSpace.Consolidate();

            // Then log the state
            _logger(freeSpace.ToString());

            // - Game text (at original offsets)
            // 122d..1286 inclusive
            // 197e..19ad inclusive
            foreach (var gameText in GameText)
            {
                var data = gameText.GetData();
                data.CopyTo(memory, gameText.Offset);
                _logger($"- Wrote game text \"{gameText.Text}\" at offset ${gameText.Offset:X}, length {data.Count} bytes");
            }

            /*
            // - Level palettes (at original offsets)
            // 629e..65ed inclusive, with Sky Base lightning not covered
            foreach (var palette in Levels.SelectMany(x => new[]{x.Palette, x.CyclingPalette}).Distinct())
            {
                var data = palette.GetData();
                data.CopyTo(memory, palette.Offset);
                _logger($"- Wrote palette at offset ${palette.Offset:X}, length {data.Count} bytes");
            }
            */

            // - Floors
            // Must be in the range 14000..23fff
            foreach (var group in Levels.GroupBy(l => l.Floor))
            {
                var floor = group.Key;
                assetsToPack.Add(new AssetToPack(
                    $"Floor data for {string.Join(", ", group)}",
                    new Game.Asset
                    {
                        Type = Game.Asset.Types.Misc,
                        Restrictions = { CanCrossBanks = true, MinimumOffset = 0x14000, MaximumOffset = 0x24000 }
                    },
                    floor,
                    floor.GetData()));
            }

            // - Sprite tile sets
            // Game engine expects data in the range 24000..33fff
            foreach (var group in Levels.GroupBy(l => l.SpriteTileSet))
            {
                var tileSet = group.Key;
                assetsToPack.Add(new AssetToPack(
                    $"Sprite tiles for {string.Join(", ", group)}",
                    new Game.Asset
                    {
                        Type = Game.Asset.Types.SpriteTileSet,
                        Restrictions = { CanCrossBanks = true, MinimumOffset = 0x24000, MaximumOffset = 0x34000 }
                    },
                    tileSet,
                    tileSet.GetData()));
            }

            // - Lavel background art
            // Game engine expects data in the range 30000..3ffff
            foreach (var group in Levels.GroupBy(l => l.TileSet))
            {
                var tileSet = group.Key;
                assetsToPack.Add(new AssetToPack(
                    $"Background tiles for {string.Join(", ", group)}",
                    new Game.Asset
                    {
                        Type = Game.Asset.Types.TileSet,
                        Restrictions = { CanCrossBanks = true, MinimumOffset = 0x30000, MaximumOffset = 0x40000 }
                    },
                    tileSet,
                    tileSet.GetData()));
            }

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

                _logger($"- Wrote block mapping for level(s) {string.Join(", ", group)} at offset ${group.Key.Blocks[0].Offset:X}");
            }

            // - Level objects
            foreach (var group in Levels.GroupBy(l => l.Objects))
            {
                var levelObjects = group.Key;
                assetsToPack.Add(new AssetToPack(
                    $"Objects data for {string.Join(", ", group)}",
                    new Game.Asset
                    {
                        Type = Game.Asset.Types.SpriteTileSet,
                        Restrictions = { MinimumOffset = 0x15580, MaximumOffset = 0x16000 } // Object lists are in the same bank as the headers
                    },
                    levelObjects,
                    levelObjects.GetData()));
            }

            // We avoid writing the same item twice by different routes...
            var writtenItems = new HashSet<IDataItem>();
            var writtenReferences = new HashSet<int>();

            // First we do the ones with absolute positions. There's generally nothing to gain by relocating them, so we just copy the data.
            foreach (var item in assetsToPack
                         .Where(x => x.Asset.References != null && x.Asset.References.Any(r => r.Type == Game.Reference.Types.Absolute))
                         .ToList())
            {
                var offset = item.Asset.OriginalOffset;
                item.Data.CopyTo(memory, offset);
                _logger($"- Wrote data for asset {item.Name} at its original location {offset:X}, length {item.Data.Count} bytes");
                freeSpace.Remove(offset, item.Data.Count);
                writtenItems.Add(item.DataItem);
                assetsToPack.Remove(item);
            }

            // Then the ones that need to be packed... with constrained ones first
            try
            {
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
                        .OrderBy(x => freeSpace.GetEaseOfPlacing(x.Data.Count, x.Asset.Restrictions.MinimumOffset, x.Asset.Restrictions.MaximumOffset))
                        .ThenByDescending(x => x.Data.Count)
                        .First();
                    WriteAsset(item, writtenItems, writtenReferences, freeSpace, memory, followers);
                    assetsToPack.Remove(item);
                }
            }
            catch (Exception ex)
            {
                _logger(ex.Message);
                _logger(freeSpace.ToString());
            }

            // - Level headers (at original offsets). We do these last so they pick up info from the contained objects.
            foreach (var level in Levels)
            {
                level.GetData().CopyTo(memory, level.Offset);
                _logger($"- Wrote level header for {level} at offset ${level.Offset:X}");
            }

            _logger($"Built ROM image in {sw.Elapsed}");

            return memory;
        }

        private void WriteAsset(AssetToPack item, ISet<IDataItem> writtenItems, HashSet<int> writtenReferences, FreeSpace freeSpace, byte[] memory, Dictionary<string, AssetToPack> followers)
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
                _logger($"- Data for asset {item.Name} was already written");
                offset = item.DataItem.Offset;
            }
            else
            {
                offset = freeSpace.FindSpace(sizeNeeded, item.Asset.Restrictions);
                item.DataItem.Offset = offset;
                item.Data.CopyTo(memory, offset);
                _logger($"- Wrote data for asset {item.Name} at {offset:X}, length {item.Data.Count} bytes");
                freeSpace.Remove(offset, item.Data.Count);
                _logger(freeSpace.ToString());

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
                        _logger($" - Reference at {reference.Offset:X} was already written");
                        continue;
                    }

                    writtenReferences.Add(reference.Offset);
                    switch (reference.Type)
                    {
                        case Game.Reference.Types.PageNumber:
                            var pageNumber = (byte)(offset / 0x4000 + reference.Delta);
                            memory[reference.Offset] = pageNumber;
                            _logger(
                                $" - Wrote page number ${pageNumber:X} for offset {offset:X} at reference at {reference.Offset:X}");
                            break;
                        case Game.Reference.Types.Size:
                            memory[reference.Offset + 0] = (byte)(size & 0xff);
                            memory[reference.Offset + 1] = (byte)(size >> 8);
                            _logger($" - Wrote size ${size:X} at reference at {reference.Offset:X}");
                            break;
                        case Game.Reference.Types.Size8:
                            if (size > 255)
                            {
                                throw new Exception($"Cannot write size {size} because it exceeds 8 bits");
                            }
                            memory[reference.Offset] = (byte)(size & 0xff);
                            _logger($" - Wrote size ${size:X} at reference at {reference.Offset:X}");
                            break;
                        case Game.Reference.Types.Slot1:
                            var value = (uint)(offset % 0x4000 + 0x4000 + reference.Delta);
                            memory[reference.Offset + 0] = (byte)(value & 0xff);
                            memory[reference.Offset + 1] = (byte)(value >> 8);
                            _logger(
                                $" - Wrote location ${value:X} for offset {offset:X} at reference at {reference.Offset:X}");
                            break;
                    }
                }
            }

            // If there are any followers, write them now
            if (followers.TryGetValue(item.Name, out var follower))
            {
                // We modify its restrictions to require it to be exactly after this one
                follower.Asset.Restrictions.MinimumOffset = item.DataItem.Offset + item.Data.Count;
                follower.Asset.Restrictions.MaximumOffset = follower.Asset.Restrictions.MinimumOffset + follower.Data.Count;
                WriteAsset(follower, writtenItems, writtenReferences, freeSpace, memory, followers);
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

        public class Space
        {
            public int Total { get; set; }
            public int Used { get; set; }
        }

        // All the numbers in these need to match what's in MakeRom
        public Space GetFloorSpace() =>
            new()
            {
                Total = 0x20000 - 0x16dea,
                Used = Levels.Select(x => x.Floor).Distinct().Sum(x => x.GetData().Count)
            };
        public Space GetFloorTileSetSpace() =>
            new()
            {
                Total = 0x3da28 - 0x32FE6,
                Used = Levels.Select(x => x.TileSet).Distinct().Sum(x => x.GetData().Count)
            };
        public Space GetSpriteTileSetSpace() =>
            new()
            {
                Total = 0x2EEB1 - 0x2a12a,
                Used = Levels.Select(x => x.SpriteTileSet)
                    .Distinct()
                    .Except(Art.SelectMany(x => x.SpriteTileSets))
                    .Sum(x => x.GetData().Count)
            };

        public void ChangeTileSet(ArtItem item, TileSet value)
        {
            // We want to replace the object only in the context of this art item.
            item.TileSet = value;
            // The tricky part is this lookup...
            var asset = item.Assets.Find(x => x.Type == Game.Asset.Types.TileSet);
            _assetsLookup[asset] = value;
        }
    }
}
