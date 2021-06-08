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

        // Colors
        // TODO: Allow to be modified
        public Color Color_Background { get; set; } = new Color(0x28, 0x35, 0x93);
        public Color Color_MapBackground { get; set; } = new Color(0x79, 0x86, 0xCB);
        public Color Color_ObjBounds { get; set; } = new Color(0xf4, 0x43, 0x36);
        public Color Color_ObjLinks { get; set; } = new Color(0xFD, 0xD8, 0x35);
        public Color Color_ObjOffsetPos { get; set; } = new Color(0xFF, 0x57, 0x22);
        public Color Color_ObjOffsetPivot { get; set; } = new Color(0xFF, 0xC1, 0x07);
        public Color Color_ObjOffsetGeneric { get; set; } = new Color(0x7E, 0x57, 0xC2);
        public Color Color_TileSelection { get; set; } = new Color(0xFD, 0xD8, 0x35);
        public Color Color_TileTiling { get; set; } = new Color(0xFF, 0x8F, 0x00);

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
            MapSize = new Point(data.Layers.Max(x => x.Rectangle.Right), data.Layers.Max(x => x.Rectangle.Bottom));
        }

        public Layer GetActiveFullScreenLayer() => FullscreenLayer?.IsSelected == true ? FullscreenLayer : null;

        #endregion
    }
}