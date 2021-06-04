namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// An added game
    /// </summary>
    public class UserData_Game
    {
        /// <summary>
        /// The game path, either a directory or a file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The specified game name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The game mode
        /// </summary>
        public GameMode Mode { get; set; }
    }
}