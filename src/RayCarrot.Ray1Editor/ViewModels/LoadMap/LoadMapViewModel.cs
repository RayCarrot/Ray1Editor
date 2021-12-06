using NLog;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace RayCarrot.Ray1Editor
{
    public class LoadMapViewModel : AppViewBaseViewModel
    {
        #region Constructor

        public LoadMapViewModel()
        {
            // Set properties
            Games = new ObservableCollection<LoadGameViewModel>(App.UserData.App_Games.Select(x => new LoadGameViewModel(this, x)));

            // Create commands
            AddGameCommand = new RelayCommand(AddGame);
            LoadMapCommand = new RelayCommand(LoadMap);
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Commands

        public ICommand AddGameCommand { get; }
        public ICommand LoadMapCommand { get; }

        #endregion

        #region Public Properties

        public ObservableCollection<LoadGameViewModel> Games { get; }
        
        public LoadGameViewModel SelectedGame { get; set; }

        public LoadGameLevelViewModel SelectedLevel { get; set; }

        #endregion

        #region Public Methods

        public override void Initialize() { }

        public void AddGame()
        {
            var editGameVM = new EditGameViewModel();

            var result = R1EServices.UI.EditGame(editGameVM);

            if (!result)
                return;

            var game = new UserData_Game()
            {
                Name = editGameVM.GameName,
                Path = editGameVM.GamePath,
                GameID = editGameVM.SelectedGame.ID
            };

            App.UserData.App_Games.Add(game);
            Games.Add(new LoadGameViewModel(this, game));

            Logger.Log(LogLevel.Info, "Added game with mode {0}", game.GameID);
        }

        public void LoadMap()
        {
            Logger.Log(LogLevel.Info, "Loading editor with mode {0}", SelectedGame.Game.GameID);

            var gameData = SelectedGame.Game;
            var g = Ray1Editor.Games.FromID(gameData.GameID);

            // Verify the path exists
            if (g.PathType == GameModePathType.File && !File.Exists(gameData.Path) || 
                g.PathType == GameModePathType.Directory && !Directory.Exists(gameData.Path))
            {
                R1EServices.UI.DisplayMessage("The specified game path doesn't exist", "Path not found", DialogMessageType.Error);
                SelectedGame.Edit();
                return;
            }

            App.ChangeView(AppViewModel.AppView.Editor, new EditorViewModel(gameData, SelectedGame.Manager, SelectedLevel.Settings));

            // Set the app title
            App.SetTitle($"{SelectedLevel.Header}");
        }

        #endregion
    }
}