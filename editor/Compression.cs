using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace sth1edwv
{
    public static class Compression
    {
        public static byte[] DecompressRle(Memory memory, int address, int size)
        {
            // Decompress into a new stream
            using var m = memory.GetStream(address, size);
            using var result = new MemoryStream();
            while (m.Position < m.Length)
            {
                var b1 = (byte)m.ReadByte();
                result.WriteByte(b1);
                if (m.Position == m.Length)
                {
                    // Done
                    break;
                }
                var b2 = (byte)m.ReadByte();
                if (b1 != b2)
                {
                    // Not RLE, rewind and loop
                    m.Seek(-1, SeekOrigin.Current);
                    continue;
                }

                // Duplicate indicates RLE
                // Next byte is a count
                var count = m.ReadByte();
                // 0 means 256
                if (count == 0)
                {
                    count = 256;
                }
                // Emit that many
                for (var i = 0; i < count; ++i)
                {
                    result.WriteByte(b1);
                }
            }
            return result.ToArray();
        }

        public static byte[] CompressRle(byte[] data)
        {
            using var m = new MemoryStream(data);
            using var result = new MemoryStream();
            while (m.Position < m.Length)
            {
                // Look for a run of bytes
                var b = m.ReadByte();
                var runLength = 1;
                for (; runLength < 257; ++runLength)
                {
                    var next = m.ReadByte();
                    if (next == -1)
                    {
                        // End of stream
                        break;
                    }

                    if (next != b)
                    {
                        // Different byte, put it back and stop counting
                        m.Seek(-1, SeekOrigin.Current);
                        break;
                    }
                }
                // And emit
                result.WriteByte((byte)b);
                if (runLength > 1)
                {
                    result.WriteByte((byte)b);
                    --runLength; // stored as n-1
                    result.WriteByte((byte)runLength);
                }
            }
            return result.ToArray();
        }

        public static byte[] DecompressArt(Memory data, int offset)
        {
            var magic = data.String(offset, 2);
            if (magic != "HY")
            {
                throw new Exception("Invalid magic number");
            }
            var duplicateRowsOffset = offset + data.Word(offset + 2);
            var artDataOffset = offset + data.Word(offset + 4);
            var rowCount = data.Word(offset + 6);
            if (rowCount % 8 != 0)
            {
                throw new Exception($"Row count {rowCount} is not a multiple of 8");
            }

            var nextArtDataOffset = artDataOffset;
            // We work through the data based on these...
            using var bitMasks = data.GetStream(offset + 8, rowCount / 8);
            using var duplicatesData = data.GetStream(duplicateRowsOffset, rowCount*2); // Real size is <= rowCount * 2 but hard to predict
            using var result = new MemoryStream();
            while (bitMasks.Position != bitMasks.Length)
            {
                var bitmask = bitMasks.ReadByte();
                for (var i = 0; i < 8; ++i)
                {
                    var bit = bitmask & 1;
                    bitmask >>= 1;
                    byte[] row;
                    if (bit == 1)
                    {
                        // Duplicate
                        var duplicateIndex = duplicatesData.ReadByte();
                        if ((duplicateIndex & 0xf0) == 0xf0)
                        {
                            // Higher indices are two bytes
                            duplicateIndex &= 0xf;
                            duplicateIndex <<= 8;
                            duplicateIndex |= duplicatesData.ReadByte();
                        }

                        row = PlanarToChunky(data, artDataOffset + duplicateIndex * 4, 1).ToArray();
                    }
                    else
                    {
                        row = PlanarToChunky(data, nextArtDataOffset, 1).ToArray();
                        nextArtDataOffset += 4;
                    }
                    result.Write(row, 0, row.Length);
                }
            }

            // We assume that the data is contiguous from the header to the end of whichever of the art data and duplicates data comes last.

            return result.ToArray();
        }

        private static IEnumerable<byte> PlanarToChunky(IReadOnlyList<byte> data, int offset, int rowCount, int bytes = 4)
        {
            for (var row = 0; row < rowCount; ++row)
            {
                // Each pixel is from four bitplanes
                for (var i = 0; i < 8; ++i)
                {
                    var value = 0;
                    for (var b = 0; b < bytes; ++b)
                    {
                        var bit = (data[offset + row * bytes + b] >> (7 - i)) & 1;
                        value |= bit << b;
                    }

                    yield return (byte)value;
                }
            }
        }

        extension(IEnumerable<byte> input)
        {
            public IEnumerable<byte[]> ToChunks(int bytesPerChunk)
            {
                var buffer = new byte[bytesPerChunk];
                var index = 0;
                foreach (var b in input)
                {
                    buffer[index++] = b;
                    if (index == bytesPerChunk)
                    {
                        yield return buffer;
                        buffer = new byte[bytesPerChunk];
                        index = 0;
                    }
                }
            }

            public IEnumerable<byte> PlanarToChunky(int bitPlanes = 4)
            {
                using var enumerator = input.GetEnumerator();
                var data = new byte[bitPlanes];
                for (;;)
                {
                    // Get planar bytes for 8 pixels
                    for (var i = 0; i < bitPlanes; ++i)
                    {
                        if (!enumerator.MoveNext())
                        {
                            yield break;
                        }

                        data[i] = enumerator.Current;
                    }
                    // Each pixel is from four bitplanes
                    for (var i = 0; i < 8; ++i)
                    {
                        var value = 0;
                        for (var b = 0; b < bitPlanes; ++b)
                        {
                            var bit = (data[b] >> (7 - i)) & 1;
                            value |= bit << b;
                        }

                        yield return (byte)value;
                    }
                }
            }

            public IEnumerable<byte> ChunkyToPlanar(int bitPlanes = 4)
            {
                // Each byte is a chunky index in the range 0..15.
                // We interleave each 8 bytes to four bitplanes.
                using var enumerator = input.GetEnumerator();
                var data = new byte[8];
                for (;;)
                {
                    // Get 8 bytes
                    for (int i = 0; i < 8; ++i)
                    {
                        if (!enumerator.MoveNext())
                        {
                            yield break;
                        }

                        data[i] = enumerator.Current;
                    }
                    // Convert them
                    for (var plane = 0; plane < bitPlanes; ++plane)
                    {
                        var b = 0;
                        for (var i = 0; i < 8; ++i)
                        {
                            var bit = (data[i] >> plane) & 1;
                            b |= bit << (7 - i);
                        }

                        yield return (byte)b;
                    }
                }
            }
        }

        public static byte[] CompressArt(MemoryStream source)
        {
            var artData = new List<byte[]>();
            var duplicates = new List<int>();
            var bitMasks = new List<byte>();
            var seenLines = new Dictionary<ulong, int>();
            
            // We work through it one row at a time...
            source.Position = 0;
            byte bitmask = 0;
            var linesConsumed = 0;
            using var reader = new BinaryReader(source);
            while (source.Position < source.Length)
            {
                // Read a line as a 64-bit number
                var line = reader.ReadBytes(8);
                var asInt = BitConverter.ToUInt64(line, 0);
                ++linesConsumed;
                bitmask >>= 1;
                // See if we already have it
                if (seenLines.TryGetValue(asInt, out var index))
                {
                    // If found, add the reference
                    duplicates.Add(index);
                    // And set the bit on the left
                    bitmask |= 0x80;
                }
                else
                {
                    // If not found, add to art data
                    artData.Add(line);
                    seenLines.Add(asInt, artData.Count - 1);
                }

                // If we have finished a tile, emit the bitmask
                if (linesConsumed == 8)
                {
                    bitMasks.Add(bitmask);
                    linesConsumed = 0;
                }
            }

            // Now we emit it back...
            using var result = new MemoryStream();
            using var resultWriter = new BinaryWriter(result);
            // Header - magic chars
            resultWriter.Write("HY"u8.ToArray());

            var duplicatesOffset = bitMasks.Count + 8;
            var artOffset = duplicatesOffset + duplicates.Sum(x => x < 0xf0 ? 1 : 2);
            var rowCount = bitMasks.Count * 8;

            resultWriter.Write((ushort)duplicatesOffset);
            resultWriter.Write((ushort)artOffset);
            resultWriter.Write((ushort)rowCount);

            // Next the bitmasks
            resultWriter.Write(bitMasks.ToArray());
            // The duplicates data
            foreach (var index in duplicates)
            {
                // 1-2 bytes encoding
                if (index < 0xf0)
                {
                    resultWriter.Write((byte)index);
                }
                else
                {
                    resultWriter.Write((byte)(0xf0 | (index >> 8)));
                    resultWriter.Write((byte)(index & 0xff));
                }
            }
            // And the art data, converting to chunky
            resultWriter.Write(artData.SelectMany(row => row.ChunkyToPlanar()).ToArray());
            return result.ToArray();
        }
    }
}