namespace NjmgLib;
internal class BackgroundTilemapInfo
{
    public GbPointer SourcePointerReference { get; set; }
    public List<string> CharmapSources { get; } = new();

    public BackgroundTilemapInfo(
        GbPointer sourcePointerReference,
        IEnumerable<string> charmapSources)
    {
        SourcePointerReference = sourcePointerReference;
        CharmapSources.AddRange(charmapSources);
    }
}
