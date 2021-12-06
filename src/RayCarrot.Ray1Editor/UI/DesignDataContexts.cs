using BinarySerializer;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace RayCarrot.Ray1Editor
{
    public static class DesignDataContexts
    {
        public static EditorViewModel EditorViewModel
        {
            get
            {
                var vm = new EditorViewModel(new UserData_Game(), new R1_PC_GameManager(), null)
                {
                    SelectedObject = new R1_GameObject(new ObjData()),
                    DebugText = "Debug Text",
                    SelectedObjectName = "Object Name",
                };

                vm.ObjFields.Add(new EditorIntFieldViewModel("Int field", "Int field info", () => 0, _ => {}));
                vm.ObjFields.Add(new EditorDropDownFieldViewModel("Drop-down field", "Drop-down field info", () => 0, x => {}, () => new EditorDropDownFieldViewModel.DropDownItem[]
                {
                    new EditorDropDownFieldViewModel.DropDownItem("Item 1", null),
                    new EditorDropDownFieldViewModel.DropDownItem("Item 2", null),
                }));

                vm.Layers.AddRange(new LayerEditorViewModel[]
                {
                    new LayerEditorViewModel(new R1_TileMapLayer(new MapTile[4], Point.Zero, new Point(2, 2), new TileSet(null, new Point(16)))),
                    new LayerEditorViewModel(new R1_TileMapLayer(new MapTile[8], Point.Zero, new Point(4, 2), new TileSet(null, new Point(16)))),
                });

                foreach (LayerEditorViewModel l in vm.Layers)
                    l.RecreateFields();

                vm.GameObjects.AddRange(new GameObjectListItemViewModel[]
                {
                    new GameObjectListItemViewModel(new R1_GameObject(new ObjData()
                    {
                        Type = ObjType.TYPE_BB1
                    })),
                    new GameObjectListItemViewModel(new R1_GameObject(new ObjData()
                    {
                        Type = ObjType.TYPE_RAY_POS
                    })),
                });

                vm.Palettes.AddRange(new PaletteEditorViewModel[]
                {
                    new PaletteEditorViewModel(DummyPalette, true, _ => {}),
                    new PaletteEditorViewModel(DummyPalette, false, _ => {}),
                });

                vm.SelectedObjectScript = $"LABEL 2:\r\n\tLEFT 200;\r\n\tGOTO LINE 1;\r\nLABEL 3:\r\n\tRIGHT 200;\r\n\tGOTO LINE 4;\r\nLABEL 1:\r\n\tNOP 200;\r\n\tGOTO LINE 7;\r\n\tINVALID_CMD 255;";

                return vm;
            }
        }

        public static EditPaletteViewModel EditPaletteViewModel => new EditPaletteViewModel(DummyPalette);

        private static Palette DummyPalette => new SerializablePalette<BGR888Color>(Enumerable.Range(0, 256).Select(x => new BGR888Color()
        {
            R = (byte)x
        }).ToArray(), "Design-time palette");

        public static DialogMessageViewModel DialogMessageViewModel => new DialogMessageViewModel()
        {
            MessageText = "Message text",
            Title = "Header text",
            MessageType = DialogMessageType.Information,
            DialogImageSource = new ImageSourceConverter().ConvertFromString($"{App.WPFAppBasePath}UI/Img/{DialogMessageType.Information}.png") as ImageSource,
            DialogActions = new ObservableCollection<DialogMessageActionViewModel>()
            {
                new DialogMessageActionViewModel
                {
                    DisplayText = "Ok",
                    DisplayDescription = "Ok",
                    ActionResult = false,
                    IsDefault = true,
                    IsCancel = false,
                    ShouldCloseDialog = true,
                    OnHandled = null
                }
            },
            DefaultActionResult = false
        };
    }
}