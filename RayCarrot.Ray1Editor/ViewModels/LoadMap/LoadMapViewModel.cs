using NLog;
using RayCarrot.UI;
using System.Collections.ObjectModel;
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

            // TODO: Move to UI manager
            var editGameWin = new EditGameWindow(editGameVM);
            editGameWin.ShowDialog();

            if (editGameWin.DialogResult != true)
                return;

            var game = new UserData_Game()
            {
                Name = editGameVM.GameName,
                Path = editGameVM.GamePath,
                Mode = editGameVM.GameMode
            };

            App.UserData.App_Games.Add(game);
            Games.Add(new LoadGameViewModel(this, game));

            Logger.Log(LogLevel.Info, "Added game with mode {0}", game.Mode);
        }

        public void LoadMap()
        {
            Logger.Log(LogLevel.Info, "Loading editor with mode {0}", SelectedGame.Game.Mode);

            // TODO: Verify the game path exists before loading the editor to avoid crash
            App.ChangeView(AppViewModel.AppView.Editor, new EditorViewModel(SelectedGame.Game, SelectedGame.Manager, SelectedLevel.Settings));

            // Set the app title
            App.SetTitle($"{SelectedLevel.Header}");
        }

        #endregion
    }
}