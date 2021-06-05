using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Linq;
using BinarySerializer.Ray1;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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

            // TODO: Allow to be modified
            BackgroundColor = Color.BlueViolet;
            MapBackgroundColor = Color.CornflowerBlue;
            //BackgroundColor = new Color(0x0f, 0x0e, 0x1b);
            //MapBackgroundColor = new Color(0x1d, 0x1b, 0x32);
            ObjBoundsColor = Color.Red;
            ObjLinksColor = new Color(0xFD, 0xD8, 0x35);

            State = new EditorState();

            Mouse = new WpfMouse(this);
            Keyboard = new WpfKeyboard(this);
        }

        #endregion

        #region Private Fields

        private GameObject _selectedObject;
        private EditorMode _mode;

        #endregion

        #region Public Properties

        // General
        public EditorViewModel VM => (EditorViewModel)DataContext;
        public Color BackgroundColor { get; set; }
        public Color MapBackgroundColor { get; set; }
        public Color ObjBoundsColor { get; set; }
        public Color ObjLinksColor { get; set; }
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
        public Point MapSize { get; protected set; }

        // Objects
        public GameObject HoverObject { get; protected set; }
        public GameObject SelectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                VM.UpdateSelectedObject(_selectedObject);
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
            Cam = new Camera();

            // Reset the camera
            ResetCamera();

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
            MapSize = new Point(GameData.Layers.Max(x => x.Rectangle.Right), GameData.Layers.Max(x => x.Rectangle.Bottom));

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

            // Get the delta-time
            var deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            // Update layers
            foreach (var layer in GameData.Layers)
                layer.Update();

            // Update objects
            foreach (var obj in GameData.Objects)
                obj.Update(deltaTime);

            var mouse = Mouse.GetState();
            var mousePos = mouse.Position;
            var mouseWorldPos = Cam.ToWorld(mousePos.ToVector2());

            // Update the camera
            Cam.ViewArea = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            Cam.Update(deltaTime, mouse, Keyboard.GetState());

            // Get the object we're hovering over
            if (CanHoverOverObject)
                HoverObject = GameData.Objects.FirstOrDefault(x => x.Bounds.Contains(mouseWorldPos));

            switch (Mode)
            {
                case EditorMode.Tiles:
                    UpdateModeTiles(deltaTime, mouse);
                    break;
                
                case EditorMode.Objects:
                    UpdateModeObjects(deltaTime, mouse);
                    break;

                case EditorMode.Links:
                    UpdateModeLinks(deltaTime, mouse);
                    break;
            }

            // TODO: Remove or change implementation. Combining strings every frame is not good for performance. This is only here for debugging.
            VM.DebugText = $"Mouse (world): {mouseWorldPos}{Environment.NewLine}" +
                           $"Mouse (local): {mousePos}{Environment.NewLine}" +
                           $"Zoom: {Cam.Zoom * 100} %{Environment.NewLine}" +
                           $"Position: {Cam.Position}";
        }

        protected void UpdateModeTiles(double deltaTime, MouseState mouse)
        {

        }

        protected void UpdateModeObjects(double deltaTime, MouseState mouse)
        {
            var mousePos = mouse.Position;
            var mouseWorldPos = Cam.ToWorld(mousePos.ToVector2());

            if (mouse.LeftButton == ButtonState.Pressed)
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
                        PrevMousePos = mouseWorldPos;
                        DraggingObjectInitialPosition = SelectedObject.Position;
                    }
                }

                if (IsDraggingObject && SelectedObject != null)
                {
                    SelectedObject.Position += (mouseWorldPos - PrevMousePos).ToPoint();
                    PrevMousePos = mouseWorldPos;

                    // Auto-scroll if the object has been dragged from its initial position and is near an edge
                    if (DraggingObjectInitialPosition != SelectedObject.Position)
                        AutoScrollAtEdge(mousePos);
                }
            }
            else
            {
                IsDraggingObject = false;
            }
        }

        protected void UpdateModeLinks(double deltaTime, MouseState mouse)
        {
            var mousePos = mouse.Position;
            var mouseWorldPos = Cam.ToWorld(mousePos.ToVector2());

            if (mouse.LeftButton == ButtonState.Pressed)
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
                        PrevMousePos = mouseWorldPos;
                        SelectedLinkObject.LinkGripPosition = mouseWorldPos.ToPoint();
                    }
                }

                if (IsDraggingLink && SelectedLinkObject != null)
                {
                    SelectedLinkObject.LinkGripPosition += (mouseWorldPos - PrevMousePos).ToPoint();
                    PrevMousePos = mouseWorldPos;

                    AutoScrollAtEdge(mousePos);
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
            GraphicsDevice.Clear(BackgroundColor);

            // Begin drawing using the camera's transform matrix
            SpriteBatch.Begin(
                samplerState: SamplerState.PointClamp, 
                transformMatrix: Cam.TransformMatrix);

            // Draw map background color
            SpriteBatch.DrawFilledRectangle(new Rectangle(Point.Zero, MapSize), MapBackgroundColor);

            // Draw the content
            Draw(SpriteBatch);

            // End the drawing
            SpriteBatch.End();
        }

        // TODO: Only render what is in view
        protected void Draw(SpriteBatch s)
        {
            // Draw each layer
            foreach (var layer in GameData.Layers.Where(x => x.IsVisible))
                layer.Draw(s);

            // Draw objects
            foreach (var obj in GameData.Objects)
                obj.Draw(s);

            // Draw links if in links mode
            if (Mode == EditorMode.Links)
                foreach (var obj in GameData.Objects)
                    obj.DrawLinks(s, ObjLinksColor);

            if (CanHoverOverObject)
            {
                // Draw object bounds. Always show the bounds for the selected object.
                // Then if we're hovering over another object without dragging we can
                // show that too (the object we're dragging will always be selected!).
                // When in links mode we show the bounds for the object the link is being
                // moved for.
                if (SelectedObject != null)
                    s.DrawRectangle(SelectedObject.Bounds, ObjBoundsColor);
                if (HoverObject != null && !IsDraggingObject && !IsDraggingLink)
                    s.DrawRectangle(HoverObject.Bounds, ObjBoundsColor);
                else if (SelectedLinkObject != null)
                    s.DrawRectangle(SelectedLinkObject.Bounds, ObjBoundsColor);
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

        public void ResetCamera()
        {
            Cam.Zoom = 1;
            Cam.Position = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
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