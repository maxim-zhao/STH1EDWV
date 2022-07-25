using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace sth1edwv
{
    public class MusicTrack : IDataItem
    {
        public List<Channel> Channels { get; } = new();
        public int Offset { get; set; }

        public IList<byte> GetData()
        {
            // TODO this when it's time to save
            throw new NotImplementedException();
        }

        public int Index { get; }

        public MusicTrack(Memory memory, int offset, int index)
        {
            Offset = offset;
            Index = index;
            // The start is five relative pointers. The fifth is always 0.
            for (var i = 0; i < 5; ++i)
            {
                var relativeOffset = memory.Word(offset + i * 2);
                if (relativeOffset == 0)
                {
                    Channels.Add(new Channel());
                }
                else
                {
                    var channelOffset = offset + relativeOffset;
                    Channels.Add(new Channel(memory, channelOffset));
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"#{Index} @ {Offset:X}: ");
            if (IntroLength > TimeSpan.Zero)
            {
                sb.Append($"{IntroLength:mm:ss}");
            }

            return sb.ToString();
        }

        public TimeSpan IntroLength { get; set; }

        public class Channel
        {
            public List<IChannelData> Data { get; } = new();

            public Channel()
            {
            }

            public Channel(Memory memory, int offset)
            {
                bool hasEnded = false;
                while (!hasEnded)
                {
                    var b = memory[offset++];
                    switch (b)
                    {
                        case 0x80:
                            Data.Add(new TempoControl(memory, ref offset));
                            break;
                        case 0x81:
                            Data.Add(new Volume(memory, ref offset));
                            break;
                        case 0x82:
                            Data.Add(new Envelope(memory, ref offset));
                            break;
                        case 0x83:
                            Data.Add(new Modulation(memory, ref offset));
                            break;
                        case 0x84:
                            Data.Add(new Detune(memory, ref offset));
                            break;
                        case 0x85:
                            Data.Add(new Dummy(memory, ref offset));
                            break;
                        case 0x86:
                            Data.Add(new LoopStart());
                            break;
                        case 0x87:
                            Data.Add(new LoopEnd(memory, ref offset));
                            break;
                        case 0x88:
                            Data.Add(new MasterLoopPoint());
                            break;
                        case 0x89:
                            Data.Add(new NoiseMode(memory, ref offset));
                            break;
                        case 0x8a:
                            Data.Add(new NoteLength(memory, ref offset));
                            break;
                        case 0x8b:
                            Data.Add(new VolumeUp());
                            break;
                        case 0x8c:
                            Data.Add(new VolumeDown());
                            break;
                        case 0x8d:
                            Data.Add(new Hold());
                            break;
                        case 0xfe:
                            Data.Add(new EndOfSfx());
                            hasEnded = true;
                            break;
                        case 0xff:
                            Data.Add(new EndOfMusic());
                            hasEnded = true;
                            break;
                        case >= 0x00 and < 0x70:
                            Data.Add(new ToneNote(b, memory, ref offset));
                            break;
                        case >= 0x70 and < 0x7f:
                            Data.Add(new NoiseNote(b, memory, ref offset));
                            break;
                        case 0x7f:
                            Data.Add(new Rest(b, memory, ref offset));
                            break;
                    }
                }
            }
        }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(Channels, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            });
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

        public class NoiseNote : IChannelData
        {
            public enum Drums
            {
                BassDrum,
                SnareDrum,
                Invalid
            }

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
            public enum Values
            {
                A,
                ASharp,
                B,
                C,
                CSharp,
                D,
                DSharp,
                E,
                F,
                FSharp,
                G,
                GSharp,
                LowFSharp,
                LowG,
                LowGSharp,
                Error
            }

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
            public NoteLength(Memory memory, ref int offset) : base(memory, ref offset)
            {
            }

            public override string ToString()
            {
                return $"Note length {Value}";
            }
        }

        internal class NoiseMode : Dummy
        {
            public NoiseMode(Memory memory, ref int offset) : base(memory, ref offset)
            {
            }

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
                Level1 = memory[offset++];
                DecayRate1 = memory[offset++];
                Level2 = memory[offset++];
                DecayRate2 = memory[offset++];
                Level3 = memory[offset++];
                DecayRate3 = memory[offset++];
            }

            public override string ToString()
            {
                return
                    $"Level1 {Level1}, decay {DecayRate1} => {Level2}, {DecayRate2} => {Level3}, {DecayRate3} => 0";
            }

            public byte Level1 { get; set; }
            public byte DecayRate1 { get; set; }
            public byte Level2 { get; set; }
            public byte DecayRate2 { get; set; }
            public byte Level3 { get; set; }
            public byte DecayRate3 { get; set; }
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

        internal class Volume : IChannelData
        {
            public byte Value { get; set; }

            public Volume(Memory memory, ref int offset)
            {
                Value = memory[offset++];
            }

            public override string ToString()
            {
                return $"Volume: {Value} = {(Value == 0x0 ? "silent" : $"{(0xf - Value) * -2}dB")}";
            }
        }

        public interface IChannelData
        {
        }

        internal class TempoControl : IChannelData
        {
            public ushort Divider { get; set; }
            public ushort Multiplier { get; set; }

            public TempoControl(Memory memory, ref int offset)
            {
                Divider = memory.Word(offset);
                offset += 2;
                Multiplier = memory.Word(offset);
                offset += 2;
            }

            public override string ToString()
            {
                return $"Tempo scale: {Multiplier}/{Divider} = {120.0 * Multiplier / Divider} BPM";
            }
        }
    }
}