using System.Collections.Generic;
using System.Linq;

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

        public class NamedObject(int type, string name)
        {
            public int Type { get; } = type;
            public string Name { get; } = name;

            public override string ToString()
            {
                return $"{Type:X2}: {Name}";
            }
        }

        public static readonly List<NamedObject> Names =
        [
            new(0x00, "NONE"),
            new(0x01, "Super Ring monitor"),
            new(0x02, "Power Sneakers monitor"),
            new(0x03, "One-Up monitor"),
            new(0x04, "Shield monitor"),
            new(0x05, "Invincibility monitor"),
            new(0x06, "Chaos Emerald"),
            new(0x07, "End sign"),
            new(0x08, "Badnik 'Crabmeat' (GH)"),
            new(0x09, "Wooden platform - Swinging (GH)"),
            new(0x0A, "Explosion"),
            new(0x0B, "Wooden platform (GH)"),
            new(0x0C, "Wooden platform - Falls when touched (GH)"),
            new(0x0E, "Badnik 'Buzz Bomber' (GH/B)"),
            new(0x0F, "Wooden platform - Sliding left-right (GH)"),
            new(0x10, "Badnik 'Moto Bug' (GH)"),
            new(0x11, "Badnik 'Newtron' (GH)"),
            new(0x12, "Robotnik - Green Hill Boss (GH)"),
            new(0x13, "Warp door (SB)"),
            new(0x14, "Fireball (right) (SB)"),
            new(0x15, "Fireball (left) (SB)"),
            new(0x16, "Flame Thrower (SB)"),
            new(0x17, "Door - Opens from left only (SB)"),
            new(0x18, "Door - Opens from right only (SB)"),
            new(0x19, "Door - Two ways (SB)"),
            new(0x1A, "Electric sphere (SB)"),
            new(0x1B, "Badnik 'Ball Hog' (SB)"),
            new(0x1C, "Unknown - Ball from the 'Ball Hog' ?"),
            new(0x1D, "Switch (SB, L, others ?)"),
            new(0x1E, "Switch Activated Door (SB)"),
            new(0x1F, "Badnik 'Caterkiller' (SB)"),
            new(0x21, "Bumper - Sliding left-right (Bonus Stage ?)"),
            new(0x22, "Robotnik - Scrap Brain Boss (SB)"),
            new(0x23, "Free animal - Rabbit"),
            new(0x24, "Free animal - Bird"),
            new(0x25, "Animal Cell"),
            new(0x26, "Badnik 'Chopper' (J, B)"),
            new(0x27, "Vertical Step - Falling from a waterfall (J)"),
            new(0x28, "Horizontal Step - Falling from a waterfall (J)"),
            new(0x29, "Floating Step - Sonic can travel with it (J)"),
            new(0x2C, "Robotnik - Jungle Boss (J)"),
            new(0x2D, "Badnik 'Yadrin' (B)"),
            new(0x2E, "Falling Bridge (B)"),
            new(0x30, "Passing Clouds (SKYB)"),
            new(0x31, "Propeller (SKYB)"),
            new(0x32, "Badnik 'Bomb' (SKYB)"),
            new(0x33, "Cannon Ball (SKYB)"),
            new(0x35, "Badnik 'Unidus' (SKYB)"),
            new(0x37, "Rotating cannon (SKYB)"),
            new(0x38, "Flying platform (SKYB)"),
            new(0x39, "Spiked wall slowly moving right (SKYB)"),
            new(0x3A, "Small cannon in Sky Base Act 1 (SKYB)"),
            new(0x3B, "Flying platform moving up-down (SKYB)"),
            new(0x3C, "Badnik 'Jaws' (L)"),
            new(0x3D, "Rotating spiked ball (L)"),
            new(0x3E, "Spear, shifting up-down (L)"),
            new(0x3F, "Fire ball thrower (L)"),
            new(0x40, "Water Level Object (L)"),
            new(0x41, "Bubble Maker (L)"),
            new(0x44, "Badnik 'Burrobot' (L)"),
            new(0x45, "Platform, move up when touched (L)"),
            new(0x46, "Electrical Hazard for the Sky Base Boss (SKYB)"),
            new(0x48, "Robotnik - Bridge Boss (SB)"),
            new(0x49, "Robotnik - Labyrinth Boss (L)"),
            new(0x4A, "Robotnik - Sky Base Boss (SKYB)"),
            new(0x4B, "Zone that makes you fall (like in GH2)"),
            new(0x4C, "Flipper (Bonus Stage)"),
            new(0x4D, "RESET!"),
            new(0x4E, "Balance (B)"),
            new(0x4F, "RESET!"),
            new(0x50, "Flower (GH)"),
            new(0x51, "Arrow monitor"),
            new(0x52, "Continue monitor"),
            new(0x53, "Final animation in GH, when Sonic falls on Robotnik (then, goes to the next level)"),
            new(0x54, "Emeralds animation (on the map), when Sonic has them all (and goes to the next level)"),
            new(0x55, "Makes Sonic blink for a short time"),
            new(0xFF, "NONE")
        ];

        public static readonly Dictionary<int, NamedObject> NamesById = Names.ToDictionary(x => x.Type);
        public static readonly Dictionary<string, NamedObject> ObjectsByName = Names.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First());

    }
}