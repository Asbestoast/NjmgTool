namespace NjmgLib;
public struct Range : IEquatable<Range>
{
    public uint Start { get; set; }
    public uint End { get; set; }
    public uint Length => End - Start;

    public Range(uint start, uint end)
    {
        if (end < start) throw new ArgumentException("Start must come before end.");
        Start = start;
        End = end;
    }

    public bool IsTouching(Range other)
    {
        return (Start >= other.Start && Start <= other.End) ||
               (End >= other.Start && End <= other.End) ||
               (other.Start >= Start && other.Start <= End) ||
               (other.End >= Start && other.End <= End);
    }

    public static bool operator ==(Range l, Range r)
    {
        return l.Equals(r);
    }

    public static bool operator !=(Range l, Range r)
    {
        return !(l == r);
    }

    public override string ToString()
    {
        if (Length == 0) return "(Empty)";
        return $"${Start:X}-{End - 1:X} (${Length:X} byte{(Length == 1 ? string.Empty : "s")})";
    }

    public bool Equals(Range other)
    {
        return Start == other.Start && End == other.End;
    }

    public override bool Equals(object? obj)
    {
        return obj is Range other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }
}