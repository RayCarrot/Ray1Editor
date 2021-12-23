using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using NLog;

namespace RayCarrot.Ray1Editor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : BaseWindow
{
    public MainWindow()
    {
        InitializeComponent();

        R1EServices.App.UserData.UI_WindowState?.ApplyToWindow(this);

        Loaded += MainWindow_LoadedAsync;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private async void MainWindow_LoadedAsync(object sender, RoutedEventArgs e)
    {
        var app = R1EServices.App;

        // First launch
        if (app.UserData.App_IsFirstLaunch)
        {
            var win = new FirstLaunchWindow();
            win.ShowDialog();
            app.UserData.App_IsFirstLaunch = false;
        }
        // Updated
        else if (app.HasUpdated)
        {
            // Show changelog
            var win = new ChangelogWindow();
            win.ShowDialog();
        }

        if (File.Exists(app.Path_UpdaterFile))
        {
            int retryTime = 0;

            // Wait until we can write to the file (i.e. it closing after an update)
            while (!R1EServices.File.CheckFileWriteAccess(app.Path_UpdaterFile))
            {
                retryTime++;

                // Try for 2 seconds first
                if (retryTime < 10)
                {
                    Logger.Log(LogLevel.Warn, "The updater can not be removed due to not having write access. Retrying {0}", retryTime);

                    await Task.Delay(100);
                }
                // Now it's taking a long time... Try for 10 more seconds
                else if (retryTime < 40)
                {
                    Logger.Log(LogLevel.Warn, "The updater can not be removed due to not having write access. Retrying {0}", retryTime);

                    await Task.Delay(200);
                }
                // Give up and let the deleting of the file give an error message
                else
                {
                    Logger.Log(LogLevel.Error, $"The updater can not be removed due to not having write access");
                    break;
                }
            }

            try
            {
                // Remove the updater
                File.Delete(app.Path_UpdaterFile);

                Logger.Log(LogLevel.Info, "The updater has been removed");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Deleting updater");
                R1EServices.UI.DisplayMessage($"The updater could not be deleted. Error message: {ex.Message}", "Error deleting updater", DialogMessageType.Error);
            }
        }

        if (app.UserData.Update_CheckOnLaunch)
            await R1EServices.App.CheckForUpdatesAsync(false, false);
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
        // Update the saved window state
        R1EServices.App.UserData.UI_WindowState = UserData_WindowSessionState.GetWindowState(this);
    }
}