using BinarySerializer;
using BinarySerializer.Ray1;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Ray1Editor.Rayman1;

public class R1_GameObject : GameObject
{
    public R1_GameObject(ObjData objData, Ray1Settings settings, R1_EventDefinition def = null)
    {
        ObjData = objData;
        EventDefinition = def;

        UpdateScripts();

        SecondaryName = def?.Name;
            
        // TODO: Use object flags to determine if an object is always?
        var typeInfo = ObjData.Type.GetAttribute<ObjTypeInfoAttribute>();
        if (def?.CodeNames?.FirstOrDefault() == "always" || 
            typeInfo?.Flag == ObjTypeFlag.Always && !(settings.EngineVersion == Ray1EngineVersion.PS1_JPDemoVol3 && ObjData.Type == ObjType.TYPE_DARK2_PINK_FLY))
            Tags = "always";
    }

    // Data
    public ObjData ObjData { get; }
    public R1_EventDefinition EventDefinition { get; }
    public override BinarySerializable SerializableData => ObjData;
    public new R1_GameData Data => (R1_GameData)base.Data;
    public override void Load()
    {
        var settings = Data.Context.GetSettings<Ray1Settings>();

        var isEDUorKIT = settings.EngineVersion == Ray1EngineVersion.PC_Edu ||
                         settings.EngineVersion == Ray1EngineVersion.PS1_Edu ||
                         settings.EngineVersion == Ray1EngineVersion.PC_Kit ||
                         settings.EngineVersion == Ray1EngineVersion.PC_Fan;

        // Do like the game and keep track of the initial values
        ObjData.InitialEtat = ObjData.Etat;
        ObjData.InitialSubEtat = ObjData.SubEtat;
        ObjData.DisplayPrio = !isEDUorKIT 
            ? R1_ObjHelpers.GetDisplayPrio(ObjData.Type, ObjData.HitPoints, settings.World, settings.Level) 
            : R1_ObjHelpers.GetDisplayPrio_Kit(ObjData.Type, ObjData.HitPoints, true);
        ObjData.InitialXPosition = (short)ObjData.XPosition;
        ObjData.InitialYPosition = (short)ObjData.YPosition;
        ObjData.CurrentAnimationIndex = 0;
        //ObjData.InitialHitPoints = ObjData.HitPoints; // NOTE: Can't do this with HP due to later versions storing it as a uint

        // Set random frame
        if (ObjData.Type.UsesRandomFrame())
            ForceFrame = (byte)Data.GetNextRandom(CurrentAnimation?.Frames.Length ?? 1);
    }
    public override void Save()
    {
        // Restore the initial values. This is to make sure any temporary version of the values won't be saved,
        // such as when loading from memory. This also allows us to for example "play" object commands and such
        // in the editor without effecting their initial states.
        ObjData.Etat = ObjData.InitialEtat;
        ObjData.SubEtat = ObjData.InitialSubEtat;
        ObjData.XPosition = ObjData.InitialXPosition;
        ObjData.YPosition = ObjData.InitialYPosition;
        //ObjData.ActualHitPoints = ObjData.InitialHitPoints; // NOTE: Can't do this with HP due to later versions storing it as a uint
    }

    // Layout
    public override GameObjType Type => GameObjType.Object;
    public override Point Position
    {
        get => new Point(ObjData.XPosition, ObjData.YPosition);
        set
        {
            ObjData.XPosition = ObjData.InitialXPosition = (short)value.X;
            ObjData.YPosition = ObjData.InitialYPosition = (short)value.Y;
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
                    if (ObjData.PS1Demo_IsFlipped && ViewModel.LoadFromMemory)
                        return true;
                }
                else
                {
                    if (ObjData.PS1_RuntimeFlags.HasFlag(ObjData.PS1_ObjFlags.IsFlipped))
                        return true;
                }
            }

            // If loading from memory, check only runtime flags
            if (ViewModel.LoadFromMemory)
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
    public override string Tags { get; }

    // Scripts
    private string _scripts;
    public override string Scripts => _scripts;
    public override bool CanEditScripts => true;
    private void UpdateScripts()
    {
        var cmdLines = ObjData.Commands?.ToTranslatedStrings(ObjData.LabelOffsets, 1);

        if (cmdLines != null)
            _scripts = String.Join(Environment.NewLine, cmdLines);
    }
    public override void EditScripts()
    {
        var vm = new R1_EditCmdsViewModel(ObjData.Commands, ObjData.LabelOffsets);
        var win = new R1_EditCmdsWindow(vm);
        win.ShowDialog();

        if (win.DialogResult != true)
            return;

        CommandCollection cmds = vm.GetCommands();

        if (cmds.Commands.Length == 0)
        {
            R1EServices.UI.DisplayMessage("At least one command has to be specified",
                "Error saving commands", DialogMessageType.Error);
            return;
        }

        // Validate the commands
        for (int i = 0; i < cmds.Commands.Length; i++)
        {
            CommandType type = cmds.Commands[i].CommandType;
            byte[] args = cmds.Commands[i].Arguments;

            // Correct the number of arguments
            int argsCount = Command.GetArgumentsCount(Data.Context.GetSettings<Ray1Settings>().EngineVersion, type, args);

            if (args.Length != argsCount)
            {
                Array.Resize(ref args, argsCount);
                cmds.Commands[i].Arguments = args;
            }

            // Validate the commands are correctly terminated
            bool isLast = i == cmds.Commands.Length - 1;
            bool isInvalidCmd = type is CommandType.INVALID_CMD or CommandType.INVALID_CMD_DEMO;

            if (!isLast && isInvalidCmd)
            {
                R1EServices.UI.DisplayMessage("The commands can't be terminated (33, 255) before the end", 
                    "Error saving commands", DialogMessageType.Error);
                return;
            }
            else if (isLast && !isInvalidCmd)
            {
                R1EServices.UI.DisplayMessage("The commands have to end with the command terminator (33, 255)", 
                    "Error saving commands", DialogMessageType.Error);
                return;
            }
            else if (isInvalidCmd && args[0] != 0xFF)
            {
                R1EServices.UI.DisplayMessage("The command terminator (33) has to have 255 as its argument", 
                    "Error saving commands", DialogMessageType.Error);
                return;
            }
        }

        ObjData.Commands = cmds;
        ObjData.LabelOffsets = vm.GetLabelOffsets();

        UpdateScripts();
    }

    // Animations
    public override ObjAnimation_HitBoxLayer[] HitBoxLayers => null; // TODO: Implement from ZDC
    public override ObjAnimation CurrentAnimation => Data.GetAnimations(ObjData.AnimationCollection)?.ElementAtOrDefault(CurrentState?.AnimationIndex ?? -1);
    public override int AnimationFrame
    {
        get => ObjData.CurrentAnimationFrame;
        set => ObjData.CurrentAnimationFrame = (byte)value;
    }
    public byte ForceFrame { get; set; }
    public override int AnimSpeed => (ObjData.Type.IsHPFrame() ? 0 : CurrentState?.AnimationSpeed ?? 0);
    public override TextureSheet SpriteSheet => Data.GetSprites(ObjData.SpriteCollection);
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
    protected override bool HalfAnimFramePos => ObjData.Type == ObjType.TYPE_DEMI_RAYMAN;
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
        if (ViewModel.LoadFromMemory || ObjData.Type.UsesEditorFrame())
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
            DrawOffset(r, Position + new Point(0, hy), ViewModel.Colors[EditorColor.ObjOffsetGeneric]);
    }
}