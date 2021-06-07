using System.Windows;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for EditPaletteWindow.xaml
    /// </summary>
    public partial class EditPaletteWindow : BaseWindow
    {
        public EditPaletteWindow(EditPaletteViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}