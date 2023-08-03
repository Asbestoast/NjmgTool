namespace NjmgLib;
public sealed class Reference
{
    public uint Address { get; set; }
    public ReferenceType Type { get; set; }

    public Reference(uint address, ReferenceType type)
    {
        Address = address;
        Type = type;
    }
}
