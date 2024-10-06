using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace sth1edwv.GameObjects
{
    public class LevelObject: IDataItem
    {
        public int Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public LevelObject()
        {
        }

        public LevelObject(IReadOnlyList<byte> memory, int offset)
        {
            Offset = offset;
            Type = memory[offset++];
            X = memory[offset++];
            Y = memory[offset];
        }

        public int Offset { get; set; }

        public IList<byte> GetData()
        {
            return new List<byte> { (byte)Type, (byte)X, (byte)Y };
        }

        public override string ToString()
        {
            var name = NamesById.TryGetValue(Type, out var obj) ? obj.Name : "UNKNOWN";

            return $"({X}, {Y}) 0x{Type:X2} = {name}";
        }

        public class NamedObject
        {
            public int Type { get; }
            public string Name { get; }

            public NamedObject(int type, string name)
            {
                Type = type;
                Name = name;
            }

            public override string ToString()
            {
                return $"{Type:X2}: {Name}";
            }
        }

        public static readonly List<NamedObject> Names =
        [
            new NamedObject(0x00, "NONE"),
            new NamedObject(0x01, "Super Ring monitor"),
            new NamedObject(0x02, "Power Sneakers monitor"),
            new NamedObject(0x03, "One-Up monitor"),
            new NamedObject(0x04, "Shield monitor"),
            new NamedObject(0x05, "Invincibility monitor"),
            new NamedObject(0x06, "Chaos Emerald"),
            new NamedObject(0x07, "End sign"),
            new NamedObject(0x08, "Badnik 'Crabmeat' (GH)"),
            new NamedObject(0x09, "Wooden platform - Swinging (GH)"),
            new NamedObject(0x0A, "Explosion"),
            new NamedObject(0x0B, "Wooden platform (GH)"),
            new NamedObject(0x0C, "Wooden platform - Falls when touched (GH)"),
            new NamedObject(0x0E, "Badnik 'Buzz Bomber' (GH/B)"),
            new NamedObject(0x0F, "Wooden platform - Sliding left-right (GH)"),
            new NamedObject(0x10, "Badnik 'Moto Bug' (GH)"),
            new NamedObject(0x11, "Badnik 'Newtron' (GH)"),
            new NamedObject(0x12, "Robotnik - Green Hill Boss (GH)"),
            new NamedObject(0x13, "Warp door (SB)"),
            new NamedObject(0x14, "Fireball (right) (SB)"),
            new NamedObject(0x15, "Fireball (left) (SB)"),
            new NamedObject(0x16, "Flame Thrower (SB)"),
            new NamedObject(0x17, "Door - Opens from left only (SB)"),
            new NamedObject(0x18, "Door - Opens from right only (SB)"),
            new NamedObject(0x19, "Door - Two ways (SB)"),
            new NamedObject(0x1A, "Electric sphere (SB)"),
            new NamedObject(0x1B, "Badnik 'Ball Hog' (SB)"),
            new NamedObject(0x1C, "Unknown - Ball from the 'Ball Hog' ?"),
            new NamedObject(0x1D, "Switch (SB, L, others ?)"),
            new NamedObject(0x1E, "Switch Activated Door (SB)"),
            new NamedObject(0x1F, "Badnik 'Caterkiller' (SB)"),
            new NamedObject(0x21, "Bumper - Sliding left-right (Bonus Stage ?)"),
            new NamedObject(0x22, "Robotnik - Scrap Brain Boss (SB)"),
            new NamedObject(0x23, "Free animal - Rabbit"),
            new NamedObject(0x24, "Free animal - Bird"),
            new NamedObject(0x25, "Animal Cell"),
            new NamedObject(0x26, "Badnik 'Chopper' (J, B)"),
            new NamedObject(0x27, "Vertical Step - Falling from a waterfall (J)"),
            new NamedObject(0x28, "Horizontal Step - Falling from a waterfall (J)"),
            new NamedObject(0x29, "Floating Step - Sonic can travel with it (J)"),
            new NamedObject(0x2C, "Robotnik - Jungle Boss (J)"),
            new NamedObject(0x2D, "Badnik 'Yadrin' (B)"),
            new NamedObject(0x2E, "Falling Bridge (B)"),
            new NamedObject(0x30, "Passing Clouds (SKYB)"),
            new NamedObject(0x31, "Propeller (SKYB)"),
            new NamedObject(0x32, "Badnik 'Bomb' (SKYB)"),
            new NamedObject(0x33, "Cannon Ball (SKYB)"),
            new NamedObject(0x35, "Badnik 'Unidus' (SKYB)"),
            new NamedObject(0x37, "Rotating cannon (SKYB)"),
            new NamedObject(0x38, "Flying platform (SKYB)"),
            new NamedObject(0x39, "Spiked wall slowly moving right (SKYB)"),
            new NamedObject(0x3A, "Small cannon in Sky Base Act 1 (SKYB)"),
            new NamedObject(0x3B, "Flying platform moving up-down (SKYB)"),
            new NamedObject(0x3C, "Badnik 'Jaws' (L)"),
            new NamedObject(0x3D, "Rotating spiked ball (L)"),
            new NamedObject(0x3E, "Spear, shifting up-down (L)"),
            new NamedObject(0x3F, "Fire ball thrower (L)"),
            new NamedObject(0x40, "Water Level Object (L)"),
            new NamedObject(0x41, "Bubble Maker (L)"),
            new NamedObject(0x44, "Badnik 'Burrobot' (L)"),
            new NamedObject(0x45, "Platform, move up when touched (L)"),
            new NamedObject(0x46, "Electrical Hazard for the Sky Base Boss (SKYB)"),
            new NamedObject(0x48, "Robotnik - Bridge Boss (SB)"),
            new NamedObject(0x49, "Robotnik - Labyrinth Boss (L)"),
            new NamedObject(0x4A, "Robotnik - Sky Base Boss (SKYB)"),
            new NamedObject(0x4B, "Zone that makes you fall (like in GH2)"),
            new NamedObject(0x4C, "Flipper (Bonus Stage)"),
            new NamedObject(0x4D, "RESET!"),
            new NamedObject(0x4E, "Balance (B)"),
            new NamedObject(0x4F, "RESET!"),
            new NamedObject(0x50, "Flower (GH)"),
            new NamedObject(0x51, "Arrow monitor"),
            new NamedObject(0x52, "Continue monitor"),
            new NamedObject(0x53, "Final animation in GH, when Sonic falls on Robotnik (then, goes to the next level)"),
            new NamedObject(0x54,
                "Emeralds animation (on the map), when Sonic has them all (and goes to the next level)"),
            new NamedObject(0x55, "Makes Sonic blink for a short time"),
            new NamedObject(0xFF, "NONE")
        ];

        public static readonly Dictionary<int, NamedObject> NamesById = Names.ToDictionary(x => x.Type);
        public static readonly Dictionary<string, NamedObject> ObjectsByName = Names.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First());

    }
}