using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sth1edwv
{
    public class FreeSpace
    {
        public class Span
        {
            public int Start { get; set; }
            public int End { get; set; } // Non-inclusive so the difference is the byte count
            public int Size => End - Start;
            public override string ToString()
            {
                return $"{Start:X}..{End:X} = {Size}";
            }
        }

        private readonly List<Span> _spans = new();
        public int Maximum { get; set; }
        public IEnumerable<Span> Spans => _spans;
        public int MaximumUsed
        {
            get
            {
                var last = _spans.Last();
                if (last.End == Maximum)
                {
                    return last.Start;
                }

                return Maximum;
            }
        }

        public void Add(int start, int end)
        {
            if (start == 0 || end == 0)
            {
                throw new Exception("Offset is not set");
            }

            // We add it at the right position
            var insertionIndex = 0;
            if (_spans.Count > 0)
            {
                // Insert before the next bigger
                insertionIndex = _spans.FindIndex(x => x.Start > start);
                // Or at the end if none
                if (insertionIndex == -1)
                {
                    insertionIndex = _spans.Count;
                }
            }
            // Check for overlap
            if (insertionIndex > 0)
            {
                if (_spans[insertionIndex - 1].End > start)
                {
                    throw new Exception($"Can't add free space {start:X}..{end:X} because one already exists from {_spans[insertionIndex - 1].Start:X}..{_spans[insertionIndex - 1].End:X}");
                }
            }

            if (insertionIndex < _spans.Count && _spans[insertionIndex].Start < end)
            {
                throw new Exception($"Can't add free space from {start:X}..{end:X} because one already exists from {_spans[insertionIndex].Start:X}..{_spans[insertionIndex].End:X}");

            }
            // Then insert
            _spans.Insert(insertionIndex, new Span { Start = start, End = end });
        }

        public void Consolidate()
        {
            // We join any neighbouring spans together
            for (var i = 0; i < _spans.Count - 1; )
            {
                if (_spans[i].End == _spans[i + 1].Start)
                {
                    _spans[i].End = _spans[i + 1].End;
                    _spans.RemoveAt(i+1);
                }
                else
                {
                    ++i;
                }
            }
        }

        public override string ToString()
        {
            return $"{_spans.Sum(x => x.Size)} bytes in {_spans.Count} spans: {string.Join(" | ", _spans)}";
        }

        public void Remove(int offset, int size)
        {
            // Find the span it affects
            var indexOfSpan = _spans.FindLastIndex(x => x.Start <= offset);
            if (indexOfSpan == -1)
            {
                throw new Exception($"No free space contains the removed part at ${offset:X}");
            }

            var span = _spans[indexOfSpan];

            if (span.End < offset + size)
            {
                throw new Exception($"Span from {span.Start}..{span.End} is too small for {offset}..{offset + size}");
            }

            if (span.Start == offset)
            {
                if (span.End == offset + size)
                {
                    // If it is a full match, remove it
                    _spans.RemoveAt(indexOfSpan);
                }
                else
                {
                    // Else chop from the start
                    span.Start += size;
                }
            }
            else if (span.End == offset + size)
            {
                // Chop from the end
                span.End -= size;
            }
            else
            {
                // Else we need to split the span
                var newSpan = new Span { Start = offset + size, End = span.End };
                span.End = offset;
                _spans.Insert(indexOfSpan + 1, newSpan);
            }
        }

        public int FindSpace(int size, Cartridge.Game.LocationRestriction restriction)
        {
            IEnumerable<Span> possibleSpans = _spans;
            if (restriction.MinimumOffset > 0)
            {
                // We filter out the ones too low, and split any crossing the boundary
                possibleSpans = possibleSpans
                    .Where(x => x.End > restriction.MinimumOffset)
                    .Select(x => new Span { Start = Math.Max(restriction.MinimumOffset, x.Start), End = x.End });
            }

            if (restriction.MaximumOffset < int.MaxValue)
            {
                // Same for the max
                possibleSpans = possibleSpans
                    .Where(x => x.Start <= restriction.MaximumOffset)
                    .Select(x => new Span { Start = x.Start, End = Math.Min(restriction.MaximumOffset, x.End) });
            }

            if (!restriction.CanCrossBanks)
            {
                // We split all the spans at bank boundaries
                possibleSpans = possibleSpans.SelectMany(SplitToBanks);
            }

            // MustFollow restrictions should already be handled outside this method

            // We try to find the smallest span that will fit the data, to minimize waste.
            var span = possibleSpans.Where(x => x.Size >= size)
                .OrderBy(x => x.Size)
                .FirstOrDefault();
            if (span != null)
            {
                if (span.Start > 0x3ff20)
                {
                    span.Start += 1;
                }
                return span.Start;
            }

            // If we get here then we failed
            throw new Exception($"Unable to find free space big enough for {size} bytes");
        }

        private IEnumerable<Span> SplitToBanks(Span source)
        {
            var startBank = source.Start / 0x4000;
            var endBank = (source.End - 1) / 0x4000;
            if (startBank == endBank)
            {
                // Nothing to do
                yield return source;
                yield break;
            }
            // Else start to split...
            var start = source.Start;
            for (var i = startBank; i <= endBank; ++i)
            {
                var end = Math.Min(source.End, (i + 1) * 0x4000);
                yield return new Span { Start = start, End = end };
                // Use this span's end as start for next one
                start = end;
            }
        }

        public int GetEaseOfPlacing(int size, int minOffset, int maxOffset)
        {
            // We return the amount of free space we might fit it in
            return _spans
                .Select(x => new Span { Start = Math.Max(minOffset, x.Start), End = Math.Min(maxOffset, x.End) }) // Trim to fit
                .Where(x => x.Size >= size) // Big enough
                .Sum(x => x.Size - size); // Count of "slack" bytes
        }

        public FreeSpace Clone()
        {
            var result = new FreeSpace { Maximum = Maximum };
            result._spans.AddRange(_spans.Select(x => new Span { Start = x.Start, End = x.End }));
            return result;
        }
    }
}