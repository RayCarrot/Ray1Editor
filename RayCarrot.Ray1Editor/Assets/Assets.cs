using System.IO;
using System.Reflection;

namespace RayCarrot.Ray1Editor
{
    public static class Assets
    {
        public const string R1_TypeCollision = "Assets/Collision/R1_TypeCollision.png";

        public static Stream GetAsset(string path)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream($"{nameof(RayCarrot)}.{nameof(Ray1Editor)}.{path.Replace('\\', '.').Replace('/', '.')}");
        }
    }
}