using System.Collections;

namespace NjmgLib;
public sealed class RangeMap : IEnumerable<Range>
{
    public IReadOnlyList<Range> Ranges => _readOnlyRanges ??= _ranges.AsReadOnly();
    private IReadOnlyList<Range>? _readOnlyRanges;
    private readonly List<Range> _ranges = new();

    private void GetTouchingRangeIndices(Range range, out int start, out int end)
    {
        int startIndex;
        for (startIndex = 0; startIndex < _ranges.Count; startIndex++)
        {
            if (range.IsTouching(_ranges[startIndex])) break;
        }
        int endIndex;
        for (endIndex = startIndex + 1; endIndex < _ranges.Count; endIndex++)
        {
            if (!range.IsTouching(_ranges[endIndex])) break;
        }
        start = startIndex;
        end = endIndex;
    }

    public void MarkRange(Range range)
    {
        MarkRange(range.Start, range.End);
    }

    public void MarkRange(uint start, uint end)
    {
        var affectedRange = new Range(start, end);
        if (affectedRange.Length == 0) return;

        GetTouchingRangeIndices(affectedRange, out var startIndex, out var endIndex);

        if (startIndex == _ranges.Count)
        {
            var insertionIndex = 0;
            for (; insertionIndex < _ranges.Count; insertionIndex++)
            {
                if (_ranges[insertionIndex].Start >= start) break;
            }
            _ranges.Insert(insertionIndex, affectedRange);
        }
        else
        {
            var expandedSegment = new Range(
                Math.Min(affectedRange.Start, _ranges[startIndex].Start),
                Math.Max(affectedRange.End, _ranges[endIndex - 1].End));
            _ranges[startIndex] = expandedSegment;
            _ranges.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
        }
    }

    public void ClearRange(Range range)
    {
        ClearRange(range.Start, range.End);
    }

    public void ClearRange(uint start, uint end)
    {
        var affectedRange = new Range(start, end);
        if (affectedRange.Length == 0) return;

        GetTouchingRangeIndices(affectedRange, out var startIndex, out var endIndex);

        if (startIndex == _ranges.Count)
        {
            return;
        }
        else
        {
            var clippedRangeA = new Range(
                _ranges[startIndex].Start,
                Math.Max(Math.Min(_ranges[startIndex].End, affectedRange.Start), _ranges[startIndex].Start));
            var clippedRangeB = new Range(
                Math.Min(Math.Max(_ranges[endIndex - 1].Start, affectedRange.End), _ranges[endIndex - 1].End),
                _ranges[endIndex - 1].End);

            var removalStartIndex = startIndex;
            if (clippedRangeA.Length > 0)
            {
                _ranges[removalStartIndex++] = clippedRangeA;
            }

            if (clippedRangeB.Length > 0 && clippedRangeB != clippedRangeA)
            {
                if (removalStartIndex < endIndex)
                {
                    _ranges[removalStartIndex++] = clippedRangeB;
                }
                else
                {
                    _ranges.Insert(removalStartIndex, clippedRangeB);
                    return;
                }
            }

            var removalAmount = endIndex - removalStartIndex;
            if (removalAmount > 0)
            {
                _ranges.RemoveRange(removalStartIndex, removalAmount);
            }
        }
    }

    public IEnumerator<Range> GetEnumerator()
    {
        return _ranges.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}