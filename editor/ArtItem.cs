using System.Collections.Generic;
using sth1edwv.GameObjects;

namespace sth1edwv
{
    /// <summary>
    /// Represents one non-level "thing", which may have multiple types of data.
    /// </summary>
    public class ArtItem
    {
        public TileSet TileSet { get; set; }
        public Dictionary<string, Palette> Palettes { get; } = [];
        public bool PaletteEditable { get; set; }
        public string Name { get; init; }
        public TileMap TileMap { get; set; }
        public Dictionary<string, TileSet> SpriteTileSets { get; } = [];
        public List<TileMapData> TileMapData { get; } = [];
        public List<Cartridge.Game.Asset> Assets { get; } = [];
        public List<RawValue> RawValues { get; } = [];

        public override string ToString()
        {
            return Name;
        }
    }
}