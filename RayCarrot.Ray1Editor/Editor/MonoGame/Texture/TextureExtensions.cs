using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    public static class TextureExtensions
    {
        public static Texture2D AddToManager(this Texture2D tex, TextureManager manager) => manager.AddTexture(tex);
        public static TextureSheet AddToManager(this TextureSheet tex, TextureManager manager) => manager.AddTextureSheet(tex);
    }
}