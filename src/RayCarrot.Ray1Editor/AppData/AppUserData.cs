using System;
using System.Collections.Generic;

namespace RayCarrot.Ray1Editor
{
    public class AppUserData
    {
        /// <summary>
        /// Default constructor. This will always reset the app data to ensure any values missed when reading will still be set correctly.
        /// </summary>
        public AppUserData() => Reset();

        /// <summary>
        /// Resets all properties to their default values
        /// </summary>
        public void Reset()
        {
            App_Version = R1EServices.App.CurrentAppVersion;
            App_Games = new List<UserData_Game>();
            App_IsFirstLaunch = true;
            UI_WindowState = null;
            UI_EditorTabsWidth = 350;
            Editor_PauseWhenInactive = true;
            Serializer_EnableLog = false;
            Serializer_CreateBackupOnWrite = true;
            Theme_Dark = true;
            Theme_Sync = false;
            PS1_mkpsxisoPath = null;
            Update_CheckOnLaunch = true;
            Update_GetBeta = false;
        }

        /// <summary>
        /// Verifies and corrects any invalid values
        /// </summary>
        public void Verify()
        {
            App_Version ??= R1EServices.App.CurrentAppVersion;
            App_Games ??= new List<UserData_Game>();
        }

        /// <summary>
        /// The last recorded app version. Used to check if an update has occurred since the last run.
        /// </summary>
        public Version App_Version { get; set; }

        /// <summary>
        /// The added games
        /// </summary>
        public List<UserData_Game> App_Games { get; set; }

        public bool App_IsFirstLaunch { get; set; }

        /// <summary>
        /// The window state
        /// </summary>
        public UserData_WindowSessionState UI_WindowState { get; set; }

        /// <summary>
        /// Indicates if the editor should automatically pause when not active
        /// </summary>
        public bool Editor_PauseWhenInactive { get; set; }

        public double UI_EditorTabsWidth { get; set; }

        /// <summary>
        /// Indicates if the serializer log is enabled
        /// </summary>
        public bool Serializer_EnableLog { get; set; }

        public bool Serializer_CreateBackupOnWrite { get; set; }

        public bool Theme_Dark { get; set; }
        public bool Theme_Sync { get; set; }

        public string PS1_mkpsxisoPath { get; set; }

        public bool Update_CheckOnLaunch { get; set; }
        public bool Update_GetBeta { get; set; }
    }
}