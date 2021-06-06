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
            CanEdit = Layer.CanEdit;
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
                
                OnPropertyChanged(nameof(IsVisible));

                foreach (var l in EditorViewModel.Layers)
                {
                    l.OnPropertyChanged(nameof(IsSelected));
                    l.CanEditVisibility = !l.IsSelected;
                }
            }
        }
        public bool CanEdit { get; set; }
        public bool CanEditVisibility { get; set; }

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