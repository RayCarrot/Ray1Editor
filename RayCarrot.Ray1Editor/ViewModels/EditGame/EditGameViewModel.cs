using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class EditGameViewModel : BaseViewModel
    {
        #region Constructor

        public EditGameViewModel()
        {
            // Set properties
            AvailableGameModes = new ObservableCollection<GameMode>(Enum.GetValues(typeof(GameMode)).Cast<GameMode>());

            // Create commands
            BrowseCommand = new RelayCommand(Browse);
        }

        #endregion

        #region Commands

        public ICommand BrowseCommand { get; }

        #endregion

        #region Public Properties

        public string GameName { get; set; }
        public string GamePath { get; set; }
        public ObservableCollection<GameMode> AvailableGameModes { get; }
        public GameMode GameMode { get; set; }

        #endregion

        #region Public Methods

        public void Browse()
        {
            var pathType = GameMode.GetAttribute<GameModeInfoAttribute>().PathType;

            if (pathType == GameModePathType.File)
            {
                // Create the dialog
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Title = "Select the game file",
                    FileName = GamePath,
                    CheckFileExists = true,
                };

                // Show the dialog and get the result
                if (openFileDialog.ShowDialog() == true)
                    GamePath = openFileDialog.FileName;
            }
            else if (pathType == GameModePathType.Directory)
            {
                using var dialog = new CommonOpenFileDialog
                {
                    Title = "Select the game directory",
                    InitialDirectory = GamePath,
                    AllowNonFileSystemItems = false,
                    IsFolderPicker = true,
                    EnsureFileExists = true,
                    EnsurePathExists = true
                };

                // Show the dialog
                var dialogResult = dialog.ShowDialog(Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive));

                if (dialogResult == CommonFileDialogResult.Ok)
                    GamePath = dialog.FileName;
            }
        }

        #endregion
    }
}