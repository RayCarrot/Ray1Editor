using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ray1Editor.Updater
{
    /// <summary>
    /// A base window to inherit from
    /// </summary>
    public class BaseWindow : MetroWindow
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseWindow()
        {
            // Set title style
            TitleCharacterCasing = CharacterCasing.Normal;

            // Disable maximize button
            ShowMaxRestoreButton = false;

            // Set icon style
            Icon = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RayCarrot.Ray1Editor.Updater;component/Img/AppIcon.ico") as ImageSource;
            IconBitmapScalingMode = BitmapScalingMode.NearestNeighbor;
        }
    }
}