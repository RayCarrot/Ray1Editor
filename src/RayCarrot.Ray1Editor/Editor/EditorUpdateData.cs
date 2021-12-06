using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayCarrot.Ray1Editor
{
    public class EditorUpdateData
    {
        private Keys[] PressedKeys { get; set; }
        private Dictionary<Keys, bool> PreviousPressedKeys { get; } = new Dictionary<Keys, bool>();

        public float DeltaTime { get; set; }
        public MouseState Mouse { get; set; }
        public Vector2 MousePosition { get; set; }
        public KeyboardState Keyboard { get; set; }
        public StringBuilder DebugText { get; } = new StringBuilder();

        public void Update()
        {
            PressedKeys = Keyboard.GetPressedKeys();
        }

        public bool IsKeyDown(Keys key, bool singleInput = true)
        {
            var isDown = PressedKeys.Contains(key);

            if (!isDown)
            {
                PreviousPressedKeys[key] = false;
                return false;
            }

            if (PreviousPressedKeys.ContainsKey(key) && PreviousPressedKeys[key] && singleInput)
                return false;

            return PreviousPressedKeys[key] = true;
        }
    }
}