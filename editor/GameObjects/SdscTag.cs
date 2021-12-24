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

            Version = Memory.FromBcd(memory[0x7fe4]) + (decimal)Memory.FromBcd(memory[0x7fe5]) / 100;
            Date = new DateTime(Memory.FromBcd(memory.Word(0x7fe8)), Memory.FromBcd(memory[0x7fe7]), Memory.FromBcd(memory[0x7fe6]));
            Author = ReadString(memory, 0x7fea);
            Title = ReadString(memory, 0x7fec);
            Notes = ReadString(memory, 0x7fee);
        }

        private static string ReadString(Memory memory, int pointerOffset)
        {
            var pointer = memory.Word(pointerOffset);
            return pointer is 0 or 0xffff ? "" : memory.NullTerminatedString(pointer);
        }

        public int Offset { get; set; }
        public IList<byte> GetData()
        {
            return Encoding.ASCII.GetBytes("SDSC")
                .Append((byte)Memory.ToBcd((int)Version))
                .Append((byte)Memory.ToBcd((int)(Version * 100) % 100))
                .Append((byte)Memory.ToBcd(Date.Day))
                .Append((byte)Memory.ToBcd(Date.Month))
                .Append((byte)Memory.ToBcd(Date.Year % 100))
                .Append((byte)Memory.ToBcd(Date.Year / 100))
                // Pointers are left empty
                .ToList();
        }
    }
}
