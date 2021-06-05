using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    // TODO: Allow the layer to be resized. When this happens we need to re-arrange the tiles so to keep them aligned as best as possible. Other things in the editor, such as the max width and height, also need to be re-calculated.

    /// <summary>
    /// A tile map layer
    /// </summary>
    public class TileMapLayer : Layer
    {
        public TileMapLayer(MapTile[] tileMap, Point position, Point mapSize, TileSet tileSet)
        {
            TileMap = tileMap;
            MapSize = mapSize;
            TileSet = tileSet;
            Rectangle = new Rectangle(position, mapSize * TileSet.TileSize);

            IsVisible = true;
        }

        /// <summary>
        /// The map size, in tiles
        /// </summary>
        public Point MapSize { get; }

        // TODO: Either create common tile class to avoid using the R1 type or create TileMapLayer_R1 for R1 types
        public MapTile[] TileMap { get; }

        /// <summary>
        /// The tile set, containing the tile textures
        /// </summary>
        public TileSet TileSet { get; }

        public override string Name => $"Map";
        public override Rectangle Rectangle { get; }
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
            throw new NotImplementedException("Map resizing has not been implemented");
        }

        public override void Draw(SpriteBatch s)
        {
            for (int y = 0; y < MapSize.Y; y++)
            {
                for (int x = 0; x < MapSize.X; x++)
                {
                    var tile = TileMap[y * MapSize.X + x];

                    var dest = new Rectangle(
                        x: x * TileSet.TileSize.X + Rectangle.X, 
                        y: y * TileSet.TileSize.Y + Rectangle.Y, 
                        width: TileSet.TileSize.X, 
                        height: TileSet.TileSize.Y);
                    var src = TileSet.TileSheet.Entries[tile.TileMapY].Source;

                    s.Draw(TileSet.TileSheet.Sheet, dest, src, Color.White);
                }
            }
        }
    }
}