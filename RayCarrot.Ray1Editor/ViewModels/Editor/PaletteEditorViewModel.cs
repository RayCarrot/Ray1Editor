using System;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class PaletteEditorViewModel : BaseViewModel
    {
        public PaletteEditorViewModel(Palette palette, bool isSelected, Action<Palette> onSelectedAction)
        {
            Palette = palette;
            OnSelectedAction = onSelectedAction;
            _isSelected = isSelected;
        }

        private bool _isSelected;

        public Palette Palette { get; }
        public Action<Palette> OnSelectedAction { get; }
        public string Header => Palette.Name;
        public string Offset => Palette.Pointer?.ToString();

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;

                if (IsSelected)
                    OnSelectedAction(Palette);
            }
        }
    }
}