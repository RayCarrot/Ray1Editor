namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// A base editor element
    /// </summary>
    public abstract class EditorElement
    {
        /// <summary>
        /// The editor state
        /// </summary>
        public EditorState EditorState { get; set; }
        
        /// <summary>
        /// The editor camera
        /// </summary>
        public Camera Camera { get; set; }
        
        /// <summary>
        /// The editor data
        /// </summary>
        public GameData Data { get; set; }
    }
}