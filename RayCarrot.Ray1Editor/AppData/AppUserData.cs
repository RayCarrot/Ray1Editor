using System;
using System.Collections.Generic;
using System.IO;

namespace RayCarrot.Ray1Editor
{
    public class AppUserData
    {
        /// <summary>
        /// Resets all properties to their default values
        /// </summary>
        public void Reset()
        {
            AppVersion = AppViewModel.Instance.CurrentAppVersion;
            WindowState = null;
            Games = new List<UserData_Game>();
        }

        /// <summary>
        /// Verifies and corrects any invalid values
        /// </summary>
        public void Verify()
        {
            AppVersion ??= AppViewModel.Instance.CurrentAppVersion;
            Games ??= new List<UserData_Game>();

            if (!File.Exists(SerializerLogFilePath))
                EnableSerializerLog = false;
        }

        /// <summary>
        /// The last recorded app version. Used to check if an update has occurred since the last run.
        /// </summary>
        public Version AppVersion { get; set; }

        /// <summary>
        /// The window state
        /// </summary>
        public UserData_WindowSessionState WindowState { get; set; }

        /// <summary>
        /// The added games
        /// </summary>
        public List<UserData_Game> Games { get; set; }

        public bool EnableSerializerLog { get; set; }
        public string SerializerLogFilePath { get; set; }

        // TODO: Save obj field panel resized size
        // TODO: Save theme (dark/light, color, sync)
    }
}