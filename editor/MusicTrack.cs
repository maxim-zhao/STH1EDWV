using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace sth1edwv
{
    public class FlattenedMusicTrack
    {
        public int Offset { get; }
        public int Index { get; }

        public interface IEvent
        {
            // Nothing
        }

        public enum Notes
        {
            A, ASharp, B, C, CSharp, D, DSharp, E, F, FSharp, G, GSharp
        }

        public class Empty : IEvent
        {
            // Nothing
        }

        public class PlayNote: IEvent
        {
            public Notes Note { get; set; }
            public int Octave { get; set; }
        }

        public class KeyUp : IEvent
        {
            // Nothing
        }

        public class OtherEvent : IEvent
        {
            public IChannelData ChannelData { get; set; }
        }

        public List<List<IEvent>> Events { get; set; }

        public FlattenedMusicTrack(Memory memory, int offset, int index)
        {
            Offset = offset;
            Index = index;

            // Load the raw data
            var data = new MusicTrack(memory, offset, index);

            // Walk through that data to a "flattened" form for each channel,
            // with one object per tick.
            Events = data.Channels.Select(x => x.AsFlattenedEnumerable().ToList()).ToList();
        }
    }

    public class MusicTrack
    {
        public List<Channel> Channels { get; } = new();
        public int Offset { get; }
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
                    Channels.Add(new Channel(null, 0));
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
            return $"{Index} @ {Offset:X}, {Channels.Count(x => x.Data.Any())} channels";
        }

        public class Channel
        {
            public List<IChannelData> Data { get; } = new();

            public Channel(Memory memory, int offset)
            {
                if (offset != 0)
                {
                    LoadData(memory, offset);
                }
            }

            private void LoadData(Memory memory, int offset)
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
                            Data.Add(new Rest(memory, ref offset));
                            break;
                    }
                }
            }

            public IEnumerable<FlattenedMusicTrack.IEvent> AsFlattenedEnumerable()
            {
                var currentNoteLength = 0;
                var keyIsDown = false;
                var holdActive = false;
                ToneNote lastToneNote = null;
                Stack<int> loopStack = new();

                for (var i = 0; i < Data.Count; i++)
                {
                    var channelData = Data[i];
                    switch (channelData)
                    {
                        case Attenuation:
                        case Detune:
                        case Envelope:
                        case TempoControl:
                        case VolumeDown:
                        case VolumeUp:
                        case Modulation:
                        case NoiseMode:
                        case MasterLoopPoint:
                        case NoiseNote:
                            yield return new FlattenedMusicTrack.OtherEvent { ChannelData = channelData };
                            break;
                        case NoteLength noteLength:
                            // We keep track of this but emit nothing
                            currentNoteLength = noteLength.Value;
                            break;
                        case Dummy:
                            // Discard these
                            break;
                        case EndOfSfx:
                        case EndOfMusic:
                            // These signal the end
                            yield break;
                        case Hold:
                            // This signals that we don't emit a key up before the next key down
                            holdActive = true;
                            break;
                        case LoopStart:
                            // Start a new loop with a count of 0
                            loopStack.Push(0);
                            break;
                        case LoopEnd loopEnd:
                            // If loop stack is empty, it's an error
                            if (loopStack.Count == 0)
                            {
                                throw new Exception("Loop end with empty loop stack");
                            }
                            // Else we increment the loop counter...
                            var counter = loopStack.Pop();
                            ++counter;
                            if (counter < loopEnd.RepeatCount)
                            {
                                // Loop again
                                i = loopEnd.LoopBackPoint; // TODO is this right?
                                loopStack.Push(counter);
                            }
                            break;
                        case Rest rest:
                            if (keyIsDown)
                            {
                                yield return new FlattenedMusicTrack.KeyUp();
                                keyIsDown = false;
                            }
                            foreach (var _ in Enumerable.Repeat(0, rest.Length == 0 ? currentNoteLength : rest.Length))
                            {
                                // Emit empty events for the duration of the note
                                yield return new FlattenedMusicTrack.Empty();
                            }
                            break;
                        case ToneNote toneNote:
                            // If hold is active and the previous note is still active, don't emit an event, just more time
                            if (!keyIsDown || !holdActive)
                            {
                                yield return new FlattenedMusicTrack.PlayNote
                                {
                                    Note = toneNote.NoteNumber switch
                                    {
                                        ToneNote.Values.A => FlattenedMusicTrack.Notes.A,
                                        ToneNote.Values.ASharp => FlattenedMusicTrack.Notes.ASharp,
                                        ToneNote.Values.B => FlattenedMusicTrack.Notes.B,
                                        ToneNote.Values.C => FlattenedMusicTrack.Notes.C,
                                        ToneNote.Values.CSharp => FlattenedMusicTrack.Notes.CSharp,
                                        ToneNote.Values.D => FlattenedMusicTrack.Notes.D,
                                        ToneNote.Values.DSharp => FlattenedMusicTrack.Notes.DSharp,
                                        ToneNote.Values.E => FlattenedMusicTrack.Notes.E,
                                        ToneNote.Values.F => FlattenedMusicTrack.Notes.F,
                                        ToneNote.Values.FSharp => FlattenedMusicTrack.Notes.FSharp,
                                        ToneNote.Values.G => FlattenedMusicTrack.Notes.G,
                                        ToneNote.Values.GSharp => FlattenedMusicTrack.Notes.GSharp,
                                        ToneNote.Values.LowA => FlattenedMusicTrack.Notes.A,
                                        ToneNote.Values.LowASharp => FlattenedMusicTrack.Notes.ASharp,
                                        ToneNote.Values.LowB => FlattenedMusicTrack.Notes.B,
                                        _ => throw new ArgumentOutOfRangeException()
                                    },
                                    Octave = toneNote.NoteNumber > ToneNote.Values.GSharp
                                        ? toneNote.Octave + 2 // "Low" notes are in the previous octave
                                        : toneNote.Octave +
                                          3 // Octave 0 in-game is MIDI octave 3. (Octaves start at C.)
                                };
                                lastToneNote = toneNote;
                            }
                            else
                            {
                                // Sanity-check hold
                                if (toneNote.NoteNumber != lastToneNote.NoteNumber ||
                                    toneNote.Octave != lastToneNote.Octave)
                                {
                                    // We don't expect this to happen?
                                    throw new Exception("Held note changes pitch");
                                }
                            }

                            // Time passes
                            foreach (var _ in Enumerable.Repeat(0, toneNote.Length == 0 ? currentNoteLength - 1 : toneNote.Length - 1))
                            {
                                // Emit empty events for the duration of the note, minus 1 as we treat the note itself as taking one tick?
                                yield return new FlattenedMusicTrack.Empty();
                            }

                            // Hold should be cleared no matter what
                            holdActive = false;
                            break;
                    }
                }
            }
        }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this.Channels, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter> { new StringEnumConverter()}
            });
        }
    }

    internal class Rest : IChannelData
    {
        public Rest(Memory memory, ref int offset)
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
        public enum Values { C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B, LowA, LowASharp, LowB, Error }
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
            return $"Default note length {Value}";
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
            return $"Envelope: Attack {Attack}, decay {Decay1Rate} => {Decay1Level}, {Decay2Rate} => {Decay2Level}, {Decay3Rate} => 0";
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
            return $"Dummy: {Value}";
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
            return $"Detune: {Value}";
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
            return $"Modulation: Delay: {Delay}, Speed: {Speed}, Count: {Count}, ChangePerStep: {ChangePerStep}";
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

    public interface IChannelData
    {
    }

    internal class TempoControl: IChannelData
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