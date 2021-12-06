using NLog;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace RayCarrot.Ray1Editor
{
    public class EditPaletteViewModel : BaseViewModel
    {
        #region Constructor

        public EditPaletteViewModel(Palette palette)
        {
            Palette = palette;
            ColorWrap = palette.DisplayWrap ?? 16;

            Items = new ObservableCollection<PaletteEntryViewModel>(palette.Colors.Select((x, i) => 
                new PaletteEntryViewModel(Color.FromArgb(x.A, x.R, x.G, x.B), palette.IsFirstTransparent && i == 0, palette.CanEditAlpha)));
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Properties

        public Palette Palette { get; }
        public int ColorWrap { get; }

        public ObservableCollection<PaletteEntryViewModel> Items { get; }
        public PaletteEntryViewModel SelectedItem { get; set; }

        #endregion

        #region Public Methods

        public void UpdatePalette()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var c = Items[i].Color;
                Palette.Colors[i] = new Microsoft.Xna.Framework.Color(c.R, c.G, c.B, c.A);
            }

            Logger.Log(LogLevel.Trace, "Updated modified palette");
        }

        #endregion
    }
}