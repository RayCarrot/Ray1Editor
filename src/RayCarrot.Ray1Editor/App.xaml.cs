using ControlzEx.Theming;
using NLog;
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
        /// <summary>
        /// The application base path to use for WPF related operations
        /// </summary>
        public const string WPFAppBasePath = "pack://application:,,,/RayCarrot.Ray1Editor;component/";

        public const string EditorIntFieldTemplateKey = "EditorIntFieldTemplate";
        public const string EditorStringFieldTemplateKey = "EditorStringFieldTemplate";
        public const string EditorBoolFieldTemplateKey = "EditorBoolFieldTemplate";
        public const string EditorDropDownFieldTemplateKey = "EditorDropDownFieldTemplate";
        public const string EditorPointFieldTemplateKey = "EditorPointFieldTemplate";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public new static App Current => (App)Application.Current;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Initialize the view model
            R1EServices.App.Initialize(e.Args);

            // Update the theme
            UpdateTheme();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            R1EServices.App.Unload();
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

        public void UpdateTheme()
        {
            var data = R1EServices.App.UserData;
            const string color = "Purple";

            if (data.Theme_Sync)
            {
                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
                ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
            }
            else
            {
                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.DoNotSync;
                ThemeManager.Current.ChangeTheme(this, $"{(data.Theme_Dark ? "Dark" : "Light")}.{color}");
            }

            Logger.Log(LogLevel.Trace, "Updated the theme");
        }
    }
}