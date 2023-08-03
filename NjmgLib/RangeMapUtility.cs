namespace NjmgLib;
internal static class RangeMapUtility
{
    public static uint AllocateSpace(RangeMap map, uint size)
    {
        if (!TryAllocateSpace(map, size, out var address))
        {
            throw new ArgumentException("Allocation failed: No suitable space available", nameof(map));
        }
        return address;
    }

    public static bool TryAllocateSpace(RangeMap map, uint size, out uint address)
    {
        foreach (var range in map)
        {
            if (range.Length < size) continue;
            address = range.Start;
            map.ClearRange(range.Start, range.Start + size);
            return true;
        }
        address = default;
        return false;
    }

    private static bool TryAllocateSpace_internal(
        IReadOnlyDictionary<ushort, RangeMap> freeSpace, uint size, ushort bank, out GbPointer allocatedPointer)
    {
        if (!freeSpace.TryGetValue(bank, out var map)) goto fail;
        if (!TryAllocateSpace(map, size, out var address)) goto fail;
        allocatedPointer = new GbPointer(bank, checked((ushort)address));
        return true;
        fail:
        allocatedPointer = default;
        return false;
    }

    public static bool TryAllocateSpace(
        IReadOnlyDictionary<ushort, RangeMap> freeSpace, uint size, ushort bank, out GbPointer allocatedPointer)
    {
        if (TryAllocateSpace_internal(freeSpace, size, bank, out allocatedPointer))
            return true;
        if (bank == GbPointer.CommonRomBank) return false;
        return TryAllocateSpace_internal(freeSpace, size, GbPointer.CommonRomBank, out allocatedPointer);
    }

    public static void PrintRangeMap(RangeMap map, bool showGaps = false, string indent = "")
    {
        Range? previousSegment = null;
        foreach (var segment in map.Ranges.OrderBy(i => i.Start))
        {
            if (showGaps && previousSegment != null)
            {
                if (segment.Start != previousSegment.Value.End)
                {
                    var gapSegment = new Range(previousSegment.Value.End, segment.Start);
                    Console.WriteLine($"{indent}*GAP* {gapSegment} *GAP*");
                }
            }

            Console.WriteLine($"{indent}SEGMT {segment}");
            previousSegment = segment;
        }

        Console.WriteLine($"{indent}Total bytes: ${map.Ranges.Select(i => i.Length).Sum():X}");
    }
}
