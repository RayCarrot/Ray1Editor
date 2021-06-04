using System.ComponentModel;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            AppViewModel.Instance.UserData.WindowState?.ApplyToWindow(this);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // Update the saved window state
            AppViewModel.Instance.UserData.WindowState = UserData_WindowSessionState.GetWindowState(this);
        }
    }
}