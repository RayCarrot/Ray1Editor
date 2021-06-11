using System.Diagnostics;
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
            VersionTextBlock.Text = AppViewModel.Instance.CurrentAppVersion.ToString();
        }

        private void SourceCodeButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppViewModel.Instance.OpenURL("https://github.com/RayCarrot/RayCarrot.Ray1Editor");
        }

        private void Ray1MapButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppViewModel.Instance.OpenURL("https://raym.app/maps_r1");
        }
    }
}