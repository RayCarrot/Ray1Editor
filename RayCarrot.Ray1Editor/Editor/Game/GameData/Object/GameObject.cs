using System;
using System.Linq;
using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    public abstract class GameObject : EditorElement
    {
        // Data
        public abstract BinarySerializable SerializableData { get; }
        public abstract void Load();
        public abstract void Save();

        // Layout
        public abstract Point Position { get; set; }
        public virtual float Scale => 1f;
        public virtual bool FlipHorizontally => false;
        public virtual bool FlipVertically => false;
        public virtual float? Rotation => null;
        public virtual int OffsetSize => 8;

        // Animations
        public Rectangle Bounds { get; set; }
        public Rectangle WorldBounds => new Rectangle(Bounds.Location + Position, Bounds.Size);
        public Point Center { get; set; }
        public virtual ObjAnimation_HitBoxLayer[] HitBoxLayers => new ObjAnimation_HitBoxLayer[0];
        public abstract ObjAnimation CurrentAnimation { get; }
        public virtual int AnimationFrame { get; set; }
        public abstract int AnimSpeed { get; }
        public float AnimationFrameFloat { get; set; }
        public abstract TextureSheet SpriteSheet { get; }
        public virtual Point Pivot => Point.Zero;
        public void UpdateFrame(float deltaTime)
        {
            if (!ShouldUpdateFrame() || CurrentAnimation == null)
                return;

            // Increment frame if animating
            if (EditorState.AnimateObjects && AnimSpeed > 0)
                AnimationFrameFloat += (EditorState.FramesPerSecond / AnimSpeed) * deltaTime;

            // Loop around if over the frame limit
            bool isFinished = false;
            if (AnimationFrameFloat >= CurrentAnimation.Frames.Length)
            {
                AnimationFrameFloat %= CurrentAnimation.Frames.Length;
                isFinished = true;
            }

            // Update the frame
            AnimationFrame = (byte)Math.Floor(AnimationFrameFloat);

            // Trigger animation finished event
            if (isFinished || AnimSpeed == 0)
                OnFinishedAnimation();
        }
        protected virtual bool ShouldUpdateFrame() => true;
        protected virtual void OnFinishedAnimation() { }
        public virtual void ResetFrame()
        {
            if (!EditorState.LoadFromMemory)
            {
                AnimationFrame = 0;
                AnimationFrameFloat = 0;
            }
        }

        // Links
        protected virtual int LinkGripSize => 16;
        protected virtual int LinkLineThickness => 2;
        public Point LinkGripPosition { get; set; }
        public Point GetLinkGripSnappedPosition => LinkGripPosition - new Point(LinkGripPosition.X % LinkGripSize - LinkGripSize / 2, LinkGripPosition.Y % LinkGripSize - LinkGripSize / 2);
        public int LinkGroup { get; set; }
        public virtual bool CanBeLinkedToGroup => false;
        //public virtual IEnumerable<int> Links => new int[0];
        //public virtual bool CanBeLinked => false;

        // Info
        public abstract string PrimaryName { get; } // Official
        public abstract string SecondaryName { get; } // Unofficial
        public virtual string DebugText => null;

        // Update
        public virtual void Update(EditorUpdateData updateData)
        {
            // Update the frame
            UpdateFrame(updateData.DeltaTime);
        }
        public virtual void Draw(SpriteBatch s)
        {
            // TODO: Show some sort of dummy texture for objects without rendered sprites

            var anim = CurrentAnimation;

            if (anim == null)
                return;

            var frame = anim.Frames.ElementAtOrDefault(AnimationFrame);
            var sheet = SpriteSheet;

            if (frame == null)
                throw new Exception("Out of range frame!");

            int leftX = 0, bottomY = 0, rightX = 0, topY = 0;
            bool first = true;

            foreach (var layer in frame.SpriteLayers)
            {
                var spriteEntry = sheet.Entries[layer.SpriteIndex];

                if (spriteEntry == null)
                    continue;

                var dest = new Rectangle(Position + layer.Position, new Point(spriteEntry.Source.Width, spriteEntry.Source.Height));

                var effects = SpriteEffects.None;

                if (layer.IsFlippedHorizontally)
                    effects |= SpriteEffects.FlipHorizontally;
                if (layer.IsFlippedVertically)
                    effects |= SpriteEffects.FlipVertically;

                s.Draw(
                    texture: sheet.Sheet, 
                    destinationRectangle: dest, 
                    sourceRectangle: spriteEntry.Source, 
                    color: Color.White, 
                    rotation: 0, 
                    origin: Vector2.Zero, 
                    effects: effects, 
                    layerDepth: 0);

                var layerMaxX = layer.Position.X + spriteEntry.Source.Width;
                var layerMaxY = layer.Position.Y + spriteEntry.Source.Height;

                if (layer.Position.X < leftX || first) leftX = layer.Position.X;
                if (layer.Position.Y < topY || first) topY = layer.Position.Y;
                if (layerMaxX > rightX || first) rightX = layerMaxX;
                if (layerMaxY > bottomY || first) bottomY = layerMaxY;

                if (first)
                    first = false;
            }

            Bounds = new Rectangle(new Point(leftX, topY), new Point(rightX - leftX, bottomY - topY));
            Center = new Point(leftX + (rightX - leftX) / 2, topY + (bottomY - topY) / 2);
        }
        public virtual void DrawLinks(SpriteBatch s)
        {
            if (CanBeLinkedToGroup)
            {
                var linkGrip = GetLinkGripSnappedPosition.ToVector2();

                s.DrawLine(new Vector2(Position.X + Center.X, Position.Y + Center.Y), linkGrip, EditorState.Color_ObjLinks, LinkLineThickness);
                s.DrawFilledRectangle(linkGrip - new Vector2(LinkGripSize / 2f), new Vector2(LinkGripSize), EditorState.Color_ObjLinks);
            }
        }
        public virtual void DrawOffsets(SpriteBatch s)
        {
            DrawOffset(s, Position, EditorState.Color_ObjOffsetPos);

            if (Pivot != Point.Zero)
                DrawOffset(s, Position + Pivot, EditorState.Color_ObjOffsetPivot);
        }
        public virtual void DrawOffset(SpriteBatch s, Point pos, Color c)
        {
            var halfSize = OffsetSize / 2;
            s.DrawLine(new Vector2(pos.X - halfSize, pos.Y), new Vector2(pos.X + halfSize, pos.Y), c, 1);
            s.DrawLine(new Vector2(pos.X, pos.Y - halfSize), new Vector2(pos.X, pos.Y + halfSize), c, 1);
        }
    }
}