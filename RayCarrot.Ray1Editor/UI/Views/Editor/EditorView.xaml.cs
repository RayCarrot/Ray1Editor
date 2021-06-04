using System.Windows.Controls;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView(EditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EditorViewModel.SelectedObject) && viewModel.SelectedObject != null)
                    EditorTabControl.SelectedIndex = 3;
            };
        }
    }
}