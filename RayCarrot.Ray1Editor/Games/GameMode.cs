namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// The available game modes to use for the editor
    /// </summary>
    public enum GameMode
    {
        [GameModeInfo("Rayman 1 (PC - 1.21)", typeof(R1_PC_GameManager), GameModePathType.Directory)]
        R1_PC_1_21,

        // TODO: Add remaining PC versions
    }
}