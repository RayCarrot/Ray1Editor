using ICSharpCode.AvalonEdit.Highlighting;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ray1Editor;

/// <summary>
/// Interaction logic for EditorTab_Properties.xaml
/// </summary>
public partial class EditorTab_Properties : UserControl
{
    public EditorTab_Properties()
    {
        InitializeComponent();

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

        Loaded += (_, _) =>
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        };
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.SelectedObjectScript))
            ScriptTextEditor.Text = ViewModel.SelectedObjectScript;
    }

    public EditorViewModel ViewModel
    {
        get => DataContext as EditorViewModel;
        set => DataContext = value;
    }

    private void TextEditor_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Redirect the mouse wheel movement to allow scrolling

        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = e.Source
        };

        PropertiesScrollViewer?.RaiseEvent(eventArg);
        e.Handled = true;
    }
}