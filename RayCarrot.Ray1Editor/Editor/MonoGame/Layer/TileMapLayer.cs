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

        protected bool IsSelectingTiles { get; set; }
        protected Point MapSelectionPoint1 { get; set; }
        protected Point MapSelectionPoint2 { get; set; }
        protected Point? MapPreviewOrigin { get; set; }

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

        protected Point GetHoverTile(Vector2 mousePos)
        {
            var tileMapPos = mousePos.ToPoint() - Rectangle.Location;
            return new Point(
                x: Math.Clamp(tileMapPos.X / TileSet.TileSize.X, 0, MapSize.X - 1),
                y: Math.Clamp(tileMapPos.Y / TileSet.TileSize.Y, 0, MapSize.Y - 1));
        }

        protected Point GetMapSelectionSource() => new Point(
            Math.Min(MapSelectionPoint1.X, MapSelectionPoint2.X),
            Math.Min(MapSelectionPoint1.Y, MapSelectionPoint2.Y));

        protected Point GetMapSelectionDestination() => new Point(
            Math.Max(MapSelectionPoint1.X, MapSelectionPoint2.X),
            Math.Max(MapSelectionPoint1.Y, MapSelectionPoint2.Y));

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

        public T GetTileAt(int x, int y)
        {
            return TileMap[y * MapSize.X + x];
        }

        protected Rectangle GetTileRect(int x, int y)
        {
            return new Rectangle(
                x: x * TileSet.TileSize.X + Position.X,
                y: y * TileSet.TileSize.Y + Position.Y,
                width: TileSet.TileSize.X,
                height: TileSet.TileSize.Y);
        }

        protected Rectangle GetTileSourceRect(T tile)
        {
            return TileSet.TileSheet.Entries[GetTileSetIndex(tile)].Source;
        }

        public override void UpdateLayerEditing(EditorUpdateData updateData)
        {
            var hoverTile = GetHoverTile(updateData.MousePosition);

            if (updateData.Mouse.LeftButton == ButtonState.Pressed)
            {
                MapPreviewOrigin = null;

                if (!IsSelectingTiles)
                    MapSelectionPoint1 = hoverTile;

                MapSelectionPoint2 = hoverTile;

                IsSelectingTiles = true;
            }
            else
            {
                IsSelectingTiles = false;

                MapPreviewOrigin = Rectangle.Contains(updateData.MousePosition) ? hoverTile : null;
            }

            updateData.DebugText.AppendLine($"Tile selection: {MapSelectionPoint1} -> {MapSelectionPoint2}");
        }

        public override void ResetLayerEditing()
        {
            IsSelectingTiles = false;
            MapPreviewOrigin = null;
        }

        public override void Draw(SpriteBatch s)
        {
            Rectangle? previewBox = null;
            Point? selectionSourcePoint = null;

            if (MapPreviewOrigin != null)
            {
                var preview = MapPreviewOrigin.Value;

                selectionSourcePoint = GetMapSelectionSource();
                var destPoint = GetMapSelectionDestination();

                var width = destPoint.X - selectionSourcePoint.Value.X + 1;
                var height = destPoint.Y - selectionSourcePoint.Value.Y + 1;

                previewBox = new Rectangle(preview, new Point(width, height));
            }

            // Draw map
            for (int y = 0; y < MapSize.Y; y++)
            {
                for (int x = 0; x < MapSize.X; x++)
                {
                    var sourceTileX = x;
                    var sourceTileY = y;

                    // Check if a preview tile should be drawn instead
                    if (previewBox?.Contains(x, y) == true)
                    {
                        sourceTileX = selectionSourcePoint.Value.X + (sourceTileX - previewBox.Value.X);
                        sourceTileY = selectionSourcePoint.Value.Y + (sourceTileY - previewBox.Value.Y);
                    }

                    var tile = GetTileAt(sourceTileX, sourceTileY);

                    var dest = GetTileRect(x, y);
                    var src = GetTileSourceRect(tile);

                    s.Draw(TileSet.TileSheet.Sheet, dest, src, Color.White);
                }
            }

            // Draw selection
            if (IsSelectingTiles)
            {
                selectionSourcePoint ??= GetMapSelectionSource();
                var destPoint = GetMapSelectionDestination();

                var w = destPoint.X - selectionSourcePoint.Value.X + 1;
                var h = destPoint.Y - selectionSourcePoint.Value.Y + 1;

                var rect = new Rectangle(
                    x: Position.X + selectionSourcePoint.Value.X * TileSet.TileSize.X,
                    y: Position.Y + selectionSourcePoint.Value.Y * TileSet.TileSize.Y,
                    width: w * TileSet.TileSize.X,
                    height: h * TileSet.TileSize.Y);

                s.DrawRectangle(rect, EditorState.Color_TileSelection, 1);
            }
        }

        #endregion
    }
}