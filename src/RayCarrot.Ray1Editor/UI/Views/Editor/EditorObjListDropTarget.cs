using GongSolutions.Wpf.DragDrop;

namespace RayCarrot.Ray1Editor;

public class EditorObjListDropTarget : DefaultDropHandler
{
    public EditorObjListDropTarget(EditorViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public EditorViewModel ViewModel { get; }

    public override void Drop(IDropInfo dropInfo)
    {
        // Handle the drop
        base.Drop(dropInfo);

        // Get the source object being moved
        var src = (GameObjectListItemViewModel)dropInfo.Data;
            
        // Move the object to the new position in the editor data
        var objects = ViewModel.EditorScene.GameData.Objects;
        objects.Move(objects.IndexOf(src.Obj), dropInfo.InsertIndex);

        // Select the item again since it will have been deselected
        ViewModel.SelectedGameObjectItem = src;
    }
}