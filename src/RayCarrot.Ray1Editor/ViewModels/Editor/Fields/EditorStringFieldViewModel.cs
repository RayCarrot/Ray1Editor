using System;

namespace RayCarrot.Ray1Editor
{
    public class EditorStringFieldViewModel : EditorFieldViewModel<string>
    {
        public EditorStringFieldViewModel(string header, string info, Func<string> getValueAction, Action<string> setValueAction, int maxLength = Int32.MaxValue) : base(header, info, getValueAction, setValueAction)
        {
            MaxLength = maxLength;
        }

        public int MaxLength { get; }
    }
}