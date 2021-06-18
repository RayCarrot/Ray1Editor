using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for LoadMapWindow.xaml
    /// </summary>
    public partial class LoadMapView : UserControl
    {
        public LoadMapView()
        {
            InitializeComponent();
            Loaded += (_, _) => GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(GamesListBox, new LoadMapGamesListDropTarget());
        }

        public LoadMapViewModel VM => (LoadMapViewModel)DataContext;

        private void LevelItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VM.LoadMap();
        }
    }
}