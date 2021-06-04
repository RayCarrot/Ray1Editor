using System;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Extension methods for <see cref="GameMode"/>
    /// </summary>
    public static class GameModeExtensions
    {
        /// <summary>
        /// Gets the <see cref="GameManager"/> for the game mode
        /// </summary>
        /// <param name="mode">The game mode</param>
        /// <returns>The manager</returns>
        public static GameManager GetManager(this GameMode mode) => (GameManager)Activator.CreateInstance(mode.GetAttribute<GameModeInfoAttribute>().ManagerType);
    }
}