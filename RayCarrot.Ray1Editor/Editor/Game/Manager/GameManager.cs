using System;
using System.Collections.Generic;
using BinarySerializer;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A game manager for managing a game in the editor
    /// </summary>
    public abstract class GameManager
    {
        /// <summary>
        /// Gets all available levels to load for the game
        /// </summary>
        /// <returns>The levels</returns>
        public abstract IEnumerable<LoadGameLevelViewModel> GetLevels();

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
        /// <param name="getSelectedObj">A func for getting the currently selected object</param>
        /// <returns>The editor fields</returns>
        public abstract IEnumerable<EditorFieldViewModel> GetEditorObjFields(Func<GameObject> getSelectedObj);
    }
}