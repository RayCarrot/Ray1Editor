﻿using Microsoft.Xna.Framework.Graphics;
using NLog;
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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected HashSet<Texture2D> Textures { get; }
        protected HashSet<PalettedTextureData> PalettedTextureDatas { get; }
        protected HashSet<TextureSheet> TextureSheets { get; }
        public GraphicsDevice GraphicsDevice { get; }

        protected IEnumerable<PalettedTextureData> EnumeratePalettedData()
        {
            foreach (var sheet in TextureSheets.OfType<PalettedTextureSheet>())
            {
                foreach (var tex in sheet.PalettedTextureDatas.Where(x => x != null))
                {
                    yield return tex;
                }
            }

            foreach (var tex in PalettedTextureDatas.Where(x => x != null))
                yield return tex;
        }

        public void AddTexture(Texture2D tex) => Textures.Add(tex);
        public void AddPalettedTexture(PalettedTextureData palData)
        {
            Textures.Add(palData.Texture);
            PalettedTextureDatas.Add(palData);
        }

        public void AddTextureSheet(TextureSheet sheet) => TextureSheets.Add(sheet);

        public void RefreshPalette(Palette pal)
        {
            foreach (var tex in EnumeratePalettedData().Where(x => x.Palette == pal))
                tex.Apply();

            Logger.Log(LogLevel.Info, "Refreshed palette {0}", pal.Name);
        }

        public void SwapPalettes(Palette oldPal, Palette newPal)
        {
            foreach (var tex in EnumeratePalettedData().Where(x => x.Palette == oldPal))
            {
                tex.Palette = newPal;
                tex.Apply();
            }

            Logger.Log(LogLevel.Info, "Swapped palette {0} with {0}", oldPal.Name, newPal.Name);
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