using System.Collections.ObjectModel;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class LayerEditorViewModel : BaseViewModel
    {
        public LayerEditorViewModel(Layer layer)
        {
            Layer = layer;
            Fields = new ObservableCollection<EditorFieldViewModel>();
        }

        public ObservableCollection<EditorFieldViewModel> Fields { get; }
        public Layer Layer { get; }
        public string Header => $"Layer 0 ({Layer.Name})"; // TODO: Set index based on order
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