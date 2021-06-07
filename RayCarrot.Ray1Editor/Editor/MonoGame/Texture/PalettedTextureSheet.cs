using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class PalettedTextureSheet : TextureSheet
    {
        public PalettedTextureSheet(TextureManager manager, IList<Point?> dimensions, int sheetWidth = 1024) : base(manager, dimensions, sheetWidth)
        {
            PalettedTextureDatas = new PalettedTextureData[dimensions.Count];
        }

        public PalettedTextureData[] PalettedTextureDatas { get; }

        public void InitEntry(int index, Palette palette, byte[] imgData, PalettedTextureData.ImageFormat format = PalettedTextureData.ImageFormat.BPP_8, int imgDataStartIndex = 0, int imgDataLength = -1)
        {
            PalettedTextureDatas[index] = new PalettedTextureData(Sheet, imgData, Entries[index].Source, format, palette, imgDataStartIndex, imgDataLength);
            PalettedTextureDatas[index].Apply();
        }
    }
}