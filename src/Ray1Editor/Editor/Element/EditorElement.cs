namespace Ray1Editor;

/// <summary>
/// A base editor element
/// </summary>
public abstract class EditorElement
{
    /// <summary>
    /// The editor state
    /// </summary>
    public EditorSceneViewModel ViewModel { get; set; }
        
    /// <summary>
    /// The editor camera
    /// </summary>
    public Camera Camera { get; set; }
        
    /// <summary>
    /// The editor data
    /// </summary>
    public GameData Data { get; set; }
}