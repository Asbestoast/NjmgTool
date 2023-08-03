namespace NjmgLib;
public struct GbPointer
{
    public const int Rom0Start = 0;
    public const int Rom0End = 0x4000;
    public const int RomXStart = 0x4000;
    public const int RomXEnd = 0x8000;
    public const int RomStart = 0;
    public const int RomEnd = 0x8000;
    private const int RomBankSizeBits = 14;
    public const int RomBankSize = 1 << RomBankSizeBits;

    public const ushort CommonRomBank = 0;

    public ushort Bank { get; set; }
    public ushort Address { get; set; }

    public GbPointer(ushort bank, ushort address)
    {
        Bank = bank;
        Address = address;
    }

    public uint ToAbsoluteRomAddress()
    {
        if (Address <= 0x3FFF)
            return Address;
        else if (Address <= 0x7FFF)
            return (uint)(Address - 0x4000 + (Bank << RomBankSizeBits));
        else
            throw new InvalidOperationException("Operation is only valid on a ROM pointer.");
    }

    public static GbPointer FromAbsoluteRomAddress(uint romOffset)
    {
        var bank = (ushort)(romOffset >> RomBankSizeBits);
        var address = (ushort)((romOffset & ((1 << RomBankSizeBits) - 1)) + (bank > 0 ? RomXStart : 0));
        return new GbPointer(bank, address);
    }

    public override string ToString()
    {
        return $"{Bank:X}:{Address:X4}";
    }
}