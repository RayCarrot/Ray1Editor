using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    public abstract class TileMapLayer<T> : TileMapBaseLayer<T>
    {
        protected TileMapLayer(T[] tileMap, Point position, Point mapSize, TileSet tileSet) : base(tileMap, position, mapSize, tileSet)
        {
            LinkedLayers = new HashSet<TileMapLayer<T>>();
        }

        #region Private Fields

        private TileSetLayer<T> _tileSetLayer;

        #endregion

        #region Protected Properties

        protected bool IsShowingTileSet { get; set; }
        protected TileSetLayer<T> TileSetLayer
        {
            get
            {
                if (_tileSetLayer == null)
                {
                    var map = GetTileSetMap();
                    var width = GetTileSetMapWidth();

                    _tileSetLayer = new TileSetLayer<T>(map, Position, new Point(width, map.Length / width), TileSet,
                        GetTileSetIndex, CloneTile)
                    {
                        EditorState = EditorState,
                        Camera = Camera,
                        Data = Data
                    };
                }

                return _tileSetLayer;
            }
        }

        #endregion

        #region Public Properties

        public HashSet<TileMapLayer<T>> LinkedLayers { get; }

        public virtual bool CanBeResized => true;
        public override bool CanEdit => true;
        public override string Name => $"Map";

        #endregion

        #region Protected Methods

        protected abstract T CreateNewTile();
        protected abstract T[] GetTileSetMap();
        protected abstract int GetTileSetMapWidth();

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

        public override void OnModeChanged(EditorMode oldMode, EditorMode newMode)
        {
            base.OnModeChanged(oldMode, newMode);

            IsShowingTileSet = false;

            if (EditorState.FullscreenLayer == this)
                EditorState.FullscreenLayer = null;
        }

        public override void UpdateLayerEditing(EditorUpdateData updateData)
        {
            if (IsShowingTileSet)
                TileSetLayer.UpdateLayerEditing(updateData);
            else
                base.UpdateLayerEditing(updateData);
        }

        public override void Draw(SpriteBatch s)
        {
            if (IsShowingTileSet)
                TileSetLayer.Draw(s);
            else
                base.Draw(s);
        }

        #endregion
    }
}