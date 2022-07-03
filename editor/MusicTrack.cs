using System;
using System.Collections.Generic;

namespace sth1edwv
{
    internal class MusicTrack
    {
        private readonly List<Channel> _channels = new();

        public MusicTrack(Memory memory, int offset)
        {
            // The start is five relative pointers. The fifth is always 0 - it represents SFX in the music engine.
            for (var i = 0; i < 4; ++i)
            {
                var relativeOffset = memory.Word(offset + i * 2);
                var channelOffset = offset + relativeOffset;
                _channels.Add(new Channel(memory, channelOffset));
            }
        }

        private class Channel
        {
            private readonly List<IChannelData> _data = new();

            public Channel(Memory memory, int offset)
            {
                while (true)
                {
                    var b = memory[offset++];
                    switch (b)
                    {
                        case 0x80:
                            _data.Add(new TempoControl(memory, ref offset));
                            break;
                        case 0x81:
                            _data.Add(new Attenuation(memory, ref offset));
                            break;
                        case 0x82:
                            _data.Add(new Envelope(memory, ref offset));
                            break;
                        case 0x83:
                            _data.Add(new Modulation(memory, ref offset));
                            break;
                        case 0x84:
                            _data.Add(new Detune(memory, ref offset));
                            break;
                        case 0x85:
                            _data.Add(new Dummy(memory, ref offset));
                            offset += 2;
                            break;
                        case 0x86:
                            _data.Add(new LoopStart());
                            break;
                        case 0x87:
                            _data.Add(new LoopEnd(memory, ref offset));
                            break;
                        case 0x88:
                            _data.Add(new MasterLoopPoint());
                            break;
                        case 0x89:
                            _data.Add(new NoiseMode(memory, ref offset));
                            break;
                        case 0x8a:
                            _data.Add(new NoteLength(memory, ref offset));
                            break;
                        case 0x8b:
                            _data.Add(new VolumeUp());
                            break;
                        case 0x8c:
                            _data.Add(new VolumeDown());
                            break;
                        case 0x8d:
                            _data.Add(new Hold());
                            break;
                        case 0xfe:
                            _data.Add(new EndOfSfx());
                            break;
                        case 0xff:
                            _data.Add(new EndOfMusic());
                            break;
                        case >= 0x00 and < 0x70:
                            _data.Add(new ToneNote(b, memory, ref offset));
                            break;
                        case >= 0x70 and < 0x7f:
                            _data.Add(new NoiseNote(b, memory, ref offset));
                            break;
                        case 0x7f:
                            _data.Add(new Rest(b, memory, ref offset));
                            break;
                    }
                }
            }
        }
    }

    internal class Rest : IChannelData
    {
        public Rest(byte b, Memory memory, ref int offset)
        {
            Length = memory[offset++];
        }

        public byte Length { get; set; }

        public override string ToString()
        {
            return $"Rest length {Length}";
        }
    }

    internal class NoiseNote : IChannelData
    {
        public enum Drums { BassDrum, SnareDrum, Invalid }
        public NoiseNote(byte data, Memory memory, ref int offset)
        {
            Drum = (data & 0xf) switch
            {
                0 => Drums.BassDrum,
                1 => Drums.SnareDrum,
                _ => Drums.Invalid
            };
            Length = memory[offset++];
        }

        public Drums Drum { get; set; }
        public byte Length { get; set; }

        public override string ToString()
        {
            return $"Drum: {Drum} length {Length}";
        }

    }

    internal class ToneNote : IChannelData
    {
        public enum Values { A, ASharp, B, C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, LowFSharp, LowG, LowGSharp, Error }
        public ToneNote(byte data, Memory memory, ref int offset)
        {
            NoteNumber = (Values)(data & 0xf);
            Octave = data >> 8;
            Length = memory[offset++];
        }

        public int Octave { get; set; }

        public Values NoteNumber { get; set; }

        public byte Length { get; set; }

        public override string ToString()
        {
            return $"Octave {Octave} note {NoteNumber} length {Length}";
        }
    }

    internal class EndOfMusic : IChannelData
    {
        public override string ToString()
        {
            return "End of music";
        }
    }

    internal class EndOfSfx : IChannelData
    {
        public override string ToString()
        {
            return "End of SFX";
        }
    }

    internal class Hold : IChannelData
    {
        public override string ToString()
        {
            return "Hold note";
        }
    }

    internal class VolumeDown : IChannelData
    {
        public override string ToString()
        {
            return "Volume Down";
        }
    }

    internal class VolumeUp : IChannelData
    {
        public override string ToString()
        {
            return "Volume Up";
        }
    }

    internal class NoteLength : Dummy
    {
        public NoteLength(Memory memory, ref int offset): base(memory, ref offset) {}

        public override string ToString()
        {
            return $"Note length {Value}";
        }
    }

    internal class NoiseMode : Dummy
    {
        public NoiseMode(Memory memory, ref int offset) : base(memory, ref offset)
        { }

        public override string ToString()
        {
            return $"Noise mode {Value:X}";
        }
    }

    internal class MasterLoopPoint : IChannelData
    {
        public override string ToString()
        {
            return "Master loop point";
        }
    }

    internal class LoopEnd : IChannelData
    {
        public LoopEnd(Memory memory, ref int offset)
        {
            RepeatCount = memory[offset++];
            LoopBackPoint = memory.Word(offset);
            offset += 2;
        }

        public ushort LoopBackPoint { get; set; }

        public byte RepeatCount { get; set; }


    }

    internal class Envelope : IChannelData
    {
        public Envelope(Memory memory, ref int offset)
        {
            Attack = memory[offset++];
            Decay1Rate = memory[offset++];
            Decay1Level = memory[offset++];
            Decay2Rate = memory[offset++];
            Decay2Level = memory[offset++];
            Decay3Rate = memory[offset++];
        }

        public override string ToString()
        {
            return $"Attack {Attack}, decay {Decay1Rate} => {Decay1Level}, {Decay2Rate} => {Decay2Level}, {Decay3Rate} => 0";
        }

        public byte Attack { get; set; }
        public byte Decay1Rate { get; set; }
        public byte Decay1Level { get; set; }
        public byte Decay2Rate { get; set; }
        public byte Decay2Level { get; set; }
        public byte Decay3Rate { get; set; }
    }

    internal class LoopStart : IChannelData
    {
        public override string ToString()
        {
            return "Loop start";
        }
    }

    internal class Dummy : IChannelData
    {
        public Dummy(Memory memory, ref int offset)
        {
            Value = memory[offset++];
        }

        public byte Value { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }

    internal class Detune : IChannelData
    {
        public Detune(Memory memory, ref int offset)
        {
            Value = (short)memory.Word(offset);
            offset += 2;
        }

        public short Value { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }

    internal class Modulation : IChannelData
    {
        public byte Delay { get; set; }
        public byte Speed { get; set; }
        public byte Count { get; set; }
        public short ChangePerStep { get; set; }

        public Modulation(Memory memory, ref int offset)
        {
            Delay = memory[offset++];
            Speed = memory[offset++];
            Count = memory[offset++];
            ChangePerStep = (short)memory.Word(offset);
            offset += 2;
        }

        public override string ToString()
        {
            return $"Delay: {Delay}, Speed: {Speed}, Count: {Count}, ChangePerStep: {ChangePerStep}";
        }
    }

    internal class Attenuation : IChannelData
    {
        public byte Value { get; set; }

        public Attenuation(Memory memory, ref int offset)
        {
            Value = memory[offset++];
        }

        public override string ToString()
        {
            return $"Attenuation: {Value} = {(Value == 0xf ? "silent" : $"{Value*-2}dB")}";
        }
    }

    internal interface IChannelData
    {
    }

    internal class TempoControl: IChannelData
    {
        public byte Divider { get; set; }
        public byte Multiplier { get; set; }

        public TempoControl(Memory memory, ref int offset)
        {
            Divider = memory[offset++];
            Multiplier = memory[offset++];
        }

        public override string ToString()
        {
            return $"Tempo scale: {Multiplier}/{Divider} = {120.0 * Multiplier / Divider} BPM";
        }

    }
}