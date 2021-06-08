using System.Linq;
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
        protected override MapTile[] GetTileSetMap() => Enumerable.Range(0, TileSet.TileSheet.Entries.Length).Select(x => new MapTile()
        {
            // TODO: Update for 2D tile-sets
            // TODO: Set PC transparency
            TileMapY = (ushort)x
        }).ToArray();
        protected override int GetTileSetMapWidth() => TileSetWidth == 1 ? 24 : TileSetWidth;
        protected override int GetTileSetIndex(MapTile tile) => tile.TileMapY * TileSetWidth + tile.TileMapX;
        protected override MapTile CloneTile(MapTile srcTile, MapTile destTile) => new MapTile
        {
            Pre_SNES_Is8PxTile = srcTile.Pre_SNES_Is8PxTile,
            TileMapX = srcTile.TileMapX,
            TileMapY = srcTile.TileMapY,
            BlockType = srcTile.BlockType,
            HorizontalFlip = srcTile.HorizontalFlip,
            VerticalFlip = srcTile.VerticalFlip,
            PC_Byte_03 = srcTile.PC_Byte_03,
            TransparencyMode = srcTile.TransparencyMode,
            PC_Byte_05 = srcTile.PC_Byte_05,
            PaletteIndex = srcTile.PaletteIndex,
            Priority = srcTile.Priority
        };
    }
}