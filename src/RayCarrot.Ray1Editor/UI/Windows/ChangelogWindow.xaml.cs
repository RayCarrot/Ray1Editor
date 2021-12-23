using System.IO;
using System.Windows;

namespace RayCarrot.Ray1Editor;

/// <summary>
/// Interaction logic for ChangelogWindow.xaml
/// </summary>
public partial class ChangelogWindow : BaseWindow
{
    public ChangelogWindow()
    {
        InitializeComponent();
        Loaded += ChangelogWindow_Loaded;
    }

    private void ChangelogWindow_Loaded(object sender, RoutedEventArgs e)
    {
        using var stream = Assets.GetAsset("Assets/App/VersionHistory.txt");
        using var reader = new StreamReader(stream);
        ChangelogBox.Text = reader.ReadToEnd();
    }

    private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}