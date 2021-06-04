using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
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
            Manager = Game.Mode.GetManager();
            Levels.Clear();
            Levels.AddRange(Manager.GetLevels());
        }

        public void Edit()
        {
            var editGameVM = new EditGameViewModel()
            {
                GameName = Game.Name,
                GameMode = Game.Mode,
                GamePath = Game.Path
            };

            // TODO: Move to UI manager
            var editGameWin = new EditGameWindow(editGameVM);
            editGameWin.ShowDialog();

            if (editGameWin.DialogResult != true)
                return;

            Game.Name = editGameVM.GameName;
            Game.Path = editGameVM.GamePath;
            Game.Mode = editGameVM.GameMode;

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
        }

        public void Delete()
        {
            // Remove from the view model
            LoadMapViewModel.Games.Remove(this);

            // Clear the selection
            LoadMapViewModel.SelectedGame = null;

            // Remove from user data
            AppViewModel.Instance.UserData.Games.Remove(Game);
        }
    }
}