using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sth1edwv
{
    /// <summary>
    /// A flattened music track consists of a sequence of "rows", each corresponding to one "tick".
    /// Each row can contain "channel events". An event can include:
    /// * Note start
    /// * Note end
    /// * Other "effects" like setting the volume
    /// Each is associated with a particular channel.
    /// </summary>
    public class FlattenedMusicTrack
    {
        public interface IEvent { }

        public record Volume(byte Value) : IEvent
        {
            public override string ToString() => $"V{Value}";
        }
        public record Detune(short Value) : IEvent
        {
            public override string ToString() => $"D{Value}";
        }
        public record NoiseMode(byte Value) : IEvent
        {
            // TODO more parsing into meaning here
            public override string ToString() => $"N{Value}";
        } 
        public record Envelope(byte Level1, byte DecayRate1, byte Level2, byte DecayRate2, byte Level3, byte DecayRate3) : IEvent
        {
            public override string ToString() => $"E{Level1};{DecayRate1}->{Level2};{DecayRate2}->{Level3};{DecayRate3}";
        }
        public record Modulation(byte Delay, byte Speed, byte Count, short ChangePerStep) : IEvent
        {
            public override string ToString() => $"M{Delay};{Speed};{Count};{ChangePerStep}";
        }
        public record Tempo(ushort Multiplier, ushort Divider) : IEvent
        {
            public override string ToString() => $"T{Multiplier}/{Divider}";
        }
        public record Rest() : IEvent {
            public override string ToString() => "^^";
        }

        public record Drum(char Type) : IEvent {
            public override string ToString() => Type.ToString();
        }

        public record Note(string Name) : IEvent
        {
            public override string ToString() => Name;
        }

        public class Tick
        {
            public ChannelTick Channel0 { get; set; }
            public ChannelTick Channel1 { get; set; }
            public ChannelTick Channel2 { get; set; }
            public ChannelTick Channel3 { get; set; }
        }

        // One outer list per tick
        // One inner list per channel
        // Then a record holding one (or null) note events and many (or null) "effects"
        public record ChannelTick(IEvent Note, List<IEvent> Effects)
        {
            public override string ToString()
            {
                var sb = new StringBuilder();

                if (Note != null)
                {
                    sb.Append(Note);
                }

                sb.Append("    ");

                if (Effects != null)
                {
                    foreach (var effect in Effects)
                    {
                        sb.Append(effect).Append(' ');
                    }
                }

                return sb.ToString();
            }
        }
        public readonly List<Tick> Events = new();

        public FlattenedMusicTrack(MusicTrack data)
        {
            // We "play" the music track into our format.
            // Make a parser for each non-empty channel
            var parsers = data.Channels
                .Where(x => x.Data.Count > 0)
                .Select(x => new ChannelParser(x))
                .ToList();
            // "Play" then into our format
            while (parsers.Any(x => !x.HaveReachedEnd))
            {
                var listForThisTick = parsers.Select(x =>
                {
                    IEvent note = null;
                    var effects = new List<IEvent>();
                    foreach (var @event in x.GetEventsForOneTick())
                    {
                        switch (@event)
                        {
                            case Drum:
                            case Note:
                            case Rest:
                                note = @event;
                                break;
                            default:
                                effects.Add(@event);
                                break;
                        }
                    }

                    if (effects.Count > 0)
                    {
                        // Apply some cleanup.
                        // Squash multiple volume events into the last one
                        if (effects.OfType<Volume>().Count() > 1)
                        {
                            effects = effects
                                .Where(x => x is not Volume)
                                .Append(effects.OfType<Volume>().Last())
                                .ToList();
                        }
                    }

                    return new ChannelTick(note, effects.Count > 0 ? effects : null);
                }).ToList();
                Events.Add(new Tick
                {
                    Channel0 = listForThisTick[0],
                    Channel1 = listForThisTick[1],
                    Channel2 = listForThisTick[2],
                    Channel3 = listForThisTick[3]
                });
            }
        }

        private class ChannelParser
        {
            private readonly MusicTrack.Channel _channel;
            private int _index;
            private byte _currentVolume;
            private byte _defaultNoteLength = 1; // Hack
            private int _masterLoopPoint = -1;
            private readonly Stack<Tuple<int, int>> _loopCounters = new();
            private bool _holding;
            private int _currentNoteDuration;

            public ChannelParser(MusicTrack.Channel channel)
            {
                _channel = channel;
                _index = 0;
            }

            public bool HaveReachedEnd { get; private set; }

            public IEnumerable<IEvent> GetEventsForOneTick()
            {
                while (true)
                {
                    var e = GetNextEvent();
                    if (e == null)
                    {
                        yield break;
                    }

                    yield return e;
                }
            }

            private IEvent GetNextEvent()
            {
                // We loop through the data until we find something to return.
                // If we reach something with duration, we return nulls for the number of ticks it lasts.
                while (true)
                {
                    if (_currentNoteDuration > 0)
                    {
                        // We are in a time between events, return null until the time passes
                        --_currentNoteDuration;
                        return null;
                    }
                    // Read data
                    var data = _channel.Data[_index++];
                    switch (data)
                    {
                        case MusicTrack.Volume volume:
                            _currentVolume = volume.Value;
                            return new Volume(_currentVolume);
                        case MusicTrack.Detune detune:
                            return new Detune(detune.Value);
                        case MusicTrack.NoiseMode noiseMode:
                            return new NoiseMode(noiseMode.Value);
                        case MusicTrack.NoteLength defaultLength:
                            _defaultNoteLength = defaultLength.Value;
                            continue;
                        case MusicTrack.Dummy:
                            // Skip these
                            continue;
                        case MusicTrack.EndOfMusic:
                        case MusicTrack.EndOfSfx:
                            HaveReachedEnd = true;
                            // If we are looping, go to the loop point
                            if (_masterLoopPoint > -1)
                            {
                                _index =  _masterLoopPoint;
                            }
                            else
                            {
                                // Else we need to return nothing forever...
                                --_index;
                                return null;
                            }
                            continue;
                        case MusicTrack.Envelope envelope:
                            return new Envelope(envelope.Level1, envelope.DecayRate1, envelope.Level2, envelope.DecayRate2, envelope.Level3, envelope.DecayRate3);
                        case MusicTrack.Hold:
                            // Hold means the next note will continue the previous one, without restarting the envelope.
                            _holding = true;
                            continue;
                        case MusicTrack.LoopEnd loopEnd:
                        {
                            // Decrement counter and go to loop start
                            // We assume that the loop back point is the most recent one...
                            var counter = _loopCounters.Pop();
                            var loopCount = counter.Item2 + 1;
                            if (loopCount < loopEnd.RepeatCount)
                            {
                                // Loop again
                                _index = counter.Item1;
                                _loopCounters.Push(new Tuple<int, int>(_index, loopCount));
                            }
                            continue;
                        }
                        case MusicTrack.LoopStart:
                            // Remember this point, and that we've played it 0 times
                            _loopCounters.Push(new Tuple<int, int>(_index, 0));
                            continue;
                        case MusicTrack.MasterLoopPoint:
                            // Remember this
                            _masterLoopPoint = _index;
                            continue;
                        case MusicTrack.Modulation modulation:
                            return new Modulation(modulation.Delay, modulation.Speed, modulation.Count, modulation.ChangePerStep);
                        case MusicTrack.NoiseNote noiseNote:
                            var noiseLength = noiseNote.Length == 0 ? _defaultNoteLength : noiseNote.Length;
                            _currentNoteDuration += noiseLength;
                            if (_holding)
                            {
                                _holding = false;
                                continue;
                            }
                            return new Drum(noiseNote.Drum switch {
                                MusicTrack.NoiseNote.Drums.BassDrum => 'B',
                                MusicTrack.NoiseNote.Drums.SnareDrum => 'S',
                                _ => '?'
                            });
                        case MusicTrack.Rest rest:
                            var restLength = rest.Length == 0 ? _defaultNoteLength : rest.Length;
                            _currentNoteDuration += restLength;
                            if (_holding)
                            {
                                _holding = false;
                                continue;
                            }
                            return new Rest();
                        case MusicTrack.TempoControl tempoControl:
                            return new Tempo(tempoControl.Multiplier, tempoControl.Divider);
                        case MusicTrack.ToneNote toneNote:
                        {
                            var noteLength = toneNote.Length == 0 ? _defaultNoteLength : toneNote.Length;
                            _currentNoteDuration += noteLength;
                            if (_holding)
                            {
                                _holding = false;
                                continue;
                            }

                            return new Note(toneNote.NoteNumber switch
                            {
                                MusicTrack.ToneNote.Values.A => $"A-{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.ASharp => $"A#{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.B => $"B-{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.C => $"C-{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.CSharp => $"C#{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.D => $"D-{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.DSharp => $"D#{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.E => $"E-{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.F => $"F-{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.FSharp => $"F#{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.G => $"G-{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.GSharp => $"G#{toneNote.Octave + 3}",
                                MusicTrack.ToneNote.Values.LowFSharp => $"F#{toneNote.Octave + 2}",
                                MusicTrack.ToneNote.Values.LowG => $"G-{toneNote.Octave + 2}",
                                MusicTrack.ToneNote.Values.LowGSharp => $"G#{toneNote.Octave + 2}",
                                _ => "ERR"
                            });
                        }
                        case MusicTrack.VolumeDown:
                            if (_currentVolume > 0)
                            {
                                --_currentVolume;
                                return new Volume(_currentVolume);
                            }
                            break;
                        case MusicTrack.VolumeUp:
                            if (_currentVolume < 0xf)
                            {
                                ++_currentVolume;
                                return new Volume(_currentVolume);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(data));
                    }
                }
            }
        }
    }
}