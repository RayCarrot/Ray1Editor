using System.Linq;
using BinarySerializer;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using RayCarrot.UI;

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
                    DebugText = "Debug Text",
                    SelectedObjectName = "Object Name",
                };

                vm.ObjFields.Add(new EditorIntFieldViewModel("Int field", "Int field info", () => 0, x => {}));
                vm.ObjFields.Add(new EditorDropDownFieldViewModel("Drop-down field", "Drop-down field info", () => 0, x => {}, () => new EditorDropDownFieldViewModel.DropDownItem[]
                {
                    new EditorDropDownFieldViewModel.DropDownItem("Item 1", null),
                    new EditorDropDownFieldViewModel.DropDownItem("Item 2", null),
                }));

                vm.Layers.AddRange(new LayerEditorViewModel[]
                {
                    new LayerEditorViewModel(new TileMapLayer_R1(new MapTile[4], Point.Zero, new Point(2, 2), new TileSet(null, new Point(16)))),
                    new LayerEditorViewModel(new TileMapLayer_R1(new MapTile[8], Point.Zero, new Point(4, 2), new TileSet(null, new Point(16)))),
                });

                foreach (LayerEditorViewModel l in vm.Layers)
                    l.RecreateFields();

                vm.GameObjects.AddRange(new GameObjectListItemViewModel[]
                {
                    new GameObjectListItemViewModel(new GameObject_R1(new ObjData()
                    {
                        Type = ObjType.TYPE_BB1
                    })),
                    new GameObjectListItemViewModel(new GameObject_R1(new ObjData()
                    {
                        Type = ObjType.TYPE_RAY_POS
                    })),
                });

                vm.Palettes.AddRange(new PaletteEditorViewModel[]
                {
                    new PaletteEditorViewModel(DummyPalette, true, _ => {}),
                    new PaletteEditorViewModel(DummyPalette, false, _ => {}),
                });

                return vm;
            }
        }

        public static EditPaletteViewModel EditPaletteViewModel => new EditPaletteViewModel(DummyPalette);

        private static Palette DummyPalette => new Palette(Enumerable.Range(0, 256).Select(x => new BGR888Color()
        {
            R = (byte)x
        }), "Design-time palette");
    }
}