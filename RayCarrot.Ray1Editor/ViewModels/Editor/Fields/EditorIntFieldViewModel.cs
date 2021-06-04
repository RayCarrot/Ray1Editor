using System;

namespace RayCarrot.Ray1Editor
{
    public class EditorIntFieldViewModel : EditorFieldViewModel
    {
        public EditorIntFieldViewModel(string header, string info, Func<long> getValueAction, Action<long> setValueAction, long min = 0, long max = Int32.MaxValue) : base(header, info)
        {
            GetValueAction = getValueAction;
            SetValueAction = setValueAction;
            Min = min;
            Max = max;

            // Set initial value to avoid unnecessary settings from UI when default value is not within min/max range
            _value = min;
        }

        private long _value;

        protected Func<long> GetValueAction { get; }
        protected Action<long> SetValueAction { get; }

        public long Value
        {
            get => _value;
            set
            {
                _value = value;
                SetValueAction(value);
            }
        }

        public long Min { get; }
        public long Max { get; }

        public override void Refresh()
        {
            _value = GetValueAction();
            OnPropertyChanged(nameof(Value));
        }
    }
}