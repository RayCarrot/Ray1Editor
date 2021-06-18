using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace RayCarrot.Ray1Editor
{
    // IDEA: For maps with a lot of tiles there is a lot of lag. One way to fix that is to create a buffer texture, which holds the entire rendered map. This is then only used when NOT in edit mode since then we want to be able to quickly edit tiles.

    /// <summary>
    /// A tile map layer
    /// </summary>
    public abstract class TileMapBaseLayer<T> : Layer
    {
        #region Constructor

        protected TileMapBaseLayer(T[] tileMap, Point position, Point mapSize, TileSet tileSet)
        {
            TileMap = tileMap;
            Position = position;
            MapSize = mapSize;
            TileSet = tileSet;

            IsVisible = true;
        }

        #endregion

        #region Protected Properties

        protected TileEditorState State { get; set; }
        protected Point MapSelectionPoint1 { get; set; }
        protected Point MapSelectionPoint2 { get; set; }
        protected Point? MapPreviewOrigin { get; set; }
        protected int SelectedTilesWidth => SelectedTiles.GetLength(0);
        protected int SelectedTilesHeight => SelectedTiles.GetLength(1);

        #endregion

        #region Public Properties

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

        public override Rectangle Rectangle => new Rectangle(Position, MapSize * TileSet.TileSize);

        public T[,] SelectedTiles { get; set; }

        #endregion

        #region Protected Methods

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

        public override void UpdateLayerEditing(EditorUpdateData updateData)
        {
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

                // Get the origin tile to show the preview map from
                MapPreviewOrigin = Rectangle.Contains(updateData.MousePosition) ? hoverTile : null;

                if (CanEdit && 
                    updateData.Mouse.LeftButton == ButtonState.Pressed && 
                    updateData.Keyboard.IsKeyDown(Keys.LeftControl))
                {
                    if (State != TileEditorState.Tiling)
                        MapSelectionPoint1 = hoverTile;

                    MapSelectionPoint2 = hoverTile;

                    State = TileEditorState.Tiling;
                }
                else
                {
                    // Paste tiled tiles if switching from tiling mode
                    if (CanEdit && 
                        State == TileEditorState.Tiling && 
                        MapPreviewOrigin != null)
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
                if (CanEdit && 
                    State == TileEditorState.Idle &&
                    MapPreviewOrigin != null &&
                    updateData.IsKeyDown(Keys.LeftControl, false) && 
                    updateData.IsKeyDown(Keys.V))
                {
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

        public override void OnModeChanged(EditorMode oldMode, EditorMode newMode)
        {
            base.OnModeChanged(oldMode, newMode);

            State = newMode == EditorMode.Layers ? TileEditorState.Idle : TileEditorState.Disabled;
        }

        public override void Draw(Renderer r)
        {
            Rectangle? previewBox = null;

            if (MapPreviewOrigin != null && State == TileEditorState.Idle)
                previewBox = new Rectangle(MapPreviewOrigin.Value, new Point(SelectedTilesWidth, SelectedTilesHeight));
            else if (State == TileEditorState.Tiling)
                previewBox = GetMapSelection();

            // Only draw tiles which are visible. We could have the renderer check each tile if it's in view during the drawing, but that
            // will impact performance for larger levels with multiple tile layers due to it having to loop through every tile each frame
            var visibleArea = Camera.VisibleArea;
            var xStart = (Math.Max(visibleArea.Left, Rectangle.Left) - Rectangle.X) / TileSet.TileSize.X;
            var yStart = (Math.Max(visibleArea.Top, Rectangle.Top) - Rectangle.Y) / TileSet.TileSize.Y;
            var xEnd = Math.Ceiling((Math.Min(visibleArea.Right, Rectangle.Right) - Rectangle.X) / (float)TileSet.TileSize.X);
            var yEnd = Math.Ceiling((Math.Min(visibleArea.Bottom, Rectangle.Bottom) - Rectangle.Y) / (float)TileSet.TileSize.Y);

            // Draw map
            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    T tile;

                    // Check if a preview tile should be drawn instead
                    if (IsSelected && previewBox?.Contains(x, y) == true)
                        tile = SelectedTiles[(x - previewBox.Value.X) % SelectedTilesWidth, (y - previewBox.Value.Y) % SelectedTilesHeight];
                    else
                        tile = GetTileAt(x, y);

                    var tileIndex = GetTileSetIndex(tile);

                    // Skip fully transparent tiles to improve performance
                    if (TileSet.IsFullyTransparent(tileIndex))
                        continue;

                    var dest = GetTileRect(x, y);
                    var src = TileSet.TileSheet.Entries[tileIndex].Source;

                    r.SpriteBatch.Draw(TileSet.TileSheet.Sheet, dest, src, Color.White);
                }
            }

            // Draw selection border
            if (IsSelected && State is TileEditorState.Selecting or TileEditorState.Tiling)
            {
                var selection = GetMapSelection();

                var rect = new Rectangle(
                    x: Position.X + selection.X * TileSet.TileSize.X,
                    y: Position.Y + selection.Y * TileSet.TileSize.Y,
                    width: selection.Width * TileSet.TileSize.X,
                    height: selection.Height * TileSet.TileSize.Y);

                r.DrawRectangle(rect, State == TileEditorState.Selecting ? EditorState.Colors[EditorColor.TileSelection] : EditorState.Colors[EditorColor.TileTiling], 1);
            }
        }

        #endregion

        #region Data Types

        protected enum TileEditorState
        {
            Disabled,

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