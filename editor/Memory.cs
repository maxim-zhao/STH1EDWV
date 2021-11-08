﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace sth1edwv
{
    /// <summary>
    /// Wraps a byte[], making it read only and offering reading bytes, words, strings and converting to a stream.
    /// </summary>
    public class Memory: IReadOnlyList<byte>
    {
        private readonly byte[] _data;

        public Memory(byte[] data)
        {
            _data = data;
        }

        public IEnumerator<byte> GetEnumerator() => (IEnumerator<byte>)_data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _data.Length;
        public MemoryStream GetStream(int index, int count) => new(_data, index, count, false);

        public byte this[int index] => _data[index];

        public ushort Word(int index) => (ushort)(_data[index] | (_data[index + 1] << 8));

        public string String(int index, int length)
        {
            return Encoding.ASCII.GetString(_data, index, length);
        }
    }
}