using System;
using System.Collections;
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
            return Encoding.UTF8.GetString(_data, index, length);
        }

        public string NullTerminatedString(int index)
        {
            var endIndex = Array.IndexOf(_data, (byte)0, index);
            if (endIndex == -1)
            {
                throw new Exception($"No terminator found for string at {index:X}");
            }
            return String(index, endIndex - index);
        }

        public static int FromBcd(int bcd)
        {
            var result = 0;
            var multiplier = 1;
            while (bcd > 0)
            {
                result += (bcd & 0xf) * multiplier;
                bcd >>= 4;
                multiplier *= 10;
            }
            return result;
        }

        public static int ToBcd(int value)
        {
            var result = 0;
            var multiplier = 1;
            while (value != 0)
            {
                // Get digit from value and add in the right place
                result += (value % 10) * multiplier;
                value /= 10;
                multiplier *= 16;
            }

            return result;
        }
    }
}