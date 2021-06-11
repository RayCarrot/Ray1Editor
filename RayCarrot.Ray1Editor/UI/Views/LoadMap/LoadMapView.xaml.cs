using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for LoadMapWindow.xaml
    /// </summary>
    public partial class LoadMapView : UserControl
    {
        public LoadMapView()
        {
            InitializeComponent();
        }

        public LoadMapViewModel VM => (LoadMapViewModel)DataContext;

        private void LevelItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VM.LoadMap();
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow();
            win.ShowDialog();
        }
    }
}