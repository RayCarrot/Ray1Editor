using BinarySerializer.Ray1;

namespace RayCarrot.Ray1Editor
{
    public static class DesignDataContexts
    {
        public static EditorViewModel EditorViewModel
        {
            get
            {
                var vm = new EditorViewModel(new UserData_Game(), new GameManager_R1_PC(), null)
                {
                    SelectedObject = new GameObject_R1(new ObjData()),
                    ShowObjFields = true,
                    DebugText = "Debug Text",
                    SelectedObjectName = "Object Name"
                };

                vm.ObjFields.Add(new EditorIntFieldViewModel("Int field", "Int field info", () => 0, x => {}));
                vm.ObjFields.Add(new EditorDropDownFieldViewModel("Drop-down field", "Drop-down field info", () => 0, x => {}, () => new EditorDropDownFieldViewModel.DropDownItem[]
                {
                    new EditorDropDownFieldViewModel.DropDownItem("Item 1", null),
                    new EditorDropDownFieldViewModel.DropDownItem("Item 2", null),
                }));

                return vm;
            }
        }
    }
}