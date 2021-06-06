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
        protected override MapTile CloneTile(MapTile tile) => new MapTile
        {
            Pre_SNES_Is8PxTile = tile.Pre_SNES_Is8PxTile,
            TileMapX = tile.TileMapX,
            TileMapY = tile.TileMapY,
            BlockType = tile.BlockType,
            HorizontalFlip = tile.HorizontalFlip,
            VerticalFlip = tile.VerticalFlip,
            PC_Byte_03 = tile.PC_Byte_03,
            TransparencyMode = tile.TransparencyMode,
            PC_Byte_05 = tile.PC_Byte_05,
            PaletteIndex = tile.PaletteIndex,
            Priority = tile.Priority
        };
    }
}