using System.Collections.Generic;
using sth1edwv.GameObjects;

namespace sth1edwv
{
    public class ArtItem
    {
        public TileSet TileSet { get; set; }
        public Palette Palette { get; set; }
        public bool PaletteEditable { get; set; }
        public string Name { get; init; }
        public TileMap TileMap { get; set; }
        public List<TileSet> SpriteTileSets { get; } = [];
        public List<TileMapData> TileMapData { get; } = [];
        public List<Cartridge.Game.Asset> Assets { get; } = [];
        public List<RawValue> RawValues { get; } = [];

        public override string ToString()
        {
            return Name;
        }
    }
}