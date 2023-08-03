using System.Diagnostics;
using NjmgLib;

namespace Tests;
internal static class RangeMapTest
{
    public static void Run()
    {
        {
            // A range map should be empty by default.
            var map = new RangeMap();
            Trace.Assert(map.Ranges.Count == 0);
            ValidateRangeMap(map);
        }

        TestMarkRange();
        TestClearRange();
    }

    private static void TestMarkRange()
    {
        {
            // Creating an empty range should result in no change.
            var map = new RangeMap();
            map.MarkRange(0, 0);
            Trace.Assert(map.Ranges.Count == 0);
            ValidateRangeMap(map);
        }

        {
            // Create a single range.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[0].Length == 100);
            ValidateRangeMap(map);
        }

        {
            // Combine two range that are touching (no overlap)
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(100, 200);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 200);
            Trace.Assert(map.Ranges[0].Length == 200);
            ValidateRangeMap(map);
        }

        {
            // Creating two separate (non-touching, non-overlapping) segments.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            Trace.Assert(map.Ranges.Count == 2);
            ValidateRangeMap(map);
        }

        {
            // Combine two segments into one.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(50, 250);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 300);
            Trace.Assert(map.Ranges[0].Length == 300);
            ValidateRangeMap(map);
        }

        {
            // Combine three segments into one.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.MarkRange(50, 450);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 500);
            Trace.Assert(map.Ranges[0].Length == 500);
            ValidateRangeMap(map);
        }

        {
            // Create three segments. Combine two of them into one.
            // Leave an isolated range before the ones that were combined.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.MarkRange(250, 450);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[0].Length == 100);
            Trace.Assert(map.Ranges[1].Start == 200);
            Trace.Assert(map.Ranges[1].End == 500);
            Trace.Assert(map.Ranges[1].Length == 300);
            ValidateRangeMap(map);
        }

        {
            // Create three segments. Combine two of them into one.
            // Leave an isolated range after the ones that were combined.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.MarkRange(50, 250);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 300);
            Trace.Assert(map.Ranges[0].Length == 300);
            Trace.Assert(map.Ranges[1].Start == 400);
            Trace.Assert(map.Ranges[1].End == 500);
            Trace.Assert(map.Ranges[1].Length == 100);
            ValidateRangeMap(map);
        }

        {
            // Create four segments. Combine three of them into one.
            // Leave an isolated range before the ones that were combined.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.MarkRange(600, 700);
            map.MarkRange(250, 650);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[0].Length == 100);
            Trace.Assert(map.Ranges[1].Start == 200);
            Trace.Assert(map.Ranges[1].End == 700);
            Trace.Assert(map.Ranges[1].Length == 500);
            ValidateRangeMap(map);
        }

        {
            // Create four segments. Combine three of them into one.
            // Leave an isolated range after the ones that were combined.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.MarkRange(600, 700);
            map.MarkRange(50, 450);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 500);
            Trace.Assert(map.Ranges[0].Length == 500);
            Trace.Assert(map.Ranges[1].Start == 600);
            Trace.Assert(map.Ranges[1].End == 700);
            Trace.Assert(map.Ranges[1].Length == 100);
            ValidateRangeMap(map);
        }

        {
            // Ensure that ranges are still ordered in this particular case.
            var map = new RangeMap();
            map.MarkRange(200, 300);
            map.MarkRange(0, 100);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[0].Length == 100);
            Trace.Assert(map.Ranges[1].Start == 200);
            Trace.Assert(map.Ranges[1].End == 300);
            Trace.Assert(map.Ranges[1].Length == 100);
            ValidateRangeMap(map);
        }
    }

    private static void TestClearRange()
    {
        {
            // Clearing an empty range.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.ClearRange(50, 50);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            ValidateRangeMap(map);
        }

        {
            // Clearing an entire section (exact fit).
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.ClearRange(0, 100);
            Trace.Assert(map.Ranges.Count == 0);
            ValidateRangeMap(map);
        }

        {
            // Clearing an entire section (overhanging).
            var map = new RangeMap();
            map.MarkRange(100, 200);
            map.ClearRange(0, 300);
            Trace.Assert(map.Ranges.Count == 0);
            ValidateRangeMap(map);
        }

        {
            // Splitting a single section.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.ClearRange(25, 75);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 25);
            Trace.Assert(map.Ranges[1].Start == 75);
            Trace.Assert(map.Ranges[1].End == 100);
            ValidateRangeMap(map);
        }

        {
            // Clearing an empty region that is touching two marked regions.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.ClearRange(100, 200);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[1].Start == 200);
            Trace.Assert(map.Ranges[1].End == 300);
            ValidateRangeMap(map);
        }

        {
            // Clearing a region that is slightly overlapping with a marked region.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.ClearRange(99, 200);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 99);
            Trace.Assert(map.Ranges[1].Start == 200);
            Trace.Assert(map.Ranges[1].End == 300);
            ValidateRangeMap(map);
        }

        {
            // Clearing a region that is slightly overlapping with a marked region.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.ClearRange(100, 201);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[1].Start == 201);
            Trace.Assert(map.Ranges[1].End == 300);
            ValidateRangeMap(map);
        }

        {
            // Clearing a region that is slightly overlapping with two marked regions.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.ClearRange(99, 201);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 99);
            Trace.Assert(map.Ranges[1].Start == 201);
            Trace.Assert(map.Ranges[1].End == 300);
            ValidateRangeMap(map);
        }

        {
            // Removing multiple ranges at once.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.ClearRange(0, 300);
            Trace.Assert(map.Ranges.Count == 0);
            ValidateRangeMap(map);
        }

        {
            // Removing multiple ranges at once with an untouched section at the start.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.ClearRange(200, 500);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            ValidateRangeMap(map);
        }

        {
            // Removing multiple ranges at once with an untouched section at the end.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.ClearRange(0, 300);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 400);
            Trace.Assert(map.Ranges[0].End == 500);
            ValidateRangeMap(map);
        }

        {
            // Removing multiple ranges at once with untouched ranges at the start and end.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.MarkRange(400, 500);
            map.MarkRange(600, 700);
            map.ClearRange(200, 500);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[1].Start == 600);
            Trace.Assert(map.Ranges[1].End == 700);
            ValidateRangeMap(map);
        }

        {
            // Clipping off the beginning of a range.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.ClearRange(0, 10);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 10);
            Trace.Assert(map.Ranges[0].End == 100);
            ValidateRangeMap(map);
        }

        {
            // Clip beginning of range.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.ClearRange(0, 10);
            Trace.Assert(map.Ranges.Count == 1);
            Trace.Assert(map.Ranges[0].Start == 10);
            Trace.Assert(map.Ranges[0].End == 100);
            ValidateRangeMap(map);
        }

        {
            // Clip middle of range.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.ClearRange(1, 10);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 1);
            Trace.Assert(map.Ranges[1].Start == 10);
            Trace.Assert(map.Ranges[1].End == 100);
            ValidateRangeMap(map);
        }

        {
            // Clip beginning of range with unconnected range afterward.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.ClearRange(0, 10);
            Trace.Assert(map.Ranges.Count == 2);
            Trace.Assert(map.Ranges[0].Start == 10);
            Trace.Assert(map.Ranges[0].End == 100);
            Trace.Assert(map.Ranges[1].Start == 200);
            Trace.Assert(map.Ranges[1].End == 300);
            ValidateRangeMap(map);
        }

        {
            // Clip middle of range with unconnected range afterward.
            var map = new RangeMap();
            map.MarkRange(0, 100);
            map.MarkRange(200, 300);
            map.ClearRange(1, 10);
            Trace.Assert(map.Ranges.Count == 3);
            Trace.Assert(map.Ranges[0].Start == 0);
            Trace.Assert(map.Ranges[0].End == 1);
            Trace.Assert(map.Ranges[1].Start == 10);
            Trace.Assert(map.Ranges[1].End == 100);
            Trace.Assert(map.Ranges[2].Start == 200);
            Trace.Assert(map.Ranges[2].End == 300);
            ValidateRangeMap(map);
        }
    }

    private static void ValidateRangeMap(RangeMap map)
    {
        EnsureNoRangesTouch(map);
        EnsureRangesAreOrdered(map);
        EnsureRangesAreNotEmpty(map);
    }

    private static void EnsureRangesAreOrdered(RangeMap map)
    {
        long? previousEnd = null;
        foreach (var range in map)
        {
            Trace.Assert(!previousEnd.HasValue || range.Start > previousEnd);
            previousEnd = range.End;
        }
    }
    private static void EnsureRangesAreNotEmpty(RangeMap map)
    {
        foreach (var range in map)
        {
            Trace.Assert(range.Length > 0);
        }
    }

    private static void EnsureNoRangesTouch(RangeMap map)
    {
        for (var i = 0; i < map.Ranges.Count; i++)
        {
            for (var j = 0; j < map.Ranges.Count; j++)
            {
                if (i == j) continue;
                Trace.Assert(!map.Ranges[i].IsTouching(map.Ranges[j]));
            }
        }
    }
}
