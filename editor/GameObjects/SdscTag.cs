using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sth1edwv.GameObjects
{
    public class SdscTag: IDataItem
    {
        public string Title { get; init; }
        public string Author { get; init; }
        public string Notes { get; init; }
        public decimal Version { get; init; }
        public DateTime Date { get; init; } = DateTime.Today;

        public SdscTag()
        {
            // Default to nothing
        }

        public SdscTag(Memory memory)
        {
            if (memory.String(0x7fe0, 4) != "SDSC")
            {
                throw new Exception("No SDSC header");
            }

            Version = FromBcd(memory[0x7fe4]) + (decimal)FromBcd(memory[0x7fe5]) / 100;
            Date = new DateTime(FromBcd(memory.Word(0x7fe8)), FromBcd(memory[0x7fe7]), FromBcd(memory[0x7fe6]));
            Author = ReadString(memory, 0x7fea);
            Title = ReadString(memory, 0x7fec);
            Notes = ReadString(memory, 0x7fee);
        }

        private static string ReadString(Memory memory, int pointerOffset)
        {
            var pointer = memory.Word(pointerOffset);
            return pointer is 0 or 0xffff ? "" : memory.NullTerminatedString(pointer);
        }

        private static int FromBcd(int bcd)
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

        public int Offset { get; set; }
        public IList<byte> GetData()
        {
            return Encoding.ASCII.GetBytes("SDSC")
                .Append(ToBcd((int)Version))
                .Append(ToBcd((int)(Version * 100)))
                .Append(ToBcd(Date.Day))
                .Append(ToBcd(Date.Month))
                .Append(ToBcd(Date.Year))
                .Append(ToBcd(Date.Year / 100))
                // Pointers are left empty
                .ToList();
        }

        private static byte ToBcd(int value)
        {
            return (byte)((value % 10) + ((value / 10) % 10) * 16);
        }
    }
}
