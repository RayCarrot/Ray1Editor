﻿using MahApps.Metro.Controls;
using NLog;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A base window to inherit from
    /// </summary>
    public class BaseWindow : MetroWindow
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseWindow()
        {
            Logger.Log(LogLevel.Info, "A window is being created");

            // Default to true
            CloseWithEscape = true;

            // Set minimum size
            MinWidth = 400;
            MinHeight = 300;

            // Set title style
            TitleCharacterCasing = CharacterCasing.Normal;

            // Set icon style
            //Icon = new ImageSourceConverter().ConvertFromString(AppViewModel.WPFApplicationBasePath + "Img/RCP_Metro.ico") as ImageSource;
            IconBitmapScalingMode = BitmapScalingMode.NearestNeighbor;

            // Set owner window
            Owner = Application.Current?.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive);

            Logger.Log(LogLevel.Trace, "The owner window has been set to {0}", Owner?.ToString() ?? "null");

            // Do not show in the task bar if the window has a owner, is not the main window and a main window has been created
            if (Owner != null && Application.Current?.MainWindow != null && this != Application.Current.MainWindow)
                ShowInTaskbar = false;

            // Due to a WPF glitch the main window needs to be focused upon closing
            Closed += (_, _) =>
            {
                if (this != Application.Current.MainWindow)
                    Application.Current.MainWindow?.Focus();
            };

            // Set transition
            //WindowTransitionsEnabled = RCPServices.Data?.EnableAnimations ?? true;

            Logger.Log(LogLevel.Info, "The window {0} has been created", this);

            PreviewKeyDown += (_, e) =>
            {
                if (CloseWithEscape && e.Key == Key.Escape)
                    Close();
            };
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Shows the <see cref="Window"/> as a dialog
        /// </summary>
        public new void ShowDialog()
        {
            // Set startup location
            WindowStartupLocation = Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;

            // Show the window as a dialog
            base.ShowDialog();
        }

        /// <summary>
        /// Indicates if the escape key can be used to close the window
        /// </summary>
        public bool CloseWithEscape { get; set; }
    }
}