using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ray1Editor;

public class PalettedTextureSheet : TextureSheet
{
    public PalettedTextureSheet(TextureManager manager, IList<Point?> dimensions, int sheetWidth = 1024) : base(manager, dimensions, sheetWidth)
    {
        PalettedTextureDatas = new PalettedTextureData[dimensions.Count];
    }

    public PalettedTextureSheet(TextureManager manager, Texture2D texture, ICollection<Rectangle> textures) : base(manager, texture, textures)
    {
        PalettedTextureDatas = new PalettedTextureData[textures.Count];
    }

    public PalettedTextureData[] PalettedTextureDatas { get; }

    public void InitEntry(int index, Palette palette, byte[] imgData, PalettedTextureData.ImageFormat format = PalettedTextureData.ImageFormat.Linear_8bpp, int imgDataStartIndex = 0, int imgDataLength = -1, int paletteOffset = 0)
    {
        PalettedTextureDatas[index] = new PalettedTextureData(Sheet, imgData, Entries[index].Source, format, palette, imgDataStartIndex, imgDataLength)
        {
            PaletteOffset = paletteOffset
        };
        PalettedTextureDatas[index].Apply();
    }

    public static PalettedTextureSheet FromTileSet(TextureManager manager, Texture2D texture2D, Point tileSize, int tilesCount)
    {
        var width = texture2D.Width / tileSize.X;
        var height = texture2D.Height / tileSize.Y;

        var tiles = new Rectangle[tilesCount];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var index = y * width + x;

                if (index >= tilesCount)
                    break;

                tiles[index] = new Rectangle(new Point(x * tileSize.X, y * tileSize.Y), tileSize);
            }
        }

        return new PalettedTextureSheet(manager, texture2D, tiles);
    }
}