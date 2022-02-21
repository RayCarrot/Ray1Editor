using BinarySerializer;
using MahApps.Metro.IconPacks;
using Microsoft.Xna.Framework;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace Ray1Editor;

/// <summary>
/// A base layer element
/// </summary>
public abstract class Layer : EditorElement
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The layer name to display
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The pointer of the layer, if any
    /// </summary>
    public virtual Pointer Pointer { get; init; }

    /// <summary>
    /// The position and size of the layer
    /// </summary>
    public abstract Rectangle Rectangle { get; }

    /// <summary>
    /// Indicates if the layer can be edited
    /// </summary>
    public abstract bool CanEdit { get; }

    /// <summary>
    /// The type of layer
    /// </summary>
    public abstract LayerType Type { get; }

    /// <summary>
    /// Indicates if the layer is currently selected for editing
    /// </summary>
    public virtual bool IsSelected { get; protected set; }

    /// <summary>
    /// Indicates if the layer is currently visible
    /// </summary>
    public bool IsVisible { get; protected set; }

    public abstract IEnumerable<EditorFieldViewModel> GetFields();

    protected EditorToggleIconViewModel ToggleField_Edit { get; set; }
    protected EditorToggleIconViewModel ToggleField_Show { get; set; }

    public virtual IEnumerable<EditorToggleIconViewModel> GetToggleFields()
    {
        yield return ToggleField_Edit = new EditorToggleIconViewModel(
            iconKind: PackIconMaterialKind.PencilOutline, 
            info: "Edit Layer", 
            getValueAction: () => IsSelected, 
            setValueAction: _ =>
            {
                Select();

                ToggleField_Show.Refresh();

                foreach (var l in Data.Layers)
                {
                    l.ToggleField_Edit.Refresh();
                    l.ToggleField_Show.IsEnabled = !l.IsSelected;
                }
            });

        ToggleField_Edit.IsVisible = CanEdit;

        yield return ToggleField_Show = new EditorToggleIconViewModel(
            iconKind: PackIconMaterialKind.EyeOutline, 
            info: "Show Layer", 
            getValueAction: () => IsVisible, 
            setValueAction: x => IsVisible = x);
    }

    public virtual void Select()
    {
        Logger.Log(LogLevel.Trace, "Selected layer {0}", Name);

        IsSelected = true;
        IsVisible = true;

        if (Data == null)
            return;

        foreach (var l in Data.Layers.Where(x => x != this))
            l.IsSelected = false;
    }
    public virtual void UpdateVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        ToggleField_Show.Refresh();
    }
    public virtual void OnModeChanged(EditorMode oldMode, EditorMode newMode)
    {
        ToggleField_Edit.IsVisible = newMode == EditorMode.Layers && CanEdit;
        ToggleField_Show.IsEnabled = newMode != EditorMode.Layers || !IsSelected;

        if (newMode == EditorMode.Layers && IsSelected)
        {
            IsVisible = true;
            ToggleField_Show.Refresh();
        }
    }

    public virtual void Update(EditorUpdateData updateData) { }
    public virtual void UpdateLayerEditing(EditorUpdateData updateData) { }
    public abstract void Draw(Renderer r);

    public enum LayerType
    {
        Other,
        Background,
        Map,
        Collision,
    }
}