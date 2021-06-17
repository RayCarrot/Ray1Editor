using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using NLog;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class LoadGameViewModel : BaseViewModel
    {
        public LoadGameViewModel(LoadMapViewModel loadMapViewModel, UserData_Game game)
        {
            // Set properties
            LoadMapViewModel = loadMapViewModel;
            Game = game;
            Levels = new ObservableCollection<LoadGameLevelViewModel>();
            Header = Game.Name;

            // Create commands
            EditCommand = new RelayCommand(Edit);
            OpenFolderCommand = new RelayCommand(OpenFolder);
            DeleteCommand = new RelayCommand(Delete);

            // Load game mode
            LoadGameMode();
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ICommand EditCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand DeleteCommand { get; }

        public LoadMapViewModel LoadMapViewModel { get; }
        public UserData_Game Game { get; }
        public GameManager Manager { get; protected set; }
        public bool IsSelected { get; set; }
        public ObservableCollection<LoadGameLevelViewModel> Levels { get; }

        public string Header { get; set; }

        public void LoadGameMode()
        {
            var game = Games.FromID(Game.GameID);
            Manager = game.GetManager();
            Levels.Clear();

            try
            {
                Levels.AddRange(Manager.GetLevels(game, Game.Path));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, ex, "Getting levels");
            }
        }

        public void Edit()
        {
            var editGameVM = new EditGameViewModel()
            {
                GameName = Game.Name,
                SelectedGameIndex = Games.LoadedGames.FindItemIndex(x => x.ID == Game.GameID),
                GamePath = Game.Path
            };

            var result = AppViewModel.Instance.UI.EditGame(editGameVM);

            if (!result)
                return;

            Game.Name = editGameVM.GameName;
            Game.Path = editGameVM.GamePath;
            Game.GameID = editGameVM.SelectedGame.ID;

            Header = Game.Name;

            LoadGameMode();
        }

        public void OpenFolder()
        {
            string path = Game.Path;

            // TODO: Move to file manager
            if (File.Exists(path))
                Process.Start("explorer.exe", "/select, \"" + path + "\"")?.Dispose();
            else if (Directory.Exists(path))
                Process.Start("explorer.exe", path)?.Dispose();

            Logger.Log(LogLevel.Trace, "Opened game path");
        }

        public void Delete()
        {
            // Remove from the view model
            LoadMapViewModel.Games.Remove(this);

            // Clear the selection
            LoadMapViewModel.SelectedGame = null;

            // Remove from user data
            AppViewModel.Instance.UserData.App_Games.Remove(Game);

            Logger.Log(LogLevel.Trace, "Removed game with mode {0}", Game.GameID);
        }
    }
}