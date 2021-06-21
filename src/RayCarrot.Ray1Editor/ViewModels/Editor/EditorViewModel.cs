using NLog;
using RayCarrot.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
            Palettes = new ObservableCollection<PaletteEditorViewModel>();
            LevelAttributeFields = new ObservableCollection<EditorFieldViewModel>();
            Layers = new ObservableCollection<LayerEditorViewModel>();
            AvailableObjects = new ObservableCollection<string>();
            GameObjects = new ObservableCollection<GameObjectListItemViewModel>();
            ObjFields = new ObservableCollection<EditorFieldViewModel>();
            
            // Create commands
            LoadOtherMapCommand = new RelayCommand(LoadOtherMap);
            SaveCommand = new RelayCommand(Save);
            ResetPositionCommand = new RelayCommand(ResetPosition);
            DeleteSelectedObjectCommand = new RelayCommand(DeleteSelectedObject);
            AddObjCommand = new RelayCommand(AddObject);
            ZoomInCommand = new RelayCommand(() => EditorScene.Cam.SetZoom(CameraZoom + 0.2f));
            ZoomOutCommand = new RelayCommand(() => EditorScene.Cam.SetZoom(CameraZoom - 0.2f));
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Commands

        public ICommand LoadOtherMapCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ResetPositionCommand { get; }
        public ICommand DeleteSelectedObjectCommand { get; }
        public ICommand AddObjCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        #endregion

        #region Private Fields

        private GameObjectListItemViewModel _selectedGameObjectItem;

        #endregion

        #region Protected Properties

        protected Palette CurrentlySelectedPalette { get; set; }

        #endregion

        #region Public Properties

        // Game data
        public UserData_Game CurrentGame { get; }
        public GameManager CurrentGameManager { get; }
        public object CurrentGameSettings { get; }

        // Editor
        public bool IsEnabled { get; set; }
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
        public bool ShowObjects
        {
            get => EditorScene?.ShowObjects ?? true;
            set => EditorScene.ShowObjects = value;
        }
        public bool ShowObjectOffsets
        {
            get => EditorScene?.ShowObjectOffsets ?? false;
            set => EditorScene.ShowObjectOffsets = value;
        }
        public bool AnimateObjects
        {
            get => EditorScene?.State.AnimateObjects ?? true;
            set => EditorScene.State.AnimateObjects = value;
        }
        public bool ShowLinks
        {
            get => EditorScene?.ShowLinks ?? false;
            set => EditorScene.ShowLinks = value;
        }
        public string DebugText { get; set; }
        public float CameraZoom
        {
            get => EditorScene?.Cam?.Zoom ?? 0;
            set => EditorScene.Cam.Zoom = value;
        }
        public ObservableCollection<ActionViewModel> GameActions { get; set; }

        // General
        public ObservableCollection<PaletteEditorViewModel> Palettes { get; }
        public ObservableCollection<EditorFieldViewModel> LevelAttributeFields { get; }
        public bool HasLevelAttributeFields => LevelAttributeFields.Any();

        // Layers
        public ObservableCollection<LayerEditorViewModel> Layers { get; }

        // Objects
        public ObservableCollection<string> AvailableObjects { get; }
        public string ObjCountInfo { get; set; }
        public int SelectedNewObjIndex { get; set; }
        public bool IsSelectingObjFromList { get; set; }
        public ObservableCollection<GameObjectListItemViewModel> GameObjects { get; }
        public GameObjectListItemViewModel SelectedGameObjectItem
        {
            get => _selectedGameObjectItem;
            set
            {
                _selectedGameObjectItem = value;

                IsSelectingObjFromList = true;
                EditorScene.SelectedObject = value?.Obj;
                IsSelectingObjFromList = false;

                if (value != null)
                    EditorScene.GoToObject(value.Obj);
            }
        }

        // Properties
        public string SelectedObjectName { get; set; }
        public string SelectedObjectOffset { get; set; }
        public ObservableCollection<EditorFieldViewModel> ObjFields { get; }
        public string SelectedObjectScript { get; set; }

        #endregion

        #region Event Handlers

        private void Cam_ZoomChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CameraZoom));
        }

        #endregion

        #region Public Methods (from scene)

        public void OnEditorLoaded()
        {
            if (EditorScene.Stage == EditorScene.EditorStage.Error)
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;

            UpdateObjCountInfo();

            AvailableObjects.AddRange(CurrentGameManager.GetAvailableObjects(EditorScene.GameData));

            // Recreate the object fields
            RecreateObjFields();

            // Set up palettes
            Palettes.AddRange(EditorScene.GameData.TextureManager.Palettes.Select((x, i) => new PaletteEditorViewModel(x, i == 0, pal =>
            {
                EditorScene.GameData.TextureManager.SwapPalettes(CurrentlySelectedPalette, pal);
                CurrentlySelectedPalette = pal;
            })));

            CurrentlySelectedPalette = Palettes.FirstOrDefault()?.Palette;

            // Set up level attribute fields
            LevelAttributeFields.AddRange(CurrentGameManager.GetEditorLevelAttributeFields(EditorScene.GameData));
            OnPropertyChanged(nameof(HasLevelAttributeFields));

            foreach (var field in LevelAttributeFields)
                field.Refresh();

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

            // Get game actions
            var actions = CurrentGameManager.GetActions(EditorScene.GameData);
            GameActions = actions == null ? null : new ObservableCollection<ActionViewModel>(actions);

            OnPropertyChanged(nameof(CameraZoom));

            // Subscribe to events
            EditorScene.Cam.ZoomChanged += Cam_ZoomChanged;

            // Default to objects mode
            Mode = EditorMode.Objects;
        }

        public void OnSelectedObjectChanged(GameObject obj)
        {
            SelectedObject = obj;

            _selectedGameObjectItem = obj == null ? null : GameObjects.First(x => x.Obj == obj);
            OnPropertyChanged(nameof(SelectedGameObjectItem));

            SelectedObjectName = obj?.DisplayName;
            SelectedObjectOffset = obj?.SerializableData?.Offset?.ToString();
            SelectedObjectScript = obj?.Scripts;
        }

        public void OnModeChanged(EditorMode oldMode, EditorMode newMode) { }
        
        public void OnObjectAdded(GameObject obj)
        {
            GameObjects.Add(new GameObjectListItemViewModel(obj));
            UpdateObjCountInfo();
        }

        public void OnObjectRemoved(GameObject obj)
        {
            GameObjects.Remove(GameObjects.First(x => x.Obj == obj));
            UpdateObjCountInfo();
        }

        public void OnUpdate()
        {
            foreach (var l in Layers)
                l.Update();
        }

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
            App.SetTitle(null);
        }

        public void LoadEditor()
        {
            // Make sure any old editor instance gets unloaded
            UnloadEditor();

            var c = new EditorContext(CurrentGame.Path);
            c.AddSettings(Games.FromID(CurrentGame.GameID));

            // Create a new editor scene instance for the current game
            EditorScene = new EditorScene(
                manager: CurrentGameManager,
                context: c,
                gameSettings: CurrentGameSettings,
                colors: EditorColorProfileViewModel.GetViewModels.FirstOrDefault(x => x.ID == App.UserData.Theme_EditorColors)?.GetColorsFunc() ?? EditorColors.Colors_LightBlue);
        }

        public void UnloadEditor()
        {
            // Dispose the current instance (this will also unload the monogame resources)
            EditorScene?.Dispose();

            // Reset values
            IsEnabled = false;
            EditorScene = null;
            SelectedObject = null;
            DebugText = null;
            GameActions = null;
            AvailableObjects.Clear();
            SelectedObjectName = null;
            Palettes.Clear();
            LevelAttributeFields.Clear();
            Layers.Clear();
            ObjFields.Clear();
        }

        public void RecreateObjFields()
        {
            // Clear previous fields
            ObjFields.Clear();

            // Add general fields
            ObjFields.Add(new EditorPointFieldViewModel(
                header: "Position", 
                info: null, 
                getValueAction: () => SelectedObject.Position, 
                setValueAction: x => SelectedObject.Position = x, 
                min: Int32.MinValue, 
                max: Int32.MaxValue));
            
            // Add game-specific fields
            ObjFields.AddRange(CurrentGameManager.GetEditorObjFields(EditorScene.GameData, () => SelectedObject));
        }

        public void RefreshObjFields()
        {
            if (SelectedObject == null) 
                return;

            foreach (var field in ObjFields)
                field.Refresh();
        }

        public void DeleteSelectedObject()
        {
            if (SelectedObject != null)
                EditorScene.RemoveObject(SelectedObject);
        }

        public void AddObject()
        {
            EditorScene.AddObject(SelectedNewObjIndex);
        }

        public void UpdateObjCountInfo()
        {
            ObjCountInfo = $"({EditorScene.GameData.Objects.Count}/{CurrentGameManager.GetMaxObjCount(EditorScene.GameData)})";
        }

        public void ResetPosition()
        {
            EditorScene.Cam.ResetCamera();
        }

        public void DoAndPause(Action action)
        {
            var wasPaused = IsPaused;

            if (!wasPaused)
                IsPaused = true;

            try
            {
                action();
            }
            finally
            {
                if (!wasPaused)
                    IsPaused = false;
            }
        }

        public void Save()
        {
            try
            {
                EditorScene.Save();
                R1EServices.UI.DisplayMessage("Successfully saved", "Saved", DialogMessageType.Success);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Saving");
                R1EServices.UI.DisplayMessage($"An error occurred while saving. Error message: {ex.Message}", "Error saving", DialogMessageType.Error);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            UnloadEditor();
        }

        #endregion
    }
}