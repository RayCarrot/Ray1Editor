using GongSolutions.Wpf.DragDrop;

namespace RayCarrot.Ray1Editor
{
    public class LoadMapGamesListDropTarget : DefaultDropHandler
    {
        public override void Drop(IDropInfo dropInfo)
        {
            // Handle the drop
            base.Drop(dropInfo);

            // Get the source object being moved
            var src = (LoadGameViewModel)dropInfo.Data;
            
            // Move the object to the new position in the app data
            var objects = R1EServices.App.UserData.App_Games;
            objects.Move(objects.IndexOf(src.Game), dropInfo.InsertIndex);
        }
    }
}