using System.Collections.Generic;
using BinarySerializer;
using BinarySerializer.Ray1;

namespace RayCarrot.Ray1Editor;

public class R1_PC_GameData : R1_GameData
{
    public R1_PC_GameData(Context context, TextureManager textureManager) : base(context, textureManager) { }

    public PC_DES[] PC_DES { get; set; }
    public AnimationCollection[] PC_LoadedAnimations { get; set; }
    public IReadOnlyList<Palette> PC_Palettes { get; set; }
    public MapTile.PC_TransparencyMode[] PC_TileSetTransparencyModes { get; set; }
    public PC_LevelDefines LevelDefines { get; set; }
    public PC_ProfileDefine ProfileDefines { get; set; }
}