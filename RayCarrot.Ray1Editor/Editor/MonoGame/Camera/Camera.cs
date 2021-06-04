using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace RayCarrot.Ray1Editor
{
    public class Camera
    {
        public float Zoom { get; set; } = 1;
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public float MinZoom { get; set; } = 0.2f;
        public float MaxZoom { get; set; } = 2.5f;
        public float ZoomSpeed { get; set; } = 0.001f;

        public bool IsDraggingCamera { get; protected set; }
        public double CameraSpeed { get; set; } = 500;
        public Vector2 ViewArea { get; set; }

        protected Vector2 PrevMousePos { get; set; }
        protected int PrevScrollWheelValue { get; set; }

        // TODO: Cache this, only recreate when a value is modified
        public Matrix TransformMatrix =>
            Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom) *
            Matrix.CreateTranslation(new Vector3(ViewArea / 2, 0));

        public Vector2 ToWorld(Vector2 pos) => Vector2.Transform(pos, Matrix.Invert(TransformMatrix));
        public Vector2 ToScreen(Vector2 pos) => Vector2.Transform(pos, TransformMatrix);

        public void Update(double deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            UpdateZoom(deltaTime, mouse);

            // Move camera with WASD
            if (keyboard.IsKeyDown(Keys.W))
                Position += new Vector2(0, (float)(CameraSpeed * deltaTime * -1));
            if (keyboard.IsKeyDown(Keys.A))
                Position += new Vector2((float)(CameraSpeed * deltaTime * -1), 0);
            if (keyboard.IsKeyDown(Keys.S))
                Position += new Vector2(0, (float)(CameraSpeed * deltaTime));
            if (keyboard.IsKeyDown(Keys.D))
                Position += new Vector2((float)(CameraSpeed * deltaTime), 0);

            // Get the mouse position
            var mousePos = mouse.Position;
            var mouseWorldPos = ToWorld(mousePos.ToVector2());

            if (mouse.RightButton == ButtonState.Pressed)
            {
                if (!IsDraggingCamera)
                {
                    IsDraggingCamera = true;
                    PrevMousePos = mouseWorldPos;
                }

                Position += PrevMousePos - mouseWorldPos;
                mouseWorldPos = ToWorld(mousePos.ToVector2());
                PrevMousePos = mouseWorldPos;
            }
            else
            {
                IsDraggingCamera = false;
            }
        }

        public void UpdateZoom(double deltaTime, MouseState mouse)
        {
            var scrollChange = mouse.ScrollWheelValue - PrevScrollWheelValue;
            PrevScrollWheelValue = mouse.ScrollWheelValue;

            Zoom = Math.Clamp(Zoom + scrollChange * ZoomSpeed, MinZoom, MaxZoom);
        }
    }
}