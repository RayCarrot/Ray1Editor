using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Image;
using MahApps.Metro.IconPacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A background layer for the PC version of Rayman 1, allowing changing the background to update the level palettes
    /// </summary>
    public class R1_PC_BackgroundLayer : BackgroundLayer
    {
        public R1_PC_BackgroundLayer(BackgroundEntry_R1_PC[] backgroundEntries, Point position, int defaultIndex, string name = "Background") : base(backgroundEntries, position, defaultIndex, name)
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