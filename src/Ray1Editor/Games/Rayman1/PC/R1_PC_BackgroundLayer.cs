using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Image;
using MahApps.Metro.IconPacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ray1Editor;

/// <summary>
/// A background layer for the PC version of Rayman 1, allowing changing the background to update the level palettes
/// </summary>
public class R1_PC_BackgroundLayer : BackgroundLayer
{
    // ReSharper disable once CoVariantArrayConversion
    public R1_PC_BackgroundLayer(BackgroundEntry_R1_PC[] backgroundEntries, Point position, int defaultIndex, string name = "Background", bool isVisible = true) : base(backgroundEntries, position, defaultIndex, name)
    {
        ExcludeTileColors = true;
        AutoUpdatePalette = true;
        IsVisible = isVisible;
    }

    public bool ExcludeTileColors { get; set; }
    public bool AutoUpdatePalette { get; set; }

    protected EditorToggleIconViewModel ToggleField_ExcludeTileColors { get; set; }

    public override IEnumerable<EditorToggleIconViewModel> GetToggleFields()
    {
        yield return ToggleField_ExcludeTileColors = new EditorToggleIconViewModel(
            iconKind: PackIconMaterialKind.GridOff,
            info: "Exclude colors used for the tiles when updating the palette after switching backgrounds. This will prevent the tile colors from being irreversibly changed based on the background change, but will result in some colors being incorrect for the backgrounds.",
            getValueAction: () => ExcludeTileColors,
            setValueAction: x => ExcludeTileColors = x);

        yield return new EditorToggleIconViewModel(
            iconKind: PackIconMaterialKind.PaletteOutline,
            info: "Automatically update the palette when the background changes",
            getValueAction: () => AutoUpdatePalette,
            setValueAction: x =>
            {
                AutoUpdatePalette = x;
                ToggleField_ExcludeTileColors.IsVisible = x;
            });

        foreach (var f in base.GetToggleFields())
            yield return f;
    }

    public override void ChangeBackground(int newIndex)
    {
        base.ChangeBackground(newIndex);

        if (AutoUpdatePalette)
        {
            foreach (var pal in ((R1_PC_GameData)Data).PC_Palettes)
            {
                var pcx = ((BackgroundEntry_R1_PC)BackgroundEntries[SelectedBackgroundIndex]).PCX;

                byte[] exclude;
                    
                if (ExcludeTileColors)
                {
                    // Get all the tile colors and exclude them from the palette updating. This is because the boss
                    // backgrounds use the tile portions of the palettes, thus switching to a boss background in a
                    // non-boss level will break the tile colors which can't be reversed.
                    var tiles = ((PalettedTextureSheet)Data.Layers.OfType<R1_TileMapLayer>().First().TileSet.TileSheet).PalettedTextureDatas;
                    exclude = tiles.SelectMany(x => x.ImgData).Distinct().ToArray();
                }
                else
                {
                    exclude = new byte[0];
                }

                foreach (var b in pcx.ScanLines.SelectMany(x => x).Distinct().Where(x => !exclude.Contains(x)))
                {
                    var c = pcx.VGAPalette[b];
                    pal.Colors[b] = new Color(c.Red, c.Green, c.Blue, c.Alpha);
                }

                Data.TextureManager.RefreshPalette(pal);
            }
        }
    }

    public record BackgroundEntry_R1_PC(Func<Texture2D> GetTex, Pointer Offset, string Name, PCX PCX) : BackgroundEntry(GetTex, Offset, Name);
}