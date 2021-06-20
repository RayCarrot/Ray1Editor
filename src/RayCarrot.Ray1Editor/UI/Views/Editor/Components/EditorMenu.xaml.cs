using System.Linq;
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

            // Update colors
            ViewModel.EditorScene.State.Colors = EditorColorProfileViewModel.GetViewModels.FirstOrDefault(x => x.ID == R1EServices.App.UserData.Theme_EditorColors)?.GetColorsFunc() ?? EditorColors.Colors_LightBlue;
        }

        private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.DoAndPause(() =>
            {
                var win = new AboutWindow();
                win.ShowDialog();
            });
        }
    }
}