using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Image;
using MahApps.Metro.IconPacks;

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


        public record BackgroundEntry(Texture2D Tex, Pointer Offset, string Name);
    }

    /// <summary>
    /// A background layer for the PC version of Rayman 1, allowing changing the background to update the level palettes
    /// </summary>
    public class BackgroundLayer_R1_PC : BackgroundLayer
    {
        public BackgroundLayer_R1_PC(BackgroundEntry_R1_PC[] backgroundEntries, Point position, int defaultIndex, string name = "Background") : base(backgroundEntries, position, defaultIndex, name)
        {
            AutoUpdatePalette = true;
        }

        public bool AutoUpdatePalette { get; set; }

        public override IEnumerable<EditorToggleIconViewModel> GetToggleFields()
        {
            yield return new EditorToggleIconViewModel(
                iconKind: PackIconMaterialKind.PaletteOutline,
                info: "Automatically update the palette when the background changes",
                getValueAction: () => AutoUpdatePalette,
                setValueAction: x => AutoUpdatePalette = x);

            foreach (var f in base.GetToggleFields())
                yield return f;
        }

        public override void ChangeBackground(int newIndex)
        {
            base.ChangeBackground(newIndex);

            if (AutoUpdatePalette)
            {
                foreach (var pal in Data.Palettes)
                {
                    var pcx = ((BackgroundEntry_R1_PC)BackgroundEntries[SelectedBackgroundIndex]).PCX;

                    foreach (var b in pcx.ScanLines.SelectMany(x => x).Distinct())
                    {
                        var c = pcx.VGAPalette[b];
                        pal.Colors[b] = new Color(c.Red, c.Green, c.Blue, c.Alpha);
                    }

                    Data.TextureManager.RefreshPalette(pal);
                }
            }
        }

        public record BackgroundEntry_R1_PC(Texture2D Tex, Pointer Offset, string Name, PCX PCX) : BackgroundEntry(Tex, Offset, Name);
    }
}