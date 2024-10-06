using System;
using System.Collections.Generic;
using System.Linq;

namespace sth1edwv.GameObjects
{
    public class BlockMapping: IDisposable
    {
        public List<Block> Blocks { get; } = [];
    
        public BlockMapping(Cartridge cartridge, int address, int blockCount, int solidityIndex, TileSet tileSet)
        {
            var solidityOffset = cartridge.Memory.Word(0x3A65 + solidityIndex * 2);
            for (var i = 0; i < blockCount; ++i)
            {
                Blocks.Add(new Block(cartridge.Memory, address + i * 16, solidityOffset + i, tileSet, i));
            }
        }

        public void Dispose()
        {
            foreach (var block in Blocks)
            {
                block.Dispose();
            }
            Blocks.Clear();
        }

        public void UpdateUsageForLevel(Level level)
        {
            foreach (var block in Blocks)
            {
                block.UsageCount = 0;
            }

            foreach (var index in level.Floor.BlockIndices.Where(x => x < Blocks.Count))
            {
                ++Blocks[index].UsageCount;
            }
        }

        public void UpdateGlobalUsage(IEnumerable<Level> levels)
        {
            foreach (var block in Blocks)
            {
                block.GlobalUsageCount = 0;
            }

            // We do not consider level bounds?
            foreach (var index in levels
                .Where(l => l.BlockMapping == this)
                .Select(l => l.Floor)
                .Distinct()
                .SelectMany(f => f.BlockIndices)
                .Where(x => x < Blocks.Count))
            {
                ++Blocks[index].GlobalUsageCount;
            }
        }

        public override string ToString()
        {
            return $"{Blocks.Count} blocks @ {Blocks[0].Offset:X}";
        }
    }
}
