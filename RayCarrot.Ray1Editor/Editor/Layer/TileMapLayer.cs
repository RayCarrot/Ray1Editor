﻿using MahApps.Metro.IconPacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using NLog;

namespace RayCarrot.Ray1Editor
{
    public abstract class TileMapLayer<T> : TileMapBaseLayer<T>
    {
        #region Constructor

        protected TileMapLayer(T[] tileMap, Point position, Point mapSize, TileSet tileSet) : base(tileMap, position, mapSize, tileSet)
        {
            LinkedLayers = new HashSet<TileMapLayer<T>>();
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Private Fields

        private TileSetLayer<T> _tileSetLayer;
        private bool _isSelected;

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

        public override bool IsSelected
        {
            get => _isSelected;
            protected set
            {
                _isSelected = value;

                if (ToggleField_TileSet != null)
                    ToggleField_TileSet.IsEnabled = value;

                if (!value)
                {
                    ToggleTileSet(false);
                    ToggleField_TileSet?.Refresh();
                }
            }
        }

        protected Vector2 SavedCamPos_Map { get; set; }
        protected Vector2 SavedCamPos_TileSet { get; set; }
        protected float SavedCamZoom_Map { get; set; } = 1;
        protected float SavedCamZoom_TileSet { get; set; } = 1;
        protected Point SavedMapSize_Map { get; set; }

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

        protected void ToggleTileSet(bool isVisible)
        {
            if (isVisible == IsShowingTileSet)
                return;

            IsShowingTileSet = isVisible;

            if (isVisible)
            {
                SavedCamPos_Map = Camera.Position;
                SavedCamZoom_Map = Camera.Zoom;
                SavedMapSize_Map = EditorState.MapSize;

                Camera.Position = SavedCamPos_TileSet;
                Camera.Zoom = SavedCamZoom_TileSet;
                EditorState.MapSize = TileSetLayer.Rectangle.Size + TileSetLayer.Rectangle.Location;

                EditorState.FullscreenLayer ??= this;
            }
            else
            {
                SavedCamPos_TileSet = Camera.Position;
                SavedCamZoom_TileSet = Camera.Zoom;

                Camera.Position = SavedCamPos_Map;
                Camera.Zoom = SavedCamZoom_Map;
                EditorState.MapSize = SavedMapSize_Map;

                if (EditorState.FullscreenLayer == this)
                    EditorState.FullscreenLayer = null;

                // Copy over the selected tiles from the tile set
                SelectedTiles = TileSetLayer.SelectedTiles;
            }

            Logger.Log(LogLevel.Info, "Toggled the tile map layer tile set");
        }

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

        protected EditorToggleIconViewModel ToggleField_TileSet { get; set; }

        public override IEnumerable<EditorToggleIconViewModel> GetToggleFields()
        {
            yield return ToggleField_TileSet = new EditorToggleIconViewModel(
                iconKind: PackIconMaterialKind.Grid, 
                info: "View tileset", 
                getValueAction: () => IsShowingTileSet, 
                setValueAction: ToggleTileSet);

            ToggleField_TileSet.IsEnabled = IsSelected;

            foreach (var f in base.GetToggleFields())
                yield return f;
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

            Logger.Log(LogLevel.Info, "Updated the tile map layer size to {0}", newSize);
        }

        public override void OnModeChanged(EditorMode oldMode, EditorMode newMode)
        {
            base.OnModeChanged(oldMode, newMode);

            ToggleField_TileSet.IsVisible = newMode == EditorMode.Layers && CanEdit;

            ToggleTileSet(false);
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