using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A tile map layer
    /// </summary>
    public abstract class TileMapLayer<T> : Layer
    {
        #region Constructor

        protected TileMapLayer(T[] tileMap, Point position, Point mapSize, TileSet tileSet)
        {
            TileMap = tileMap;
            Position = position;
            MapSize = mapSize;
            TileSet = tileSet;

            LinkedLayers = new HashSet<TileMapLayer<T>>();
            IsVisible = true;
        }

        #endregion

        #region Protected Properties

        protected Rectangle? TileSelection { get; set; }

        #endregion

        #region Public Properties

        public HashSet<TileMapLayer<T>> LinkedLayers { get; }

        /// <summary>
        /// The map position
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// The map size, in tiles
        /// </summary>
        public Point MapSize { get; protected set; }

        /// <summary>
        /// The tile map
        /// </summary>
        public T[] TileMap { get; protected set; }

        /// <summary>
        /// The tile set, containing the tile textures
        /// </summary>
        public TileSet TileSet { get; }

        public virtual bool CanBeResized => true;

        public override string Name => $"Map";
        public override Rectangle Rectangle => new Rectangle(Position, MapSize * TileSet.TileSize);
        public override bool CanEdit => true;

        #endregion

        #region Protected Methods

        protected abstract T CreateNewTile();
        protected abstract int GetTileSetIndex(T tile);

        #endregion

        #region Public Methods

        public override IEnumerable<EditorFieldViewModel> GetFields()
        {
            if (CanBeResized)
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
            var newMap = new T[newSize.X * newSize.Y];

            // Copy tiles which fit the new size
            for (int y = 0; y < newSize.Y; y++)
            {
                for (int x = 0; x < newSize.X; x++)
                {
                    if (x < MapSize.X && y < MapSize.Y)
                        newMap[y * newSize.X + x] = TileMap[y * MapSize.X + x];
                    else
                        newMap[y * newSize.X + x] = CreateNewTile();
                }
            }

            MapSize = newSize;
            TileMap = newMap;

            foreach (var l in LinkedLayers)
            {
                l.MapSize = newSize;
                l.TileMap = newMap;
            }

            EditorState.UpdateMapSize(Data);
        }

        public override void UpdateLayerEditing(double deltaTime, MouseState mouse)
        {
            // TODO: Update layer editing
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
                    var src = TileSet.TileSheet.Entries[GetTileSetIndex(tile)].Source;

                    s.Draw(TileSet.TileSheet.Sheet, dest, src, Color.White);
                }
            }
        }

        #endregion
    }
}