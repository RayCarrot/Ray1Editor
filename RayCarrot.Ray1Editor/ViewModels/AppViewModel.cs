using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        }

        #endregion

        #region Paths

        public string Path_AppDataDir { get; }
        public string Path_AppUserDataFile { get; }

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes the current view to the specified one
        /// </summary>
        /// <param name="view">The view to change to</param>
        /// <param name="vm">The view model for the new view</param>
        public void ChangeView(AppView view, AppViewBaseViewModel vm)
        {
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

        public void Initialize()
        {
            // Create the data directory
            Directory.CreateDirectory(Path_AppDataDir);

            // Load the app user data
            LoadAppUserData();

            if (UserData.AppVersion < CurrentAppVersion)
                PostUpdate();

            // Update the version
            UserData.AppVersion = CurrentAppVersion;

            // Change the view to the load map view
            ChangeView(AppView.LoadMap, new LoadMapViewModel());
        }

        public void PostUpdate() { }

        public void Unload()
        {
            // Dispose the current view model
            CurrentAppViewViewModel?.Dispose();

            // Save the app user data
            SaveAppUserData();
        }

        public void LoadAppUserData()
        {
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
                    // TODO: Log exception

                    MessageBox.Show($"An error occurred when loading the app user data. Error message: {ex.Message}");

                    ResetAppUserData();
                }
            }
            else
            {
                ResetAppUserData();
            }
        }

        public void ResetAppUserData()
        {
            UserData = new AppUserData();
            UserData.Reset();
        }

        public void SaveAppUserData()
        {
            // Serialize to JSON and save to file
            var json = JsonConvert.SerializeObject(UserData, Formatting.Indented, new StringEnumConverter());
            File.WriteAllText(Path_AppUserDataFile, json);
        }

        #endregion

        #region Data Types

        public enum AppView
        {
            Editor,
            LoadMap
        }

        #endregion
    }
}