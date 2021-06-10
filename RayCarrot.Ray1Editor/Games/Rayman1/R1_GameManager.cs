using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
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
        public const string AssetPath_CollisionFile = "Assets/Collision/R1_TypeCollision.png";
        public const string AssetPath_EventsDir = "Assets/Rayman1/Events/";
        public const string AssetPath_EventsFile = AssetPath_EventsDir + "Events.csv";

        protected abstract int MaxObjType { get; }
        protected virtual bool UsesLocalCommands => false;

        public IEnumerable<LoadGameLevelViewModel> GetLevels(Ray1EngineVersion engineVersion, Ray1PCVersion pcVersion = Ray1PCVersion.None, string volume = null)
        {
            // TODO: Breakout is only in PC version. GBA version has extra multiplayer maps. Saturn version has unused maps (include those?).

            return new LoadGameLevelViewModel[]
            {
                new LoadGameLevelViewModel("Jungle", null),
                new LoadGameLevelViewModel("Pink Plant Woods 1", new Ray1Settings(engineVersion, World.Jungle, 1, pcVersion, volume)),
                new LoadGameLevelViewModel("Pink Plant Woods 2", new Ray1Settings(engineVersion, World.Jungle, 2, pcVersion, volume)),
                new LoadGameLevelViewModel("Pink Plant Woods 3 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 3, pcVersion, volume)),
                new LoadGameLevelViewModel("Pink Plant Woods 4", new Ray1Settings(engineVersion, World.Jungle, 4, pcVersion, volume)),
                new LoadGameLevelViewModel("Anguish Lagoon 1", new Ray1Settings(engineVersion, World.Jungle, 5, pcVersion, volume)),
                new LoadGameLevelViewModel("Anguish Lagoon 2 - Boss", new Ray1Settings(engineVersion, World.Jungle, 6, pcVersion, volume)),
                new LoadGameLevelViewModel("Anguish Lagoon 3 - Bzzit Flight", new Ray1Settings(engineVersion, World.Jungle, 7, pcVersion, volume)),
                new LoadGameLevelViewModel("Anguish Lagoon 4 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 8, pcVersion, volume)),
                new LoadGameLevelViewModel("The Swamps of Forgetfulness 1", new Ray1Settings(engineVersion, World.Jungle, 9, pcVersion, volume)),
                new LoadGameLevelViewModel("The Swamps of Forgetfulness 2", new Ray1Settings(engineVersion, World.Jungle, 10, pcVersion, volume)),
                new LoadGameLevelViewModel("The Swamps of Forgetfulness 3", new Ray1Settings(engineVersion, World.Jungle, 11, pcVersion, volume)),
                new LoadGameLevelViewModel("Moskito's Nest 1", new Ray1Settings(engineVersion, World.Jungle, 12, pcVersion, volume)),
                new LoadGameLevelViewModel("Moskito's Nest 2", new Ray1Settings(engineVersion, World.Jungle, 13, pcVersion, volume)),
                new LoadGameLevelViewModel("Moskito's Nest 3 - Boss Chase", new Ray1Settings(engineVersion, World.Jungle, 14, pcVersion, volume)),
                new LoadGameLevelViewModel("Moskito's Nest 4", new Ray1Settings(engineVersion, World.Jungle, 15, pcVersion, volume)),
                new LoadGameLevelViewModel("Moskito's Nest 5 - Boss", new Ray1Settings(engineVersion, World.Jungle, 16, pcVersion, volume)),
                new LoadGameLevelViewModel("Moskito's Nest 6 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 17, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Jungle 1", new Ray1Settings(engineVersion, World.Jungle, 18, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Jungle 2", new Ray1Settings(engineVersion, World.Jungle, 19, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Jungle 3", new Ray1Settings(engineVersion, World.Jungle, 20, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Jungle 4", new Ray1Settings(engineVersion, World.Jungle, 21, pcVersion, volume)),
                new LoadGameLevelViewModel("(Minigame) Ray Breakout", new Ray1Settings(engineVersion, World.Jungle, 22, pcVersion, volume)),

                new LoadGameLevelViewModel("Music", null),
                new LoadGameLevelViewModel("Bongo Hills 1", new Ray1Settings(engineVersion, World.Music, 1, pcVersion, volume)),
                new LoadGameLevelViewModel("Bongo Hills 2", new Ray1Settings(engineVersion, World.Music, 2, pcVersion, volume)),
                new LoadGameLevelViewModel("Bongo Hills 3", new Ray1Settings(engineVersion, World.Music, 3, pcVersion, volume)),
                new LoadGameLevelViewModel("Bongo Hills 4", new Ray1Settings(engineVersion, World.Music, 4, pcVersion, volume)),
                new LoadGameLevelViewModel("Bongo Hills 5", new Ray1Settings(engineVersion, World.Music, 5, pcVersion, volume)),
                new LoadGameLevelViewModel("Bongo Hills 6", new Ray1Settings(engineVersion, World.Music, 6, pcVersion, volume)),
                new LoadGameLevelViewModel("Allegro Presto 1", new Ray1Settings(engineVersion, World.Music, 7, pcVersion, volume)),
                new LoadGameLevelViewModel("Allegro Presto 2", new Ray1Settings(engineVersion, World.Music, 8, pcVersion, volume)),
                new LoadGameLevelViewModel("Allegro Presto 3", new Ray1Settings(engineVersion, World.Music, 9, pcVersion, volume)),
                new LoadGameLevelViewModel("Allegro Presto 4", new Ray1Settings(engineVersion, World.Music, 10, pcVersion, volume)),
                new LoadGameLevelViewModel("Allegro Presto 5 - Betilla", new Ray1Settings(engineVersion, World.Music, 11, pcVersion, volume)),
                new LoadGameLevelViewModel("Gong Heights 1", new Ray1Settings(engineVersion, World.Music, 12, pcVersion, volume)),
                new LoadGameLevelViewModel("Gong Heights 2", new Ray1Settings(engineVersion, World.Music, 13, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Sax's Hullaballoo 1", new Ray1Settings(engineVersion, World.Music, 14, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Sax's Hullaballoo 2 - Boss Chase", new Ray1Settings(engineVersion, World.Music, 15, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Sax's Hullaballoo 3 - Boss", new Ray1Settings(engineVersion, World.Music, 16, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Music 1", new Ray1Settings(engineVersion, World.Music, 17, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Music 2", new Ray1Settings(engineVersion, World.Music, 18, pcVersion, volume)),

                new LoadGameLevelViewModel("Mountain", null),
                new LoadGameLevelViewModel("Twilight Gulch 1", new Ray1Settings(engineVersion, World.Mountain, 1, pcVersion, volume)),
                new LoadGameLevelViewModel("Twilight Gulch 2", new Ray1Settings(engineVersion, World.Mountain, 2, pcVersion, volume)),
                new LoadGameLevelViewModel("The Hard Rocks 1", new Ray1Settings(engineVersion, World.Mountain, 3, pcVersion, volume)),
                new LoadGameLevelViewModel("The Hard Rocks 2", new Ray1Settings(engineVersion, World.Mountain, 4, pcVersion, volume)),
                new LoadGameLevelViewModel("The Hard Rocks 3", new Ray1Settings(engineVersion, World.Mountain, 5, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Stone's Peaks 1", new Ray1Settings(engineVersion, World.Mountain, 6, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Stone's Peaks 2", new Ray1Settings(engineVersion, World.Mountain, 7, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Stone's Peaks 3", new Ray1Settings(engineVersion, World.Mountain, 8, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Stone's Peaks 4", new Ray1Settings(engineVersion, World.Mountain, 9, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Stone's Peaks 5 - Boss", new Ray1Settings(engineVersion, World.Mountain, 10, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Stone's Peaks 6 - Betilla", new Ray1Settings(engineVersion, World.Mountain, 11, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Mountain 1", new Ray1Settings(engineVersion, World.Mountain, 12, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Mountain 2", new Ray1Settings(engineVersion, World.Mountain, 13, pcVersion, volume)),

                new LoadGameLevelViewModel("Image", null),
                new LoadGameLevelViewModel("Eraser Plains 1", new Ray1Settings(engineVersion, World.Image, 1, pcVersion, volume)),
                new LoadGameLevelViewModel("Eraser Plains 2", new Ray1Settings(engineVersion, World.Image, 2, pcVersion, volume)),
                new LoadGameLevelViewModel("Eraser Plains 3", new Ray1Settings(engineVersion, World.Image, 3, pcVersion, volume)),
                new LoadGameLevelViewModel("Eraser Plains 4 - Boss", new Ray1Settings(engineVersion, World.Image, 4, pcVersion, volume)),
                new LoadGameLevelViewModel("Pencil Pentathlon 1", new Ray1Settings(engineVersion, World.Image, 5, pcVersion, volume)),
                new LoadGameLevelViewModel("Pencil Pentathlon 2", new Ray1Settings(engineVersion, World.Image, 6, pcVersion, volume)),
                new LoadGameLevelViewModel("Pencil Pentathlon 3", new Ray1Settings(engineVersion, World.Image, 7, pcVersion, volume)),
                new LoadGameLevelViewModel("Space Mama's Crater 1", new Ray1Settings(engineVersion, World.Image, 8, pcVersion, volume)),
                new LoadGameLevelViewModel("Space Mama's Crater 2", new Ray1Settings(engineVersion, World.Image, 9, pcVersion, volume)),
                new LoadGameLevelViewModel("Space Mama's Crater 3", new Ray1Settings(engineVersion, World.Image, 10, pcVersion, volume)),
                new LoadGameLevelViewModel("Space Mama's Crater 4 - Boss", new Ray1Settings(engineVersion, World.Image, 11, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Image 1", new Ray1Settings(engineVersion, World.Image, 12, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Image 2", new Ray1Settings(engineVersion, World.Image, 13, pcVersion, volume)),

                new LoadGameLevelViewModel("Cave", null),
                new LoadGameLevelViewModel("Crystal Palace 1", new Ray1Settings(engineVersion, World.Cave, 1, pcVersion, volume)),
                new LoadGameLevelViewModel("Crystal Palace 2", new Ray1Settings(engineVersion, World.Cave, 2, pcVersion, volume)),
                new LoadGameLevelViewModel("Eat at Joe's 1", new Ray1Settings(engineVersion, World.Cave, 3, pcVersion, volume)),
                new LoadGameLevelViewModel("Eat at Joe's 2", new Ray1Settings(engineVersion, World.Cave, 4, pcVersion, volume)),
                new LoadGameLevelViewModel("Eat at Joe's 3", new Ray1Settings(engineVersion, World.Cave, 5, pcVersion, volume)),
                new LoadGameLevelViewModel("Eat at Joe's 4", new Ray1Settings(engineVersion, World.Cave, 6, pcVersion, volume)),
                new LoadGameLevelViewModel("Eat at Joe's 5", new Ray1Settings(engineVersion, World.Cave, 7, pcVersion, volume)),
                new LoadGameLevelViewModel("Eat at Joe's 6", new Ray1Settings(engineVersion, World.Cave, 8, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Skops' Stalactites 1", new Ray1Settings(engineVersion, World.Cave, 9, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Skops' Stalactites 2 - Boss", new Ray1Settings(engineVersion, World.Cave, 10, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Skops' Stalactites 3 - Boss", new Ray1Settings(engineVersion, World.Cave, 11, pcVersion, volume)),
                new LoadGameLevelViewModel("(Bonus) Cave 1", new Ray1Settings(engineVersion, World.Cave, 12, pcVersion, volume)),

                new LoadGameLevelViewModel("Cake", null),
                new LoadGameLevelViewModel("Mr Dark's Dare 1", new Ray1Settings(engineVersion, World.Cake, 1, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Dark's Dare 2", new Ray1Settings(engineVersion, World.Cake, 2, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Dark's Dare 3", new Ray1Settings(engineVersion, World.Cake, 3, pcVersion, volume)),
                new LoadGameLevelViewModel("Mr Dark's Dare 4 - Boss", new Ray1Settings(engineVersion, World.Cake, 4, pcVersion, volume)),
            };
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
                    var c = new Context(data.Context.BasePath);
                    c.AddSettings(data.Context.GetSettings<Ray1Settings>());
                    return c;
                }) ?? new byte[0];
                labelOffsets = compiledData?.LabelOffsets ?? new ushort[0];
            }
            else
            {
                compiledCmds = e.Commands?.ToBytes(() =>
                {
                    var c = new Context(data.Context.BasePath);
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
            {
                // TODO: Logging
                //Debug.LogWarning($"Matching event not found for event with type {e.Type}, etat {e.Etat} & subetat {e.SubEtat} in level {Settings.World}-{Settings.Level}");
            }

            // Return the item
            return match;
        }

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

        public override IEnumerable<string> GetAvailableObjects(GameData gameData)
        {
            return ((R1_GameData)gameData).EventDefinitions.Select(x => x.Name);
        }

        public override GameObject CreateGameObject(GameData gameData, int index)
        {
            var data = (R1_GameData)gameData;
            var def = data.EventDefinitions[index];

            // Get the commands and label offsets
            CommandCollection cmds = null;
            ushort[] labelOffsets = null;

            // If local (non-compiled) commands are used, attempt to get them from the event info or decompile the compiled ones
            if (UsesLocalCommands)
            {
                cmds = ObjCommandCompiler.Decompile(new ObjCommandCompiler.CompiledObjCommandData(CommandCollection.FromBytes(def.Commands, () =>
                {
                    var c = new Context(data.Context.BasePath);
                    c.AddSettings(data.Context.GetSettings<Ray1Settings>());
                    return c;
                }), def.LabelOffsets), def.Commands);
            }
            else if (def.Commands.Any())
            {
                cmds = CommandCollection.FromBytes(def.Commands, () =>
                {
                    var c = new Context(data.Context.BasePath);
                    c.AddSettings(data.Context.GetSettings<Ray1Settings>());
                    return c;
                });
                labelOffsets = def.LabelOffsets.Any() ? def.LabelOffsets : null;
            }

            var des = data.DES.First(x => x.Name == def.DES);
            var eta = data.ETA.First(x => x.Name == def.ETA);

            var obj = new R1_GameObject(new ObjData()
            {
                Type = (ObjType)def.Type,
                Etat = def.Etat,
                SubEtat = def.SubEtat,
                OffsetBX = def.OffsetBX,
                OffsetBY = def.OffsetBY,
                OffsetHY = def.OffsetHY,
                FollowSprite = def.FollowSprite,
                DisplayPrio = 0,
                HitSprite = def.HitSprite,
                Commands = cmds,
                LabelOffsets = labelOffsets,
                Animations = des.AnimationsData,
                ImageBuffer = des.ImageBuffer,
                Sprites = des.SpritesData,
                ETA = eta.ETA,

            }, def);

            obj.ObjData.SetFollowEnabled(data.Context.GetSettings<Ray1Settings>(), def.FollowEnabled);

            // We need to set the hit points after the type
            obj.ObjData.ActualHitPoints = def.HitPoints;

            return obj;
        }
    }
}