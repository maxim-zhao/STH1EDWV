using System.Collections.Generic;
using System.Linq;

namespace sth1edwv.GameObjects
{
    /// <summary>
    /// Represents some raw tilemap data in the form x, y, data, $ff.
    /// This is used for things like the level names, PRESS BUTTON text, etc
    /// </summary>
    public class TileMapData : IDataItem
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public List<byte> Values { get; } = new();

        public TileMapData(Memory memory, int offset, string name)
        {
            Offset = offset;
            Name = name;
            // First two bytes are X, Y
            X = memory[offset++];
            Y = memory[offset++];
            // Following bytes are the data, terminated with a $ff
            for (;;)
            {
                var value = memory[offset++];
                if (value == 0xff)
                {
                    break;
                }
                Values.Add(value);
            }
        }

        public int Offset { get; set; }
        public string Name { get; }
        
        public IList<byte> GetData()
        {
            return new[] { X, Y }.Concat(Values).Append((byte)0xff).ToList();
        }
    }
}