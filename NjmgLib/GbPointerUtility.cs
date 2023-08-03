using System.Diagnostics.Contracts;
using System.Text;

namespace NjmgLib;
internal static class GbPointerUtility
{
    [Pure]
    public static GbPointer GetArrayItemPointer(GbPointer array, int index, uint size)
    {
        return new GbPointer(array.Bank, checked((ushort)(array.Address + size * index)));
    }

    public static GbPointer GetPointerTableItem(GbPointer table, int index, Stream stream)
    {
        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        r.BaseStream.Position = GetArrayItemPointer(table, index, sizeof(ushort)).ToAbsoluteRomAddress();
        return new GbPointer(table.Bank, r.ReadUInt16());
    }
}
