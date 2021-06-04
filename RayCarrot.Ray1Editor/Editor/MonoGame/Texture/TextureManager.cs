using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    public class TextureManager : IDisposable
    {
        public TextureManager(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            Textures = new HashSet<Texture2D>();
        }

        protected HashSet<Texture2D> Textures { get; }
        public GraphicsDevice GraphicsDevice { get; }

        public Texture2D AddTexture(Texture2D tex)
        {
            Textures.Add(tex);
            return tex;
        }

        public TextureSheet AddTextureSheet(TextureSheet tex)
        {
            Textures.Add(tex.Sheet);
            return tex;
        }

        // TODO: Add functionality for palette swapping etc.

        public void Dispose()
        {
            foreach (var tex in Textures)
                tex?.Dispose();
        }
    }
}