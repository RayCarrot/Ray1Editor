using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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

        #region Private Fields

        private bool _isPasteKeyDown;

        #endregion

        #region Protected Properties

        protected TileEditorState State { get; set; }
        protected Point MapSelectionPoint1 { get; set; }
        protected Point MapSelectionPoint2 { get; set; }
        protected Point? MapPreviewOrigin { get; set; }
        protected T[,] SelectedTiles { get; set; }
        protected int SelectedTilesWidth => SelectedTiles.GetLength(0);
        protected int SelectedTilesHeight => SelectedTiles.GetLength(1);

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
        protected abstract T CloneTile(T srcTile, T destTile);

        protected Point GetHoverTile(Vector2 mousePos)
        {
            var tileMapPos = mousePos.ToPoint() - Rectangle.Location;
            return new Point(
                x: Math.Clamp(tileMapPos.X / TileSet.TileSize.X, 0, MapSize.X - 1),
                y: Math.Clamp(tileMapPos.Y / TileSet.TileSize.Y, 0, MapSize.Y - 1));
        }

        protected Rectangle GetMapSelection()
        {
            var selectionSourcePoint = new Point(
                Math.Min(MapSelectionPoint1.X, MapSelectionPoint2.X),
                Math.Min(MapSelectionPoint1.Y, MapSelectionPoint2.Y));
            var destPoint = new Point(
                Math.Max(MapSelectionPoint1.X, MapSelectionPoint2.X),
                Math.Max(MapSelectionPoint1.Y, MapSelectionPoint2.Y));

            var w = destPoint.X - selectionSourcePoint.X + 1;
            var h = destPoint.Y - selectionSourcePoint.Y + 1;

            return new Rectangle(selectionSourcePoint, new Point(w, h));
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

        public void SetTileAt(T tile, int x, int y)
        {
            TileMap[y * MapSize.X + x] = tile;
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
            if (!updateData.Keyboard.IsKeyDown(Keys.V))
                _isPasteKeyDown = false;

            // Get the tile the mouse is over
            var hoverTile = GetHoverTile(updateData.MousePosition);

            // If left mouse button is pressed down tiles are being selected
            if (updateData.Mouse.LeftButton == ButtonState.Pressed && !updateData.Keyboard.IsKeyDown(Keys.LeftControl))
            {
                // Reset the preview origin. This gets set once the mouse button is released.
                MapPreviewOrigin = null;

                // Set the initial selection point if we were previously not selecting tiles
                if (State != TileEditorState.Selecting)
                    MapSelectionPoint1 = hoverTile;

                MapSelectionPoint2 = hoverTile;

                State = TileEditorState.Selecting;
            }
            else
            {
                // Store the selected tiles if we were previously selecting tiles or no selection has been specified
                if (State == TileEditorState.Selecting || SelectedTiles == null)
                {
                    var selection = GetMapSelection();

                    SelectedTiles = new T[selection.Width, selection.Height];

                    for (int y = 0; y < selection.Height; y++)
                    {
                        var originY = selection.Y + y;

                        for (int x = 0; x < selection.Width; x++)
                        {
                            var originX = selection.X + x;

                            SelectedTiles[x, y] = GetTileAt(originX, originY);
                        }
                    }
                }

                MapPreviewOrigin = Rectangle.Contains(updateData.MousePosition) ? hoverTile : null;

                if (updateData.Mouse.LeftButton == ButtonState.Pressed && updateData.Keyboard.IsKeyDown(Keys.LeftControl))
                {
                    if (State != TileEditorState.Tiling)
                        MapSelectionPoint1 = hoverTile;

                    MapSelectionPoint2 = hoverTile;

                    State = TileEditorState.Tiling;
                }
                else
                {
                    // Paste tiled tiles if switching from tiling mode
                    if (State == TileEditorState.Tiling && MapPreviewOrigin != null)
                    {
                        var selection = GetMapSelection();

                        var width = SelectedTilesWidth;
                        var height = SelectedTilesHeight;

                        for (int y = 0; y < selection.Height; y++)
                        {
                            var destY = selection.Y + y;

                            for (int x = 0; x < selection.Width; x++)
                            {
                                var destX = selection.X + x;
                                SetTileAt(CloneTile(SelectedTiles[x % width, y % height], GetTileAt(destX, destY)), destX, destY);
                            }
                        }
                    }

                    State = TileEditorState.Idle;
                }

                // Paste tiles with CTRL+V
                if (State == TileEditorState.Idle &&
                    MapPreviewOrigin != null &&
                    !_isPasteKeyDown &&
                    updateData.Keyboard.IsKeyDown(Keys.LeftControl) && 
                    updateData.Keyboard.IsKeyDown(Keys.V))
                {
                    _isPasteKeyDown = true;

                    var previewOrigin = MapPreviewOrigin.Value;

                    var width = SelectedTilesWidth;
                    var height = SelectedTilesHeight;

                    for (int y = 0; y < height; y++)
                    {
                        var destY = previewOrigin.Y + y;

                        if (destY >= MapSize.Y)
                            break;

                        for (int x = 0; x < width; x++)
                        {
                            var destX = previewOrigin.X + x;

                            if (destX >= MapSize.X)
                                break;

                            SetTileAt(CloneTile(SelectedTiles[x, y], GetTileAt(destX, destY)), destX, destY);
                        }
                    }
                }
            }

            updateData.DebugText.AppendLine($"Tile selection: {MapSelectionPoint1} -> {MapSelectionPoint2}");
            updateData.DebugText.AppendLine($"Tile editing state: {State}");
        }

        public override void ResetLayerEditing()
        {
            State = TileEditorState.Idle;
        }

        public override void Draw(SpriteBatch s)
        {
            Rectangle? previewBox = null;

            if (MapPreviewOrigin != null && State == TileEditorState.Idle)
                previewBox = new Rectangle(MapPreviewOrigin.Value, new Point(SelectedTilesWidth, SelectedTilesHeight));
            else if (State == TileEditorState.Tiling)
                previewBox = GetMapSelection();

            // Draw map
            for (int y = 0; y < MapSize.Y; y++)
            {
                for (int x = 0; x < MapSize.X; x++)
                {
                    var sourceTileX = x;
                    var sourceTileY = y;

                    T tile;

                    // Check if a preview tile should be drawn instead
                    if (previewBox?.Contains(x, y) == true)
                        tile = SelectedTiles[(sourceTileX - previewBox.Value.X) % SelectedTilesWidth, (sourceTileY - previewBox.Value.Y) % SelectedTilesHeight];
                    else
                        tile = GetTileAt(sourceTileX, sourceTileY);

                    var dest = GetTileRect(x, y);
                    var src = GetTileSourceRect(tile);

                    s.Draw(TileSet.TileSheet.Sheet, dest, src, Color.White);
                }
            }

            // Draw selection border
            if (State is TileEditorState.Selecting or TileEditorState.Tiling)
            {
                var selection = GetMapSelection();

                var rect = new Rectangle(
                    x: Position.X + selection.X * TileSet.TileSize.X,
                    y: Position.Y + selection.Y * TileSet.TileSize.Y,
                    width: selection.Width * TileSet.TileSize.X,
                    height: selection.Height * TileSet.TileSize.Y);

                s.DrawRectangle(rect, State == TileEditorState.Selecting ? EditorState.Color_TileSelection : EditorState.Color_TileTiling, 1);
            }
        }

        #endregion

        #region Data Types

        protected enum TileEditorState
        {
            /// <summary>
            /// The idle state. Should show the preview map as the cursor moves on the screen.
            /// </summary>
            Idle,

            /// <summary>
            /// Tiles are being selected.
            /// </summary>
            Selecting,

            /// <summary>
            /// Selected tiles are being tiled.
            /// </summary>
            Tiling,
        }

        #endregion
    }
}