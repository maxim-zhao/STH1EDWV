using System;
using System.Collections.Generic;

namespace sth1edwv.GameObjects
{
    public class RawValue(Memory memory, int offset, int size, RawValue.Encodings encoding, string name)
        : IDataItem
    {
        public int Size { get; } = size;
        public string Name { get; } = name;
        public int Value { get; set; } = size switch
        {
            1 => memory[offset],
            2 => memory.Word(offset),
            _ => throw new Exception($"Unsupported raw value size {size}")
        };

        public Encodings Encoding { get; } = encoding;

        public enum Encodings { Byte, Bcd }

        public int Offset { get; set; } = offset;

        public IList<byte> GetData()
        {
            return Size switch
            {
                1 => [(byte)Value],
                2 => new[] { (byte)Value, (byte)(Value >> 8) },
                _ => throw new Exception($"Unsupported raw value size {Size}")
            };
        }
    }
}