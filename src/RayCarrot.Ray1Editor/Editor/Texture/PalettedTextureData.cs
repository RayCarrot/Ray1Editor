using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace RayCarrot.Ray1Editor;

public class PalettedTextureData
{
    public PalettedTextureData(GraphicsDevice g, byte[] imgData, Point size, ImageFormat format, Palette palette, int imgDataStartIndex = 0, int imgDataLength = -1) : this(new Texture2D(g, size.X, size.Y), imgData, new Rectangle(Point.Zero, size), format, palette, imgDataStartIndex, imgDataLength)
    {
            
    }

    public PalettedTextureData(Texture2D texture, byte[] imgData, Rectangle rectangle, ImageFormat format, Palette palette, int imgDataStartIndex = 0, int imgDataLength = -1)
    {
        Texture = texture;
        ImgData = imgData;
        Rectangle = rectangle;
        Format = format;
        Palette = palette;
        ImgDataStartIndex = imgDataStartIndex;
        ImgDataLength = imgDataLength == -1 ? ImgData.Length : imgDataLength;
    }

    public Texture2D Texture { get; }
    public byte[] ImgData { get; }
    public int ImgDataStartIndex { get; }
    public int ImgDataLength { get; }
    public Rectangle Rectangle { get; }
    public ImageFormat Format { get; }
    public Palette Palette { get; set; }
    public int PaletteOffset { get; init; }

    public void Apply()
    {
        if (Format == ImageFormat.Linear_8bpp)
        {
            Texture.SetData<Color>(0, Rectangle, ImgData.Skip(ImgDataStartIndex).Take(ImgDataLength).Select(x => Palette.GetColor(PaletteOffset + x)).ToArray(), 0, ImgDataLength);
        }
        else if (Format == ImageFormat.Linear_4bpp)
        {
            var colors = new Color[Rectangle.Width * Rectangle.Height];

            for (int y = 0; y < Rectangle.Height; y++)
            {
                var colorsYOffset = y * Rectangle.Width;

                for (int x = 0; x < Rectangle.Width; x++)
                {
                    var b = ImgData[ImgDataStartIndex + (colorsYOffset + x) / 2];

                    b = (byte)BitHelpers.ExtractBits(b, 4, x % 2 == 0 ? 0 : 4);

                    colors[colorsYOffset + x] = Palette.GetColor(PaletteOffset + b);
                }
            }

            Texture.SetData<Color>(0, Rectangle, colors, 0, colors.Length);
        }
        else
        {
            throw new Exception($"Image format {Format} is not supported");
        }
    }

    public enum ImageFormat
    {
        Linear_4bpp,
        Linear_8bpp,
    }
}