using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Data for the state of the editor
    /// </summary>
    public class EditorState
    {
        #region Colors

        public Dictionary<EditorColor, Color> Colors { get; set; }

        #endregion

        #region Textures

        public EditorTextures EditorTextures { get; set; }

        #endregion

        #region State

        public Point MapSize { get; set; }

        /// <summary>
        /// The layer that is currently displaying in full screen mode, thus not showing anything else, or null if none
        /// </summary>
        public Layer FullscreenLayer { get; set; }

        public bool LoadFromMemory { get; set; } = false;

        #endregion

        #region Settings

        public bool AnimateObjects { get; set; } = true;
        public float FramesPerSecond { get; set; } = 60f;
        public int AutoScrollMargin { get; set; } = 40;
        public int AutoScrollSpeed { get; set; } = 7;

        #endregion

        #region Public Methods

        public void UpdateMapSize(GameData data)
        {
            if (!data.Layers.Any())
            {
                MapSize = Point.Zero;
                return;
            }

            MapSize = new Point(data.Layers.Max(x => x.Rectangle.Right), data.Layers.Max(x => x.Rectangle.Bottom));
        }

        public Layer GetActiveFullScreenLayer() => FullscreenLayer?.IsSelected == true ? FullscreenLayer : null;

        #endregion
    }
}