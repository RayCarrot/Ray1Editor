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
            ViewModel = viewModel;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (!ViewModel.IsSelectingObjFromList && e.PropertyName == nameof(EditorViewModel.SelectedObject) && ViewModel.SelectedObject != null)
                    EditorTabControl.SelectedIndex = 3;
            };

        }

        public EditorViewModel ViewModel
        {
            get => DataContext as EditorViewModel;
            set => DataContext = value;
        }
    }
}