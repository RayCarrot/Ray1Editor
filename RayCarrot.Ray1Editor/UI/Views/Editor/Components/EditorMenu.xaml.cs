using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for EditorMenu.xaml
    /// </summary>
    public partial class EditorMenu : Menu
    {
        public EditorMenu()
        {
            InitializeComponent();
        }

        public EditorViewModel ViewModel
        {
            get => DataContext as EditorViewModel;
            set => DataContext = value;
        }

        private void CloseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void ShowControlsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.DoAndPause(() =>
            {
                var win = new EditorControlsWindow();
                win.ShowDialog();
            });
        }

        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.DoAndPause(() =>
            {
                var win = new SettingsWindow();
                win.ShowDialog();
            });
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.DoAndPause(() =>
            {
                var win = new AboutWindow();
                win.ShowDialog();
            });
        }
    }
}