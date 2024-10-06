using System;
using System.Collections.Generic;

namespace sth1edwv.GameObjects
{
    public class RawValue : IDataItem
    {
        public int Size { get; }
        public string Name { get; }
        public int Value { get; set; }
        public Encodings Encoding { get; }
        public enum Encodings { Byte, Bcd }

        public RawValue(Memory memory, int offset, int size, Encodings encoding, string name)
        {
            Encoding = encoding;
            Size = size;
            Name = name;
            Offset = offset;
            Value = size switch
            {
                1 => memory[offset],
                2 => memory.Word(offset),
                _ => throw new Exception($"Unsupported raw value size {size}")
            };
        }

        public int Offset { get; set; }
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