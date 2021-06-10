using System.IO;
using System.Reflection;

namespace RayCarrot.Ray1Editor
{
    public static class Assets
    {
        public static Stream GetAsset(string path)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream($"{nameof(RayCarrot)}.{nameof(Ray1Editor)}.{path.Replace('\\', '.').Replace('/', '.')}");
        }
    }
}