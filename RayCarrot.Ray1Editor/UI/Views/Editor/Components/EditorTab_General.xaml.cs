using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for EditorTab_General.xaml
    /// </summary>
    public partial class EditorTab_General : UserControl
    {
        public EditorTab_General()
        {
            InitializeComponent();
        }

        public EditorViewModel ViewModel
        {
            get => DataContext as EditorViewModel;
            set => DataContext = value;
        }

        private void EditPaletteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var pal = (PaletteEditorViewModel)((FrameworkElement)sender).DataContext;

            var wasPaused = ViewModel.IsPaused;

            if (!wasPaused)
                ViewModel.IsPaused = true;

            var editVM = new EditPaletteViewModel(pal.Palette);

            var win = new EditPaletteWindow(editVM);
            win.ShowDialog();

            if (!wasPaused)
                ViewModel.IsPaused = false;

            if (win.DialogResult != true)
                return;

            // Update the palette with the modifications
            editVM.UpdatePalette();

            ViewModel.EditorScene.TextureManager.RefreshPalette(editVM.Palette);
        }
    }
}