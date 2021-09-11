using System;
using ControlzEx.Theming;
using NLog;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using NLog.Targets;

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
                // Log the exception
                Logger.Fatal(e?.Exception, "Unhandled exception");

                // Get the path to log to
                string logPath = Path.Combine(Directory.GetCurrentDirectory(), "crashlog.txt");

                // Write log
                File.WriteAllLines(logPath, LogManager.Configuration.FindTargetByName<MemoryTarget>("memory")?.Logs ?? new string[]
                {
                    "Service not available",
                    Environment.NewLine,
                    e?.Exception?.ToString()
                });

                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e?.Exception?.Message}" +
                                $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}A crash log has been created under {logPath}.",
                    "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e?.Exception?.Message}",
                    "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
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