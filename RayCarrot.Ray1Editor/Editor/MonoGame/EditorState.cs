using System.Linq;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Data for the state of the editor
    /// </summary>
    public class EditorState
    {
        public Point MapSize { get; protected set; }
        public bool AnimateObjects { get; set; } = true;
        public bool LoadFromMemory { get; set; } = false;
        public float FramesPerSecond { get; set; } = 60f;
        public int AutoScrollMargin { get; set; } = 40;
        public int AutoScrollSpeed { get; set; } = 7;

        public void UpdateMapSize(GameData data)
        {
            MapSize = new Point(data.Layers.Max(x => x.Rectangle.Right), data.Layers.Max(x => x.Rectangle.Bottom));
        }
    }
}