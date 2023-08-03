using System.Drawing;

namespace NjmgLib;
public sealed class ImagePlacement
{
    public string Source { get; set; } = string.Empty;
    public long Offset { get; set; }
    public List<Color> Palette { get; } = new();
    public Rectangle? Bounds { get; set; }
}
