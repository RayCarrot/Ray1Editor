using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RayCarrot.Ray1Editor
{
    public class EditorUpdateData
    {
        public float DeltaTime { get; set; }
        public MouseState Mouse { get; set; }
        public Vector2 MousePosition { get; set; }
        public KeyboardState Keyboard { get; set; }
    }
}