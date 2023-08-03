namespace NjmgLib;
public sealed class Script
{
    public bool UseExpandedRom { get; set; }
    public List<ScriptTarget> Targets { get; } = new();
    public Dictionary<int, DialogueData> Dialogues { get; } = new();
    public Dictionary<int, WordData> Words { get; } = new();
    public List<ImagePlacement> Images { get; } = new();
    public Dictionary<int, Tilemap> Tilemaps { get; } = new();
    public Dictionary<int, BackgroundTilemap> BackgroundTilemaps { get; } = new();
    public Dictionary<int, Menu> Menus { get; } = new();
    public List<Patch> Patches { get; } = new();
}
