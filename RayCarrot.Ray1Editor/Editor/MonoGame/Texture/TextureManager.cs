using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.Ray1Editor
{
    public class TextureManager : IDisposable
    {
        public TextureManager(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            Textures = new HashSet<Texture2D>();
            PalettedTextureDatas = new HashSet<PalettedTextureData>();
            TextureSheets = new HashSet<TextureSheet>();
        }

        protected HashSet<Texture2D> Textures { get; }
        protected HashSet<PalettedTextureData> PalettedTextureDatas { get; }
        protected HashSet<TextureSheet> TextureSheets { get; }
        public GraphicsDevice GraphicsDevice { get; }

        public void AddTexture(Texture2D tex) => Textures.Add(tex);
        public void AddPalettedTexture(PalettedTextureData palData)
        {
            Textures.Add(palData.Texture);
            PalettedTextureDatas.Add(palData);
        }

        public void AddTextureSheet(TextureSheet sheet) => TextureSheets.Add(sheet);

        public void RefreshPalette(Palette pal)
        {
            foreach (var sheet in TextureSheets.OfType<PalettedTextureSheet>())
            {
                foreach (var tex in sheet.PalettedTextureDatas.Where(x => x != null && x.Palette == pal))
                {
                    tex.Apply();
                }
            }

            foreach (var tex in PalettedTextureDatas)
                tex.Apply();
        }

        public void Dispose()
        {
            foreach (var tex in Textures)
                tex?.Dispose();

            foreach (var tex in TextureSheets)
                tex?.Dispose();
        }
    }
}