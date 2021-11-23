﻿using System.Collections.Generic;
using sth1edwv.GameObjects;

namespace sth1edwv
{
    public class ArtItem
    {
        public TileSet TileSet { get; set; }
        public Palette Palette { get; set; }
        public bool PaletteEditable { get; set; }
        public string Name { get; set; }
        public bool IsSprites { get; set; }
        public TileMap TileMap { get; set; }
        public List<TileSet> SpriteTileSets { get; } = new();

        public override string ToString()
        {
            return Name;
        }
    }
}