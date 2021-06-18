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
            Loaded += (_, _) => GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(GamesListBox, new LoadMapGamesListDropTarget());
        }

        public LoadMapViewModel VM => (LoadMapViewModel)DataContext;

        private void LevelItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VM.LoadMap();
        }

        private void CloseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
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
            await AppViewModel.Instance.CheckForUpdatesAsync(true, false);
        }
    }
}