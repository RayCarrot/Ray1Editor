using System;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public abstract class EditorFieldViewModel : BaseViewModel
    {
        protected EditorFieldViewModel(string header, string info = null)
        {
            Header = header;
            Info = info;
        }

        public string Header { get; }
        public string Info { get; }

        public abstract void Refresh();
    }

    public class EditorFieldViewModel<T> : EditorFieldViewModel
    {
        public EditorFieldViewModel(string header, string info, Func<T> getValueAction, Action<T> setValueAction) : base(header, info)
        {
            GetValueAction = getValueAction;
            SetValueAction = setValueAction;
        }

        protected T _value;

        protected Func<T> GetValueAction { get; }
        protected Action<T> SetValueAction { get; }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                SetValueAction(value);
            }
        }

        public override void Refresh()
        {
            _value = GetValueAction();
            OnPropertyChanged(nameof(Value));
        }
    }
}