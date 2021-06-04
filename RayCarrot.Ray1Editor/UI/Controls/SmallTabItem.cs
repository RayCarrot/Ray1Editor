using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.Ray1Editor
{
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
    }
}
