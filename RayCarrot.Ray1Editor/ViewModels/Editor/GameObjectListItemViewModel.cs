using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    public class GameObjectListItemViewModel : BaseViewModel
    {
        public GameObjectListItemViewModel(GameObject obj)
        {
            Obj = obj;
        }

        public string Header => Obj.DisplayName;
        public GameObject Obj { get; }
    }
}