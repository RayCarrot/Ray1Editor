using System.Collections.ObjectModel;
using System.Linq;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class LayerEditorViewModel : BaseViewModel
    {
        public LayerEditorViewModel(EditorViewModel editorViewModel, Layer layer)
        {
            EditorViewModel = editorViewModel;
            Layer = layer;
            Fields = new ObservableCollection<EditorFieldViewModel>();
        }

        public ObservableCollection<EditorFieldViewModel> Fields { get; }
        public EditorViewModel EditorViewModel { get; }
        public Layer Layer { get; }
        public string Header => Layer.Name;
        public bool IsSelected
        {
            get => Layer.IsSelected;
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                Layer.Select();

                foreach (var l in EditorViewModel.Layers.Where(x => x != this))
                    l.OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool IsVisible
        {
            get => Layer.IsVisible;
            set => Layer.IsVisible = value;
        }

        public void RecreateFields()
        {
            Fields.Clear();
            Fields.AddRange(Layer.GetFields());
        }

        public void RefreshFields()
        {
            foreach (var field in Fields)
                field.Refresh();
        }
    }
}