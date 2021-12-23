using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor;

public static class EditorColors
{
    private static Dictionary<EditorColor, Color> BaseColors => new Dictionary<EditorColor, Color>()
    {
        [EditorColor.SelectedObjBounds] = new Color(0xf4, 0x43, 0x36),
        [EditorColor.HoveringObjBounds] = new Color(0xFF, 0xEB, 0x3B),
        [EditorColor.ObjLinksEnabled] = new Color(0xFD, 0xD8, 0x35),
        [EditorColor.ObjLinksDisabled] = new Color(0xE5, 0x73, 0x73),
        [EditorColor.ObjOffsetPos] = new Color(0xFF, 0x57, 0x22),
        [EditorColor.ObjOffsetPivot] = new Color(0xFF, 0xC1, 0x07),
        [EditorColor.ObjOffsetGeneric] = new Color(0x7E, 0x57, 0xC2),
        [EditorColor.TileSelecting] = new Color(0xFD, 0xD8, 0x35),
        [EditorColor.TileSelection] = new Color(0xFF, 0x70, 0x43),
        [EditorColor.TileTiling] = new Color(0xFF, 0x8F, 0x00),
        [EditorColor.TileGrid] = new Color(0x00, 0x00, 0x00),
    };

    public static Dictionary<EditorColor, Color> Colors_LightBlue
    {
        get
        {
            var d = BaseColors;

            d[EditorColor.MapBackground] = new Color(0x79, 0x86, 0xCB);
            d[EditorColor.Background] = new Color(0x28, 0x35, 0x93);

            return d;
        }
    }

    // From ray1map: rcp
    public static Dictionary<EditorColor, Color> Colors_Dark
    {
        get
        {
            var d = BaseColors;

            d[EditorColor.MapBackground] = new Color(0x1d, 0x1b, 0x32);
            d[EditorColor.Background] = new Color(0x0f, 0x0e, 0x1b);

            return d;
        }
    }

    // From ray1map: main
    public static Dictionary<EditorColor, Color> Colors_Ray1Map
    {
        get
        {
            var d = BaseColors;

            d[EditorColor.MapBackground] = new Color(0x36, 0x1a, 0x42);
            d[EditorColor.Background] = new Color(0x1b, 0x0b, 0x22);

            return d;
        }
    }

    // From ray1map: raymap
    public static Dictionary<EditorColor, Color> Colors_Raymap
    {
        get
        {
            var d = BaseColors;

            d[EditorColor.MapBackground] = new Color(0x15, 0x30, 0x37);
            d[EditorColor.Background] = new Color(0x0b, 0x1d, 0x22);

            return d;
        }
    }
}