using BinarySerializer;
using RayCarrot.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.Ray1Editor
{
    public class EditorViewModel : AppViewBaseViewModel
    {
        #region Constructor

        public EditorViewModel(UserData_Game currentGame, GameManager currentGameManager, object currentGameSettings)
        {
            // Set properties
            CurrentGame = currentGame;
            CurrentGameManager = currentGameManager;
            CurrentGameSettings = currentGameSettings;
            Layers = new ObservableCollection<LayerEditorViewModel>();
            GameObjects = new ObservableCollection<GameObjectListItemViewModel>();
            ObjFields = new ObservableCollection<EditorFieldViewModel>();

            // Create commands
            LoadOtherMapCommand = new RelayCommand(LoadOtherMap);
            ResetPositionCommand = new RelayCommand(ResetPosition);
            SaveCommand = new RelayCommand(Save);
        }

        #endregion

        #region Commands

        public ICommand LoadOtherMapCommand { get; }
        public ICommand ResetPositionCommand { get; }
        public ICommand SaveCommand { get; }

        #endregion

        #region Private Fields

        private GameObjectListItemViewModel _selectedGameObjectItem;

        #endregion

        #region Public Properties

        // Game data
        public UserData_Game CurrentGame { get; }
        public GameManager CurrentGameManager { get; }
        public object CurrentGameSettings { get; }

        // Editor
        public EditorScene EditorScene { get; set; }
        public EditorMode Mode
        {
            get => EditorScene?.Mode ?? EditorMode.None;
            set => EditorScene.Mode = value;
        }
        public GameObject SelectedObject { get; set; }
        public bool IsPaused
        {
            get => EditorScene?.IsPaused ?? false;
            set => EditorScene.IsPaused = value;
        }
        public string DebugText { get; set; }

        // Layers
        public ObservableCollection<LayerEditorViewModel> Layers { get; }

        // Objects
        public ObservableCollection<GameObjectListItemViewModel> GameObjects { get; }
        public GameObjectListItemViewModel SelectedGameObjectItem
        {
            get => _selectedGameObjectItem;
            set
            {
                _selectedGameObjectItem = value;
                EditorScene.SelectedObject = value.Obj;
                EditorScene.GoToObject(value.Obj);
            }
        }

        // Properties
        public string SelectedObjectName { get; set; }
        public ObservableCollection<EditorFieldViewModel> ObjFields { get; }

        #endregion

        #region Public Methods (from scene)

        public void OnEditorLoaded()
        {
            // Set up layers
            Layers.AddRange(EditorScene.GameData.Layers.Select(x => new LayerEditorViewModel(x)));

            // Create layer fields
            foreach (var layer in Layers)
            {
                layer.RecreateFields();
                layer.RefreshFields();
            }

            // Create object items
            GameObjects.AddRange(EditorScene.GameData.Objects.Select(x => new GameObjectListItemViewModel(x)));

            // Default to objects mode
            Mode = EditorMode.Objects;
        }

        public void OnSelectedObjectChanged(GameObject obj)
        {
            SelectedObject = obj;

            _selectedGameObjectItem = obj == null ? null : GameObjects.First(x => x.Obj == obj);
            OnPropertyChanged(nameof(SelectedGameObjectItem));

            SelectedObjectName = obj?.PrimaryName;
            RefreshObjFields();
        }

        public void OnModeChanged(EditorMode oldMode, EditorMode newMode) { }

        #endregion

        #region Public Methods

        public override void Initialize()
        {
            // Load the editor
            LoadEditor();
        }

        public void LoadOtherMap()
        {
            App.ChangeView(AppViewModel.AppView.LoadMap, new LoadMapViewModel());
        }

        public void LoadEditor()
        {
            // Make sure any old editor instance gets unloaded
            UnloadEditor();

            // Recreate the fields
            RecreateObjFields();
            
            // Create a new editor scene instance for the current game
            EditorScene = new EditorScene(
                manager: CurrentGameManager,
                context: new Context(CurrentGame.Path),
                gameSettings: CurrentGameSettings);
        }

        public void UnloadEditor()
        {
            // Dispose the current instance (this will also unload the monogame resources)
            EditorScene?.Dispose();

            // Reset values
            EditorScene = null;
            SelectedObject = null;
            DebugText = null;
            SelectedObjectName = null;
            Layers.Clear();
            ObjFields.Clear();
        }

        public void RecreateObjFields()
        {
            // Clear previous fields
            ObjFields.Clear();

            // Add general fields
            // TODO: Update position when object moves
            ObjFields.Add(new EditorPointFieldViewModel(
                header: "Position", 
                info: null, 
                getValueAction: () => SelectedObject.Position, 
                setValueAction: x => SelectedObject.Position = x, 
                min: Int32.MinValue, 
                max: Int32.MaxValue));
            
            // Add game-specific fields
            ObjFields.AddRange(CurrentGameManager.GetEditorObjFields(() => SelectedObject));
        }

        public void RefreshObjFields()
        {
            if (SelectedObject == null) 
                return;

            foreach (var field in ObjFields)
                field.Refresh();
        }

        public void ResetPosition()
        {
            EditorScene.Cam.ResetCamera();
        }

        public void Save()
        {
            // TODO: Try/catch
            EditorScene.Save();
            MessageBox.Show("Saved!"); // TODO: Have custom dialog window
        }

        public override void Dispose()
        {
            base.Dispose();

            UnloadEditor();
        }

        #endregion
    }
}