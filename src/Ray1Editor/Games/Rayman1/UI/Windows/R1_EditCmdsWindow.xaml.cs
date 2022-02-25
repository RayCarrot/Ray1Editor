using System.Windows;

namespace Ray1Editor.Rayman1;

/// <summary>
/// Interaction logic for R1_EditCmdsWindow.xaml
/// </summary>
public partial class R1_EditCmdsWindow : BaseWindow
{
    public R1_EditCmdsWindow(R1_EditCmdsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = ViewModel = viewModel;
    }

    public R1_EditCmdsViewModel ViewModel { get; }

    private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}