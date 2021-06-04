using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A tile set
    /// </summary>
    public class TileSet
    {
        public TileSet(TextureSheet tileSheet, Point tileSize)
        {
            TileSheet = tileSheet;
            TileSize = tileSize;
        }

        public TextureSheet TileSheet { get; }
        public Point TileSize { get; }
    }
}