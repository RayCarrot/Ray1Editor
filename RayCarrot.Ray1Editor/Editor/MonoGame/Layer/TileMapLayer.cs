using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A tile map layer
    /// </summary>
    public class TileMapLayer : Layer
    {
        public TileMapLayer(MapTile[] tileMap, Point position, Point mapSize, TileSet tileSet)
        {
            TileMap = tileMap;
            Position = position;
            MapSize = mapSize;
            TileSet = tileSet;

            IsVisible = true;
        }

        public Point Position { get; }

        /// <summary>
        /// The map size, in tiles
        /// </summary>
        public Point MapSize { get; protected set; }

        // TODO: Either create common tile class to avoid using the R1 type or create TileMapLayer_R1 for R1 types
        public MapTile[] TileMap { get; protected set; }

        /// <summary>
        /// The tile set, containing the tile textures
        /// </summary>
        public TileSet TileSet { get; }

        public override string Name => $"Map";
        public override Rectangle Rectangle => new Rectangle(Position, MapSize * TileSet.TileSize);
        public override IEnumerable<EditorFieldViewModel> GetFields()
        {
            yield return new EditorPointFieldViewModel(
                header: "Map Size",
                info: "The size of the map in tiles",
                getValueAction: () => MapSize,
                setValueAction: UpdateMapSize,
                min: 1,
                max: Int16.MaxValue);
        }

        public void UpdateMapSize(Point newSize)
        {
            var newMap = new MapTile[newSize.X * newSize.Y];

            // Copy tiles which fit the new size
            for (int y = 0; y < newSize.Y; y++)
            {
                for (int x = 0; x < newSize.X; x++)
                {
                    if (x < MapSize.X && y < MapSize.Y)
                        newMap[y * newSize.X + x] = TileMap[y * MapSize.X + x];
                    else
                        newMap[y * newSize.X + x] = new MapTile();
                }
            }

            MapSize = newSize;
            TileMap = newMap;
            EditorState.UpdateMapSize(Data);
        }

        public override void Draw(SpriteBatch s)
        {
            for (int y = 0; y < MapSize.Y; y++)
            {
                for (int x = 0; x < MapSize.X; x++)
                {
                    var tile = TileMap[y * MapSize.X + x];

                    var dest = new Rectangle(
                        x: x * TileSet.TileSize.X + Position.X, 
                        y: y * TileSet.TileSize.Y + Position.Y, 
                        width: TileSet.TileSize.X, 
                        height: TileSet.TileSize.Y);
                    var src = TileSet.TileSheet.Entries[tile.TileMapY].Source;

                    s.Draw(TileSet.TileSheet.Sheet, dest, src, Color.White);
                }
            }
        }
    }
}