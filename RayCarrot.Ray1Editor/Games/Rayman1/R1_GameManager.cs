using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer;

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
            data.Rayman = ObjData.GetRayman(data.Context, data.Objects.OfType<R1_GameObject>().FirstOrDefault(x => x.ObjData.Type == ObjType.TYPE_RAY_POS)?.ObjData);
        }

        public override void PostLoad(GameData gameData)
        {
            var data = (R1_GameData)gameData;

            // Hard-code event animations for the different Rayman types
            Animation[] rayAnim = null;

            var ray = data.Rayman ?? data.Objects.OfType<R1_GameObject>().FirstOrDefault(x => x.ObjData.Type == ObjType.TYPE_RAY_POS)?.ObjData;

            if (ray != null)
                rayAnim = ray.Animations;

            if (rayAnim != null)
            {
                var miniRay = data.Objects.OfType<R1_GameObject>().FirstOrDefault(x => x.ObjData.Type == ObjType.TYPE_DEMI_RAYMAN);

                // TODO: If not found in map use template
                if (miniRay != null)
                {
                    data.Animations.Remove(miniRay.ObjData.Animations);
                    miniRay.ObjData.Animations = rayAnim.Select(x => new Animation
                    {
                        LayersPerFrameSerialized = x.LayersPerFrameSerialized,
                        FrameCountSerialized = x.FrameCountSerialized,
                        Layers = x.Layers.Select(l => new AnimationLayer
                        {
                            IsFlippedHorizontally = l.IsFlippedHorizontally,
                            IsFlippedVertically = l.IsFlippedVertically,
                            XPosition = (byte)(l.XPosition / 2),
                            YPosition = (byte)(l.YPosition / 2),
                            SpriteIndex = l.SpriteIndex
                        }).ToArray(),
                    }).ToArray();
                    data.Animations[miniRay.ObjData.Animations] = miniRay.ObjData.Animations.Select(x => ToCommonAnimation(x)).ToArray();
                }

                var badRay = data.Objects.OfType<R1_GameObject>().FirstOrDefault(x => x.ObjData.Type == ObjType.TYPE_BLACK_RAY);

                // TODO: If not found in map use template
                if (badRay != null)
                {
                    data.Animations.Remove(badRay.ObjData.Animations);
                    badRay.ObjData.Animations = rayAnim;
                }
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

                    // TODO: Uncomment? Game does this on load.
                    //e.ObjData.XPosition = (short)(baseEvent.ObjData.XPosition + 32 * index * (baseEvent.ObjData.HitPoints - 2));
                    //e.ObjData.YPosition = baseEvent.ObjData.YPosition;

                    linkedIndex = data.LinkTable[linkedIndex];
                } while (i != linkedIndex);
            }
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

        public override IEnumerable<string> GetAvailableObjects(GameData gameData)
        {
            return ((R1_GameData)gameData).EventDefinitions.Select(x => x.Name);
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
            obj.ObjData.Animations = des.AnimationsData;
            obj.ObjData.ImageBuffer = des.ImageBuffer;
            obj.ObjData.Sprites = des.SpritesData;
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

        #endregion

        #region Data

        // TODO: Move to BinarySerializer.Ray1
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
    }
}