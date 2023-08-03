using System.Text;

namespace NjmgLib;
public sealed class MenuItem
{
    public const uint SizeOf = 8;

    public byte Left { get; set; }
    public byte Up { get; set; }
    public byte Right { get; set; }
    public byte Down { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Character { get; set; }
    public byte Orientation { get; set; }

    public MenuItem()
    {
    }

    public MenuItem(Stream stream)
    {
        using var r = new BinaryReader(stream, Encoding.ASCII, true);
        Left = r.ReadByte();
        Up = r.ReadByte();
        Right = r.ReadByte();
        Down = r.ReadByte();
        X = r.ReadByte();
        Y = r.ReadByte();
        Character = r.ReadByte();
        Orientation = r.ReadByte();
    }

    public void Write(Stream stream)
    {
        using var w = new BinaryWriter(stream, Encoding.ASCII, true);
        w.Write(Left);
        w.Write(Up);
        w.Write(Right);
        w.Write(Down);
        w.Write(X);
        w.Write(Y);
        w.Write(Character);
        w.Write(Orientation);
    }
}
