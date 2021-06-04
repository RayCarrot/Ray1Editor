namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Extension methods for <see cref="EditorElement"/>
    /// </summary>
    public static class EditorElementExtensions
    {
        public static T LoadElement<T>(this T element, EditorScene editor) where T : EditorElement => editor.LoadElement<T>(element);
    }
}