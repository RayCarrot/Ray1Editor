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

        public Dictionary<EditorColor, Color> Colors { get; set; } = Colors_Ray1Editor;

        // TODO: Support multiple color profiles
        public static Dictionary<EditorColor, Color> Colors_Ray1Editor => new Dictionary<EditorColor, Color>()
        {
            [EditorColor.Background] = new Color(0x28, 0x35, 0x93),
            [EditorColor.MapBackground] = new Color(0x79, 0x86, 0xCB),
            [EditorColor.SelectedObjBounds] = new Color(0xf4, 0x43, 0x36),
            [EditorColor.HoveringObjBounds] = new Color(0xFF, 0xEB, 0x3B),
            [EditorColor.ObjLinksEnabled] = new Color(0xFD, 0xD8, 0x35),
            [EditorColor.ObjLinksDisabled] = new Color(0xE5, 0x73, 0x73),
            [EditorColor.ObjOffsetPos] = new Color(0xFF, 0x57, 0x22),
            [EditorColor.ObjOffsetPivot] = new Color(0xFF, 0xC1, 0x07),
            [EditorColor.ObjOffsetGeneric] = new Color(0x7E, 0x57, 0xC2),
            [EditorColor.TileSelection] = new Color(0xFD, 0xD8, 0x35),
            [EditorColor.TileTiling] = new Color(0xFF, 0x8F, 0x00),
        };

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