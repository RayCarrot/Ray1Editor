using System;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class CollisionMapLayer_R1 : CollisionMapLayer<MapTile>
    {
        public CollisionMapLayer_R1(MapTile[] tileMap, Point position, Point mapSize, TextureManager textureManager) : base(tileMap, position, mapSize, LoadTileSet(textureManager))
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

            var tex = new TextureSheet(textureManager.GraphicsDevice, Assets.R1_TypeCollision, tiles).AddToManager(textureManager);

            return new TileSet(tex, tileSize);
        }

        protected override MapTile CreateNewTile() => throw new Exception("A collision tile can't be created from the collision map layer");

        protected override int GetTileSetIndex(MapTile tile) => tile.BlockType;

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