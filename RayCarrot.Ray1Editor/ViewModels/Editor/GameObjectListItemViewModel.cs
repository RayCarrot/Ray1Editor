using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class GameObjectListItemViewModel : BaseViewModel
    {
        public GameObjectListItemViewModel(GameObject obj)
        {
            Obj = obj;
        }

        public string Header => Obj.SecondaryName ?? Obj.PrimaryName;
        public GameObject Obj { get; }
    }
}