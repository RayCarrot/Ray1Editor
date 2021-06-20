using BinarySerializer;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Base game manager for Rayman 1
    /// </summary>
    public abstract class R1_GameManager : GameManager
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Asset Paths

        public const string AssetPath_CollisionFile = "Assets/Collision/R1_TypeCollision.png";
        public const string AssetPath_EventsDir = "Assets/Rayman1/Events/";
        public const string AssetPath_EventsFile = AssetPath_EventsDir + "Events.csv";

        #endregion

        #region Protected Properties

        protected abstract int MaxObjType { get; }
        protected virtual bool UsesLocalCommands => false;

        #endregion

        #region Manager

        public override IEnumerable<EditorFieldViewModel> GetEditorObjFields(GameData gameData, Func<GameObject> getSelectedObj)
        {
            // Helper methods
            R1_GameObject getObj() => (R1_GameObject)getSelectedObj();
            ObjData getObjData() => getObj().ObjData;

            var data = (R1_GameData)gameData;
            var settings = data.Context.GetSettings<Ray1Settings>();

            var dropDownItems_type = Enum.GetValues(typeof(ObjType)).
                Cast<ObjType>().
                Where(x => (int)x <= MaxObjType && x != ObjType.ObjType_255).
                Select(x => new EditorDropDownFieldViewModel.DropDownItem<ObjType>(x.ToString(), x)).
                ToArray();
            var dropDownLookup_type = dropDownItems_type.Select((x, i) => new { x, i }).ToDictionary(x => x.x.Data, x => x.i);

            var dropDownItems_des = data.DES.Select((x, i) => new EditorDropDownFieldViewModel.DropDownItem<R1_GameData.DESData>($"DES {i + 1}{(x.Name != null ? $" ({x.Name})" : "")}", x)).ToArray();
            var dropDownLookup_des = dropDownItems_des.Select((x, i) => new { x, i }).ToDictionary(x => x.x.Data.SpritesData, x => x.i);

            var dropDownItems_eta = data.ETA.Select((x, i) => new EditorDropDownFieldViewModel.DropDownItem<ETA>($"ETA {i}{(x.Name != null ? $" ({x.Name})" : "")}", x.ETA)).ToArray();
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
                getValueAction: () => dropDownLookup_des[getObjData().SpriteCollection],
                setValueAction: x =>
                {
                    getObjData().SpriteCollection = dropDownItems_des[x].Data.SpritesData;
                    getObjData().AnimationCollection = dropDownItems_des[x].Data.AnimationsData;
                    getObjData().ImageBuffer = dropDownItems_des[x].Data.ImageBuffer;
                },
                getItemsAction: () => dropDownItems_des);

            yield return new EditorDropDownFieldViewModel(
                header: "ETA",
                info: "The group of states to use. This determines from which group the state indices are for.",
                getValueAction: () => dropDownLookup_eta[getObjData().ETA],
                setValueAction: x =>
                {
                    var obj = getObjData();
                    obj.ETA = dropDownItems_eta[x].Data;
                    obj.Etat = obj.InitialEtat = 0;
                    obj.SubEtat = obj.InitialSubEtat = 0;
                },
                getItemsAction: () => dropDownItems_eta);

            yield return new EditorDropDownFieldViewModel(
                header: "State",
                info: "The object state (ETA), grouped by the primary state (etat) and sub-state (sub-etat). The state primarily determines which animation to play, but also other factors such as how the object should behave (based on the type).",
                getValueAction: () => dropDownLookup_state[getObjData().ETA][DropDownFieldData_State.GetID(getObjData().Etat, getObjData().SubEtat)],
                setValueAction: x =>
                {
                    var obj = getObjData();

                    obj.Etat = obj.InitialEtat = dropDownItems_state[getObjData().ETA][x].Data.Etat;
                    obj.SubEtat = obj.InitialSubEtat = dropDownItems_state[getObjData().ETA][x].Data.SubEtat;
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

        public override void PostLoad(GameData gameData)
        {
            var data = (R1_GameData)gameData;

            // Hard-code event animations for the different Rayman types. By default they have 0 animations in the files.
            AnimationCollection rayAnim = null;

            // Try and get the Rayman template
            var ray = data.ObjTemplates.TryGetValue(R1_GameData.WldObjType.Ray);

            // Get Rayman's animations
            if (ray != null)
                rayAnim = ray.AnimationCollection;

            if (rayAnim != null)
            {
                // Try and get the small Rayman template
                var miniRay = data.ObjTemplates.TryGetValue(R1_GameData.WldObjType.RayLittle);

                // If found we replace the empty animation array with Rayman's (on PS1 it already uses Rayman's animations, but not on PC)
                if (miniRay != null)
                    data.Animations[miniRay.AnimationCollection] = data.Animations[rayAnim];

                // Bad Rayman has no template due to being world specific, so we just try and find him in the map. This won't work if he wasn't in the map initially.
                var badRay = data.Objects.OfType<R1_GameObject>().FirstOrDefault(x => x.ObjData.Type == ObjType.TYPE_BLACK_RAY);

                if (badRay != null)
                    data.Animations[badRay.ObjData.AnimationCollection] = data.Animations[rayAnim];
            }

            // Set frames for linked events
            for (int i = 0; i < data.Objects.Count; i++)
            {
                // Recreated from allocateOtherPosts
                var baseEvent = (R1_GameObject)data.Objects[i];
                var linkedIndex = data.LinkTable[i];

                if (!baseEvent.ObjData.Type.UsesRandomFrameLinks() || i == linkedIndex)
                    continue;

                var index = 0;

                do
                {
                    index++;

                    var e = (R1_GameObject)data.Objects[linkedIndex];
                    e.ForceFrame = (byte)((baseEvent.ForceFrame + index) % (e.CurrentAnimation?.Frames.Length ?? 1));

                    e.ObjData.XPosition = (short)(baseEvent.ObjData.XPosition + 32 * index * (baseEvent.ObjData.HitPoints - 2));
                    e.ObjData.YPosition = baseEvent.ObjData.YPosition;

                    linkedIndex = data.LinkTable[linkedIndex];
                } while (i != linkedIndex);
            }
        }

        public override IEnumerable<string> GetAvailableObjects(GameData gameData)
        {
            return ((R1_GameData)gameData).EventDefinitions?.Select(x => x.Name) ?? new string[0];
        }

        public override GameObject CreateGameObject(GameData gameData, int index)
        {
            var data = (R1_GameData)gameData;
            var settings = data.Context.GetSettings<Ray1Settings>();
            var def = data.EventDefinitions[index];

            // Get the commands and label offsets
            CommandCollection cmds = null;
            ushort[] labelOffsets = null;

            // If local (non-compiled) commands are used, attempt to get them from the event info or decompile the compiled ones
            if (UsesLocalCommands)
            {
                cmds = ObjCommandCompiler.Decompile(new ObjCommandCompiler.CompiledObjCommandData(CommandCollection.FromBytes(def.Commands, () =>
                {
                    var c = new EditorContext(data.Context.BasePath, noLog: true);
                    c.AddSettings(settings);
                    return c;
                }), def.LabelOffsets), def.Commands);
            }
            else if (def.Commands.Any())
            {
                cmds = CommandCollection.FromBytes(def.Commands, () =>
                {
                    var c = new EditorContext(data.Context.BasePath, noLog: true);
                    c.AddSettings(settings);
                    return c;
                });
                labelOffsets = def.LabelOffsets.Any() ? def.LabelOffsets : null;
            }

            var des = data.DES.First(x => x.Name == def.DES);
            var eta = data.ETA.First(x => x.Name == def.ETA);

            var obj = new R1_GameObject(ObjData.CreateObj(settings), def);

            obj.ObjData.Type = (ObjType)def.Type;
            obj.ObjData.Etat = def.Etat;
            obj.ObjData.SubEtat = def.SubEtat;
            obj.ObjData.OffsetBX = def.OffsetBX;
            obj.ObjData.OffsetBY = def.OffsetBY;
            obj.ObjData.OffsetHY = def.OffsetHY;
            obj.ObjData.FollowSprite = def.FollowSprite;
            obj.ObjData.HitSprite = def.HitSprite;
            obj.ObjData.Commands = cmds;
            obj.ObjData.LabelOffsets = labelOffsets;
            obj.ObjData.AnimationCollection = des.AnimationsData;
            obj.ObjData.ImageBuffer = des.ImageBuffer;
            obj.ObjData.SpriteCollection = des.SpritesData;
            obj.ObjData.ETA = eta.ETA;
            obj.ObjData.SetFollowEnabled(data.Context.GetSettings<Ray1Settings>(), def.FollowEnabled);
            obj.ObjData.ActualHitPoints = def.HitPoints;

            Logger.Log(LogLevel.Trace, "Created object {0}", obj.DisplayName);

            return obj;
        }

        public override int GetMaxObjCount(GameData gameData)
        {
            switch (gameData.Context.GetSettings<Ray1Settings>().EngineVersion)
            {
                case Ray1EngineVersion.PS1_JPDemoVol3:
                case Ray1EngineVersion.PS1_JPDemoVol6:
                case Ray1EngineVersion.PS1:
                case Ray1EngineVersion.PS1_EUDemo:
                case Ray1EngineVersion.PS1_JP:
                case Ray1EngineVersion.Saturn:
                    return 254; // Event index is a byte, 0xFF is Rayman

                case Ray1EngineVersion.R2_PS1:
                    return 254; // Event index is a short, so might be higher

                case Ray1EngineVersion.PC:
                case Ray1EngineVersion.PocketPC:
                case Ray1EngineVersion.GBA:
                case Ray1EngineVersion.DSi:
                    return 254; // Event index is a short, so might be higher

                case Ray1EngineVersion.PC_Kit:
                case Ray1EngineVersion.PC_Fan:
                case Ray1EngineVersion.PC_Edu:
                case Ray1EngineVersion.PS1_Edu:
                    return 700; // This is the max in KIT/FAN - same in EDU?

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int MaxDisplayPrio => 7;

        #endregion

        #region Public Methods

        public IEnumerable<LoadGameLevelViewModel> GetLevels(Ray1EngineVersion engineVersion, Ray1PCVersion pcVersion = Ray1PCVersion.None, string volume = null)
        {
            yield return new LoadGameLevelViewModel("Jungle", null);
            yield return new LoadGameLevelViewModel("Pink Plant Woods 1", new Ray1Settings(engineVersion, World.Jungle, 1, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Pink Plant Woods 2", new Ray1Settings(engineVersion, World.Jungle, 2, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Pink Plant Woods 3 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 3, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Pink Plant Woods 4", new Ray1Settings(engineVersion, World.Jungle, 4, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Anguish Lagoon 1", new Ray1Settings(engineVersion, World.Jungle, 5, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Anguish Lagoon 2 - Boss", new Ray1Settings(engineVersion, World.Jungle, 6, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Anguish Lagoon 3 - Bzzit Flight", new Ray1Settings(engineVersion, World.Jungle, 7, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Anguish Lagoon 4 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 8, pcVersion, volume));
            yield return new LoadGameLevelViewModel("The Swamps of Forgetfulness 1", new Ray1Settings(engineVersion, World.Jungle, 9, pcVersion, volume));
            yield return new LoadGameLevelViewModel("The Swamps of Forgetfulness 2", new Ray1Settings(engineVersion, World.Jungle, 10, pcVersion, volume));
            yield return new LoadGameLevelViewModel("The Swamps of Forgetfulness 3", new Ray1Settings(engineVersion, World.Jungle, 11, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Moskito's Nest 1", new Ray1Settings(engineVersion, World.Jungle, 12, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Moskito's Nest 2", new Ray1Settings(engineVersion, World.Jungle, 13, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Moskito's Nest 3 - Boss Chase", new Ray1Settings(engineVersion, World.Jungle, 14, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Moskito's Nest 4", new Ray1Settings(engineVersion, World.Jungle, 15, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Moskito's Nest 5 - Boss", new Ray1Settings(engineVersion, World.Jungle, 16, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Moskito's Nest 6 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 17, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Jungle 1", new Ray1Settings(engineVersion, World.Jungle, 18, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Jungle 2", new Ray1Settings(engineVersion, World.Jungle, 19, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Jungle 3", new Ray1Settings(engineVersion, World.Jungle, 20, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Jungle 4", new Ray1Settings(engineVersion, World.Jungle, 21, pcVersion, volume));
            
            if (engineVersion is Ray1EngineVersion.PC or Ray1EngineVersion.DSi)
                yield return new LoadGameLevelViewModel("(Minigame) Ray Breakout", new Ray1Settings(engineVersion, World.Jungle, 22, pcVersion, volume));

            yield return new LoadGameLevelViewModel("Music", null);
            yield return new LoadGameLevelViewModel("Bongo Hills 1", new Ray1Settings(engineVersion, World.Music, 1, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Bongo Hills 2", new Ray1Settings(engineVersion, World.Music, 2, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Bongo Hills 3", new Ray1Settings(engineVersion, World.Music, 3, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Bongo Hills 4", new Ray1Settings(engineVersion, World.Music, 4, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Bongo Hills 5", new Ray1Settings(engineVersion, World.Music, 5, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Bongo Hills 6", new Ray1Settings(engineVersion, World.Music, 6, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Allegro Presto 1", new Ray1Settings(engineVersion, World.Music, 7, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Allegro Presto 2", new Ray1Settings(engineVersion, World.Music, 8, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Allegro Presto 3", new Ray1Settings(engineVersion, World.Music, 9, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Allegro Presto 4", new Ray1Settings(engineVersion, World.Music, 10, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Allegro Presto 5 - Betilla", new Ray1Settings(engineVersion, World.Music, 11, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Gong Heights 1", new Ray1Settings(engineVersion, World.Music, 12, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Gong Heights 2", new Ray1Settings(engineVersion, World.Music, 13, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Sax's Hullaballoo 1", new Ray1Settings(engineVersion, World.Music, 14, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Sax's Hullaballoo 2 - Boss Chase", new Ray1Settings(engineVersion, World.Music, 15, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Sax's Hullaballoo 3 - Boss", new Ray1Settings(engineVersion, World.Music, 16, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Music 1", new Ray1Settings(engineVersion, World.Music, 17, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Music 2", new Ray1Settings(engineVersion, World.Music, 18, pcVersion, volume));

            yield return new LoadGameLevelViewModel("Mountain", null);
            yield return new LoadGameLevelViewModel("Twilight Gulch 1", new Ray1Settings(engineVersion, World.Mountain, 1, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Twilight Gulch 2", new Ray1Settings(engineVersion, World.Mountain, 2, pcVersion, volume));
            yield return new LoadGameLevelViewModel("The Hard Rocks 1", new Ray1Settings(engineVersion, World.Mountain, 3, pcVersion, volume));
            yield return new LoadGameLevelViewModel("The Hard Rocks 2", new Ray1Settings(engineVersion, World.Mountain, 4, pcVersion, volume));
            yield return new LoadGameLevelViewModel("The Hard Rocks 3", new Ray1Settings(engineVersion, World.Mountain, 5, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Stone's Peaks 1", new Ray1Settings(engineVersion, World.Mountain, 6, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Stone's Peaks 2", new Ray1Settings(engineVersion, World.Mountain, 7, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Stone's Peaks 3", new Ray1Settings(engineVersion, World.Mountain, 8, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Stone's Peaks 4", new Ray1Settings(engineVersion, World.Mountain, 9, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Stone's Peaks 5 - Boss", new Ray1Settings(engineVersion, World.Mountain, 10, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Stone's Peaks 6 - Betilla", new Ray1Settings(engineVersion, World.Mountain, 11, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Mountain 1", new Ray1Settings(engineVersion, World.Mountain, 12, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Mountain 2", new Ray1Settings(engineVersion, World.Mountain, 13, pcVersion, volume));

            yield return new LoadGameLevelViewModel("Image", null);
            yield return new LoadGameLevelViewModel("Eraser Plains 1", new Ray1Settings(engineVersion, World.Image, 1, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eraser Plains 2", new Ray1Settings(engineVersion, World.Image, 2, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eraser Plains 3", new Ray1Settings(engineVersion, World.Image, 3, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eraser Plains 4 - Boss", new Ray1Settings(engineVersion, World.Image, 4, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Pencil Pentathlon 1", new Ray1Settings(engineVersion, World.Image, 5, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Pencil Pentathlon 2", new Ray1Settings(engineVersion, World.Image, 6, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Pencil Pentathlon 3", new Ray1Settings(engineVersion, World.Image, 7, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Space Mama's Crater 1", new Ray1Settings(engineVersion, World.Image, 8, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Space Mama's Crater 2", new Ray1Settings(engineVersion, World.Image, 9, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Space Mama's Crater 3", new Ray1Settings(engineVersion, World.Image, 10, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Space Mama's Crater 4 - Boss", new Ray1Settings(engineVersion, World.Image, 11, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Image 1", new Ray1Settings(engineVersion, World.Image, 12, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Image 2", new Ray1Settings(engineVersion, World.Image, 13, pcVersion, volume));

            yield return new LoadGameLevelViewModel("Cave", null);
            yield return new LoadGameLevelViewModel("Crystal Palace 1", new Ray1Settings(engineVersion, World.Cave, 1, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Crystal Palace 2", new Ray1Settings(engineVersion, World.Cave, 2, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eat at Joe's 1", new Ray1Settings(engineVersion, World.Cave, 3, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eat at Joe's 2", new Ray1Settings(engineVersion, World.Cave, 4, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eat at Joe's 3", new Ray1Settings(engineVersion, World.Cave, 5, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eat at Joe's 4", new Ray1Settings(engineVersion, World.Cave, 6, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eat at Joe's 5", new Ray1Settings(engineVersion, World.Cave, 7, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Eat at Joe's 6", new Ray1Settings(engineVersion, World.Cave, 8, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Skops' Stalactites 1", new Ray1Settings(engineVersion, World.Cave, 9, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Skops' Stalactites 2 - Boss", new Ray1Settings(engineVersion, World.Cave, 10, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Skops' Stalactites 3 - Boss", new Ray1Settings(engineVersion, World.Cave, 11, pcVersion, volume));
            yield return new LoadGameLevelViewModel("(Bonus) Cave 1", new Ray1Settings(engineVersion, World.Cave, 12, pcVersion, volume));

            yield return new LoadGameLevelViewModel("Cake", null);
            yield return new LoadGameLevelViewModel("Mr Dark's Dare 1", new Ray1Settings(engineVersion, World.Cake, 1, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Dark's Dare 2", new Ray1Settings(engineVersion, World.Cake, 2, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Dark's Dare 3", new Ray1Settings(engineVersion, World.Cake, 3, pcVersion, volume));
            yield return new LoadGameLevelViewModel("Mr Dark's Dare 4 - Boss", new Ray1Settings(engineVersion, World.Cake, 4, pcVersion, volume));
        }

        /// <summary>
        /// Converts a Rayman 1 animation to an editor animation
        /// </summary>
        /// <param name="anim">The animation</param>
        /// <param name="baseSpriteIndex">The base sprite index</param>
        /// <returns>The editor animation</returns>
        public ObjAnimation ToCommonAnimation(IAnimation anim, int baseSpriteIndex = 0)
        {
            // Create the animation
            var animation = new ObjAnimation
            {
                Frames = new ObjAnimation_Frame[anim.FrameCount],
            };

            // The layer index
            var layer = 0;

            var layers = anim.Layers;

            // Create each frame
            for (int i = 0; i < anim.FrameCount; i++)
            {
                // Create the frame
                var frame = new ObjAnimation_Frame(new ObjAnimation_SpriteLayer[anim.LayersPerFrame]);

                if (layers != null)
                {

                    // Create each layer
                    for (var layerIndex = 0; layerIndex < anim.LayersPerFrame; layerIndex++)
                    {
                        var animationLayer = layers[layer];
                        layer++;

                        // Create the animation part
                        var part = new ObjAnimation_SpriteLayer
                        {
                            SpriteIndex = animationLayer.SpriteIndex + baseSpriteIndex,
                            Position = new Point(animationLayer.XPosition, animationLayer.YPosition),
                            IsFlippedHorizontally = animationLayer.IsFlippedHorizontally,
                            IsFlippedVertically = animationLayer.IsFlippedVertically
                        };

                        // Add the part
                        frame.SpriteLayers[layerIndex] = part;
                    }
                }
                // Set the frame
                animation.Frames[i] = frame;
            }

            return animation;
        }

        public void InitRandom(R1_GameData data)
        {
            data.RandomIndex = (byte)data.Random.Next(0, 256);
            data.RandomArray = data.Context.Deserializer.SerializeFromBytes<Array<ushort>>(RandomArrayData, "RandomArrayData", onPreSerialize: x => x.Length = 256, name: nameof(data.RandomArray)).Value;
        }

        public void LoadRayman(R1_GameData data)
        {
            if (!data.ObjTemplates.ContainsKey(R1_GameData.WldObjType.Ray))
                return;

            var template = data.ObjTemplates[R1_GameData.WldObjType.Ray];

            var ray = new ObjData
            {
                PC_SpritesIndex = template.PC_SpritesIndex,
                PC_AnimationsIndex = template.PC_AnimationsIndex,
                PC_ImageBufferIndex = template.PC_ImageBufferIndex,
                PC_ETAIndex = template.PC_ETAIndex,
                SpritesPointer = template.SpritesPointer,
                AnimationsPointer = template.AnimationsPointer,
                ImageBufferPointer = template.ImageBufferPointer,
                ETAPointer = template.ETAPointer,
                CommandsPointer = template.CommandsPointer,
                LabelOffsetsPointer = template.LabelOffsetsPointer,
                SpriteCollection = template.SpriteCollection,
                AnimationCollection = template.AnimationCollection,
                ImageBuffer = template.ImageBuffer,
                ETA = template.ETA
            };

            ray.InitRayman(data.Context, data.Objects.OfType<R1_GameObject>().FirstOrDefault(x => x.ObjData.Type == ObjType.TYPE_RAY_POS)?.ObjData);

            data.Rayman = ray;
        }

        public void LoadEditorEventDefinitions(R1_GameData data)
        {
            var engine = R1_EventDefinition.Engine.R1;
            var settings = data.Context.GetSettings<Ray1Settings>();

            if (settings.EngineVersion is Ray1EngineVersion.PC_Edu or Ray1EngineVersion.PS1_Edu)
                engine = R1_EventDefinition.Engine.EDU;
            else if (settings.EngineVersion is Ray1EngineVersion.PC_Kit or Ray1EngineVersion.PC_Fan)
                engine = R1_EventDefinition.Engine.KIT;

            using var stream = Assets.GetAsset(AssetPath_EventsFile);
            data.EventDefinitions = R1_EventDefinition.ReadCSV(stream).
                Where(x => x.Worlds.Contains(settings.World) && 
                           x.Engines.Contains(engine) && 
                           data.DES.Any(d => d.Name == x.DES) && 
                           data.ETA.Any(e => e.Name == x.ETA)).
                ToArray();
        }

        public void LoadMap(R1_GameData data, TextureManager textureManager, TileSet tileSet, MapTile[] tileMap, int width, int height, Pointer mapPointer, int tileSetWidth = 1)
        {
            var mapLayer = new R1_TileMapLayer(tileMap, Point.Zero, new Point(width, height), tileSet, tileSetWidth)
            {
                Pointer = mapPointer
            };
            var colLayer = new R1_CollisionMapLayer(tileMap, Point.Zero, new Point(width, height), textureManager)
            {
                Pointer = mapPointer
            };

            mapLayer.LinkedLayers.Add(colLayer);
            mapLayer.Select();

            data.Layers.Add(mapLayer);
            data.Layers.Add(colLayer);
        }

        public R1_EventDefinition FindMatchingEventDefinition(R1_GameData data, ObjData e)
        {
            byte[] compiledCmds;
            ushort[] labelOffsets;

            if (UsesLocalCommands)
            {
                var compiledData = e.Commands == null ? null : ObjCommandCompiler.Compile(e.Commands);
                compiledCmds = compiledData?.Commands?.ToBytes(() =>
                {
                    var c = new EditorContext(data.Context.BasePath, noLog: true);
                    c.AddSettings(data.Context.GetSettings<Ray1Settings>());
                    return c;
                }) ?? new byte[0];
                labelOffsets = compiledData?.LabelOffsets ?? new ushort[0];
            }
            else
            {
                compiledCmds = e.Commands?.ToBytes(() =>
                {
                    var c = new EditorContext(data.Context.BasePath, noLog: true);
                    c.AddSettings(data.Context.GetSettings<Ray1Settings>());
                    return c;
                }) ?? new byte[0];
                labelOffsets = e.LabelOffsets ?? new ushort[0];
            }

            // Helper method for comparing the commands
            bool compareCommands(R1_EventDefinition eventInfo) =>
                eventInfo.LabelOffsets.SequenceEqual(labelOffsets) &&
                eventInfo.Commands.SequenceEqual(compiledCmds);

            // Find a matching item
            var match = findMatch(EventMatchFlags.All) ??
                        findMatch(EventMatchFlags.All & ~EventMatchFlags.HitSprite & ~EventMatchFlags.Commands) ??
                        findMatch(EventMatchFlags.Type | EventMatchFlags.Etat | EventMatchFlags.SubEtat);

            R1_EventDefinition findMatch(EventMatchFlags flags)
            {
                return data.EventDefinitions.FirstOrDefault(x => 
                    (!flags.HasFlag(EventMatchFlags.Type) || x.Type == (ushort)e.Type) &&
                    (!flags.HasFlag(EventMatchFlags.Etat) || x.Etat == e.Etat) &&
                    (!flags.HasFlag(EventMatchFlags.SubEtat) || x.SubEtat == e.SubEtat) &&
                    (!flags.HasFlag(EventMatchFlags.OffsetBX) || x.OffsetBX == e.OffsetBX) &&
                    (!flags.HasFlag(EventMatchFlags.OffsetBY) || x.OffsetBY == e.OffsetBY) &&
                    (!flags.HasFlag(EventMatchFlags.OffsetHY) || x.OffsetHY == e.OffsetHY) &&
                    (!flags.HasFlag(EventMatchFlags.FollowSprite) || x.FollowSprite == e.FollowSprite) &&
                    (!flags.HasFlag(EventMatchFlags.HitPoints) || x.HitPoints == e.ActualHitPoints) &&
                    (!flags.HasFlag(EventMatchFlags.HitSprite) || x.HitSprite == e.HitSprite) &&
                    (!flags.HasFlag(EventMatchFlags.FollowEnabled) || x.FollowEnabled == e.GetFollowEnabled(data.Context.GetSettings<Ray1Settings>())) && 
                    (!flags.HasFlag(EventMatchFlags.Commands) || compareCommands(x)));
            }

            // Log if not found
            if (match == null && data.EventDefinitions.Any())
                Logger.Log(LogLevel.Warn, "Matching event not found for event with type {0}, etat {1} & subetat {2}", e.Type, e.Etat, e.SubEtat);

            // Return the item
            return match;
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

        protected T LoadEditorNameTable<T>(string name, EditorNameTableType type)
        {
            using var stream = Assets.GetAsset(AssetPath_EventsDir + $"{name}_{type.ToString().ToLower()}.json");
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonTextReader);
        }

        #endregion

        #region Data Types

        [Flags]
        protected enum EventMatchFlags : ushort
        {
            None = 0,

            Type = 1 << 0,
            Etat = 1 << 1,
            SubEtat = 1 << 2,
            OffsetBX = 1 << 3,
            OffsetBY = 1 << 4,
            OffsetHY = 1 << 5,
            FollowSprite = 1 << 6,
            HitPoints = 1 << 7,
            HitSprite = 1 << 8,
            FollowEnabled = 1 << 9,
            Commands = 1 << 10,

            All = UInt16.MaxValue,
        }

        protected enum EditorNameTableType
        {
            DES,
            ETA
        }

        #endregion

        #region Data

        // IDEA: Move to BinarySerializer.Ray1
        protected byte[] RandomArrayData { get; } =
        {
            0xDE, 0x00, 0x25, 0x02, 0xC8, 0x00, 0xCD, 0x02, 0xCC, 0x03, 0x19, 0x01,
            0xC6, 0x01, 0x6F, 0x00, 0xCA, 0x02, 0x41, 0x02, 0x2A, 0x00, 0xA9, 0x00,
            0x43, 0x03, 0xBD, 0x02, 0x0E, 0x03, 0x4F, 0x03, 0xD6, 0x03, 0xE0, 0x00,
            0xB5, 0x01, 0xCF, 0x03, 0x5B, 0x03, 0xB1, 0x03, 0x3E, 0x03, 0xCD, 0x01,
            0x6B, 0x02, 0xA5, 0x02, 0x66, 0x02, 0x32, 0x02, 0xE1, 0x02, 0x74, 0x00,
            0x9F, 0x01, 0x7C, 0x00, 0xAF, 0x02, 0xE6, 0x01, 0xF6, 0x01, 0x41, 0x02,
            0x60, 0x01, 0x79, 0x03, 0x0E, 0x01, 0xB8, 0x00, 0xB1, 0x01, 0xC7, 0x02,
            0xA7, 0x00, 0x27, 0x02, 0x94, 0x02, 0x7E, 0x02, 0x03, 0x00, 0x26, 0x03,
            0x13, 0x01, 0xD8, 0x01, 0x8B, 0x01, 0x81, 0x01, 0x53, 0x02, 0x69, 0x02,
            0x1E, 0x01, 0xAE, 0x00, 0x38, 0x03, 0x2E, 0x01, 0x55, 0x01, 0xA2, 0x01,
            0xF6, 0x00, 0xA7, 0x01, 0x37, 0x00, 0xF9, 0x01, 0xEF, 0x03, 0x01, 0x00,
            0xA3, 0x01, 0x47, 0x00, 0x4B, 0x00, 0x04, 0x01, 0xE6, 0x03, 0x6B, 0x01,
            0x9E, 0x01, 0xC9, 0x00, 0xC9, 0x00, 0xD8, 0x00, 0xFF, 0x00, 0x08, 0x03,
            0x8E, 0x03, 0x9F, 0x03, 0xF1, 0x02, 0xD8, 0x01, 0x20, 0x02, 0x24, 0x00,
            0x85, 0x00, 0xD5, 0x01, 0xEF, 0x01, 0x63, 0x02, 0x03, 0x01, 0x75, 0x00,
            0xCE, 0x02, 0x98, 0x02, 0x8B, 0x03, 0x80, 0x03, 0x2B, 0x00, 0x36, 0x02,
            0x0A, 0x02, 0x7B, 0x02, 0x15, 0x03, 0x03, 0x01, 0xDE, 0x00, 0xF1, 0x00,
            0x57, 0x01, 0x43, 0x02, 0x88, 0x00, 0x74, 0x02, 0x1F, 0x00, 0xCB, 0x03,
            0xD9, 0x00, 0x3B, 0x03, 0x84, 0x02, 0xDC, 0x02, 0xED, 0x02, 0x35, 0x02,
            0x49, 0x02, 0xAC, 0x00, 0x54, 0x01, 0x7A, 0x03, 0x4C, 0x03, 0x75, 0x02,
            0xD0, 0x03, 0xF7, 0x03, 0xED, 0x03, 0xF3, 0x02, 0x0A, 0x02, 0x2E, 0x01,
            0xDF, 0x02, 0x24, 0x01, 0x8F, 0x02, 0xF8, 0x01, 0xB3, 0x00, 0x3C, 0x02,
            0x89, 0x01, 0x19, 0x03, 0x8E, 0x01, 0x9A, 0x02, 0x82, 0x00, 0x94, 0x00,
            0x58, 0x01, 0xAD, 0x03, 0x9E, 0x02, 0x98, 0x02, 0xD6, 0x02, 0x9F, 0x02,
            0xA5, 0x02, 0xE2, 0x02, 0xFD, 0x03, 0xAF, 0x01, 0x3F, 0x02, 0x81, 0x01,
            0xEF, 0x03, 0x0F, 0x00, 0xC4, 0x03, 0xCF, 0x01, 0xF3, 0x00, 0x2F, 0x02,
            0xFA, 0x01, 0x81, 0x02, 0xD4, 0x02, 0x55, 0x02, 0x28, 0x03, 0xBA, 0x01,
            0x03, 0x00, 0x57, 0x03, 0xDD, 0x02, 0xF2, 0x03, 0xD4, 0x01, 0x69, 0x01,
            0xC1, 0x03, 0x93, 0x02, 0x42, 0x02, 0xCB, 0x02, 0xE4, 0x00, 0x3D, 0x01,
            0x97, 0x03, 0x49, 0x00, 0xD4, 0x03, 0x73, 0x02, 0x53, 0x00, 0x63, 0x03,
            0xE8, 0x02, 0xB2, 0x02, 0xB3, 0x03, 0xF9, 0x01, 0x24, 0x01, 0xB9, 0x02,
            0x3D, 0x01, 0x6B, 0x01, 0x05, 0x03, 0xE8, 0x03, 0xAF, 0x03, 0xFA, 0x00,
            0xA4, 0x01, 0xA7, 0x03, 0xAD, 0x01, 0x5A, 0x01, 0x8B, 0x03, 0x95, 0x00,
            0x94, 0x00, 0x4A, 0x01, 0x9B, 0x00, 0x7F, 0x02, 0xCC, 0x03, 0x13, 0x01,
            0x66, 0x00, 0xEB, 0x03, 0xFB, 0x00, 0xDD, 0x00, 0x57, 0x00, 0x1C, 0x02,
            0x82, 0x03, 0x9E, 0x03, 0x0F, 0x01, 0x75, 0x02, 0x93, 0x03, 0x9F, 0x02,
            0x55, 0x00, 0x10, 0x02, 0x4A, 0x03, 0x64, 0x03, 0xF4, 0x02, 0x74, 0x02,
            0x30, 0x02, 0xE5, 0x03, 0xEE, 0x03, 0x41, 0x00, 0x77, 0x01, 0xEB, 0x02,
            0x63, 0x00, 0xB9, 0x02, 0x5A, 0x01, 0x76, 0x00, 0x84, 0x01, 0x02, 0x01,
            0x04, 0x02, 0x14, 0x00, 0xFC, 0x03, 0x01, 0x00, 0x54, 0x00, 0xFD, 0x00,
            0x2B, 0x02, 0xB0, 0x01, 0xE1, 0x00, 0xD6, 0x01, 0x95, 0x00, 0xD1, 0x00,
            0xA9, 0x01, 0x09, 0x00, 0xDB, 0x01, 0xD2, 0x01, 0xBA, 0x00, 0x0A, 0x00,
            0x07, 0x03, 0x1B, 0x07, 0x07, 0x03, 0x1B, 0x03
        };

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