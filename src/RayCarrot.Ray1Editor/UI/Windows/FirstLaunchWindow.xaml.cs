using System.Windows;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for FirstLaunchWindow.xaml
    /// </summary>
    public partial class FirstLaunchWindow : BaseWindow
    {
        public FirstLaunchWindow()
        {
            InitializeComponent();
        }

        private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}