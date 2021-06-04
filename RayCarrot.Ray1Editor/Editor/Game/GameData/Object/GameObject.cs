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

        // Animations
        public Rectangle Bounds { get; set; }
        public Point Center { get; set; }
        public virtual ObjAnimation_HitBoxLayer[] HitBoxLayers => new ObjAnimation_HitBoxLayer[0];
        public abstract ObjAnimation CurrentAnimation { get; }
        public virtual int AnimationFrame { get; set; }
        public abstract int AnimSpeed { get; }
        public double AnimationFrameFloat { get; set; }
        public abstract TextureSheet SpriteSheet { get; }
        public virtual Vector2 Pivot => Vector2.Zero;
        public void UpdateFrame(double deltaTime)
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
        public Point LinkGripPosition { get; set; }
        public int LinkGroup { get; set; }
        public virtual bool CanBeLinkedToGroup => false;
        //public virtual IEnumerable<int> Links => new int[0];
        //public virtual bool CanBeLinked => false;

        // Info
        public abstract string PrimaryName { get; } // Official
        public abstract string SecondaryName { get; } // Unofficial
        public virtual string DebugText => null;

        // Update
        public virtual void Update(double deltaTime)
        {
            // Update the frame
            UpdateFrame(deltaTime);
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

            // TODO: Make these relative to the position rather than absolute positions?
            Bounds = new Rectangle(new Point(Position.X + leftX, Position.Y + topY), new Point(rightX - leftX, bottomY - topY));
            Center = new Point(Position.X + leftX + (rightX - leftX) / 2, Position.Y + topY + (bottomY - topY) / 2);
        }
        public virtual void DrawLinks(SpriteBatch s)
        {
            if (CanBeLinkedToGroup && LinkGroup > 0)
            {
                s.DrawLine(Center.ToVector2(), LinkGripPosition.ToVector2(), Color.Yellow, 2);
                s.DrawFilledRectangle(LinkGripPosition.ToVector2() - new Vector2(8), new Vector2(16), Color.Yellow);
            }
        }
    }
}