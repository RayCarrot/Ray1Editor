using BinarySerializer.Ray1;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ray1Editor.Rayman1;

public class R1_KIT_PC_GameManager : R1_EDU_PC_GameManager
{
    #region Paths

    public override string Path_VigFile(Ray1Settings settings) => Path_DataDir + "VIGNET.DAT";
    public override string Path_WorldFile(Ray1Settings settings) => Path_DataDir + $"RAY{(int)settings.World:00}.WLD";
    public override string Path_LevelFile(Ray1Settings settings) => Path_DataDir + $"{GetShortWorldName(settings.World)}{settings.Level:00}.LEV";

    #endregion

    #region Manager

    public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path)
    {
        var g = (Games.R1_Game)game;

        // Get the default volume to use
        var vol = GetDefaultVolume(path);

        return new LoadGameLevelViewModel[]
        {
            new LoadGameLevelViewModel("Jungle", null),
            new LoadGameLevelViewModel("The Sky's the Limit", new Ray1Settings(g.EngineVersion, World.Jungle, 1, volume: vol)),
            new LoadGameLevelViewModel("Fruity Fun", new Ray1Settings(g.EngineVersion, World.Jungle, 2, volume: vol)),
            new LoadGameLevelViewModel("Junglemania", new Ray1Settings(g.EngineVersion, World.Jungle, 3, volume: vol)),
            new LoadGameLevelViewModel("Ring a Ling", new Ray1Settings(g.EngineVersion, World.Jungle, 4, volume: vol)),

            new LoadGameLevelViewModel("Music", null),
            new LoadGameLevelViewModel("Gone with the Wind", new Ray1Settings(g.EngineVersion, World.Music, 1, volume: vol)),
            new LoadGameLevelViewModel("Scale the Scales", new Ray1Settings(g.EngineVersion, World.Music, 2, volume: vol)),
            new LoadGameLevelViewModel("Music Lessons", new Ray1Settings(g.EngineVersion, World.Music, 3, volume: vol)),
            new LoadGameLevelViewModel("Melodic Maracas", new Ray1Settings(g.EngineVersion, World.Music, 4, volume: vol)),
                
            new LoadGameLevelViewModel("Mountain", null),
            new LoadGameLevelViewModel("Treetop Adventure", new Ray1Settings(g.EngineVersion, World.Mountain, 1, volume: vol)),
            new LoadGameLevelViewModel("Tough Climb", new Ray1Settings(g.EngineVersion, World.Mountain, 2, volume: vol)),
            new LoadGameLevelViewModel("Tip-Top Tempest", new Ray1Settings(g.EngineVersion, World.Mountain, 3, volume: vol)),
            new LoadGameLevelViewModel("The Diabolical Pursuit", new Ray1Settings(g.EngineVersion, World.Mountain, 4, volume: vol)),
                
            new LoadGameLevelViewModel("Image", null),
            new LoadGameLevelViewModel("The Five Doors", new Ray1Settings(g.EngineVersion, World.Image, 1, volume: vol)),
            new LoadGameLevelViewModel("Pencil Pentathalon", new Ray1Settings(g.EngineVersion, World.Image, 2, volume: vol)),
            new LoadGameLevelViewModel("Eraser Mania", new Ray1Settings(g.EngineVersion, World.Image, 3, volume: vol)),
            new LoadGameLevelViewModel("Tic Tack Toe", new Ray1Settings(g.EngineVersion, World.Image, 4, volume: vol)),
                
            new LoadGameLevelViewModel("Cave", null),
            new LoadGameLevelViewModel("Peaks and Rocks", new Ray1Settings(g.EngineVersion, World.Cave, 1, volume: vol)),
            new LoadGameLevelViewModel("Dark Journey", new Ray1Settings(g.EngineVersion, World.Cave, 2, volume: vol)),
            new LoadGameLevelViewModel("Dreaded Caves", new Ray1Settings(g.EngineVersion, World.Cave, 3, volume: vol)),
            new LoadGameLevelViewModel("Dire Darkness", new Ray1Settings(g.EngineVersion, World.Cave, 4, volume: vol)),
                
            new LoadGameLevelViewModel("Cake", null),
            new LoadGameLevelViewModel("Chocolate Trap", new Ray1Settings(g.EngineVersion, World.Cake, 1, volume: vol)),
            new LoadGameLevelViewModel("Crazy Candy", new Ray1Settings(g.EngineVersion, World.Cake, 2, volume: vol)),
            new LoadGameLevelViewModel("Bonbon-a-rama", new Ray1Settings(g.EngineVersion, World.Cake, 3, volume: vol)),
            new LoadGameLevelViewModel("Whipped Cream Challenge", new Ray1Settings(g.EngineVersion, World.Cake, 4, volume: vol)),
        };
    }

    #endregion

    #region R1 Manager

    protected override int MaxObjType => (int)ObjType.MS_super_kildoor;

    protected override string[] LoadNameTable_DES(R1_GameData data, Games.R1_Game game, Ray1Settings settings)
    {
        return data.Context.GetMainFileObject<PC_WorldFile>(Path_WorldFile(settings)).DESFileNames.Select(x => x.Length > 4 ? x[..^4] : null).ToArray();
    }

    protected override string[] LoadNameTable_ETA(R1_GameData data, Games.R1_Game game, Ray1Settings settings)
    {
        return data.Context.GetMainFileObject<PC_WorldFile>(Path_WorldFile(settings)).ETAFileNames.Select(x => x.Length > 4 ? x[..^4] : null).ToArray();
    }

    public string GetDefaultVolume(string path)
    {
        // Get the volumes
        var volumes = Directory.GetDirectories(Path.Combine(path, Path_DataDir)).Select(Path.GetFileName).ToArray();

        // Get the volume to use, defaulting to USA if available
        return volumes.Contains("USA") ? "USA" : volumes.FirstOrDefault(x => x.Length <= 3);
    }

    #endregion
}