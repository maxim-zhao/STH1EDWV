using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace sth1edwv.GameObjects
{
    public class LevelObjectSet: IEnumerable<LevelObject>, IDataItem
    {
        // We thinly wrap a list
        public IEnumerator<LevelObject> GetEnumerator() => _objects.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();

        private readonly List<LevelObject> _objects;

        public LevelObjectSet(Cartridge cartridge, int offset)
        {
            Offset = offset;
            var count = cartridge.Memory[offset] - 1;
            _objects = Enumerable.Range(0, count)
                .Select(i => new LevelObject(cartridge.Memory, offset + i * 3 + 1))
                .ToList();
        }

        public override string ToString()
        {
            return $"{_objects.Count} objects @ {Offset:X}";
        }

        public int Offset { get; set; }

        public IList<byte> GetData()
        {
            return new[] { (byte)(_objects.Count + 1) }
                .Concat(_objects.SelectMany(x => x.GetData()))
                .ToList();
        }

        public void Add(LevelObject levelObject)
        {
            _objects.Add(levelObject);
        }

        public void Remove(LevelObject levelObject)
        {
            _objects.Remove(levelObject);
        }
    }
}
