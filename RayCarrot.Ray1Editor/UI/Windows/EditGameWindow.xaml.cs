using System.Windows;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for EditGameWindow.xaml
    /// </summary>
    public partial class EditGameWindow : BaseWindow
    {
        public EditGameWindow(EditGameViewModel viewModel)
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