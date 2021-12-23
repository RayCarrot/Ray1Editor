using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.Ray1Editor;

public class AppViewModel : BaseViewModel
{
    #region Constructor

    public AppViewModel()
    {
        Path_AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ray1Editor");
        Path_AppUserDataFile = Path.Combine(Path_AppDataDir, $"Settings.json");
        Path_LogFile = Path.Combine(Path_AppDataDir, $"Log.txt");
        Path_ArchivedLogFile = Path.Combine(Path_AppDataDir, $"Log_archived.txt");
        Path_SerializerLogFile = Path.Combine(Path_AppDataDir, $"SerializerLog.txt");
        Path_UpdaterFile = Path.Combine(Path_AppDataDir, $"Updater.exe");

        CloseAppCommand = new RelayCommand(() => App.Current.Shutdown());
    }

    #endregion

    #region Paths

    public string Path_AppDataDir { get; }
    public string Path_AppUserDataFile { get; }
    public string Path_LogFile { get; }
    public string Path_ArchivedLogFile { get; }
    public string Path_SerializerLogFile { get; }
    public string Path_UpdaterFile { get; }

    #endregion

    #region URLs

    public string Url_Ray1EditorHome => "https://raym.app/ray1editor";
    public string Url_Ray1EditorUpdateManifest => "https://raym.app/ray1editor/update_manifest.json";
    public string Url_Ray1EditorGitHub => "https://github.com/RayCarrot/RayCarrot.Ray1Editor";
    public string Url_Ray1Map => "https://raym.app/maps_r1";

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand CloseAppCommand { get; }

    #endregion

    #region Private Properties

    private bool CheckingForUpdates { get; set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The current app version
    /// </summary>
    public Version CurrentAppVersion => new Version(0, 1, 2, 0);

    /// <summary>
    /// Indicates if the current version is a BETA version
    /// </summary>
    public bool IsBeta => true;

    /// <summary>
    /// The loaded app user data
    /// </summary>
    public AppUserData UserData { get; set; }

    /// <summary>
    /// The currently app view
    /// </summary>
    public AppView CurrentAppView { get; protected set; }

    /// <summary>
    /// THe view model for the current app view
    /// </summary>
    public AppViewBaseViewModel CurrentAppViewViewModel { get; protected set; }

    /// <summary>
    /// The current application title
    /// </summary>
    public string Title { get; protected set; }

    /// <summary>
    /// Indicates if an update was performed prior to launching this instance
    /// </summary>
    public bool HasUpdated { get; protected set; }

    #endregion

    #region Public Methods

    public void SetTitle(string state)
    {
        Title = "Ray1Editor";

        if (IsBeta)
            Title += $" (BETA)";

        if (state != null)
            Title += $" - {state}";
        else
            Title += $" {CurrentAppVersion}";

        Logger.Log(LogLevel.Trace, "Title set to {0}", Title);
    }

    /// <summary>
    /// Changes the current view to the specified one
    /// </summary>
    /// <param name="view">The view to change to</param>
    /// <param name="vm">The view model for the new view</param>
    public void ChangeView(AppView view, AppViewBaseViewModel vm)
    {
        Logger.Log(LogLevel.Info, "Changing view from {0} to {1}", CurrentAppView, view);

        // Dispose the current view model
        CurrentAppViewViewModel?.Dispose();

        // Save the app user data
        SaveAppUserData();

        // Set the new view
        CurrentAppViewViewModel = vm;
        CurrentAppView = view;

        // Initialize the view model
        CurrentAppViewViewModel.Initialize();
    }

    public void Initialize(string[] args)
    {
        // Create the data directory
        Directory.CreateDirectory(Path_AppDataDir);

        InitializeLogging(args);

        Logger.Log(LogLevel.Info, "Initializing application with app version {0}", CurrentAppVersion);

        // Default the title
        SetTitle(null);

        // Load the app user data
        LoadAppUserData();

        if (UserData.App_Version < CurrentAppVersion)
            PostUpdate(UserData.App_Version);

        // Update the version
        UserData.App_Version = CurrentAppVersion;

        // Register encodings
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Change the view to the load map view
        ChangeView(AppView.LoadMap, new LoadMapViewModel());

        Logger.Log(LogLevel.Info, "Initialized application");
    }

    public void InitializeLogging(string[] args)
    {
        // Create a new logging configuration
        var logConfig = new LoggingConfiguration();

#if DEBUG
        // On debug we default it to log trace
        LogLevel logLevel = LogLevel.Trace;
#else
            // If not on debug we default to log info
            LogLevel logLevel = LogLevel.Info;
#endif

        // Allow the log level to be specified from a launch argument
        if (args.Contains("-loglevel"))
        {
            string argLogLevel = args[args.FindItemIndex(x => x == "-loglevel") + 1];
            logLevel = LogLevel.FromString(argLogLevel);
        }

        const string logLayout = "${time:invariant=true}|${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}";
        bool logToFile = !args.Contains("-nofilelog");
        bool logToMemory = !args.Contains("-nomemlog");
        bool logToViewer = args.Contains("-logviewer");

        // Log to file
        if (logToFile)
        {
            logConfig.AddRule(logLevel, LogLevel.Fatal, new FileTarget("file")
            {
                // Archive a maximum of 5 logs. This makes it easier going back to check errors which happened on older instances of the app.
                ArchiveOldFileOnStartup = true,
                ArchiveFileName = Path_ArchivedLogFile,
                MaxArchiveFiles = 5,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    
                // Keep the file open and disable concurrent writes to improve performance
                KeepFileOpen = true,
                ConcurrentWrites = false,

                // Set the file path and layout
                FileName = Path_LogFile,
                Layout = logLayout,
            });
        }

        if (logToMemory)
        {
            logConfig.AddRule(logLevel, LogLevel.Fatal, new MemoryTarget("memory")
            {
                Layout = logLayout,
            });
        }

        // Log to log viewer
        if (logToViewer)
        {
            // TODO: Add a log viewer
            //LogViewerViewModel = new LogViewerViewModel();

            //// Always log from trace to fatal to include all logs
            //logConfig.AddRuleForAllLevels(new MethodCallTarget("logviewer", async (logEvent, _) =>
            //{
            //    // Await to avoid blocking
            //    await App.Current.Dispatcher.InvokeAsync(() =>
            //    {
            //        var log = new LogItemViewModel(logEvent.Level, logEvent.Exception, logEvent.TimeStamp, logEvent.LoggerName, logEvent.FormattedMessage);
            //        log.IsVisible = log.LogLevel >= LogViewerViewModel.ShowLogLevel;
            //        LogViewerViewModel.LogItems.Add(log);
            //    });
            //}));
        }

        // Apply config
        LogManager.Configuration = logConfig;
    }

    public void PostUpdate(Version prevVersion)
    {
        HasUpdated = true;
    }

    public async Task CheckForUpdatesAsync(bool isManualSearch, bool forceUpdate)
    {
        if (CheckingForUpdates)
            return;

        try
        {
            CheckingForUpdates = true;

            // Check for updates
            var result = await R1EServices.Updater.CheckAsync(forceUpdate && isManualSearch, UserData.Update_GetBeta || IsBeta);

            // Check if there is an error
            if (result.ErrorMessage != null)
            {
                Logger.Log(LogLevel.Error, result.Exception, "Checking for updates");

                R1EServices.UI.DisplayMessage($"The update check failed. Error message: {result.ErrorMessage}{Environment.NewLine}To manually update the app, go to {Url_Ray1EditorHome} and download the latest version.", "Error checking for updates", DialogMessageType.Error);

                return;
            }

            // Check if no new updates were found
            if (!result.IsNewUpdateAvailable)
            {
                if (isManualSearch)
                    R1EServices.UI.DisplayMessage($"The latest version {CurrentAppVersion} is already installed", "No new version found", DialogMessageType.Information); 

                return;
            }

            try
            {
                var updateMessage = !result.IsBetaUpdate
                    ? $"A new update is available to download. Download now?{Environment.NewLine}{Environment.NewLine}" +
                      $"News: {Environment.NewLine}{result.DisplayNews}"
                    : $"A new beta update is available to download. Download now?{Environment.NewLine}{Environment.NewLine}" +
                      $"News: {Environment.NewLine}{result.DisplayNews}";

                if (R1EServices.UI.DisplayMessage(updateMessage, "New update available", DialogMessageType.Information, true))
                {
                    // Launch the updater
                    var succeeded = R1EServices.Updater.Update(result, false);

                    if (!succeeded)
                        Logger.Log(LogLevel.Warn, "The updater failed to update the program");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Updating Ray1Editor");
                R1EServices.UI.DisplayMessage($"AN error occurred while updating. Error message: {ex.Message}", "Error updating", DialogMessageType.Error);
            }
        }
        finally
        {
            CheckingForUpdates = false;
        }
    }

    public void Unload()
    {
        Logger.Log(LogLevel.Info, "Unloading the application");

        // Dispose the current view model
        CurrentAppViewViewModel?.Dispose();

        // Save the app user data
        SaveAppUserData();

        Logger.Log(LogLevel.Info, "Unloaded the application");

        // Shut down the logger
        LogManager.Shutdown();
    }

    public void LoadAppUserData()
    {
        Logger.Log(LogLevel.Info, "Loading app user data from {0}", Path_AppUserDataFile);

        if (File.Exists(Path_AppUserDataFile))
        {
            try
            {
                var json = File.ReadAllText(Path_AppUserDataFile);
                UserData = JsonConvert.DeserializeObject<AppUserData>(json, new StringEnumConverter());

                if (UserData == null)
                    throw new Exception($"User data was empty");

                UserData.Verify();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Error loading app user data");

                MessageBox.Show($"An error occurred when loading the app user data. Error message: {ex.Message}");

                ResetAppUserData();
            }
        }
        else
        {
            Logger.Log(LogLevel.Info, "No app user data found");
            ResetAppUserData();
        }
    }

    public void ResetAppUserData()
    {
        UserData = new AppUserData();

        Logger.Log(LogLevel.Info, "Reset the app user data");
    }

    public void SaveAppUserData()
    {
        Logger.Log(LogLevel.Trace, "Saving the app user data");

        // Serialize to JSON and save to file
        try
        {
            var json = JsonConvert.SerializeObject(UserData, Formatting.Indented, new StringEnumConverter());
            File.WriteAllText(Path_AppUserDataFile, json);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Fatal, ex, "Error saving the app user data");
            throw;
        }

        Logger.Log(LogLevel.Info, "Saved the app user data");
    }

    #endregion

    #region Data Types

    public enum AppView
    {
        None,
        Editor,
        LoadMap
    }

    #endregion
}