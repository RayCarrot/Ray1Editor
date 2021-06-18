using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace RayCarrot.Ray1Editor
{
    public abstract class GameObject : EditorElement
    {
        // Data
        public abstract BinarySerializable SerializableData { get; }
        public abstract void Load();
        public abstract void Save();

        // Layout
        public abstract GameObjType Type { get; }
        public virtual int DefaultIconSize => 32;
        public abstract Point Position { get; set; }
        public virtual float Scale => 1f;
        public virtual bool FlipHorizontally => false;
        public virtual bool FlipVertically => false;
        public virtual float? Rotation => null;
        public virtual int OffsetSize => 8;
        public virtual int DisplayPrio => 0;

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
        protected virtual bool HalfAnimFramePos => false;
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
        public Rectangle LinkGripBounds { get; set; }
        protected virtual int LinkGripSize => 16;
        protected virtual int LinkLineThickness => 2;
        private Point? _prevLinkGripPosition;
        public Point LinkGripPosition { get; set; }
        public int LinkGroup { get; set; }
        public virtual bool CanBeLinkedToGroup => false;
        //public virtual IEnumerable<int> Links => new int[0];
        //public virtual bool CanBeLinked => false;

        // Info
        public abstract string PrimaryName { get; } // Official
        public abstract string SecondaryName { get; } // Unofficial
        public string DisplayName => SecondaryName ?? PrimaryName;
        public virtual string Tags => null;
        //public virtual string DebugText => null;
        public virtual string Scripts => null;

        // Update
        public virtual void Update(EditorUpdateData updateData)
        {
            // Update the frame
            UpdateFrame(updateData.DeltaTime);

            if (_prevLinkGripPosition != LinkGripPosition)
            {
                // Update the link grip bounds
                var linkGrip = LinkGripPosition - new Point(LinkGripPosition.X % LinkGripSize - LinkGripSize / 2, LinkGripPosition.Y % LinkGripSize - LinkGripSize / 2);

                LinkGripBounds = new Rectangle(linkGrip - new Point(LinkGripSize / 2), new Point(LinkGripSize));
            }

            _prevLinkGripPosition = LinkGripPosition;
        }
        public virtual void Draw(Renderer r)
        {
            var anim = CurrentAnimation;

            if (anim == null)
            {
                DrawDefault(r);
                return;
            }

            var frame = anim.Frames.ElementAtOrDefault(AnimationFrame);
            var sheet = SpriteSheet;

            if (frame == null)
                throw new Exception("Out of range frame!");

            var flipX = FlipHorizontally;
            var flipY = FlipVertically;

            int leftX = 0, bottomY = 0, rightX = 0, topY = 0;
            bool first = true;

            foreach (var layer in frame.SpriteLayers)
            {
                var spriteEntry = sheet.Entries[layer.SpriteIndex];

                if (spriteEntry == null)
                    continue;

                var x = layer.Position.X;
                var y = layer.Position.Y;

                if (HalfAnimFramePos)
                {
                    x /= 2;
                    y /= 2;
                }

                var pos = new Point(
                    (int)((x - Pivot.X) * (flipX ? -1f : 1f) * Scale + Pivot.X - (flipX ? spriteEntry.Source.Width : 0)),
                    (int)((y - Pivot.Y) * (flipY ? -1f : 1f) * Scale + Pivot.Y - (flipY ? spriteEntry.Source.Height : 0)));

                var effects = SpriteEffects.None;

                if (layer.IsFlippedHorizontally ^ flipX)
                    effects |= SpriteEffects.FlipHorizontally;

                if (layer.IsFlippedVertically ^ flipY)
                    effects |= SpriteEffects.FlipVertically;

                var dest = new Rectangle(Position + pos, new Point(spriteEntry.Source.Width, spriteEntry.Source.Height));

                r.Draw(
                    texture: sheet.Sheet, 
                    destinationRectangle: dest, 
                    sourceRectangle: spriteEntry.Source, 
                    rotation: 0, 
                    origin: Vector2.Zero, 
                    effects: effects, 
                    layerDepth: 0);

                var layerMaxX = pos.X + spriteEntry.Source.Width;
                var layerMaxY = pos.Y + spriteEntry.Source.Height;

                if (pos.X < leftX || first) leftX = pos.X;
                if (pos.Y < topY || first) topY = pos.Y;
                if (layerMaxX > rightX || first) rightX = layerMaxX;
                if (layerMaxY > bottomY || first) bottomY = layerMaxY;

                if (first)
                    first = false;
            }

            // Make sure at least one sprite was rendered, otherwise fall back to the default rendering
            if (first)
            {
                DrawDefault(r);
                return;
            }

            Bounds = new Rectangle(new Point(leftX, topY), new Point(rightX - leftX, bottomY - topY));
            Center = new Point(leftX + (rightX - leftX) / 2, topY + (bottomY - topY) / 2);
        }
        public virtual void DrawDefault(Renderer r)
        {
            Texture2D tex = Type switch
            {
                GameObjType.Object => EditorState.EditorTextures.Icon_Object,
                GameObjType.Trigger => EditorState.EditorTextures.Icon_Trigger,
                GameObjType.WayPoint => EditorState.EditorTextures.Icon_WayPoint,
                _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
            };

            var dest = new Rectangle(Position - new Point(DefaultIconSize / 2), new Point(DefaultIconSize));

            r.Draw(tex, dest);
            
            Bounds = new Rectangle(dest.Location - Position, dest.Size);
            Center = Point.Zero;
        }
        public virtual void DrawLinks(Renderer r)
        {
            if (CanBeLinkedToGroup)
            {
                var color = LinkGroup == 0 ? EditorState.Colors[EditorColor.ObjLinksDisabled] : EditorState.Colors[EditorColor.ObjLinksEnabled];

                r.DrawLine(new Vector2(Position.X + Center.X, Position.Y + Center.Y), LinkGripBounds.Center.ToVector2(), color, LinkLineThickness);
                r.DrawFilledRectangle(LinkGripBounds, color);
            }
        }
        public virtual void DrawOffsets(Renderer r)
        {
            DrawOffset(r, Position, EditorState.Colors[EditorColor.ObjOffsetPos]);

            if (Pivot != Point.Zero)
                DrawOffset(r, Position + Pivot, EditorState.Colors[EditorColor.ObjOffsetPivot]);
        }
        public virtual void DrawOffset(Renderer r, Point pos, Color c)
        {
            var halfSize = OffsetSize / 2;
            r.DrawLine(new Vector2(pos.X - halfSize, pos.Y), new Vector2(pos.X + halfSize, pos.Y), c, 1);
            r.DrawLine(new Vector2(pos.X, pos.Y - halfSize), new Vector2(pos.X, pos.Y + halfSize), c, 1);
        }

        // Data types
        public enum GameObjType
        {
            Object,
            Trigger,
            WayPoint
        }
    }
}