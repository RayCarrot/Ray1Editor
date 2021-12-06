using System.Collections.ObjectModel;
using BinarySerializer;

namespace RayCarrot.Ray1Editor
{
    public class LayerEditorViewModel : BaseViewModel
    {
        public LayerEditorViewModel(Layer layer)
        {
            Layer = layer;
            Fields = new ObservableCollection<EditorFieldViewModel>();
            ToggleFields = new ObservableCollection<EditorToggleIconViewModel>();
            _prevPointer = layer.Pointer;
        }

        private Pointer _prevPointer;

        public ObservableCollection<EditorFieldViewModel> Fields { get; }
        public ObservableCollection<EditorToggleIconViewModel> ToggleFields { get; }
        public Layer Layer { get; }
        public string Header => Layer.Name;
        public string Offset => Layer.Pointer?.ToString();

        public void RecreateFields()
        {
            Fields.Clear();
            Fields.AddRange(Layer.GetFields());

            ToggleFields.Clear();
            ToggleFields.AddRange(Layer.GetToggleFields());
        }

        public void RefreshFields()
        {
            foreach (var field in Fields)
                field.Refresh();

            foreach (var field in ToggleFields)
                field.Refresh();
        }

        public void Update()
        {
            if (Layer.Pointer != _prevPointer)
            {
                OnPropertyChanged(nameof(Offset));
                _prevPointer = Layer.Pointer;
            }
        }
    }
}