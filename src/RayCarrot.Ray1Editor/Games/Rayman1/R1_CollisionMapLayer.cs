using System;
using System.Linq;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class R1_CollisionMapLayer : CollisionMapLayer<MapTile>
    {
        public R1_CollisionMapLayer(MapTile[] tileMap, Point position, Point mapSize, TextureManager textureManager) : base(tileMap, position, mapSize, LoadTileSet(textureManager))
        { }

        public override bool CanBeResized => false;

        protected static TileSet LoadTileSet(TextureManager textureManager)
        {
            var tileSize = new Point(16, 16);
            var (w, h) = new Point(8, 4);
            var tiles = new Rectangle[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tiles[(h - y - 1) * w + x] = new Rectangle(new Point(x * tileSize.X, y * tileSize.Y), tileSize);
                }
            }

            var tex = new TextureSheet(textureManager, R1_GameManager.AssetPath_CollisionFile, tiles);

            return new TileSet(tex, tileSize);
        }

        protected override MapTile CreateNewTile() => new MapTile();

        protected override MapTile[] GetTileSetMap() => Enumerable.Range(0, TileSet.TileSheet.Entries.Length).Select(x => new MapTile()
        {
            BlockType = (ushort)x
        }).ToArray();
        protected override int GetTileSetMapWidth() => 8;

        protected override int GetTileSetIndex(MapTile tile) => tile.BlockType;

        protected override MapTile CloneTile(MapTile srcTile, MapTile destTile) => new MapTile
        {
            Pre_SNES_Is8PxTile = destTile.Pre_SNES_Is8PxTile,
            TileMapX = destTile.TileMapX,
            TileMapY = destTile.TileMapY,
            BlockType = srcTile.BlockType,
            HorizontalFlip = destTile.HorizontalFlip,
            VerticalFlip = destTile.VerticalFlip,
            PC_Byte_03 = destTile.PC_Byte_03,
            TransparencyMode = destTile.TransparencyMode,
            PC_Byte_05 = destTile.PC_Byte_05,
            PaletteIndex = destTile.PaletteIndex,
            Priority = destTile.Priority
        };
    }
}