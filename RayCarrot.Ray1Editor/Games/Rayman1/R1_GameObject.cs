using BinarySerializer;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace RayCarrot.Ray1Editor
{
    public class R1_GameObject : GameObject
    {
        public R1_GameObject(ObjData objData, R1_EventDefinition def = null)
        {
            ObjData = objData;

            var cmdLines = objData.Commands?.ToTranslatedStrings(objData.LabelOffsets, 1);

            if (cmdLines != null) 
                Scripts = String.Join(Environment.NewLine, cmdLines);

            SecondaryName = def?.Name;
        }

        // Data
        public ObjData ObjData { get; }
        public override BinarySerializable SerializableData => ObjData;
        public new R1_GameData Data => (R1_GameData)base.Data;
        public override void Load()
        {
            var settings = Data.Context.GetSettings<Ray1Settings>();

            ObjData.InitialEtat = ObjData.Etat;
            ObjData.InitialSubEtat = ObjData.SubEtat;
            ObjData.DisplayPrio = R1_ObjHelpers.GetDisplayPrio(ObjData.Type, ObjData.HitPoints, settings.World, settings.Level);
            ObjData.InitialXPosition = (short)ObjData.XPosition;
            ObjData.InitialYPosition = (short)ObjData.YPosition;
            ObjData.CurrentAnimationIndex = 0;
            ObjData.InitialHitPoints = ObjData.HitPoints;

            // Set random frame
            if (ObjData.Type.UsesRandomFrame())
                ForceFrame = (byte)Data.GetNextRandom(CurrentAnimation?.Frames.Length ?? 1);
        }
        public override void Save()
        {
            ObjData.Etat = ObjData.InitialEtat;
            ObjData.SubEtat = ObjData.InitialSubEtat;
            //ObjData.DisplayPrio = ObjData.InitialDisplayPrio;

            // TODO: Set other runtime values like hp etc.?
        }

        // Layout
        public override GameObjType Type => GameObjType.Object;
        public override Point Position
        {
            get => new Point(ObjData.XPosition, ObjData.YPosition);
            set
            {
                ObjData.XPosition = value.X;
                ObjData.YPosition = value.Y;
            }
        }
        public override bool FlipHorizontally
        {
            get
            {
                var settings = Data.Context.GetSettings<Ray1Settings>();

                if (ObjData.IsPCFormat(settings))
                {
                    if (ObjData.PC_Flags.HasFlag(ObjData.PC_ObjFlags.IsFlipped))
                        return true;
                }
                else
                {
                    if (settings.EngineVersion == Ray1EngineVersion.PS1_JPDemoVol3)
                    {
                        if (ObjData.PS1Demo_IsFlipped && EditorState.LoadFromMemory)
                            return true;
                    }
                    else
                    {
                        if (ObjData.PS1_RuntimeFlags.HasFlag(ObjData.PS1_ObjFlags.IsFlipped))
                            return true;
                    }
                }

                // If loading from memory, check only runtime flags
                if (EditorState.LoadFromMemory)
                    return false;

                // Check if it's the pin event and if the hp flag is set
                if (ObjData.Type == ObjType.TYPE_PUNAISE3 && ObjData.HitPoints == 1)
                    return true;

                // If the first command changes its direction to right, flip the event (a bit hacky, but works for trumpets etc.)
                if (ObjData.Commands?.Commands?.FirstOrDefault()?.CommandType == CommandType.GO_RIGHT)
                    return true;

                return false;
            }
        }
        public override int DisplayPrio => ObjData.DisplayPrio;

        // Links
        public override bool CanBeLinkedToGroup => true;

        // Info
        public override string PrimaryName => (ushort)ObjData.Type < 262 ? $"{ObjData.Type.ToString().Replace("TYPE_", "")}" : $"TYPE_{(ushort)ObjData.Type}";
        public override string SecondaryName { get; }
        public override string Scripts { get; }

        // Animations
        public override ObjAnimation_HitBoxLayer[] HitBoxLayers => null; // TODO: Implement from ZDC
        public override ObjAnimation CurrentAnimation => Data.GetAnimations(ObjData.Animations)?.ElementAtOrDefault(CurrentState?.AnimationIndex ?? -1);
        public override int AnimationFrame
        {
            get => ObjData.CurrentAnimationFrame;
            set => ObjData.CurrentAnimationFrame = (byte)value;
        }
        public byte ForceFrame { get; set; }
        public override int AnimSpeed => (ObjData.Type.IsHPFrame() ? 0 : CurrentState?.AnimationSpeed ?? 0);
        public override TextureSheet SpriteSheet => Data.GetSprites(ObjData.Sprites);
        public override Point Pivot => new Point(ObjData.OffsetBX, ObjData.OffsetBY);
        protected override bool ShouldUpdateFrame()
        {
            // Set frame based on hit points for special events
            if (ObjData.Type.IsHPFrame())
            {
                ObjData.CurrentAnimationFrame = ObjData.HitPoints;
                AnimationFrameFloat = ObjData.HitPoints;
                return false;
            }
            else if (ObjData.Type.UsesEditorFrame())
            {
                AnimationFrameFloat = ObjData.CurrentAnimationFrame;
                return false;
            }
            else if (ObjData.Type.UsesRandomFrame() || ObjData.Type.UsesFrameFromLinkChain())
            {
                ObjData.CurrentAnimationFrame = ForceFrame;
                AnimationFrameFloat = ForceFrame;
                return false;
            }
            else
            {
                return true;
            }
        }
        protected override void OnFinishedAnimation()
        {
            // TODO: Implement state link looping
            //if (EditorState.LoadFromMemory)
            //    return;

            //// Check if the state has been modified
            //if (PrevInitialState != InitialState)
            //{
            //    PrevInitialState = InitialState;

            //    // Clear encountered states
            //    EncounteredStates.Clear();
            //}

            //if (Settings.StateSwitchingMode != StateSwitchingMode.None)
            //{
            //    // Get the current state
            //    var state = CurrentState;

            //    // Add current state to list of encountered states
            //    EncounteredStates.Add(state);

            //    // Check if we've reached the end of the linking chain and we're looping
            //    if (Settings.StateSwitchingMode == StateSwitchingMode.Loop && EncounteredStates.Contains(LinkedState))
            //    {
            //        // Reset the state
            //        EventData.Etat = EventData.InitialEtat;
            //        EventData.SubEtat = EventData.InitialSubEtat;

            //        // Clear encountered states
            //        EncounteredStates.Clear();
            //    }
            //    else
            //    {
            //        // Update state values to the linked one
            //        EventData.Etat = state.LinkedEtat;
            //        EventData.SubEtat = state.LinkedSubEtat;
            //    }
            //}
            //else
            //{
            //    EventData.Etat = EventData.InitialEtat;
            //    EventData.SubEtat = EventData.InitialSubEtat;
            //}
        }
        public override void ResetFrame()
        {
            if (EditorState.LoadFromMemory || ObjData.Type.UsesEditorFrame())
                return;

            AnimationFrame = 0;
            AnimationFrameFloat = 0;
        }

        // State
        public ObjState CurrentState => GetState(ObjData.Etat, ObjData.SubEtat);
        public ObjState InitialState => GetState(ObjData.InitialEtat, ObjData.InitialSubEtat);
        public ObjState LinkedState => GetState(CurrentState?.LinkedEtat ?? -1, CurrentState?.LinkedSubEtat ?? -1);
        protected ObjState GetState(int etat, int subEtat) => ObjData.ETA.States.ElementAtOrDefault(etat)?.ElementAtOrDefault(subEtat);

        // Update
        public override void DrawOffsets(Renderer r)
        {
            base.DrawOffsets(r);

            int hy = ObjData.OffsetHY;

            if (ObjData.GetFollowEnabled(Data.Context.GetSettings<Ray1Settings>()))
                hy += CurrentAnimation?.Frames.ElementAtOrDefault(AnimationFrame)?.SpriteLayers.ElementAtOrDefault(ObjData.FollowSprite)?.Position.Y ?? 0;

            if (hy != 0)
                DrawOffset(r, Position + new Point(0, hy), EditorState.Color_ObjOffsetGeneric);
        }
    }
}