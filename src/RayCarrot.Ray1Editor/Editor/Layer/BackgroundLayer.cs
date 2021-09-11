using BinarySerializer;
using MahApps.Metro.IconPacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A background layer, showing a single texture
    /// </summary>
    public class BackgroundLayer : Layer
    {
        public BackgroundLayer(BackgroundEntry[] backgroundEntries, Point position, int defaultIndex, string name = "Background")
        {
            BackgroundEntries = backgroundEntries;
            Position = position;
            Name = name;
            IsVisible = true;

            SelectedBackgroundIndex = defaultIndex;
            _rectangle = new Rectangle(Position, Texture.Bounds.Size);
        }

        private Rectangle _rectangle;

        public override string Name { get; }
        public override Rectangle Rectangle => _rectangle;
        public override bool CanEdit => false;
        public override LayerType Type => LayerType.Background;
        public override Pointer Pointer => BackgroundEntries[SelectedBackgroundIndex].Offset;
        public BackgroundEntry[] BackgroundEntries { get; }
        public Point Position { get; }
        public int SelectedBackgroundIndex { get; protected set; }
        public Texture2D Texture => BackgroundEntries[SelectedBackgroundIndex].Tex;
        public bool RepeatX { get; set; }
        public bool RepeatY { get; set; }

        public override IEnumerable<EditorFieldViewModel> GetFields()
        {
            var items = BackgroundEntries.Select(x => new EditorDropDownFieldViewModel.DropDownItem(x.Name, x)).ToArray();

            yield return new EditorDropDownFieldViewModel(
                header: "Selected Background",
                info: null,
                getValueAction: () => SelectedBackgroundIndex,
                setValueAction: ChangeBackground,
                getItemsAction: () => items);
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

        public virtual void ChangeBackground(int newIndex)
        {
            SelectedBackgroundIndex = newIndex;
            _rectangle = new Rectangle(Position, Texture.Bounds.Size);
        }

        public override void Draw(Renderer r)
        {
            var height = RepeatY ? (ViewModel.MapSize.Y - Rectangle.Y) : Rectangle.Height;
            var width = RepeatX ? (ViewModel.MapSize.X - Rectangle.X) : Rectangle.Width;

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

                    r.Draw(Texture, dest, src);
                }
            }
        }


        public record BackgroundEntry(Func<Texture2D> GetTex, Pointer Offset, string Name)
        {
            private Texture2D _tex;
            public Texture2D Tex => _tex ??= GetTex();
        }
    }
}