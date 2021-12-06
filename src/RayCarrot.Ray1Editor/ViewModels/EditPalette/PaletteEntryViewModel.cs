using System.Windows.Media;

namespace RayCarrot.Ray1Editor
{
    public class PaletteEntryViewModel : BaseViewModel
    {
        public PaletteEntryViewModel(Color color, bool isReadOnly, bool canEditAlpha)
        {
            Color = color;
            IsReadOnly = isReadOnly;
            CanEditAlpha = canEditAlpha;
        }

        private Color _color;

        public bool IsReadOnly { get; }
        public bool CanEditAlpha { get; }

        public Color Color
        {
            get => _color;
            set
            {
                if (!CanEditAlpha)
                    value.A = 255;
                _color = value;
            }
        }
    }
}