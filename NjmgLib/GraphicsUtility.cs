using NjmgLib.Graphics;
using System.Text;

namespace NjmgLib;
internal static class GraphicsUtility
{
    public static void DrawTileToStream(IndexedImage tile, Stream stream)
    {
        if (tile.Width != Constants.TileSize.Width || tile.Height != Constants.TileSize.Height)
        {
            throw new NotSupportedException($"Tile must be {Constants.TileSize} pixels.");
        }

        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);

        for (var y = 0; y < Constants.TileSize.Height; y++)
        {
            byte alphaMask = 0;
            var bitplanes = new byte[2];
            for (var x = 0; x < Constants.TileSize.Width; x++)
            {
                alphaMask <<= 1;
                for (var i = 0; i < bitplanes.Length; i++)
                    bitplanes[i] = (byte)(bitplanes[i] << 1);
                var pixel = tile.GetPixel(x, y);
                if (pixel == 4) continue;
                alphaMask |= 1;
                for (var i = 0; i < bitplanes.Length; i++)
                    bitplanes[i] |= (byte)((pixel >> i) & 1);
            }

            if (alphaMask != 0)
            {
                for (var i = 0; i < bitplanes.Length; i++)
                {
                    var b = r.ReadByte() & ~alphaMask;
                    r.BaseStream.Position -= sizeof(byte);
                    b |= bitplanes[i] & alphaMask;
                    w.Write((byte)b);
                }
            }
            else
            {
                r.BaseStream.Position += bitplanes.Length;
            }
        }
    }
}
