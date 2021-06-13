using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using NLog;
using System;
using System.Linq;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// The editor scene
    /// </summary>
    public class EditorScene : WpfGame
    {
        #region Constructor

        public EditorScene(GameManager manager, Context context, object gameSettings)
        {
            GameManager = manager;
            Context = context;
            GameSettings = gameSettings;

            EditorUpdateData = new EditorUpdateData();
            State = new EditorState();

            Mouse = new WpfMouse(this);
            Keyboard = new WpfKeyboard(this);
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Private Fields

        private GameObject _selectedObject;
        private EditorMode _mode;

        #endregion

        #region Protected Properties

        protected EditorUpdateData EditorUpdateData { get; }

        #endregion

        #region Public Properties

        // General
        public EditorViewModel VM => (EditorViewModel)DataContext;
        public bool PauseWhenInactive => AppViewModel.Instance.UserData.Editor_PauseWhenInactive;
        public bool IsPaused { get; set; }
        public EditorState State { get; }
        public Context Context { get; }

        // Mode
        public EditorMode Mode
        {
            get => _mode;
            set
            {
                // Clear selections
                SelectedObject = null;
                IsDraggingObject = false;
                SelectedLinkObject = null;
                IsDraggingLink = false;

                foreach (var l in GameData.Layers)
                    l.OnModeChanged(_mode, value);

                VM.OnModeChanged(_mode, value);
                Logger.Log(LogLevel.Info, "Changed editor mode from {0} to {1}", _mode, value);

                _mode = value;
            }
        }
        public bool CanHoverOverObject => Mode is EditorMode.Objects or EditorMode.Links;

        // Camera
        public Camera Cam { get; protected set; }

        // Game
        public GameManager GameManager { get; }
        public object GameSettings { get; }
        public GameData GameData { get; protected set; }

        // Objects
        public bool ShowObjects { get; set; } = true;
        public GameObject HoverObject { get; protected set; }
        public GameObject SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject == value)
                    return;

                _selectedObject = value;
                VM.OnSelectedObjectChanged(_selectedObject);
                Logger.Log(LogLevel.Trace, "Selected object {0}", SelectedObject?.DisplayName ?? "null");
            }
        }
        public bool IsDraggingObject { get; protected set; }
        //public Point DraggingObjectOffset { get; protected set; }
        public Vector2 PrevMousePos { get; protected set; }
        public Point DraggingObjectInitialPosition { get; protected set; }

        // Links
        public bool IsDraggingLink { get; protected set; }
        public GameObject SelectedLinkObject { get; protected set; }

        #endregion

        #region Protected Properties

        // MonoGame
        protected SpriteBatch SpriteBatch { get; set; }
        protected WpfMouse Mouse { get; }
        protected WpfKeyboard Keyboard { get; }

        #endregion

        #region Protected Methods

        protected override void Initialize()
        {
            Logger.Log(LogLevel.Info, "Initializing the editor scene");
            Logger.Log(LogLevel.Trace, "Editor scene is initializing with manager {0} and a context with the base path {1}", GameManager, Context.BasePath);

            // Create the graphics service
            _ = new WpfGraphicsDeviceService(this);

            // Create a sprite batch
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Create the camera
            Cam = new Camera(GraphicsDevice.Viewport);

            // Reset the camera
            Cam.ResetCamera();

            // Initialize the base game, thus loading the content
            base.Initialize();

            // Notify the view model
            VM.OnEditorLoaded();

            Logger.Log(LogLevel.Info, "Initialized the editor scene");
        }

        protected override void LoadContent()
        {
            Logger.Log(LogLevel.Info, "Loading the editor content");

            // Load the game data
            using (Context)
                GameData = GameManager.Load(Context, GameSettings, new TextureManager(GraphicsDevice));

            Logger.Log(LogLevel.Trace, "Loading the editor elements");

            // Load elements
            GameData.LoadElements(this);

            // Load editor textures
            State.EditorTextures = new EditorTextures(GameData.TextureManager);
            State.EditorTextures.Init();

            // Post-load
            GameManager.PostLoad(GameData);

            Logger.Log(LogLevel.Trace, "Loading the editor objects");

            // Load objects
            foreach (var obj in GameData.Objects)
                obj.Load();

            Logger.Log(LogLevel.Trace, "Initializing the object links");

            // Initialize object links
            InitializeObjLinks();

            Logger.Log(LogLevel.Trace, "Calculating the map size");

            // Calculate the map size
            State.UpdateMapSize(GameData);

            // Load base content
            base.LoadContent();
        }

        protected void InitializeObjLinks()
        {
            foreach (var notLinkedObj in GameData.Objects.Where(x => x.LinkGroup == 0 && x.CanBeLinkedToGroup))
                notLinkedObj.LinkGripPosition = notLinkedObj.Position;

            foreach (var linkedObj in GameData.Objects.Where(x => x.LinkGroup != 0 && x.CanBeLinkedToGroup).GroupBy(x => x.LinkGroup))
            {
                var prev = linkedObj.Last();
                prev.LinkGripPosition = prev.Position;

                foreach (var e in linkedObj)
                {
                    e.LinkGripPosition = prev.LinkGripPosition;
                    prev = e;
                }
            }
        }

        protected override void UnloadContent()
        {
            Logger.Log(LogLevel.Info, "Unloading the editor scene");

            base.UnloadContent();

            SpriteBatch?.Dispose();
            GameData?.Dispose();
            Primitives2D.Dipose();

            Logger.Log(LogLevel.Info, "Unloaded the editor scene");
        }

        protected bool GetIsEditorPaused() => PauseWhenInactive && !IsActive || IsPaused;

        protected override void Update(GameTime gameTime)
        {
            if (GetIsEditorPaused())
                return;

            // Update base
            base.Update(gameTime);

            EditorUpdateData.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            EditorUpdateData.Mouse = Mouse.GetState();
            EditorUpdateData.MousePosition = Cam.ToWorld(EditorUpdateData.Mouse.Position.ToVector2());
            EditorUpdateData.Keyboard = Keyboard.GetState();
            EditorUpdateData.DebugText.Clear();

            EditorUpdateData.DebugText.AppendLine($"Mouse (world): {EditorUpdateData.MousePosition}");
            EditorUpdateData.DebugText.AppendLine($"Mouse (local): {EditorUpdateData.Mouse.Position}");

            var fullScreenLayer = State.GetActiveFullScreenLayer();

            if (fullScreenLayer == null)
            {
                // Update layers
                foreach (var layer in GameData.Layers)
                    layer.Update(EditorUpdateData);

                // Update objects
                if (ShowObjects)
                {
                    foreach (var obj in GameData.Objects)
                        obj.Update(EditorUpdateData);

                    VM.RefreshObjFields();
                }
            }
            else
            {
                fullScreenLayer.Update(EditorUpdateData);
            }

            // Update the camera
            Cam.ViewArea = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            Cam.Update(EditorUpdateData);

            // Get the object we're hovering over
            if (CanHoverOverObject && fullScreenLayer == null && ShowObjects)
                HoverObject = GameData.Objects.FirstOrDefault(x => x.WorldBounds.Contains(EditorUpdateData.MousePosition));

            switch (Mode)
            {
                case EditorMode.Layers:
                    UpdateModeLayers(EditorUpdateData);
                    break;
                
                case EditorMode.Objects:
                    if (ShowObjects)
                        UpdateModeObjects(EditorUpdateData);
                    break;

                case EditorMode.Links:
                    if (ShowObjects)
                        UpdateModeLinks(EditorUpdateData);
                    break;
            }

            VM.DebugText = EditorUpdateData.DebugText.ToString();
            VM.OnUpdate();
        }

        protected void UpdateModeLayers(EditorUpdateData updateData)
        {
            GameData.Layers.FirstOrDefault(x => x.IsSelected)?.UpdateLayerEditing(updateData);
        }

        protected void UpdateModeObjects(EditorUpdateData updateData)
        {
            // Delete object
            if (updateData.Keyboard.IsKeyDown(Keys.Delete) && SelectedObject != null)
            {
                RemoveObject(SelectedObject);
                return;
            }

            // Move object
            if (updateData.Mouse.LeftButton == ButtonState.Pressed)
            {
                if (!IsDraggingObject)
                {
                    if (HoverObject == null)
                    {
                        SelectedObject = null;
                    }
                    else
                    {
                        IsDraggingObject = true;
                        SelectedObject = HoverObject;
                        PrevMousePos = updateData.MousePosition;
                        DraggingObjectInitialPosition = SelectedObject.Position;
                    }
                }

                if (IsDraggingObject && SelectedObject != null)
                {
                    var change = updateData.MousePosition - PrevMousePos;
                    SelectedObject.Position += new Point((int)Math.Round(change.X), (int)Math.Round(change.Y));
                    PrevMousePos = updateData.MousePosition;

                    // Auto-scroll if the object has been dragged from its initial position and is near an edge
                    if (DraggingObjectInitialPosition != SelectedObject.Position)
                        AutoScrollAtEdge(updateData.Mouse.Position);
                }
            }
            else
            {
                IsDraggingObject = false;
            }
        }

        protected void UpdateModeLinks(EditorUpdateData updateData)
        {
            if (updateData.Mouse.LeftButton == ButtonState.Pressed)
            {
                if (!IsDraggingLink)
                {
                    if (HoverObject == null)
                    {
                        SelectedLinkObject = null;
                    }
                    else
                    {
                        IsDraggingLink = true;
                        SelectedLinkObject = HoverObject;
                        PrevMousePos = updateData.MousePosition;
                        SelectedLinkObject.LinkGripPosition = updateData.MousePosition.ToPoint();
                    }
                }

                if (IsDraggingLink && SelectedLinkObject != null)
                {
                    SelectedLinkObject.LinkGripPosition += (updateData.MousePosition - PrevMousePos).ToPoint();
                    PrevMousePos = updateData.MousePosition;

                    AutoScrollAtEdge(updateData.Mouse.Position);
                }
            }
            else
            {
                IsDraggingLink = false;
                SelectedLinkObject = null;
            }
        }

        public void AutoScrollAtEdge(Point mousePos)
        {
            // Auto-scroll for every edge
            autoScroll(ActualWidth - mousePos.X, 1, 0); // Right
            autoScroll(mousePos.X, -1, 0); // Left
            autoScroll(ActualHeight - mousePos.Y, 0, 1); // Bottom
            autoScroll(mousePos.Y, 0, -1); // Top

            void autoScroll(double diff, int changeX, int changeY)
            {
                if (diff < State.AutoScrollMargin)
                    Cam.Position = new Vector2(
                        Cam.Position.X + State.AutoScrollSpeed * changeX,
                        Cam.Position.Y + State.AutoScrollSpeed * changeY);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (PauseWhenInactive && !IsActive || IsPaused)
                return;

            // Base call
            base.Draw(gameTime);

            // Clear the screen with the background color
            GraphicsDevice.Clear(State.Color_Background);

            // Begin drawing using the camera's transform matrix
            SpriteBatch.Begin(
                samplerState: SamplerState.PointClamp, 
                transformMatrix: Cam.TransformMatrix);

            // Draw map background color
            SpriteBatch.DrawFilledRectangle(new Rectangle(Point.Zero, State.MapSize), State.Color_MapBackground);

            // Draw the content
            Draw(SpriteBatch);

            // End the drawing
            SpriteBatch.End();
        }

        // TODO: Only render what is in view
        protected void Draw(SpriteBatch s)
        {
            var fullScreenLayer = State.GetActiveFullScreenLayer();

            if (fullScreenLayer != null)
            {
                fullScreenLayer.Draw(s);
                return;
            }

            // Draw each layer
            foreach (var layer in GameData.Layers.Where(x => x.IsVisible))
                layer.Draw(s);

            if (ShowObjects)
            {
                var max = GameManager.MaxDisplayPrio;

                // Draw objects
                for (int i = 0; i <= max; i++)
                    foreach (var obj in GameData.Objects.Where(x => x.DisplayPrio == i))
                        obj.Draw(s);

                // Draw links if in links mode
                if (Mode == EditorMode.Links)
                    foreach (var obj in GameData.Objects)
                        obj.DrawLinks(s);

                if (CanHoverOverObject)
                {
                    // Draw object bounds. Always show the bounds for the selected object.
                    // Then if we're hovering over another object without dragging we can
                    // show that too (the object we're dragging will always be selected!).
                    // When in links mode we show the bounds for the object the link is being
                    // moved for.
                    if (SelectedObject != null)
                        s.DrawRectangle(SelectedObject.WorldBounds, State.Color_ObjBounds);
                    if (HoverObject != null && !IsDraggingObject && !IsDraggingLink)
                        s.DrawRectangle(HoverObject.WorldBounds, State.Color_ObjBounds);
                    else if (SelectedLinkObject != null)
                        s.DrawRectangle(SelectedLinkObject.WorldBounds, State.Color_ObjBounds);
                }

                // Draw offsets for the currently selected object
                SelectedObject?.DrawOffsets(s);
            }
        }

        #endregion

        #region Public Methods

        public T LoadElement<T>(T element)
            where T : EditorElement
        {
            element.EditorState = State;
            element.Camera = Cam;
            element.Data = GameData;

            return element;
        }

        public void GoToObject(GameObject obj)
        {
            Cam.TargetPosition = obj.Position.ToVector2();
            Cam.TargetZoom = 2;
            Logger.Log(LogLevel.Trace, "Targeting object {0} at {1}", obj.DisplayName, obj.Position);
        }

        public void RemoveObject(GameObject obj)
        {
            GameData.Objects.Remove(SelectedObject);

            if (SelectedObject == obj)
                SelectedObject = null;

            VM.OnObjectRemoved(obj);

            Logger.Log(LogLevel.Trace, "Removed object {0}", obj.DisplayName);
        }

        public void AddObject(int index)
        {
            if (GameData.Objects.Count >= GameManager.GetMaxObjCount(GameData))
            {
                // TODO: Custom message box in UI manager
                System.Windows.MessageBox.Show("Max obj count reached!");
                return;
            }

            var obj = GameManager.CreateGameObject(GameData, index);
            obj.Position = Cam.Position.ToPoint();
            obj.LoadElement(this);
            obj.Load();
            GameData.Objects.Add(obj);
            VM.OnObjectAdded(obj);
        }

        public void Save()
        {
            Logger.Log(LogLevel.Info, "Saving the editor changes");

            // Save objects
            foreach (var obj in GameData.Objects)
                obj.Save();

            using (Context)
                GameManager.Save(Context, GameData);

            Logger.Log(LogLevel.Info, "Saved the editor changes");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Context?.Dispose();
        }

        #endregion
    }
}