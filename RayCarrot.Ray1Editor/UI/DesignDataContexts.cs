﻿using BinarySerializer.Ray1;
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
                    new LayerEditorViewModel(vm, new TileMapLayer_R1(new MapTile[4], Point.Zero, new Point(2, 2), new TileSet(null, new Point(16)))),
                    new LayerEditorViewModel(vm, new TileMapLayer_R1(new MapTile[8], Point.Zero, new Point(4, 2), new TileSet(null, new Point(16)))),
                });

                foreach (LayerEditorViewModel l in vm.Layers)
                    l.RecreateFields();

                return vm;
            }
        }
    }
}