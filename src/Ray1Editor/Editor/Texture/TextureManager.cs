using Microsoft.Xna.Framework.Graphics;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ray1Editor;

public class TextureManager : IDisposable
{
    public TextureManager(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        Textures = new HashSet<Texture2D>();
        PalettedTextureDatas = new HashSet<PalettedTextureData>();
        TextureSheets = new HashSet<TextureSheet>();
        Palettes = new HashSet<Palette>();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public HashSet<Texture2D> Textures { get; }
    public HashSet<PalettedTextureData> PalettedTextureDatas { get; }
    public HashSet<TextureSheet> TextureSheets { get; }
    public HashSet<Palette> Palettes { get; }
    public GraphicsDevice GraphicsDevice { get; }

    protected IEnumerable<PalettedTextureData> EnumeratePalettedData()
    {
        foreach (var sheet in TextureSheets.OfType<PalettedTextureSheet>())
        {
            foreach (var tex in sheet.PalettedTextureDatas.Where(x => x != null))
            {
                yield return tex;
            }
        }

        foreach (var tex in PalettedTextureDatas.Where(x => x != null))
            yield return tex;
    }

    public Texture2D CreateTexture(int width, int height)
    {
        var tex = new Texture2D(GraphicsDevice, width, height);
        AddTexture(tex);
        return tex;
    }

    public void AddTexture(Texture2D tex) => Textures.Add(tex);
    public void AddPalettedTexture(PalettedTextureData palData)
    {
        Textures.Add(palData.Texture);
        PalettedTextureDatas.Add(palData);
    }
    public void AddPalette(Palette pal) => Palettes.Add(pal);

    public void AddTextureSheet(TextureSheet sheet) => TextureSheets.Add(sheet);

    public void RefreshPalette(Palette pal)
    {
        // Refresh every paletted texture
        foreach (var tex in EnumeratePalettedData().Where(x => x.Palette == pal))
            tex.Apply();

        // Update the palette
        pal.Update();

        Logger.Info("Refreshed palette {0}", pal.Name);
    }

    public void SwapPalettes(Palette oldPal, Palette newPal)
    {
        foreach (var tex in EnumeratePalettedData().Where(x => x.Palette == oldPal))
        {
            tex.Palette = newPal;
            tex.Apply();
        }

        Logger.Info("Swapped palette {0} with {0}", oldPal.Name, newPal.Name);
    }

    public void Dispose()
    {
        foreach (var tex in Textures)
            tex?.Dispose();

        foreach (var tex in TextureSheets)
            tex?.Dispose();
    }
}