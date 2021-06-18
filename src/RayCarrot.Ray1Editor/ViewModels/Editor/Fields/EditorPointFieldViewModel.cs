using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace RayCarrot.Ray1Editor
{
    public class EditorPointFieldViewModel : EditorFieldViewModel
    {
        public EditorPointFieldViewModel(string header, string info, Func<Point> getValueAction, Action<Point> setValueAction, int min = 0, int max = Int32.MaxValue) : base(header, info)
        {
            GetValueAction = getValueAction;
            SetValueAction = setValueAction;
            Min = min;
            Max = max;

            // Set initial values to avoid unnecessary settings from UI when default value is not within min/max range
            _x = min;
            _y = min;
        }

        private int _x;
        private int _y;

        protected Func<Point> GetValueAction { get; }
        protected Action<Point> SetValueAction { get; }

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                SetValueAction(new Point(X, Y));
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                SetValueAction(new Point(X, Y));
            }
        }

        public int Min { get; }
        public int Max { get; }

        public override void Refresh()
        {
            var p = GetValueAction();
            _x = p.X;
            _y = p.Y;
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }
    }
}