using System.Collections.Generic;
using BinarySerializer.Ray1;

namespace RayCarrot.Ray1Editor
{
    public class R1_KIT_PC_Demo_GameManager : R1_KIT_PC_GameManager
    {
        public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path)
        {
            var g = (Games.R1_Game)game;

            // Get the default volume to use
            var vol = GetDefaultVolume(path);

            return new LoadGameLevelViewModel[]
            {       
                new LoadGameLevelViewModel("Music", null),
                new LoadGameLevelViewModel("Gone with the Wind", new Ray1Settings(g.EngineVersion, World.Music, 1, volume: vol)),

                new LoadGameLevelViewModel("Image", null),
                new LoadGameLevelViewModel("Pencil Pentathalon", new Ray1Settings(g.EngineVersion, World.Image, 1, volume: vol)),

                new LoadGameLevelViewModel("Cave", null),
                new LoadGameLevelViewModel("Peaks and Rocks", new Ray1Settings(g.EngineVersion, World.Cave, 1, volume: vol)),

                new LoadGameLevelViewModel("Cake", null),
                new LoadGameLevelViewModel("Chocolate Trap", new Ray1Settings(g.EngineVersion, World.Cake, 1, volume: vol)),
            };
        }
    }
}