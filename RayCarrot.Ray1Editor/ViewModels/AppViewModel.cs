using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Config;
using NLog.Targets;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class AppViewModel : BaseViewModel
    {
        #region Singelton

        public static AppViewModel Instance { get; } = new();

        #endregion

        #region Constructor

        public AppViewModel()
        {
            Path_AppDataDir = $"AppUserData";
            Path_AppUserDataFile = Path.Combine(Path_AppDataDir, $"Settings.json");
            Path_LogFile = Path.Combine(Path_AppDataDir, $"Log.txt");
            Path_SerializerLogFile = Path.Combine(Path_AppDataDir, $"SerializerLog.txt");
        }

        #endregion

        #region Paths

        public string Path_AppDataDir { get; }
        public string Path_AppUserDataFile { get; }
        public string Path_LogFile { get; }
        public string Path_SerializerLogFile { get; }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Properties

        /// <summary>
        /// The current app version
        /// </summary>
        public Version CurrentAppVersion => new Version(0, 0, 0, 0);

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

        #endregion

        #region Public Methods

        public void OpenURL(string url)
        {
            try
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
                {
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Opening URL {0}", url);
            }
        }

        public void SetTitle(string state)
        {
            Title = "Ray1Editor";

            if (state != null)
                Title += $" - {state}";
            
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
            InitializeLogging(args.Any() && args[0] == "logtrace");

            Logger.Log(LogLevel.Info, "Initializing application with app version {0}", CurrentAppVersion);

            // Default the title
            SetTitle(null);

            // Create the data directory
            Directory.CreateDirectory(Path_AppDataDir);

            // Load the app user data
            LoadAppUserData();

            if (UserData.App_Version < CurrentAppVersion)
                PostUpdate();

            // Update the version
            UserData.App_Version = CurrentAppVersion;

            // Register encodings
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Change the view to the load map view
            ChangeView(AppView.LoadMap, new LoadMapViewModel());

            Logger.Log(LogLevel.Info, "Initialized application");
        }

        // ReSharper disable once RedundantAssignment
        public void InitializeLogging(bool logTrace)
        {
            var logConfig = new LoggingConfiguration();

#if DEBUG
            // On debug we force it to always log trace
            logTrace = true;
#endif

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            logConfig.AddRule(logTrace ? LogLevel.Trace : LogLevel.Info, LogLevel.Fatal, new FileTarget("logfile")
            {
                DeleteOldFileOnStartup = true,
                FileName = Path_LogFile
            });

            //MethodCallTarget target = new MethodCallTarget("logwindow", (logEvent, parms) =>
            //{

            //});
            //logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, target);

            // Apply config
            LogManager.Configuration = logConfig;
        }

        public void PostUpdate() { }

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
}