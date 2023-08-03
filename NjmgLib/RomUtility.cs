using System.Text;

namespace NjmgLib;
internal static class RomUtility
{
    private static RomSize FileSizeToRomSize(long fileSize)
    {
        return fileSize switch
        {
            < 0 or > 8 * 1024 * 1024 => throw new ArgumentOutOfRangeException(nameof(fileSize)),
            > 4 * 1024 * 1024 => RomSize.SIZE_8MByte,
            > 2 * 1024 * 1024 => RomSize.SIZE_4MByte,
            > 1 * 1024 * 1024 => RomSize.SIZE_2MByte,
            > 512 * 1024 => RomSize.SIZE_1MByte,
            > 256 * 1024 => RomSize.SIZE_512KByte,
            > 128 * 1024 => RomSize.SIZE_256KByte,
            > 64 * 1024 => RomSize.SIZE_128KByte,
            > 32 * 1024 => RomSize.SIZE_64KByte,
            _ => RomSize.SIZE_32KByte
        };
    }

    public static void FixRomHeader(Stream stream)
    {
        FixRomSize(stream);
        RecalculateHeaderChecksum(stream);
        RecalculateCartridgeGlobalChecksum(stream);
    }

    private static void FixRomSize(Stream stream)
    {
        const int romSizeLocation = 0x148;
        var romSize = FileSizeToRomSize(stream.Length);
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);
        stream.Position = romSizeLocation;
        w.Write((byte)romSize);
    }

    private static void RecalculateCartridgeGlobalChecksum(Stream stream)
    {
        const int checksumLocation = 0x14E;

        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);

        ushort checksum = 0;
        r.BaseStream.Position = 0;
        while (r.BaseStream.Position < w.BaseStream.Length)
        {
            if (r.BaseStream.Position == checksumLocation)
            {
                r.BaseStream.Position += sizeof(ushort);
            }
            else
            {
                checksum += r.ReadByte();
            }
        }

        r.BaseStream.Position = checksumLocation;
        w.Write((byte)(checksum >> 8));
        w.Write((byte)checksum);
    }

    private static void RecalculateHeaderChecksum(Stream stream)
    {
        const int checksumLocation = 0x14D;

        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);

        byte checksum = 0;

        stream.Position = 0x134;
        while (stream.Position <= 0x14C)
        {
            checksum -= r.ReadByte();
            checksum--;
        }

        stream.Position = checksumLocation;
        w.Write(checksum);
    }
}
