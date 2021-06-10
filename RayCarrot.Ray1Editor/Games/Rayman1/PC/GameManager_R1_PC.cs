using BinarySerializer;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Image;
using Microsoft.Xna.Framework.Graphics;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Game manager for Rayman 1 on PC
    /// </summary>
    public class GameManager_R1_PC : GameManager_R1
    {
        // TODO: Move some load code out from here and into R1 base class so it can be reused

        #region Paths

        public string Path_VigFile => $"VIGNET.DAT";
        public string Path_DataDir => $"PCMAP/";
        public string Path_FixFile => Path_DataDir + $"ALLFIX.DAT";
        public string Path_WorldFile(World world) => Path_DataDir + $"RAY{(int)world}.WLD";
        public string Path_LevelFile(World world, int level) => Path_DataDir + $"{world}/" + $"RAY{level}.LEV";

        #endregion

        #region R1 Manager

        protected override int MaxObjType => (int)ObjType.TYPE_EDU_DIRECTION;

        #endregion

        #region Manager

        public override IEnumerable<LoadGameLevelViewModel> GetLevels()
        {
            // TODO: Don't hard-code the PC version - set based on game mode
            return GetLevels(Ray1EngineVersion.PC, Ray1PCVersion.PC_1_21);
        }

        public override GameData Load(Context context, object settings, TextureManager textureManager)
        {
            // Get settings
            var ray1Settings = (Ray1Settings)settings;
            var world = ray1Settings.World;
            var level = ray1Settings.Level;

            // Add the settings
            context.AddSettings(ray1Settings);

            // Add files
            context.AddFile(new LinearSerializedFile(context, Path_VigFile));
            context.AddFile(new LinearSerializedFile(context, Path_FixFile));
            context.AddFile(new LinearSerializedFile(context, Path_WorldFile(world)));
            context.AddFile(new LinearSerializedFile(context, Path_LevelFile(world, level)));

            // Create the data
            var data = new GameData_R1(context);

            // Read the files
            var fix = FileFactory.Read<PC_AllfixFile>(Path_FixFile, context);
            var wld = FileFactory.Read<PC_WorldFile>(Path_WorldFile(world), context);
            var lev = FileFactory.Read<SerializableEditorFile<PC_LevFile>>(Path_LevelFile(world, level), context).FileData;

            // Load palettes
            LoadPalettes(data, lev);

            // Load fond (background)
            LoadFond(data, wld, lev, textureManager);

            // Load map
            LoadMap(data, lev, textureManager);

            // Load DES (sprites & animations)
            LoadDES(data, fix, wld, textureManager);

            // Load ETA (states)
            LoadETA(data, fix, wld);

            // Load objects
            LoadObjects(data, lev);

            return data;
        }

        public override void Save(Context context, GameData gameData)
        {
            // Get settings
            var ray1Settings = context.GetSettings<Ray1Settings>();
            var world = ray1Settings.World;
            var level = ray1Settings.Level;

            var data = (GameData_R1)gameData;

            // Get the level data
            var lvlData = context.GetMainFileObject<SerializableEditorFile<PC_LevFile>>(Path_LevelFile(world, level)).FileData;

            // Save the palettes
            lvlData.MapData.ColorPalettes = data.PC_Palettes.Select(x => x.ToBaseColorArray<RGB666Color>()).ToArray();

            // Save the objects
            var objData = lvlData.ObjData;
            objData.ObjCount = (ushort)gameData.Objects.Count;
            objData.Objects = gameData.Objects.Cast<GameObject_R1>().Select(x => x.ObjData).ToArray();

            var objCmds = new List<PC_CommandCollection>();

            foreach (var obj in objData.Objects)
            {
                obj.PC_AnimationsIndex = (uint)data.PC_LoadedAnimations.FindItemIndex(x => x == obj.Animations);
                obj.AnimationsCount = (byte)obj.Animations.Length;
                obj.PC_ImageBufferIndex = (uint)data.PC_DES.FindItemIndex(x => x?.ImageData == obj.ImageBuffer);
                obj.PC_SpritesIndex = (uint)data.PC_DES.FindItemIndex(x => x?.Sprites == obj.Sprites);
                obj.SpritesCount = (byte)obj.Sprites.Length;
                obj.PC_ETAIndex = (uint)data.ETA.IndexOf(obj.ETA);

                var cmds = obj.Commands ?? new CommandCollection()
                {
                    Commands = new Command[0]
                };
                var labelOffsets = obj.LabelOffsets ?? new ushort[0];

                // Add the event commands
                objCmds.Add(new PC_CommandCollection()
                {
                    CommandLength = (ushort)cmds.Commands.Select(x => x.Length).Sum(),
                    Commands = cmds,
                    LabelOffsetCount = (ushort)labelOffsets.Length,
                    LabelOffsetTable = labelOffsets
                });
            }

            objData.ObjCommands = objCmds.ToArray();
            objData.ObjLinkingTable = SaveLinkGroups(gameData.Objects);

            // Save the map
            var mapData = lvlData.MapData;
            var mapLayer = data.Layers.OfType<TileMapLayer_R1>().First();

            mapData.Width = (ushort)mapLayer.MapSize.X;
            mapData.Height = (ushort)mapLayer.MapSize.Y;

            mapData.Tiles = mapLayer.TileMap;

            // Save the background
            lvlData.FNDIndex = (byte)data.Layers.OfType<BackgroundLayer>().ElementAt(0).SelectedBackgroundIndex;
            lvlData.ScrollDiffFNDIndex = (byte)data.Layers.OfType<BackgroundLayer>().ElementAt(1).SelectedBackgroundIndex;

            // Save the file
            FileFactory.Write<SerializableEditorFile<PC_LevFile>>(Path_LevelFile(world, level), context);
        }

        // TODO: Is it better to move this to the GameObject class? Each object type can then have its own fields, but that also means the fields get recreated each time the selection changes.
        public override IEnumerable<EditorFieldViewModel> GetEditorObjFields(GameData gameData, Func<GameObject> getSelectedObj)
        {
            // Helper methods
            GameObject_R1 getObj() => (GameObject_R1)getSelectedObj();
            ObjData getObjData() => getObj().ObjData;

            var data = (GameData_R1)gameData;
            var settings = data.Context.GetSettings<Ray1Settings>();

            var dropDownItems_type = Enum.GetValues(typeof(ObjType)).
                Cast<ObjType>().
                Where(x => (int)x <= MaxObjType && x != ObjType.ObjType_255).
                Select(x => new EditorDropDownFieldViewModel.DropDownItem<ObjType>(x.ToString(), x)).
                ToArray();
            var dropDownLookup_type = dropDownItems_type.Select((x, i) => new { x, i }).ToDictionary(x => x.x.Data, x => x.i);
            
            var dropDownItems_des = data.DES.Select((x, i) => new EditorDropDownFieldViewModel.DropDownItem<GameData_R1.DESData>($"DES {i + 1}", x)).ToArray();
            var dropDownLookup_des = dropDownItems_des.Select((x, i) => new { x, i }).ToDictionary(x => x.x.Data.SpritesData, x => x.i);
            
            var dropDownItems_eta = data.ETA.Select((x, i) => new EditorDropDownFieldViewModel.DropDownItem<ETA>($"ETA {i}", x)).ToArray();
            var dropDownLookup_eta = dropDownItems_eta.Select((x, i) => new { x, i }).ToDictionary(x => x.x.Data, x => x.i);
            
            var dropDownItems_state = new Dictionary<ETA, EditorDropDownFieldViewModel.DropDownItem<DropDownFieldData_State>[]>();
            var dropDownLookup_state = new Dictionary<ETA, Dictionary<int, int>>();

            yield return new EditorDropDownFieldViewModel(
                header: "Type",
                info: null,
                getValueAction: () => dropDownLookup_type[getObjData().Type],
                setValueAction: x => getObjData().Type = dropDownItems_type[x].Data,
                getItemsAction: () => dropDownItems_type);
            
            yield return new EditorDropDownFieldViewModel(
                header: "DES",
                info: "The group of sprites and animations to use.",
                getValueAction: () => dropDownLookup_des[getObjData().Sprites],
                setValueAction: x =>
                {
                    getObjData().Sprites = dropDownItems_des[x].Data.SpritesData;
                    getObjData().Animations = dropDownItems_des[x].Data.AnimationsData;
                    getObjData().ImageBuffer = dropDownItems_des[x].Data.ImageBuffer;
                },
                getItemsAction: () => dropDownItems_des);

            yield return new EditorDropDownFieldViewModel(
                header: "ETA",
                info: "The group of states to use. This determines from which group the state indices are for.",
                getValueAction: () => dropDownLookup_eta[getObjData().ETA],
                setValueAction: x => getObjData().ETA = dropDownItems_eta[x].Data,
                getItemsAction: () => dropDownItems_eta);

            yield return new EditorDropDownFieldViewModel(
                header: "State",
                info: "The object state (ETA), grouped by the primary state (etat) and sub-state (sub-etat). The state primarily determines which animation to play, but also other factors such as how the object should behave (based on the type).",
                getValueAction: () => dropDownLookup_state[getObjData().ETA][DropDownFieldData_State.GetID(getObjData().Etat, getObjData().SubEtat)],
                setValueAction: x =>
                {
                    getObjData().Etat = dropDownItems_state[getObjData().ETA][x].Data.Etat;
                    getObjData().SubEtat = dropDownItems_state[getObjData().ETA][x].Data.SubEtat;
                },
                getItemsAction: () =>
                {
                    var eta = getObjData().ETA;

                    if (!dropDownItems_state.ContainsKey(eta))
                    {
                        dropDownItems_state[eta] = eta.States.Select((etat, etatIndex) => etat.Select((subEtat, subEtatIndex) =>
                            new EditorDropDownFieldViewModel.DropDownItem<DropDownFieldData_State>($"State {etatIndex}-{subEtatIndex} (Animation {subEtat.AnimationIndex})", new DropDownFieldData_State((byte)etatIndex, (byte)subEtatIndex)))).SelectMany(x => x).ToArray();
                        dropDownLookup_state[eta] = dropDownItems_state[eta].Select((x, i) => new { x, i }).ToDictionary(x => x.x.Data.ID, x => x.i);
                    }

                    return dropDownItems_state[eta];
                });

            yield return new EditorPointFieldViewModel(
                header: "Pivot",
                info: "The object pivot (BX and BY).",
                getValueAction: () => getSelectedObj().Pivot,
                setValueAction: x =>
                {
                    getObjData().OffsetBX = (byte)x.X;
                    getObjData().OffsetBY = (byte)x.Y;
                },
                max: Byte.MaxValue);

            yield return new EditorIntFieldViewModel(
                header: "Offset HY",
                info: "This offset is relative to the follow sprite position and is usually used for certain platform collision.",
                getValueAction: () => getObjData().OffsetHY,
                setValueAction: x => getObjData().OffsetHY = (byte)x,
                max: Byte.MaxValue);

            yield return new EditorBoolFieldViewModel(
                header: "Follow",
                info: "This indicates if the object has platform collision, such as that used on clouds and plums.",
                getValueAction: () => getObjData().GetFollowEnabled(settings),
                setValueAction: x => getObjData().SetFollowEnabled(settings, x));

            yield return new EditorIntFieldViewModel(
                header: "Follow-Sprite",
                info: "The index of the sprite which has platform collision, if follow is enabled.",
                getValueAction: () => getObjData().FollowSprite,
                setValueAction: x => getObjData().FollowSprite = (byte)x,
                max: Byte.MaxValue);

            yield return new EditorIntFieldViewModel(
                header: "Hit-Points",
                info: "This value usually determines how many hits it takes to defeat the enemy. For non-enemy objects this can have other usages, such as determining the color or changing other specific attributes.",
                getValueAction: () => getObjData().ActualHitPoints,
                setValueAction: x => getObjData().ActualHitPoints = (uint)x,
                max: settings.EngineVersion is Ray1EngineVersion.PC_Edu or Ray1EngineVersion.PS1_Edu or Ray1EngineVersion.PC_Kit or Ray1EngineVersion.PC_Fan ? Int32.MaxValue : Byte.MaxValue);

            yield return new EditorIntFieldViewModel(
                header: "Hit-Sprite",
                info: "If under 253 this is the index of the sprite which has collision, if above 253 the sprite uses type collision instead.",
                getValueAction: () => getObjData().HitSprite,
                setValueAction: x => getObjData().HitSprite = (byte)x,
                max: Byte.MaxValue);
        }

        #endregion

        #region Helpers

        public void LoadPalettes(GameData_R1 data, PC_LevFile lev)
        {
            data.PC_Palettes = lev.MapData.ColorPalettes.Select((x, i) => new Palette(x, $"Palette {i + 1}")
            {
                CanEditAlpha = false,
                IsFirstTransparent = true
            }).ToArray();
        }

        public void LoadDES(GameData_R1 data, PC_AllfixFile fix, PC_WorldFile wld, TextureManager textureManager)
        {
            data.PC_DES = new PC_DES[]
            {
                null
            }.Concat(fix.DesItems).Concat(wld.DesItems).ToArray();

            data.PC_LoadedAnimations = new Animation[data.PC_DES.Length][];

            for (int i = 1; i < data.PC_DES.Length; i++)
            {
                var des = data.PC_DES[i];

                var processedImageData = des.RequiresBackgroundClearing ? PC_DES.ProcessImageData(des.ImageData) : des.ImageData;

                var spriteSheet = new PalettedTextureSheet(textureManager, des.Sprites.Select(x => x.IsDummySprite() ? (Point?)null : new Point(x.Width, x.Height)).ToArray());

                for (int j = 0; j < des.SpritesCount; j++)
                {
                    var spriteSheetEntry = spriteSheet.Entries[j];

                    if (spriteSheetEntry == null)
                        continue;

                    var sprite = des.Sprites[j];

                    spriteSheet.InitEntry(j, data.PC_Palettes[0], processedImageData, imgDataStartIndex: (int)sprite.ImageBufferOffset, imgDataLength: sprite.Width * sprite.Height);
                }

                var loadedAnim = des.Animations.Select(x => new Animation
                {
                    LayersPerFrameSerialized = x.LayersPerFrame,
                    FrameCountSerialized = x.FrameCount,
                    Layers = x.Layers,
                    Frames = x.Frames
                }).ToArray();
                data.PC_LoadedAnimations[i] = loadedAnim;
                data.AddDES(new GameData_R1.DESData(des.Sprites, spriteSheet, loadedAnim, loadedAnim.Select(x => ToCommonAnimation(x)).ToArray(), des.ImageData));
            }
        }

        public void LoadETA(GameData_R1 data, PC_AllfixFile fix, PC_WorldFile wld)
        {
            data.ETA.AddRange(fix.Eta.Select(x => new ETA()
            {
                States = x.States
            }));
            data.ETA.AddRange(wld.Eta.Select(x => new ETA()
            {
                States = x.States
            }));
        }

        public void LoadObjects(GameData_R1 data, PC_LevFile lev)
        {
            var objIndex = 0;

            foreach (var obj in lev.ObjData.Objects)
            {
                obj.Animations = data.PC_LoadedAnimations[(int)obj.PC_AnimationsIndex];
                obj.ImageBuffer = data.PC_DES[obj.PC_SpritesIndex].ImageData;
                obj.Sprites = data.PC_DES[obj.PC_SpritesIndex].Sprites;
                obj.ETA = data.ETA[(int)obj.PC_ETAIndex];
                obj.Commands = lev.ObjData.ObjCommands[objIndex].Commands;
                obj.LabelOffsets = lev.ObjData.ObjCommands[objIndex].LabelOffsetTable;

                data.Objects.Add(new GameObject_R1(obj));

                objIndex++;
            }

            InitLinkGroups(data.Objects, lev.ObjData.ObjLinkingTable);
        }

        public void LoadMap(GameData_R1 data, PC_LevFile lev, TextureManager textureManager)
        {
            var map = lev.MapData;

            var tileSize = new Point(Ray1Settings.CellSize, Ray1Settings.CellSize);

            var tileSheet = new PalettedTextureSheet(textureManager, Enumerable.Repeat((Point?)tileSize, lev.TileTextureData.TexturesOffsetTable.Length).ToArray());

            var tileSet = new TileSet(tileSheet, tileSize);

            for (int i = 0; i < tileSet.TileSheet.Entries.Length; i++)
            {
                var tex = lev.TileTextureData.NonTransparentTextures.
                    Concat(lev.TileTextureData.TransparentTextures).
                    First(x => x.Offset == lev.TileTextureData.TexturesOffsetTable[i]);

                tileSheet.InitEntry(i, data.PC_Palettes[0], tex.ImgData.Select(x => (byte)(255 - x)).ToArray());
            }

            var mapLayer = new TileMapLayer_R1(map.Tiles, Point.Zero, new Point(map.Width, map.Height), tileSet)
            {
                Pointer = map.Offset
            };
            var colLayer = new CollisionMapLayer_R1(map.Tiles, Point.Zero, new Point(map.Width, map.Height), textureManager)
            {
                Pointer = map.Offset
            };

            mapLayer.LinkedLayers.Add(colLayer);
            mapLayer.Select();

            data.Layers.Add(mapLayer);
            data.Layers.Add(colLayer);
        }

        public void LoadFond(GameData_R1 data, PC_WorldFile wld, PC_LevFile lev, TextureManager textureManager)
        {
            // Load every available background
            var fondOptions = wld.Plan0NumPcx.Select(x => LoadFond(data, textureManager, x)).ToArray();

            // Create a layer for the normal and parallax backgrounds
            data.Layers.Add(new BackgroundLayer(fondOptions, Point.Zero, lev.FNDIndex, name: "Background"));
            data.Layers.Add(new BackgroundLayer(fondOptions, Point.Zero, lev.ScrollDiffFNDIndex, name: "Parallax Background")
            {
                IsVisible = false
            });
        }

        public BackgroundLayer.BackgroundEntry LoadFond(GameData_R1 data, TextureManager textureManager, int index)
        {
            var pcx = LoadArchiveFile<PCX>(data.Context, Path_VigFile, index);

            var imgData = pcx.ScanLines.SelectMany(x => x).ToArray();

            var tex = new PalettedTextureData(textureManager.GraphicsDevice, imgData, new Point(pcx.ImageWidth, pcx.ImageHeight), PalettedTextureData.ImageFormat.BPP_8, data.PC_Palettes[0]);

            tex.Apply();

            textureManager.AddPalettedTexture(tex);

            return new BackgroundLayer.BackgroundEntry(tex.Texture, pcx.Offset, $"{index}.pcx");
        }

        public T LoadArchiveFile<T>(Context context, string archivePath, int fileIndex)
            where T : BinarySerializable, new()
        {
            if (context.MemoryMap.Files.All(x => x.FilePath != archivePath))
                return null;

            return FileFactory.Read<PC_FileArchive>(archivePath, context).ReadFile<T>(context, fileIndex);
        }

        public int InitLinkGroups(IList<GameObject> objects, ushort[] linkTable)
        {
            int currentId = 1;

            for (int i = 0; i < objects.Count; i++)
            {
                if (i >= linkTable.Length)
                    break;

                // No link
                if (linkTable[i] == i)
                {
                    objects[i].LinkGroup = 0;
                }
                else
                {
                    // Ignore already assigned ones
                    if (objects[i].LinkGroup != 0)
                        continue;

                    // Link found, loop through everyone on the link chain
                    int nextEvent = linkTable[i];
                    objects[i].LinkGroup = currentId;
                    int prevEvent = i;
                    while (nextEvent != i && nextEvent != prevEvent)
                    {
                        prevEvent = nextEvent;
                        objects[nextEvent].LinkGroup = currentId;
                        nextEvent = linkTable[nextEvent];
                    }
                    currentId++;
                }
            }

            return currentId;
        }

        public ushort[] SaveLinkGroups(IList<GameObject> objects)
        {
            var linkTable = new ushort[objects.Count];

            List<int> alreadyChained = new List<int>();

            for (ushort i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];

                // No link
                if (obj.LinkGroup == 0)
                {
                    linkTable[i] = i;
                }
                else
                {
                    // Skip if already chained
                    if (alreadyChained.Contains(i))
                        continue;

                    // Find all the events with the same linkId and store their indexes
                    List<ushort> indexesOfSameId = new List<ushort>();
                    int cur = obj.LinkGroup;
                    foreach (var e in objects.Where(e => e.LinkGroup == cur))
                    {
                        indexesOfSameId.Add((ushort)objects.IndexOf(e));
                        alreadyChained.Add(objects.IndexOf(e));
                    }

                    // Loop through and chain them
                    for (int j = 0; j < indexesOfSameId.Count; j++)
                    {
                        int next = j + 1;
                        if (next == indexesOfSameId.Count)
                            next = 0;

                        linkTable[indexesOfSameId[j]] = indexesOfSameId[next];
                    }
                }
            }

            return linkTable;
        }

        #endregion

        #region Field Records

        protected record DropDownFieldData_State(byte Etat, byte SubEtat)
        {
            /// <summary>
            /// A unique ID for the state, used for comparisons
            /// </summary>
            public int ID => GetID(Etat, SubEtat);

            public static int GetID(byte etat, byte subEtat) => etat * 256 + subEtat;
        }

        #endregion
    }
}