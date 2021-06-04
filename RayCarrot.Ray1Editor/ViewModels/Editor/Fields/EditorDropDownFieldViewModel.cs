using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class EditorDropDownFieldViewModel : EditorFieldViewModel
    {
        public EditorDropDownFieldViewModel(string header, string info, Func<int> getValueAction, Action<int> setValueAction, Func<IEnumerable<DropDownItem>> getItemsAction) : base(header, info)
        {
            GetValueAction = getValueAction;
            SetValueAction = setValueAction;
            GetItemsAction = getItemsAction;
            Items = new ObservableCollection<DropDownItem>();
        }

        private int _selectedItem;

        protected Func<int> GetValueAction { get; }
        protected Action<int> SetValueAction { get; }
        protected Func<IEnumerable<DropDownItem>> GetItemsAction { get; }

        public int SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                SetValueAction(value);
            }
        }
        public ObservableCollection<DropDownItem> Items { get; }

        public override void Refresh()
        {
            // Set selection to -1 to avoid clearing the collection calling SelectedItem.Set
            _selectedItem = -1;
            OnPropertyChanged(nameof(SelectedItem));

            Items.Clear();
            Items.AddRange(GetItemsAction());

            _selectedItem = GetValueAction();
            OnPropertyChanged(nameof(SelectedItem));
        }

        public class DropDownItem
        {
            public DropDownItem(string header, object data)
            {
                Header = header;
                Data = data;
            }

            public string Header { get; }
            public object Data { get; }
        }
        public class DropDownItem<T> : DropDownItem
        {
            public DropDownItem(string header, T data) : base(header, data) { }

            public new T Data => (T)base.Data;
        }
    }
}