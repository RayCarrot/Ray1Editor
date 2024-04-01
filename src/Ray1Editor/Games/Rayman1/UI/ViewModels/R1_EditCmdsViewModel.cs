using System;
using System.Linq;
using BinarySerializer.Ray1;
using NLog;

namespace Ray1Editor.Rayman1;

public class R1_EditCmdsViewModel : BaseViewModel
{
    #region Constructor

    // TODO: Currently this won't work if cmds were null or empty before. Saving also expects there to be some commands.
    //       Currently this is not an issue since commands can't be edited for objects without them.
    public R1_EditCmdsViewModel(CommandCollection cmds, ushort[] labelOffsets)
    {
        Commands = String.Join(Environment.NewLine, 
            cmds.Commands.Select(x => String.Join(", ", new byte[] { (byte)x.CommandType }.Concat(x.Arguments))));
        LabelOffsets = String.Join(", ", labelOffsets ?? Array.Empty<ushort>());
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public string Commands { get; set; }
    public string LabelOffsets { get; set; }

    #endregion

    #region Public Methods

    public CommandCollection GetCommands()
    {
        return new CommandCollection
        {
            Commands = Commands.Split(Environment.NewLine).Select((line, lineIndex) =>
            {
                string[] values = line.Split(',');

                if (!Enum.TryParse(values[0].Trim(), out CommandType cmdType))
                {
                    Logger.Warn("Failed to parse command on line {0} due to the command type not being correctly formatted", lineIndex);
                    return null;
                }

                if (!Enum.IsDefined(typeof(CommandType), cmdType))
                {
                    Logger.Warn("Failed to parse command on line {0} due to the command type {1} not being defined", lineIndex, cmdType);
                    return null;
                }

                return new Command
                {
                    CommandType = cmdType,
                    Arguments = values.Skip(1).Select(arg => Byte.TryParse(arg.Trim(), out byte a) ? a : (byte)0).ToArray(),
                };
            }).Where(x => x != null).ToArray()
        };
    }

    public ushort[] GetLabelOffsets()
    {
        if (String.IsNullOrWhiteSpace(LabelOffsets))
            return Array.Empty<ushort>();

        return LabelOffsets.
            Split(',').
            Select(x => x.Trim()).
            Select(x => UInt16.TryParse(x, out ushort l) ? l : (ushort)0).
            ToArray();
    }

    #endregion
}