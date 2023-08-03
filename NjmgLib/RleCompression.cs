using System.Text;

namespace NjmgLib;
public static class RleCompression
{
    private const byte RunLengthMarker = 0xFF;
    private const int SizeOfRunLengthCommand = 3;
    private const int MaximumRunLength = 0x100;

    public static void Compress(byte[] src, Stream dst)
    {
        using var w = new BinaryWriter(dst, Encoding.ASCII, true);
        for (var i = 0; i < src.Length;)
        {
            var b = src[i++];

            var length = 1;
            for (; i < src.Length; i++)
            {
                if (src[i] != b) break;
                if (length >= MaximumRunLength) break;
                length++;
            }

            if (length > SizeOfRunLengthCommand || b == RunLengthMarker)
            {
                w.Write(RunLengthMarker);
                w.Write(b);
                w.Write((byte)length);
            }
            else
            {
                for (var j = 0; j < length; j++)
                {
                    w.Write(b);
                }
            }
        }
    }

    public static void Decompress(Stream src, byte[] dst)
    {
        using var r = new BinaryReader(src, Encoding.ASCII, true);
        for (var i = 0; i < dst.Length; i++)
        {
            var b = r.ReadByte();
            if (b == RunLengthMarker)
            {
                var value = r.ReadByte();
                var length = r.ReadByte();
                for (; i < dst.Length; i++)
                {
                    dst[i] = value;
                    length--;
                    if (length == 0) break;
                }

                if (length > 0)
                {
                    Console.WriteLine("Warning: Decompressed data extends beyond end of buffer.");
                }
            }
            else
            {
                dst[i] = b;
            }
        }
    }
}
