using System.Windows;

namespace RayCarrot.Ray1Editor;

/// <summary>
/// Interaction logic for DialogMessageWindow.xaml
/// </summary>
public partial class DialogMessageWindow : BaseWindow
{
    public DialogMessageWindow()
    {
        InitializeComponent();
    }

    private void ActionButton_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = (DialogMessageActionViewModel)((FrameworkElement)sender).DataContext;

        // Set the result
        DialogResult = vm.ActionResult;

        // Close if set to do so
        if (vm.ShouldCloseDialog)
            Close();
    }
}