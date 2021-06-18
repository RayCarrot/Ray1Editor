using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using NLog;

namespace RayCarrot.Ray1Editor
{
    public class Camera
    {
        public Camera(Viewport viewport)
        {
            Viewport = viewport;
            Zoom = 1;
            _isDirty = true;
        }

        private bool _isDirty;
        private float _zoom;
        private Vector2 _position;
        private Vector2 _viewArea;
        private Matrix _transformMatrix;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Data
        protected Viewport Viewport { get; }

        // Current state
        public float Zoom
        {
            get => _zoom;
            set
            {
                if (_zoom != value)
                    _isDirty = true;

                _zoom = value;
            }
        }

        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                    _isDirty = true;

                _position = value;
            }
        }

        //public float Rotation { get; set; }
        public Vector2 ViewArea
        {
            get => _viewArea;
            set
            {
                if (_viewArea != value)
                    _isDirty = true;

                _viewArea = value;
            }
        }

        // Zoom
        public float MinZoom { get; set; } = 0.2f;
        public float MaxZoom { get; set; } = 3f;
        public float ZoomSpeed { get; set; } = 0.0008f;

        // Moving
        public bool IsDraggingCamera { get; protected set; }
        public float CameraSpeed { get; set; } = 700;

        // Targeting
        public Vector2? TargetPosition { get; set; }
        public float? TargetZoom { get; set; }
        public float TargetLerpFactor { get; set; } = 6;

        // Saves mouse states
        protected Vector2 PrevMousePos { get; set; }
        protected int PrevScrollWheelValue { get; set; }

        public Matrix TransformMatrix
        {
            get
            {
                if (_isDirty)
                {
                    _transformMatrix = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) * 
                                       //Matrix.CreateRotationZ(Rotation) *
                                       Matrix.CreateScale(Zoom) * 
                                       Matrix.CreateTranslation(new Vector3(ViewArea / 2, 0));
                    _isDirty = false;
                }

                return _transformMatrix;
            }
        }
        public Rectangle VisibleArea { get; set; }

        public Vector2 ToWorld(Vector2 pos) => Vector2.Transform(pos, Matrix.Invert(TransformMatrix));
        public Vector2 ToScreen(Vector2 pos) => Vector2.Transform(pos, TransformMatrix);
        public bool IsInVisibleArea(Rectangle rect)
        {
            return VisibleArea.Intersects(rect);
        }
        public void ResetCamera()
        {
            Zoom = 1;
            Position = new Vector2(Viewport.Width / 2f, Viewport.Height / 2f);
            Logger.Log(LogLevel.Trace, "Reset the camera");
        }

        public void Update(EditorUpdateData updateData)
        {
            if (TargetPosition.HasValue)
            {
                if (Vector2.Distance(Position, TargetPosition.Value) < 0.4f)
                    TargetPosition = null;
                else
                    Position = Vector2.Lerp(Position, TargetPosition.Value, updateData.DeltaTime * TargetLerpFactor);
            }
            if (TargetZoom.HasValue)
            {
                if (Math.Abs(TargetZoom.Value - Zoom) < 0.04f)
                    TargetZoom = null;
                else
                    Zoom = MathHelper.Lerp(Zoom, TargetZoom.Value, updateData.DeltaTime * TargetLerpFactor);
            }

            UpdateZoom(updateData);

            var camSpeed = CameraSpeed * updateData.DeltaTime * (1 - Math.Clamp(Zoom, 0.5f, 1.5f) + 1);

            // Move camera with WASD
            if (updateData.Keyboard.IsKeyDown(Keys.W))
                Position += new Vector2(0, camSpeed * -1);
            if (updateData.Keyboard.IsKeyDown(Keys.A))
                Position += new Vector2(camSpeed * -1, 0);
            if (updateData.Keyboard.IsKeyDown(Keys.S))
                Position += new Vector2(0, camSpeed);
            if (updateData.Keyboard.IsKeyDown(Keys.D))
                Position += new Vector2(camSpeed, 0);

            if (updateData.Mouse.RightButton == ButtonState.Pressed)
            {
                if (!IsDraggingCamera)
                {
                    IsDraggingCamera = true;
                    PrevMousePos = updateData.MousePosition;
                }

                Position += PrevMousePos - updateData.MousePosition;
                PrevMousePos = ToWorld(updateData.Mouse.Position.ToVector2());
            }
            else
            {
                IsDraggingCamera = false;
            }

            // Set the visible area
            var inverseViewMatrix = Matrix.Invert(TransformMatrix);
            var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
            var tr = Vector2.Transform(new Vector2(ViewArea.X, 0), inverseViewMatrix);
            var bl = Vector2.Transform(new Vector2(0, ViewArea.Y), inverseViewMatrix);
            var br = Vector2.Transform(ViewArea, inverseViewMatrix);
            var min = new Vector2(
                MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
            var max = new Vector2(
                MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
            VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));

            // Add debug text
            updateData.DebugText.AppendLine($"Zoom: {Zoom * 100} %");
            updateData.DebugText.AppendLine($"Position: {Position}");
        }

        public void UpdateZoom(EditorUpdateData updateData)
        {
            var scrollChange = updateData.Mouse.ScrollWheelValue - PrevScrollWheelValue;
            PrevScrollWheelValue = updateData.Mouse.ScrollWheelValue;

            Zoom = Math.Clamp(Zoom + scrollChange * ZoomSpeed, MinZoom, MaxZoom);
        }
    }
}