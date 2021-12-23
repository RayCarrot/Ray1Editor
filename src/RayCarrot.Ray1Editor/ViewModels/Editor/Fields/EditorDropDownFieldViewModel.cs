﻿using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RayCarrot.Ray1Editor;

public class EditorDropDownFieldViewModel : EditorFieldViewModel
{
    public EditorDropDownFieldViewModel(string header, string info, Func<int> getValueAction, Action<int> setValueAction, Func<IReadOnlyList<DropDownItem>> getItemsAction) : base(header, info)
    {
        GetValueAction = getValueAction;
        SetValueAction = setValueAction;
        GetItemsAction = getItemsAction;
        Items = new ObservableCollection<DropDownItem>();
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private int _selectedItem;
    private IReadOnlyList<DropDownItem> _prevItems;

    protected Func<int> GetValueAction { get; }
    protected Action<int> SetValueAction { get; }
    protected Func<IReadOnlyList<DropDownItem>> GetItemsAction { get; }

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
        var newItems = GetItemsAction();

        // IDEA: Maybe improve this by only resetting the items when a flag for it gets set to avoid always getting the items collection
        if (!ReferenceEquals(newItems, _prevItems))
        {
            _prevItems = newItems;

            // Set selection to -1 to avoid clearing the collection calling SelectedItem.Set
            _selectedItem = -1;
            OnPropertyChanged(nameof(SelectedItem));

            Items.Clear();
            Items.AddRange(GetItemsAction());

            Logger.Log(LogLevel.Debug, "Recreated drop-down items for drop-down with header {0}", Header);
        }

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