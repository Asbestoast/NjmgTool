namespace NjmgLib;
public sealed class Tilemap
{
    public const int MaxWidth = byte.MaxValue;
    public const int MaxHeight = byte.MaxValue;

    public ushort LoadAddress { get; set; }
    public List<List<byte>> Rows { get; } = new();

    public byte[] ToBytes()
    {
        if (Rows.Count > MaxHeight)
            throw new InvalidOperationException("Maximum tilemap height exceeded.");
        if (Rows.Count > 0 && Rows[0].Count > MaxWidth)
            throw new InvalidOperationException("Maximum tilemap width exceeded.");

        var stream = new MemoryStream();
        using var w = new BinaryWriter(stream);

        w.Write((byte)Rows[0].Count); // Width
        w.Write((byte)Rows.Count); // Height
        foreach (var row in Rows)
        {
            foreach (var value in row)
            {
                w.Write(value);
            }
        }

        return stream.ToArray();
    }
}
