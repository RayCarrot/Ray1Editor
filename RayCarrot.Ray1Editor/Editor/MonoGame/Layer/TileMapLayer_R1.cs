using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class TileMapLayer_R1 : TileMapLayer<MapTile>
    {
        public TileMapLayer_R1(MapTile[] tileMap, Point position, Point mapSize, TileSet tileSet, int tileSetWidth = 1) : base(tileMap, position, mapSize, tileSet)
        {
            TileSetWidth = tileSetWidth;
        }

        public int TileSetWidth { get; }

        protected override MapTile CreateNewTile() => new MapTile();
        protected override int GetTileSetIndex(MapTile tile) => tile.TileMapY * TileSetWidth + tile.TileMapX;
    }
}