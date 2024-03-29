﻿using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ray1Editor;

/// <summary>
/// The editor scene
/// </summary>
public class EditorScene : WpfGame
{
    #region Constructor

    public EditorScene(GameManager manager, Context context, object gameSettings, EditorSceneViewModel viewModel)
    {
        GameManager = manager;
        Context = context;
        GameSettings = gameSettings;
        ViewModel = viewModel;

        Stage = EditorStage.Loading;
        EditorUpdateData = new EditorUpdateData();

        Mouse = new WpfMouse(this);
        Keyboard = new WpfKeyboard(this);

        LinkGroups = new Dictionary<int, List<GameObject>>();
        NextLinkGroup = 1;
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
    // TODO: Move over more properties and code to EditorSceneViewModel, remove cross-references between scene and EditorViewModel and use EditorSceneViewModel to hold the common data
    public EditorStage Stage { get; protected set; }
    public EditorViewModel VM => (EditorViewModel)DataContext;
    public bool PauseWhenInactive => R1EServices.App.UserData.Editor_PauseWhenInactive;
    public EditorSceneViewModel ViewModel { get; }
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
            Logger.Info("Changed editor mode from {0} to {1}", _mode, value);

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
    public bool ShowObjectOffsets { get; set; }
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
            Logger.Trace("Selected object {0}", SelectedObject?.DisplayName ?? "null");
        }
    }
    public bool IsDraggingObject { get; protected set; }
    public Vector2 PrevMousePos { get; protected set; }
    public Point DraggingObjectInitialPosition { get; protected set; }

    // Links
    public GameObject HoverLinkGripObj { get; protected set; }
    public bool ShowLinks { get; set; }
    public bool IsDraggingLink { get; protected set; }
    public GameObject SelectedLinkObject { get; protected set; }
    public Dictionary<int, List<GameObject>> LinkGroups { get; }
    public int NextLinkGroup { get; set; }

    #endregion

    #region Protected Properties

    // MonoGame
    protected Renderer Renderer { get; set; }
    protected WpfMouse Mouse { get; }
    protected WpfKeyboard Keyboard { get; }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        Logger.Info("Initializing the editor scene");
        Logger.Trace("Editor scene is initializing with manager {0} and a context with the base path {1}", GameManager, Context.BasePath);

        // Create the graphics service
        _ = new WpfGraphicsDeviceService(this);

        // Create the camera
        Cam = new Camera(GraphicsDevice.Viewport);

        // Reset the camera
        Cam.ResetCamera();

        // Create a renderer
        Renderer = new Renderer(Cam, new SpriteBatch(GraphicsDevice));

        // Initialize the base game, thus loading the content
        base.Initialize();

        // Notify the view model
        VM.OnEditorLoaded();

        Logger.Info("Initialized the editor scene");

        if (Stage != EditorStage.Error)
            Stage = EditorStage.Editing;
    }

    protected override void LoadContent()
    {
        try
        {
            Logger.Info("Loading the editor content");

            // Load the game data
            using (Context)
                GameData = GameManager.Load(Context, GameSettings, new TextureManager(GraphicsDevice));

            Logger.Trace("Loading the editor elements");

            // Load elements
            GameData.LoadElements(this);

            // Load editor textures
            ViewModel.EditorTextures = new EditorTextures(GameData.TextureManager);
            ViewModel.EditorTextures.Init();

            // Post-load
            GameManager.PostLoad(GameData);

            Logger.Trace("Loading the editor objects");

            // Load objects
            foreach (var obj in GameData.Objects)
                obj.Load();

            Logger.Trace("Initializing the object links");

            // Initialize object links
            InitializeObjLinks();

            Logger.Trace("Calculating the map size");

            // Calculate the map size
            ViewModel.UpdateMapSize(GameData);

            // Load base content
            base.LoadContent();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading editor");

            R1EServices.UI.DisplayMessage($"An error occurred loading the editor. Error message: {ex.Message}", "Error loading editor", DialogMessageType.Error);

            Stage = EditorStage.Error;
        }
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

            LinkGroups[linkedObj.Key] = linkedObj.ToList();

            if (linkedObj.Key >= NextLinkGroup)
                NextLinkGroup = linkedObj.Key + 1;
        }
    }

    protected override void UnloadContent()
    {
        Logger.Info("Unloading the editor scene");

        base.UnloadContent();

        Renderer?.Dispose();
        GameData?.Dispose();
        Context?.Dispose();

        Logger.Info("Unloaded the editor scene");

        Stage = EditorStage.Closed;
    }

    protected bool GetIsEditorPaused() => PauseWhenInactive && !IsActive || ViewModel.IsPaused;

    protected override void Update(GameTime gameTime)
    {
        if (Stage != EditorStage.Editing || GetIsEditorPaused())
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

        EditorUpdateData.Update();

        UpdateKeyboardShortcuts(EditorUpdateData);

        var fullScreenLayer = ViewModel.GetActiveFullScreenLayer();

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
        {
            var foundHoverObj = false;

            for (int i = GameManager.MaxDisplayPrio; i >= 0; i--)
            {
                for (int j = GameData.Objects.Count - 1; j >= 0; j--)
                {
                    if (GameData.Objects[j].DisplayPrio == i && GameData.Objects[j].WorldBounds.Contains(EditorUpdateData.MousePosition))
                    {
                        HoverObject = GameData.Objects[j];
                        foundHoverObj = true;
                        break;
                    }
                }

                if (foundHoverObj)
                    break;
            }

            if (!foundHoverObj)
                HoverObject = null;
        }

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

        VM.DebugText = R1EServices.App.UserData.UI_ShowDebugInfo ? EditorUpdateData.DebugText.ToString() : null;
        VM.OnUpdate();
    }

    protected void UpdateKeyboardShortcuts(EditorUpdateData updateData)
    {
        // TODO: VM.OnPropertyChanged calls here are temporary until this system is changed

        // P = play/pause animations
        if (updateData.IsKeyDown(Keys.P))
            ViewModel.AnimateObjects = !ViewModel.AnimateObjects;

        // O = show/hide objects
        if (updateData.IsKeyDown(Keys.O))
        {
            ShowObjects = !ShowObjects;
            VM.OnPropertyChanged(nameof(VM.ShowObjects));
        }

        // X = show/hide object offsets
        if (updateData.IsKeyDown(Keys.X))
        {
            ShowObjectOffsets = !ShowObjectOffsets;
            VM.OnPropertyChanged(nameof(VM.ShowObjectOffsets));
        }

        // L = show/hide links
        if (updateData.IsKeyDown(Keys.L))
        {
            ShowLinks = !ShowLinks;
            VM.OnPropertyChanged(nameof(VM.ShowLinks));
        }

        // M = toggle mode
        if (updateData.IsKeyDown(Keys.M))
        {
            Mode = (EditorMode)(((int)Mode % 3) + 1);
            VM.OnPropertyChanged(nameof(VM.Mode));
        }

        // T = toggle tile/map layers visibility
        if (updateData.IsKeyDown(Keys.T))
            toggleLayerVisibilities(Layer.LayerType.Map);

        // C = toggle collision layers visibility
        if (updateData.IsKeyDown(Keys.C))
            toggleLayerVisibilities(Layer.LayerType.Collision);

        // B = toggle background layers visibility
        if (updateData.IsKeyDown(Keys.B))
            toggleLayerVisibilities(Layer.LayerType.Background);

        void toggleLayerVisibilities(Layer.LayerType type)
        {
            var isVisible = GameData.Layers.FirstOrDefault(x => x.Type == type)?.IsVisible;

            if (isVisible == null)
                return;

            foreach (var l in GameData.Layers.Where(x => x.Type == type))
                l.UpdateVisibility(!isVisible.Value);
        }
    }

    protected void UpdateModeLayers(EditorUpdateData updateData)
    {
        GameData.Layers.FirstOrDefault(x => x.IsSelected)?.UpdateLayerEditing(updateData);
    }

    protected void UpdateModeObjects(EditorUpdateData updateData)
    {
        // Delete object
        if (updateData.IsKeyDown(Keys.Delete) && SelectedObject != null)
        {
            RemoveObject(SelectedObject);
            return;
        }

        // Duplicate object
        if (updateData.IsKeyDown(Keys.LeftControl, false) && updateData.IsKeyDown(Keys.D) && SelectedObject != null)
            DuplicateObject(SelectedObject);

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

        if (SelectedObject?.CurrentAnimation != null)
        {
            var change = 0;

            if (updateData.IsKeyDown(Keys.OemPlus))
                change++;
            if (updateData.IsKeyDown(Keys.OemMinus))
                change--;

            if (change != 0)
            {
                var newFrame = (SelectedObject.AnimationFrame + change) % SelectedObject.CurrentAnimation.Frames.Length;

                if (newFrame < 0)
                    newFrame += SelectedObject.CurrentAnimation.Frames.Length;

                SelectedObject.AnimationFrame = newFrame;
                SelectedObject.AnimationFrameFloat = newFrame;
            }
        }
    }

    protected void UpdateModeLinks(EditorUpdateData updateData)
    {
        // Handle toggling link groups
        if (updateData.Mouse.LeftButton == ButtonState.Pressed)
        {
            // If we were hovering over a link grip before pressing the mouse button we toggle its state
            if (HoverLinkGripObj != null)
            {
                var linkGroup = HoverLinkGripObj.LinkGroup;

                // Disabled -> enabled
                if (linkGroup == 0)
                {
                    // Find every grip at the position
                    var objects = GameData.Objects.Where(x => x.CanBeLinkedToGroup && x.LinkGripBounds.Location == HoverLinkGripObj.LinkGripBounds.Location).ToArray();

                    // If more than 1 we link them together in a new group
                    if (objects.Length > 1)
                    {
                        foreach (var obj in objects)
                            obj.LinkGroup = NextLinkGroup;

                        LinkGroups[NextLinkGroup] = objects.ToList();

                        NextLinkGroup++;
                    }
                }
                // Enabled -> disabled
                else
                {
                    foreach (var obj in LinkGroups[linkGroup])
                        obj.LinkGroup = 0;

                    LinkGroups.Remove(linkGroup);
                }
            }

            HoverLinkGripObj = null;

        }
        else
        {
            HoverLinkGripObj = GameData.Objects.LastOrDefault(x => x.CanBeLinkedToGroup && x.LinkGripBounds.Contains(EditorUpdateData.MousePosition));
        }

        // Handle dragging links
        if (updateData.Mouse.LeftButton == ButtonState.Pressed)
        {
            // Start dragging
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
            // Stop dragging
            if (SelectedLinkObject != null)
                UnlinkObject(SelectedLinkObject);

            IsDraggingLink = false;
            SelectedLinkObject = null;
        }
    }

    public void UnlinkObject(GameObject obj)
    {
        var linkGroup = obj.LinkGroup;

        if (linkGroup != 0)
        {
            LinkGroups[linkGroup].Remove(obj);
            obj.LinkGroup = 0;

            if (LinkGroups[linkGroup].Count == 1)
            {
                LinkGroups[linkGroup].First().LinkGroup = 0;
                LinkGroups[linkGroup].Clear();
            }
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
            if (diff < ViewModel.AutoScrollMargin)
                Cam.Position = new Vector2(
                    Cam.Position.X + ViewModel.AutoScrollSpeed * changeX,
                    Cam.Position.Y + ViewModel.AutoScrollSpeed * changeY);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        if (Stage != EditorStage.Editing || PauseWhenInactive && !IsActive || ViewModel.IsPaused)
            return;

        // Base call
        base.Draw(gameTime);

        // Clear the screen with the background color
        GraphicsDevice.Clear(ViewModel.Colors[EditorColor.Background]);

        // Begin drawing using the camera's transform matrix
        Renderer.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp, 
            transformMatrix: Cam.TransformMatrix);

        // Draw map background color
        Renderer.DrawFilledRectangle(new Rectangle(Point.Zero, ViewModel.MapSize), ViewModel.Colors[EditorColor.MapBackground]);

        // Draw the content
        Draw(Renderer);

        // End the drawing
        Renderer.SpriteBatch.End();
    }

    protected void Draw(Renderer r)
    {
        var fullScreenLayer = ViewModel.GetActiveFullScreenLayer();

        if (fullScreenLayer != null)
        {
            fullScreenLayer.Draw(r);
            return;
        }

        // Draw each layer
        foreach (var layer in GameData.Layers.Where(x => x.IsVisible))
            layer.Draw(r);

        if (ShowObjects)
        {
            var max = GameManager.MaxDisplayPrio;

            // Draw objects
            for (int i = 0; i <= max; i++)
                foreach (var obj in GameData.Objects.Where(x => x.DisplayPrio == i))
                    obj.Draw(r);

            // Draw links if in links mode or links are toggled to be visible
            if (Mode == EditorMode.Links || ShowLinks)
            {
                foreach (GameObject obj in GameData.Objects.Where(obj => Mode == EditorMode.Links || ShowLinks && obj.LinkGroup != 0))
                    obj.DrawLinks(r);
            }

            // Draw link grip border if hovering over it in links mode
            if (Mode == EditorMode.Links && HoverLinkGripObj != null)
                r.DrawRectangle(HoverLinkGripObj.LinkGripBounds, ViewModel.Colors[EditorColor.SelectedObjBounds]);

            if (CanHoverOverObject)
            {
                // Draw object bounds. Always show the bounds for the selected object.
                // Then if we're hovering over another object without dragging we can
                // show that too (the object we're dragging will always be selected!).
                // When in links mode we show the bounds for the object the link is being
                // moved for.
                if (SelectedObject != null) // Selected object
                    r.DrawRectangle(SelectedObject.WorldBounds, ViewModel.Colors[EditorColor.SelectedObjBounds]);
                if (HoverObject != null && !IsDraggingObject && !IsDraggingLink && HoverObject != SelectedObject) // Hovering object
                    r.DrawRectangle(HoverObject.WorldBounds, ViewModel.Colors[EditorColor.HoveringObjBounds]);
                else if (SelectedLinkObject != null) // Selected link object
                    r.DrawRectangle(SelectedLinkObject.WorldBounds, ViewModel.Colors[EditorColor.SelectedObjBounds]);
            }

            if (ShowObjectOffsets)
            {
                foreach (var obj in GameData.Objects)
                    obj.DrawOffsets(r);
            }
            else
            {
                // Draw offsets for the currently selected object
                SelectedObject?.DrawOffsets(r);
            }
        }
    }

    #endregion

    #region Public Methods

    public T LoadElement<T>(T element)
        where T : EditorElement
    {
        element.ViewModel = ViewModel;
        element.Camera = Cam;
        element.Data = GameData;

        return element;
    }

    public void GoToObject(GameObject obj)
    {
        Cam.TargetPosition = obj.Position.ToVector2();
        Cam.TargetZoom = 2;
        Logger.Trace("Targeting object {0} at {1}", obj.DisplayName, obj.Position);
    }

    public void DuplicateObject(GameObject obj)
    {
        if (GameData.Objects.Count >= GameManager.GetMaxObjCount(GameData))
        {
            R1EServices.UI.DisplayMessage("Maximum number of objects has been reached in the level", "Max object count reached", DialogMessageType.Error);

            return;
        }

        var newObj = GameManager.DuplicateObject(GameData, obj);

        newObj.Position = obj.Position + new Point(20, 0); // Offset new object slightly so it's distinct
        LoadElement(newObj);
        newObj.Load();
        GameData.Objects.Add(newObj);
        VM.OnObjectAdded(newObj);

        // Select the new object
        SelectedObject = newObj;
    }

    public void RemoveObject(GameObject obj)
    {
        GameData.Objects.Remove(SelectedObject);

        if (SelectedObject == obj)
            SelectedObject = null;

        if (obj.LinkGroup != 0)
            UnlinkObject(obj);

        VM.OnObjectRemoved(obj);

        Logger.Trace("Removed object {0}", obj.DisplayName);
    }

    public void AddObject(int index)
    {
        if (GameData.Objects.Count >= GameManager.GetMaxObjCount(GameData))
        {
            R1EServices.UI.DisplayMessage("Maximum number of objects has been reached in the level", "Max object count reached", DialogMessageType.Error);
                
            return;
        }

        var obj = GameManager.CreateGameObject(GameData, index);
        obj.Position = Cam.Position.ToPoint();
        LoadElement(obj);
        obj.Load();
        GameData.Objects.Add(obj);
        VM.OnObjectAdded(obj);
    }

    public void Save()
    {
        Logger.Info("Saving the editor changes");

        // Save objects
        foreach (var obj in GameData.Objects)
            obj.Save();

        using (Context)
            GameManager.Save(Context, GameData);

        Logger.Info("Saved the editor changes");
    }

    #endregion

    #region Data Types

    public enum EditorStage
    {
        Loading,
        Editing,
        Closed,
        Error,
    }

    #endregion
}