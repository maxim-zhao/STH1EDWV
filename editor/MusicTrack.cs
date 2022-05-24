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
                            _data.Add(new StopSFX());
                            break;
                        case 0xff:
                            _data.Add(new StopMusic());
                            break;
                        case >= 0x00 and < 0x70:
                            _data.Add(new Note(b));
                            break;
                        case >= 0x70 and < 0x7f:
                            _data.Add(new NoiseNote(b));
                            break;
                    }
                }
            }
        }
    }

    internal class LoopStart : IChannelData
    {
    }

    internal class Dummy : IChannelData
    {
        public byte Value { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }

    internal class Detune : IChannelData
    {
        public short Value { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }

    internal class Modulation : IChannelData
    {
        private readonly byte _delay;
        private readonly byte _speed;
        private readonly byte _count;
        private readonly short _changePerStep;

        public Modulation(Memory memory, ref int offset)
        {
            _delay = memory[offset++];
            _speed = memory[offset++];
            _count = memory[offset++];
            _changePerStep = (short)memory.Word(offset);
            offset += 2;
        }

        public override string ToString()
        {
            return $"Delay: {_delay}, Speed: {_speed}, Count: {_count}, ChangePerStep: {_changePerStep}";
        }
    }

    internal class Attenuation : IChannelData
    {
        private readonly byte _value;

        public Attenuation(Memory memory, ref int offset)
        {
            _value = memory[offset++];
        }

        public override string ToString()
        {
            return $"Attenuation: {_value} ({(_value == 0xf ? "silent" : $"{_value*-2}dB")})";
        }
    }

    internal interface IChannelData
    {
    }

    internal class TempoControl: IChannelData
    {
        private readonly byte _divider;
        private readonly byte _multiplier;

        public TempoControl(Memory memory, ref int offset)
        {
            _divider = memory[offset++];
            _multiplier = memory[offset++];
        }

        public override string ToString()
        {
            return $"Tempo scale: {_multiplier}/{_divider}";
        }

    }
}