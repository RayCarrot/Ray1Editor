using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using System;

namespace Ray1Editor;

public class R1_TileMapLayer : TileMapLayer<MapTile>
{
    public R1_TileMapLayer(MapTile[] tileMap, Point position, Point mapSize, TileSet tileSet, int tileSetWidth = 1) : base(tileMap, position, mapSize, tileSet)
    {
        TileSetWidth = tileSetWidth;
    }

    public int TileSetWidth { get; }

    protected override MapTile CreateNewTile() => new MapTile();
    protected override MapTile[] GetTileSetMap()
    {
        var tilesCount = TileSet.TileSheet.Entries.Length;
        var width = TileSetWidth;
        var height = (int)Math.Ceiling(TileSet.TileSheet.Entries.Length / (float)width);

        var tiles = new MapTile[tilesCount];

        for (ushort y = 0; y < height; y++)
        {
            for (ushort x = 0; x < width; x++)
            {
                var index = y * width + x;

                if (index >= tilesCount)
                    break;

                var tile = new MapTile()
                {
                    TileMapY = y,
                    TileMapX = x,
                };

                if (Data is R1_PC_GameData pcData)
                    tile.PC_RuntimeTransparencyMode = pcData.PC_TileSetTransparencyModes[y]; // PC version is 1-dimensional

                tiles[index] = tile;
            }
        }

        return tiles;
    }

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
        PC_RuntimeTransparencyMode = srcTile.PC_RuntimeTransparencyMode,
        PC_Byte_05 = srcTile.PC_Byte_05,
        PaletteIndex = srcTile.PaletteIndex,
        Priority = srcTile.Priority
    };
}