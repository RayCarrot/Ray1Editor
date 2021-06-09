using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView(EditorViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (!viewModel.IsSelectingObjFromList && e.PropertyName == nameof(EditorViewModel.SelectedObject) && viewModel.SelectedObject != null)
                    EditorTabControl.SelectedIndex = 3;

                if (e.PropertyName == nameof(EditorViewModel.SelectedObjectScript))
                    ScriptTextEditor.Text = viewModel.SelectedObjectScript;
            };

            DragDrop.SetDropHandler(ObjList, new EditorObjListDropTarget(ViewModel));

            // Hacky way of modifying some of the highlight colors so they're visible in dark mode
            foreach (var c in ScriptTextEditor.SyntaxHighlighting.NamedHighlightingColors)
            {
                switch (c.Name)
                {
                    case "Comment":
                        break;
                    case "Character":
                        break;
                    case "String":
                        break;
                    case "Preprocessor":
                        break;
                    case "Punctuation": // ==
                        c.Foreground = new SimpleHighlightingBrush(Color.FromRgb(0x00, 0x96, 0x88));
                        break;
                    case "MethodName": // IF ()
                        c.Foreground = new SimpleHighlightingBrush(Color.FromRgb(0x8B, 0xC3, 0x4A));
                        break;
                    case "Digits":
                        c.Foreground = new SimpleHighlightingBrush(Color.FromRgb(0xFF, 0xEB, 0x3B));
                        break;
                    case "CompoundKeywords":
                        break;
                    case "This":
                        break;
                    case "Operators":
                        break;
                    case "Namespace":
                        break;
                    case "Friend":
                        break;
                    case "Modifiers":
                        break;
                    case "TypeKeywords":
                        break;
                    case "BooleanConstants":
                        break;
                    case "Keywords":
                        break;
                    case "LoopKeywords":
                        break;
                    case "JumpKeywords":
                        break;
                    case "ExceptionHandling":
                        break;
                    case "ControlFlow":
                        break;
                }
            }
        }

        public EditorViewModel ViewModel
        {
            get => DataContext as EditorViewModel;
            set => DataContext = value;
        }

        private void EditPaletteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var pal = (PaletteEditorViewModel)((FrameworkElement)sender).DataContext;

            var wasPaused = ViewModel.IsPaused;

            if (!wasPaused)
                ViewModel.IsPaused = true;

            var editVM = new EditPaletteViewModel(pal.Palette);

            var win = new EditPaletteWindow(editVM);
            win.ShowDialog();

            if (!wasPaused)
                ViewModel.IsPaused = false;

            if (win.DialogResult != true)
                return;

            // Update the palette with the modifications
            editVM.UpdatePalette();

            ViewModel.EditorScene.TextureManager.RefreshPalette(editVM.Palette);
        }
    }
}