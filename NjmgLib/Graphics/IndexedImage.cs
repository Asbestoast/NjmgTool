using System.Drawing;

namespace NjmgLib.Graphics;
sealed class IndexedImage
{
    public byte[] Pixels { get; }
    public int Offset { get; }
    public int Stride { get; }

    public int Width { get; }
    public int Height { get; }

    public Rectangle Bounds => new(0, 0, Width, Height);

    public byte GetPixel(int x, int y)
    {
        CheckContainsPoint(x, y);
        return Pixels[Offset + x + y * Stride];
    }

    public void SetPixel(int x, int y, byte value)
    {
        CheckContainsPoint(x, y);
        Pixels[Offset + x + y * Stride] = value;
    }

    public IndexedImage(int width, int height, int stride)
    {
        if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (stride < 0) throw new ArgumentOutOfRangeException(nameof(stride));
        Width = width;
        Height = height;
        Stride = stride;
        Pixels = new byte[height * stride];
    }

    public IndexedImage(int width, int height, byte[] pixels, int offset, int stride)
    {
        if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (stride < 0) throw new ArgumentOutOfRangeException(nameof(stride));
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        Width = width;
        Height = height;
        Pixels = pixels;
        Offset = offset;
        Stride = stride;
    }

    public IndexedImage(int width, int height) : this(width, height, width)
    {
    }

    /// <summary>
    /// Gets a subimage that shares the same underlying pixel buffer.
    /// </summary>
    /// <param name="subimageBounds"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IndexedImage GetSubimage(Rectangle subimageBounds)
    {
        if (!Bounds.Contains(subimageBounds))
            throw new ArgumentException("Subimage must be contained within the source image.");
        return new IndexedImage(
            subimageBounds.Width,
            subimageBounds.Height,
            Pixels,
            Offset + subimageBounds.X + subimageBounds.Y * Stride,
            Stride);
    }

    private void CheckContainsPoint(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            throw new ArgumentException("The image does not contain the specified point.");
        }
    }
}