using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for LoadMapMenu.xaml
    /// </summary>
    public partial class LoadMapMenu : Menu
    {
        public LoadMapMenu()
        {
            InitializeComponent();
        }

        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow();
            win.ShowDialog();
        }

        private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new AboutWindow();
            win.ShowDialog();
        }

        private async void CheckForUpdatesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            await R1EServices.App.CheckForUpdatesAsync(true, false);
        }
    }
}