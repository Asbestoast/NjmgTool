using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NjmgLib.Graphics;
internal static class ImageUtility
{
    public static IndexedImage FromFile(string path, IReadOnlyList<Color> palette)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException("Unsupported operating system.");

        using var image = new Bitmap(path);
        var w = image.Width;
        var h = image.Height;
        var bitmapData = image.LockBits(Rectangle.FromLTRB(0, 0, w, h),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        if (bitmapData.Stride % sizeof(int) != 0)
            throw new NotSupportedException("Invalid stride.");
        var intStride = bitmapData.Stride / sizeof(int);
        var pixels = new int[intStride * h];
        Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);
        image.UnlockBits(bitmapData);

        var indexedImage = new IndexedImage(w, h);
        var argbPalette = palette.Select(i => i.ToArgb()).ToArray();

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var argb = pixels[y * intStride + x];

                var foundMatch = false;

                for (var i = 0; i < argbPalette.Length; i++)
                {
                    if (argbPalette[i] != argb) continue;
                    foundMatch = true;
                    indexedImage.SetPixel(x, y, (byte)i);
                    break;
                }

                if (!foundMatch)
                    throw new IOException($"No palette entry for color ${argb:X8}.");
            }
        }

        return indexedImage;
    }

    public static IEnumerable<IndexedImage> BreakIntoTiles(IndexedImage image, Size tileSize)
    {
        if (image.Width % tileSize.Width != 0)
        {
            throw new ArgumentException($"Image height must be a multiple of {tileSize.Width}.", nameof(image));
        }

        if (image.Height % tileSize.Height != 0)
        {
            throw new ArgumentException($"Image height must be a multiple of {tileSize.Height}.", nameof(image));
        }

        var widthTiles = image.Width / tileSize.Width;
        var heightTiles = image.Height / tileSize.Height;

        for (var yT = 0; yT < heightTiles; yT++)
        {
            for (var xT = 0; xT < widthTiles; xT++)
            {
                var x = xT * tileSize.Width;
                var y = yT * tileSize.Height;
                var subimage = image.GetSubimage(
                    new Rectangle(x, y, tileSize.Width, tileSize.Height));
                yield return subimage;
            }
        }
    }
}