using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string EditorIntFieldTemplateKey = "EditorIntFieldTemplate";
        public const string EditorBoolFieldTemplateKey = "EditorBoolFieldTemplate";
        public const string EditorDropDownFieldTemplateKey = "EditorDropDownFieldTemplate";
        public const string EditorPointFieldTemplateKey = "EditorPointFieldTemplate";

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppViewModel.Instance.Initialize();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            AppViewModel.Instance.Unload();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                File.WriteAllText($"crashlog.txt", e.Exception.ToString());
            }
            catch
            {
                // ignored
            }
        }
    }
}