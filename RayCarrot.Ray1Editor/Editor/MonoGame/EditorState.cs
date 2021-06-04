namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Data for the state of the editor
    /// </summary>
    public class EditorState
    {
        public bool AnimateObjects { get; set; } = true;
        public bool LoadFromMemory { get; set; } = false;
        public float FramesPerSecond { get; set; } = 60f;
        public int AutoScrollMargin { get; set; } = 40;
        public int AutoScrollSpeed { get; set; } = 7;
    }
}