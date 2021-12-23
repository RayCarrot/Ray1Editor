using System;
using MahApps.Metro.IconPacks;

namespace RayCarrot.Ray1Editor;

public class EditorToggleIconViewModel : BaseViewModel
{
    public EditorToggleIconViewModel(PackIconMaterialKind iconKind, string info, Func<bool> getValueAction, Action<bool> setValueAction)
    {
        IconKind = iconKind;
        Info = info;
        GetValueAction = getValueAction;
        SetValueAction = setValueAction;
        IsEnabled = true;
        IsVisible = true;
    }

    private bool _value;

    public bool IsEnabled { get; set; }
    public bool IsVisible { get; set; }
    public PackIconMaterialKind IconKind { get; }
    public string Info { get; }
    protected Func<bool> GetValueAction { get; }
    protected Action<bool> SetValueAction { get; }

    public bool Value
    {
        get => _value;
        set
        {
            _value = value;
            SetValueAction(value);
        }
    }

    public void Refresh()
    {
        _value = GetValueAction();
        OnPropertyChanged(nameof(Value));
    }
}