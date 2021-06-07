using RayCarrot.UI;
using System.Collections.ObjectModel;

namespace RayCarrot.Ray1Editor
{
    public class LayerEditorViewModel : BaseViewModel
    {
        public LayerEditorViewModel(Layer layer)
        {
            Layer = layer;
            Fields = new ObservableCollection<EditorFieldViewModel>();
            ToggleFields = new ObservableCollection<EditorToggleIconViewModel>();
        }

        public ObservableCollection<EditorFieldViewModel> Fields { get; }
        public ObservableCollection<EditorToggleIconViewModel> ToggleFields { get; }
        public Layer Layer { get; }
        public string Header => Layer.Name;

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
    }
}