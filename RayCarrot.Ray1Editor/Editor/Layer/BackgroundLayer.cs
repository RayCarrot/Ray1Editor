using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using MahApps.Metro.IconPacks;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A background layer, showing a single texture
    /// </summary>
    public class BackgroundLayer : Layer
    {
        // TODO: Pass in list of available backgrounds to select, each with a name
        public BackgroundLayer(Texture2D texture, Point position, string name = "Background")
        {
            Texture = texture;
            Rectangle = new Rectangle(position, Texture.Bounds.Size);
            Name = name;

            IsVisible = true;
        }

        public override string Name { get; }
        public override Rectangle Rectangle { get; }
        public override bool CanEdit => false;
        public Texture2D Texture { get; }
        public bool RepeatX { get; set; }
        public bool RepeatY { get; set; }

        public override IEnumerable<EditorFieldViewModel> GetFields()
        {
            // TODO: Add background selection which differs per game
            return new EditorFieldViewModel[0];
        }

        public override IEnumerable<EditorToggleIconViewModel> GetToggleFields()
        {
            yield return new EditorToggleIconViewModel(
                iconKind: PackIconMaterialKind.ArrowExpandHorizontal,
                info: "Repeat horizontally",
                getValueAction: () => RepeatX,
                setValueAction: x => RepeatX = x);

            yield return new EditorToggleIconViewModel(
                iconKind: PackIconMaterialKind.ArrowExpandVertical,
                info: "Repeat vertically",
                getValueAction: () => RepeatY,
                setValueAction: x => RepeatY = x);

            foreach (var f in base.GetToggleFields())
                yield return f;
        }

        public override void Draw(SpriteBatch s)
        {
            var height = RepeatY ? (EditorState.MapSize.Y - Rectangle.Y) : Rectangle.Height;
            var width = RepeatX ? (EditorState.MapSize.X - Rectangle.X) : Rectangle.Width;

            for (int y = 0; y < height; y += Rectangle.Height)
            {
                int h = Rectangle.Height;

                if (RepeatY && y + Rectangle.Height > height)
                    h = height % Rectangle.Height;

                for (int x = 0; x < width; x += Rectangle.Width)
                {
                    int w = Rectangle.Width;

                    if (RepeatX && x + Rectangle.Width > width)
                        w = width % Rectangle.Width;

                    var size = new Point(w, h);

                    var src = new Rectangle(Point.Zero, size);
                    var dest = new Rectangle(new Point(Rectangle.X + x, Rectangle.Y + y), size);

                    s.Draw(Texture, dest, src, Color.White);
                }
            }
        }
    }
}