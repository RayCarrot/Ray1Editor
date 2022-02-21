using BinarySerializer;
using System;
using System.Collections.Generic;

namespace Ray1Editor;

/// <summary>
/// A game manager for managing a game in the editor
/// </summary>
public abstract class GameManager
{
    /// <summary>
    /// Gets all available levels to load for the game
    /// </summary>
    /// <param name="game">The game to get the levels for</param>
    /// <param name="path">The game path</param>
    /// <returns>The levels</returns>
    public abstract IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path);

    /// <summary>
    /// Optionally gets actions which the user can initiate for the game
    /// </summary>
    /// <param name="data">The game data</param>
    /// <returns>The actions, or null if none</returns>
    public virtual IEnumerable<ActionViewModel> GetActions(GameData data) => null;

    /// <summary>
    /// Loads the game to the editor
    /// </summary>
    /// <param name="context">The serializer context</param>
    /// <param name="settings">The serializer settings</param>
    /// <param name="textureManager">The texture manager</param>
    /// <returns>The loaded game data</returns>
    public abstract GameData Load(Context context, object settings, TextureManager textureManager);

    /// <summary>
    /// Saves the game data
    /// </summary>
    /// <param name="context">The serializer context</param>
    /// <param name="gameData">The game data</param>
    public abstract void Save(Context context, GameData gameData);

    /// <summary>
    /// Gets the editor fields for an object
    /// </summary>
    /// <param name="gameData">The loaded game data</param>
    /// <param name="getSelectedObj">A func for getting the currently selected object</param>
    /// <returns>The editor fields</returns>
    public abstract IEnumerable<EditorFieldViewModel> GetEditorObjFields(GameData gameData, Func<GameObject> getSelectedObj);

    /// <summary>
    /// Gets the editor level attribute fields
    /// </summary>
    /// <param name="gameData">The game data</param>
    /// <returns>The level attribute fields</returns>
    public virtual IEnumerable<EditorFieldViewModel> GetEditorLevelAttributeFields(GameData gameData) => new EditorFieldViewModel[0];

    /// <summary>
    /// Optional post load method. Gets called after the loaded data has been initialized.
    /// </summary>
    /// <param name="gameData">The game data</param>
    public virtual void PostLoad(GameData gameData) { }

    /// <summary>
    /// Gets the available objects which can be added to the level
    /// </summary>
    /// <param name="gameData">The game data</param>
    /// <returns>The object names</returns>
    public abstract IEnumerable<string> GetAvailableObjects(GameData gameData);

    /// <summary>
    /// Creates a new game object from the collection of available objects
    /// </summary>
    /// <param name="gameData">The game data</param>
    /// <param name="index">The index from the available objects</param>
    /// <returns>The new object</returns>
    public abstract GameObject CreateGameObject(GameData gameData, int index);

    /// <summary>
    /// Duplicates (clones) an existing object
    /// </summary>
    /// <param name="gameData">The game data</param>
    /// <param name="sourceObj">The source object to duplicate</param>
    /// <returns>The new object</returns>
    public abstract GameObject DuplicateObject(GameData gameData, GameObject sourceObj);

    /// <summary>
    /// Gets the maximum number of objects which can be added to a level
    /// </summary>
    /// <returns>The maximum number of objects which can be added to a level</returns>
    public abstract int GetMaxObjCount(GameData gameData);

    /// <summary>
    /// The maximum display prio layer an object can use
    /// </summary>
    public virtual int MaxDisplayPrio => 0;
}