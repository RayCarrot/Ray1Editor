using System.Collections.Generic;
using BinarySerializer.Ray1;

namespace Ray1Editor.Rayman1;

public class R1_PS1_US_GameManager : R1_PS1_GameManager
{
    #region PS1 Properties

    public override string Path_ExeFile => "SLUS-000.05";
    public override long EXEBaseAddress => 0x80125000 - 0x800;
    public override PS1_ExecutableConfig EXEConfig => PS1_ExecutableConfig.PS1_US;
    public override Dictionary<PS1_DefinedPointer, long> DefinedPointers => PS1_DefinedPointers.PS1_US;
    public override int TileSetWidth => 16;

    #endregion

    #region R1 Manager

    public override void LoadVRAM(R1_PS1_GameData data, PS1_AllfixFile allfix, PS1_WorldFile world, PS1_LevFile lev)
    {
        data.Vram = PS1VramHelpers.PS1_FillVRAM(PS1VramHelpers.VRAMMode.Level, allfix, world, null, lev, null, true);
    }

    #endregion
}