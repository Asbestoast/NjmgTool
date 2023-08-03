using System.Text;

namespace NjmgLib;
public sealed class BackgroundTilemap
{
    public BackgroundTilemapFormat Format { get; } = BackgroundTilemapFormat.Tilemap20x18_9800;
    public int Width { get; } = 20;
    public int Height { get; } = 18;
    public List<List<byte>> Rows { get; } = new();

    public static BackgroundTilemap FromStream(Stream stream)
    {
        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        var result = new BackgroundTilemap();
        var format = (BackgroundTilemapFormat)r.ReadByte();
        if (format != result.Format)
            throw new NotSupportedException($"Background tilemap format not supported (format = '{format}'.");
        var buffer = new byte[result.Width * result.Height];
        RleCompression.Decompress(stream,  buffer);
        for (var y = 0; y < result.Height; y++)
        {
            var row = buffer.Skip(y * result.Width).Take(result.Width).ToList();
            result.Rows.Add(row);
        }
        return result;
    }

    public byte[] ToBytes()
    {
        if (Rows.Count > Height)
            throw new InvalidOperationException("Maximum tilemap height exceeded.");
        if (Rows.Count > 0 && Rows[0].Count > Width)
            throw new InvalidOperationException("Maximum tilemap width exceeded.");

        var stream = new MemoryStream();
        using var w = new BinaryWriter(stream);

        w.Write((byte)Format); // Format
        if (Format == BackgroundTilemapFormat.Tilemap20x18_9800 ||
            Format == BackgroundTilemapFormat.Tilemap20x18_9C00)
        {
            if (Rows.Any(i => i.Count != 20)) throw new IOException("Invalid map width.");
            if (Rows.Count != 18) throw new IOException("Invalid map height.");
            var bytes = Rows.SelectMany(row => row).ToArray();
            RleCompression.Compress(bytes, stream);
        }
        else
        {
            throw new NotSupportedException($"Unknown format '{Format}'.");
        }

        return stream.ToArray();
    }
}
