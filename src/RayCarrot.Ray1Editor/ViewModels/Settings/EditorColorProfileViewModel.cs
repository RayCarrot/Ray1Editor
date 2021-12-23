using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor;

public class EditorColorProfileViewModel : BaseViewModel
{
    public EditorColorProfileViewModel(string id ,string displayName, System.Windows.Media.Color colorPreview, Func<Dictionary<EditorColor, Color>> getColorsFunc)
    {
        ID = id;
        DisplayName = displayName;
        ColorPreview = colorPreview;
        GetColorsFunc = getColorsFunc;
    }

    public string ID { get; }
    public string DisplayName { get; }
    public System.Windows.Media.Color ColorPreview { get; }
    public Func<Dictionary<EditorColor, Color>> GetColorsFunc { get; }

    public static IEnumerable<EditorColorProfileViewModel> GetViewModels => new EditorColorProfileViewModel[]
    {
        new EditorColorProfileViewModel("LightBlue", "Light Blue", System.Windows.Media.Color.FromRgb(0x79, 0x86, 0xCB), () => EditorColors.Colors_LightBlue),
        new EditorColorProfileViewModel("Dark", "Dark", System.Windows.Media.Color.FromRgb(0x1d, 0x1b, 0x32), () => EditorColors.Colors_Dark),
        new EditorColorProfileViewModel("Ray1Map", "Ray1Map", System.Windows.Media.Color.FromRgb(0x36, 0x1a, 0x42), () => EditorColors.Colors_Ray1Map),
        new EditorColorProfileViewModel("Raymap", "Raymap", System.Windows.Media.Color.FromRgb(0x15, 0x30, 0x37), () => EditorColors.Colors_Raymap),
    };
}