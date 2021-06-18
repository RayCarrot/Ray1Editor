using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for EditorTab_Objects.xaml
    /// </summary>
    public partial class EditorTab_Objects : UserControl
    {
        public EditorTab_Objects()
        {
            InitializeComponent();
            Loaded += (_, _) => DragDrop.SetDropHandler(ObjList, new EditorObjListDropTarget((EditorViewModel)DataContext));
        }
    }
}