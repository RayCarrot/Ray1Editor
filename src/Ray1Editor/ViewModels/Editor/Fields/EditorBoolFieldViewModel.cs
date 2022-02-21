using System;

namespace Ray1Editor;

public class EditorBoolFieldViewModel : EditorFieldViewModel<bool>
{
    public EditorBoolFieldViewModel(string header, string info, Func<bool> getValueAction, Action<bool> setValueAction) : base(header, info, getValueAction, setValueAction) { }
}