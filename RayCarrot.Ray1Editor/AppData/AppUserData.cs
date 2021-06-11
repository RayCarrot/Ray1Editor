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
            App_Version = AppViewModel.Instance.CurrentAppVersion;
            UI_WindowState = null;
            App_Games = new List<UserData_Game>();
            Serializer_EnableLog = false;
            Theme_Dark = true;
            Theme_Sync = false;
        }

        /// <summary>
        /// Verifies and corrects any invalid values
        /// </summary>
        public void Verify()
        {
            App_Version ??= AppViewModel.Instance.CurrentAppVersion;
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

        /// <summary>
        /// The window state
        /// </summary>
        public UserData_WindowSessionState UI_WindowState { get; set; }

        /// <summary>
        /// Indicates if the serializer log is enabled
        /// </summary>
        public bool Serializer_EnableLog { get; set; }

        public bool Serializer_CreateBackupOnWrite { get; set; }

        public bool Theme_Dark { get; set; }
        public bool Theme_Sync { get; set; }


        // TODO: Save obj field panel resized size
    }
}