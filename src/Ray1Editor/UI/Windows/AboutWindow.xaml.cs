using System.Windows;

namespace Ray1Editor;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
public partial class AboutWindow : BaseWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionTextBlock.Text = $"{R1EServices.App.CurrentAppVersion}";

        if (R1EServices.App.IsBeta)
            VersionTextBlock.Text += " (BETA)";
    }

    private void AppDataButton_OnClick(object sender, RoutedEventArgs e)
    {
        R1EServices.File.OpenExplorerPath(R1EServices.App.Path_AppDataDir);
    }

    private void SourceCodeButton_OnClick(object sender, RoutedEventArgs e)
    {
        R1EServices.File.OpenURL(R1EServices.App.Url_Ray1EditorGitHub);
    }

    private void Ray1MapButton_OnClick(object sender, RoutedEventArgs e)
    {
        R1EServices.File.OpenURL(R1EServices.App.Url_Ray1Map);
    }
}