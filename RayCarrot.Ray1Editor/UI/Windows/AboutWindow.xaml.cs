using System.Windows;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : BaseWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            VersionTextBlock.Text = $"{AppViewModel.Instance.CurrentAppVersion}";

            if (AppViewModel.Instance.IsBeta)
                VersionTextBlock.Text += " (BETA)";
        }

        private void AppDataButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppViewModel.Instance.OpenExplorerPath(AppViewModel.Instance.Path_AppDataDir);
        }

        private void SourceCodeButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppViewModel.Instance.OpenURL(AppViewModel.Instance.Url_Ray1EditorGitHub);
        }

        private void Ray1MapButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppViewModel.Instance.OpenURL(AppViewModel.Instance.Url_Ray1Map);
        }
    }
}