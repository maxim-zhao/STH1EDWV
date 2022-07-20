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
                    Channels.Add(new Channel(null, 0, 0));
                }
                else
                {
                    var channelOffset = offset + relativeOffset;
                    Channels.Add(new Channel(memory, channelOffset, Offset));
                }
            }

            // Some tracks in the original game are just a looped rest. We flatten these to nothing.
            foreach (var channel in Channels.Where(channel => channel.Data.OfType<Duration>().All(x => x is Rest)))
            {
                channel.Clear();
            }

            // Some tracks may be shorter than others, to make better use of space. We need to add extra loops and change the looping to be consistent.
            if (Channels.Any(x => x.IsLooped))
            {
                var maxIntroLength = Channels.Max(x => x.IntroLength);
                var maxTotalLength = Channels.Max(x => x.TotalLength);
                foreach (var channel in Channels.Where(x =>
                             x.IsLooped && (x.TotalLength < maxTotalLength || x.IntroLength < maxIntroLength)))
                {
                    channel.ExtendTo(maxIntroLength, maxTotalLength);
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Index} @ {Offset:X}, {Channels.Count(x => x.Data.Any())} channels, ");
            sb.Append($"[{string.Join(", ", Channels.Select(x => x.Data.Count))}] commands, ");
            sb.Append("Lengths: [");
            foreach (var channel in Channels)
            {
                var introLength = 0;
                var loopLength = 0;
                var inIntro = true;
                foreach (var channelData in channel.Data)
                {
                    switch (channelData)
                    {
                        case Duration d:
                            if (inIntro)
                            {
                                introLength += d.Length;
                            }
                            else
                            {
                                loopLength += d.Length;
                            }

                            break;
                        case MasterLoopPoint:
                            inIntro = false;
                            break;
                    }
                }

                if (inIntro)
                {
                    sb.Append($"{introLength} beats");
                }
                else
                {
                    sb.Append($"{introLength} + {loopLength} beats");
                }

                sb.Append(", ");
            }

            sb.Append("]");
            return sb.ToString();
        }

        public class Channel
        {
            public List<IChannelData> Data { get; private set; } = new();
            public bool IsLooped { get; set; }
            public int IntroLength { get; set; }
            public int LoopLength { get; set; }
            public int TotalLength { get; set; }

            public Channel(Memory memory, int offset, int loopBase)
            {
                if (offset != 0)
                {
                    LoadData(memory, offset, loopBase);
                }
            }

            private void LoadData(Memory memory, int offset, int loopBase)
            {
                // We parse the data, following loops rather than emitting data for them.
                var loopCounters = new Stack<int>();
                // We also embed all the note lengths, no defaults.
                var defaultNoteLength = (byte)0;

                var haveReachedEnd = false;
                while (!haveReachedEnd)
                {
                    var b = memory[offset++];
                    switch (b)
                    {
                        case >= 0x00 and < 0x70:
                        {
                            var note = new ToneNote(b, memory, ref offset);
                            if (note.Length == 0)
                            {
                                note.Length = defaultNoteLength;
                            }

                            Data.Add(note);
                            break;
                        }
                        case >= 0x70 and < 0x7f:
                        {
                            var note = new NoiseNote(b, memory, ref offset);
                            if (note.Length == 0)
                            {
                                note.Length = defaultNoteLength;
                            }

                            Data.Add(note);
                            break;
                        }
                        case 0x7f:
                        {
                            var rest = new Rest(memory, ref offset);
                            if (rest.Length == 0)
                            {
                                rest.Length = defaultNoteLength;
                            }

                            Data.Add(rest);
                            break;
                        }
                        case 0x80:
                            Data.Add(new TempoControl(memory, ref offset));
                            break;
                        case 0x81:
                            Data.Add(new Attenuation(memory, ref offset));
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
                            // We drop these
                            var _ = new Dummy(memory, ref offset);
                            break;
                        case 0x86:
                            // Loop start
                            loopCounters.Push(0);
                            break;
                        case 0x87:
                            var loopCounter = loopCounters.Pop();
                            ++loopCounter;
                            var loopEnd = new LoopEnd(memory, ref offset);
                            if (loopEnd.RepeatCount != loopCounter)
                            {
                                // Loop back
                                offset = loopBase + loopEnd.LoopBackPoint;
                                // Sanity check - never happens in the original data
                                if (memory[offset - 1] != 0x86)
                                {
                                    throw new Exception("Bad loop back");
                                }

                                loopCounters.Push(loopCounter);
                            }

                            // Else we are done with it
                            break;
                        case 0x88:
                            Data.Add(new MasterLoopPoint());
                            break;
                        case 0x89:
                            Data.Add(new NoiseMode(memory, ref offset));
                            break;
                        case 0x8a:
                            defaultNoteLength = new DefaultNoteLength(memory, ref offset).Value;
                            // We don't add it to the data
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
                        case 0xff:
                            haveReachedEnd = true;
                            break;
                        default:
                            throw new Exception($"Invalid data {b:02X} at offset {offset - 1:05X}");
                    }
                }
            
                //Optimize();

                CalculateLengths();
            }

            private void CalculateLengths()
            {
                IsLooped = false;
                IntroLength = LoopLength = TotalLength = 0;
                var tickCount = 0;
                foreach (var channelData in Data)
                {
                    switch (channelData)
                    {
                        case Duration d:
                            tickCount += d.Length;
                            break;
                        case MasterLoopPoint:
                            IsLooped = true;
                            IntroLength = tickCount;
                            tickCount = 0;
                            break;
                    }
                }

                if (IsLooped)
                {
                    LoopLength = tickCount;
                }

                TotalLength = IntroLength + LoopLength;
            }

            private void Optimize()
            {
                // We want to:
                // * Remove any looped rest at the end
                if (IsLooped)
                {
                    if (!Data.SkipWhile(x => x is not MasterLoopPoint).Any(x => x is ToneNote or NoiseNote))
                    {
                        Data = Data.TakeWhile(x => x is not MasterLoopPoint).ToList();
                    }
                }


                // * Replace 3+ volume changes to an attenuation set
                // * Combine notes with holds, or rests, into longer durations
                // * Remove attenuation commands that set the volume to the current value

                CalculateLengths();
            }

            public void ExtendTo(int introLength, int totalLength)
            {
                // Sanity-check it. The extensions should be a multiple of the loop length (for looped tracks)
                if (LoopLength > 0)
                {
                    if (introLength > 0 && (introLength - IntroLength) % LoopLength != 0)
                    {
                        throw new Exception(
                            $"Can't extend intro to {introLength} ticks as we have intro = {IntroLength}, loop = {LoopLength}");
                    }

                    if ((totalLength - introLength) % LoopLength != 0)
                    {
                        throw new Exception(
                            $"Can't extend total to {totalLength} ticks as we have intro = {IntroLength}, loop = {LoopLength}");
                    }

                    // Else we do it. Total length first...
                    var loopsToAdd = (totalLength - TotalLength) / LoopLength;
                    var loopStart = Data.FindIndex(x => x is MasterLoopPoint) + 1;
                    var loopEnd = Data.Count;
                    for (var i = 0; i < loopsToAdd; ++i)
                    {
                        for (var j = loopStart; j < loopEnd; ++j)
                        {
                            // We want to clone the items as we don't want to edit one and change more than one
                            Data.Add((IChannelData)Data[j].Clone());
                        }
                    }

                    // Then we may need to move the loop point
                    if (introLength != IntroLength)
                    {
                        // We can reuse the objects here
                        int tickCount = 0;
                        var newData = new List<IChannelData>();
                        foreach (var channelData in Data)
                        {
                            switch (channelData)
                            {
                                case MasterLoopPoint:
                                    // Drop it
                                    continue;
                                case Duration d:
                                    newData.Add(channelData);
                                    tickCount += d.Length;
                                    if (tickCount == introLength)
                                    {
                                        newData.Add(new MasterLoopPoint());
                                    }

                                    break;
                                default:
                                    newData.Add(channelData);
                                    break;
                            }
                        }

                        Data = newData;
                    }
                }
                else
                {
                    // For unlooped tracks, they might just end early. We want to add rests to fill the space.
                    var restLength = totalLength - TotalLength;
                    while (restLength > 0)
                    {
                        int length = Math.Min(restLength, 255);
                        Data.Add(new Rest { Length = (byte)length });
                        restLength -= length;
                    }
                }

                CalculateLengths();
            }

            public void Clear()
            {
                Data.Clear();
                CalculateLengths();
            }
        }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this.Channels, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            });
        }
    }

    internal class Rest : Duration, IChannelData
    {
        public Rest()
        {
        }

        public Rest(Memory memory, ref int offset)
        {
            Length = memory[offset++];
        }

        public override string ToString()
        {
            return $"Rest length {Length}";
        }

        public object Clone()
        {
            return new Rest { Length = Length };
        }
    }

    internal class NoiseNote : Duration, IChannelData
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

        private NoiseNote()
        {
        }

        public Drums Drum { get; set; }

        public override string ToString()
        {
            return $"Drum: {Drum} length {Length}";
        }

        public object Clone()
        {
            return new NoiseNote { Drum = Drum, Length = Length };
        }
    }

    internal class Duration
    {
        public byte Length { get; set; }
    }

    internal class ToneNote : Duration, IChannelData
    {
        public enum Values
        {
            C,
            CSharp,
            D,
            DSharp,
            E,
            F,
            FSharp,
            G,
            GSharp,
            A,
            ASharp,
            B,
            LowA,
            LowASharp,
            LowB,
            Error
        }

        public ToneNote(byte data, Memory memory, ref int offset)
        {
            NoteNumber = (Values)(data & 0xf);
            Octave = data >> 8;
            Length = memory[offset++];
        }

        private ToneNote()
        {
        }

        public int Octave { get; set; }

        public Values NoteNumber { get; set; }

        public override string ToString()
        {
            return $"Octave {Octave} note {NoteNumber} length {Length}";
        }

        public object Clone()
        {
            return new ToneNote { Length = Length, NoteNumber = NoteNumber, Octave = Octave };
        }
    }

    internal class Hold : IChannelData
    {
        public override string ToString()
        {
            return "Hold note";
        }

        public object Clone()
        {
            return new Hold();
        }
    }

    internal class VolumeDown : IChannelData
    {
        public override string ToString()
        {
            return "Volume Down";
        }

        public object Clone()
        {
            return new VolumeDown();
        }
    }

    internal class VolumeUp : IChannelData
    {
        public override string ToString()
        {
            return "Volume Up";
        }

        public object Clone()
        {
            return new VolumeUp();
        }
    }

    internal class DefaultNoteLength : Dummy
    {
        public DefaultNoteLength(Memory memory, ref int offset) : base(memory, ref offset)
        {
        }

        public override string ToString()
        {
            return $"Default note length {Value}";
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

        public object Clone()
        {
            throw new Exception("Shouldn't be cloning a master loop point");
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

        public object Clone()
        {
            throw new Exception("Shouldn't be cloning a loop end");
        }
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

        private Envelope()
        {
        }

        public override string ToString()
        {
            return
                $"Envelope: Attack {Attack}, decay {Decay1Rate} => {Decay1Level}, {Decay2Rate} => {Decay2Level}, {Decay3Rate} => 0";
        }

        public object Clone()
        {
            return new Envelope()
            {
                Attack = Attack, Decay1Level = Decay1Level, Decay1Rate = Decay1Rate, Decay2Level = Decay2Level,
                Decay2Rate = Decay2Rate, Decay3Rate = Decay3Rate
            };
        }

        public byte Attack { get; set; }
        public byte Decay1Rate { get; set; }
        public byte Decay1Level { get; set; }
        public byte Decay2Rate { get; set; }
        public byte Decay2Level { get; set; }
        public byte Decay3Rate { get; set; }
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
            return $"Dummy: {Value}";
        }

        public object Clone()
        {
            throw new Exception("Shouldn't be cloning a dummy event");
        }
    }

    internal class Detune : IChannelData
    {
        public Detune(Memory memory, ref int offset)
        {
            Value = (short)memory.Word(offset);
            offset += 2;
        }

        private Detune()
        {
        }

        public short Value { get; set; }

        public override string ToString()
        {
            return $"Detune: {Value}";
        }

        public object Clone()
        {
            return new Detune() { Value = Value };
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

        private Modulation()
        {
        }

        public override string ToString()
        {
            return $"Modulation: Delay: {Delay}, Speed: {Speed}, Count: {Count}, ChangePerStep: {ChangePerStep}";
        }

        public object Clone()
        {
            return new Modulation() { Delay = Delay, Speed = Speed, Count = Count, ChangePerStep = ChangePerStep };
        }
    }

    internal class Attenuation : IChannelData
    {
        public byte Value { get; set; }

        public Attenuation(Memory memory, ref int offset)
        {
            Value = memory[offset++];
        }

        private Attenuation()
        {
        }

        public override string ToString()
        {
            return $"Attenuation: {Value} = {(Value == 0xf ? "silent" : $"{Value * -2}dB")}";
        }

        public object Clone()
        {
            return new Attenuation() { Value = Value };
        }
    }

    public interface IChannelData : ICloneable
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

        private TempoControl()
        {
        }

        public override string ToString()
        {
            return $"Tempo scale: {Multiplier}/{Divider} = {120.0 * Multiplier / Divider} BPM";
        }

        public object Clone()
        {
            return new TempoControl
            {
                Divider = Divider,
                Multiplier = Multiplier
            };
        }
    }
}