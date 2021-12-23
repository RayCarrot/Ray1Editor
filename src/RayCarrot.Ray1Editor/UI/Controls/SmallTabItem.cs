using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RayCarrot.Ray1Editor;

/// <summary>
/// A small tab item with an icon
/// </summary>
public class SmallTabItem : TabItem
{
    /// <summary>
    /// Indicates the icon kind to use for the tab header
    /// </summary>
    public PackIconMaterialKind IconKind
    {
        get => (PackIconMaterialKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    /// <summary>
    /// Dependency property for <see cref="IconKind"/>
    /// </summary>
    public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind), typeof(SmallTabItem));        
        
    /// <summary>
    /// Indicates the icon foreground brush
    /// </summary>
    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    /// <summary>
    /// Dependency property for <see cref="IconForeground"/>
    /// </summary>
    public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(SmallTabItem));
}