using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
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
        public bool PauseWhenInactive { get; set; } // TODO: Add setting
        public bool IsPaused { get; set; } // TODO: Add setting
        public EditorState State { get; }
        public Context Context { get; }
        public TextureManager TextureManager { get; protected set; }

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
                    l.ResetLayerEditing();

                VM.OnModeChanged(_mode, value);

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
        public GameObject HoverObject { get; protected set; }
        public GameObject SelectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                VM.OnSelectedObjectChanged(_selectedObject);
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
            // Create the graphics service
            _ = new WpfGraphicsDeviceService(this);

            // Create a sprite batch
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a texture manager
            TextureManager = new TextureManager(GraphicsDevice);

            // Create the camera
            Cam = new Camera(GraphicsDevice.Viewport);

            // Reset the camera
            Cam.ResetCamera();

            // Initialize the base game
            base.Initialize();

            VM.OnEditorLoaded();
        }

        protected override void LoadContent()
        {
            // Load the game data
            using (Context)
                GameData = GameManager.Load(Context, GameSettings, TextureManager);

            // Load elements
            GameData.LoadElements(this);

            // Load objects
            foreach (var obj in GameData.Objects)
                obj.Load();

            // Initialize object links
            InitializeObjLinks();

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
            base.UnloadContent();

            SpriteBatch?.Dispose();
            TextureManager?.Dispose();
            Primitives2D.Dipose();
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

            var fullScreenLayer = State.FullscreenLayer;

            if (fullScreenLayer == null)
            {
                // Update layers
                foreach (var layer in GameData.Layers)
                    layer.Update(EditorUpdateData);

                // Update objects
                foreach (var obj in GameData.Objects)
                    obj.Update(EditorUpdateData);
            }
            else
            {
                fullScreenLayer.Update(EditorUpdateData);
            }

            // Update the camera
            Cam.ViewArea = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            Cam.Update(EditorUpdateData);

            // Get the object we're hovering over
            if (CanHoverOverObject && fullScreenLayer == null)
                HoverObject = GameData.Objects.FirstOrDefault(x => x.Bounds.Contains(EditorUpdateData.MousePosition));

            switch (Mode)
            {
                case EditorMode.Layers:
                    UpdateModeLayers(EditorUpdateData);
                    break;
                
                case EditorMode.Objects:
                    UpdateModeObjects(EditorUpdateData);
                    break;

                case EditorMode.Links:
                    UpdateModeLinks(EditorUpdateData);
                    break;
            }

            VM.DebugText = EditorUpdateData.DebugText.ToString();
        }

        protected void UpdateModeLayers(EditorUpdateData updateData)
        {
            GameData.Layers.FirstOrDefault(x => x.IsSelected)?.UpdateLayerEditing(updateData);
        }

        protected void UpdateModeObjects(EditorUpdateData updateData)
        {
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
                    SelectedObject.Position += (updateData.MousePosition - PrevMousePos).ToPoint();
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
            var fullScreenLayer = State.FullscreenLayer;

            if (fullScreenLayer != null)
            {
                fullScreenLayer.Draw(s);
                return;
            }

            // Draw each layer
            foreach (var layer in GameData.Layers.Where(x => x.IsVisible))
                layer.Draw(s);

            // Draw objects
            foreach (var obj in GameData.Objects)
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
                    s.DrawRectangle(SelectedObject.Bounds, State.Color_ObjBounds);
                if (HoverObject != null && !IsDraggingObject && !IsDraggingLink)
                    s.DrawRectangle(HoverObject.Bounds, State.Color_ObjBounds);
                else if (SelectedLinkObject != null)
                    s.DrawRectangle(SelectedLinkObject.Bounds, State.Color_ObjBounds);
            }

            // Draw offsets for the currently selected object
            SelectedObject?.DrawOffsets(s);
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
        }

        public void Save()
        {
            // Save objects
            foreach (var obj in GameData.Objects)
                obj.Save();

            GameManager.Save(Context, GameData);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Context?.Dispose();
        }

        #endregion
    }
}