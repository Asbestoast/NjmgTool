namespace NjmgLib;
public sealed class TilemapInfo
{
    public GbPointer SourcePointerReference { get; set; }
    public GbPointer DestinationPointerReference { get; set; }

    public TilemapInfo(
        GbPointer sourcePointerReference,
        GbPointer destinationPointerReference)
    {
        SourcePointerReference = sourcePointerReference;
        DestinationPointerReference = destinationPointerReference;
    }
}