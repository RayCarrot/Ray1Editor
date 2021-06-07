using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class PaletteEditorViewModel : BaseViewModel
    {
        public PaletteEditorViewModel(Palette palette)
        {
            Palette = palette;
        }

        public Palette Palette { get; }
        public string Header => Palette.Name;
        public bool IsSelected { get; set; }
    }
}