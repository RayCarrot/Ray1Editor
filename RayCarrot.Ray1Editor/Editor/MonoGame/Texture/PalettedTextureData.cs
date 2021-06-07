using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
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

        public void Apply()
        {
            if (Format == ImageFormat.BPP_8)
            {
                Texture.SetData<Color>(0, Rectangle, ImgData.Select(x => Palette.GetColor(x)).ToArray(), ImgDataStartIndex, ImgDataLength);
            }
            else
            {
                throw new Exception($"Image format {Format} is not supported");
            }
        }

        public enum ImageFormat
        {
            BPP_8,
        }
    }
}