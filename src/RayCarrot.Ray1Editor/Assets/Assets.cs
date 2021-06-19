using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    public static class Assets
    {
        public static Stream GetAsset(string path)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream($"RayCarrot.Ray1Editor.{path.Replace('\\', '.').Replace('/', '.')}");
        }

        public static Texture2D GetTextureAsset(string path, TextureManager textureManager)
        {
            using var stream = GetAsset(path);
            var tex = Texture2D.FromStream(textureManager.GraphicsDevice, stream);
            textureManager.AddTexture(tex);
            return tex;
        }
    }
}