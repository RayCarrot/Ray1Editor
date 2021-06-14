using System.Collections.Generic;
using BinarySerializer.Ray1;

namespace RayCarrot.Ray1Editor
{
    public class R1_60N_PC_GameManager : R1_FAN_PC_GameManager
    {
        public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path)
        {
            var g = (Games.R1_Game)game;

            // Get the default volume to use
            var vol = GetDefaultVolume(path);

            return new LoadGameLevelViewModel[]
            {
                // TODO: Add world map?
                //new LoadGameLevelViewModel("World Map", new Ray1Settings(g.EngineVersion, World.Menu, 0, volume: vol)),
                new LoadGameLevelViewModel("Jungle", null),
                new LoadGameLevelViewModel("JUNGLE 1", new Ray1Settings(g.EngineVersion, World.Jungle, 1, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 2", new Ray1Settings(g.EngineVersion, World.Jungle, 2, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 3", new Ray1Settings(g.EngineVersion, World.Jungle, 3, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 4", new Ray1Settings(g.EngineVersion, World.Jungle, 4, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 5", new Ray1Settings(g.EngineVersion, World.Jungle, 5, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 6", new Ray1Settings(g.EngineVersion, World.Jungle, 6, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 7", new Ray1Settings(g.EngineVersion, World.Jungle, 7, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 8", new Ray1Settings(g.EngineVersion, World.Jungle, 8, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 9", new Ray1Settings(g.EngineVersion, World.Jungle, 9, volume: vol)),
                new LoadGameLevelViewModel("JUNGLE 10", new Ray1Settings(g.EngineVersion, World.Jungle, 10, volume: vol)),

                new LoadGameLevelViewModel("Music", null),
                new LoadGameLevelViewModel("MUSIC 1", new Ray1Settings(g.EngineVersion, World.Music, 1, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 2", new Ray1Settings(g.EngineVersion, World.Music, 2, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 3", new Ray1Settings(g.EngineVersion, World.Music, 3, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 4", new Ray1Settings(g.EngineVersion, World.Music, 4, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 5", new Ray1Settings(g.EngineVersion, World.Music, 5, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 6", new Ray1Settings(g.EngineVersion, World.Music, 6, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 7", new Ray1Settings(g.EngineVersion, World.Music, 7, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 8", new Ray1Settings(g.EngineVersion, World.Music, 8, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 9", new Ray1Settings(g.EngineVersion, World.Music, 9, volume: vol)),
                new LoadGameLevelViewModel("MUSIC 10", new Ray1Settings(g.EngineVersion, World.Music, 10, volume: vol)),

                new LoadGameLevelViewModel("Mountain", null),
                new LoadGameLevelViewModel("MOUNTAIN 1", new Ray1Settings(g.EngineVersion, World.Mountain, 1, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 2", new Ray1Settings(g.EngineVersion, World.Mountain, 2, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 3", new Ray1Settings(g.EngineVersion, World.Mountain, 3, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 4", new Ray1Settings(g.EngineVersion, World.Mountain, 4, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 5", new Ray1Settings(g.EngineVersion, World.Mountain, 5, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 6", new Ray1Settings(g.EngineVersion, World.Mountain, 6, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 7", new Ray1Settings(g.EngineVersion, World.Mountain, 7, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 8", new Ray1Settings(g.EngineVersion, World.Mountain, 8, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 9", new Ray1Settings(g.EngineVersion, World.Mountain, 9, volume: vol)),
                new LoadGameLevelViewModel("MOUNTAIN 10", new Ray1Settings(g.EngineVersion, World.Mountain, 10, volume: vol)),

                new LoadGameLevelViewModel("Image", null),
                new LoadGameLevelViewModel("IMAGE 1", new Ray1Settings(g.EngineVersion, World.Image, 1, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 2", new Ray1Settings(g.EngineVersion, World.Image, 2, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 3", new Ray1Settings(g.EngineVersion, World.Image, 3, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 4", new Ray1Settings(g.EngineVersion, World.Image, 4, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 5", new Ray1Settings(g.EngineVersion, World.Image, 5, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 6", new Ray1Settings(g.EngineVersion, World.Image, 6, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 7", new Ray1Settings(g.EngineVersion, World.Image, 7, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 8", new Ray1Settings(g.EngineVersion, World.Image, 8, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 9", new Ray1Settings(g.EngineVersion, World.Image, 9, volume: vol)),
                new LoadGameLevelViewModel("IMAGE 10", new Ray1Settings(g.EngineVersion, World.Image, 10, volume: vol)),

                new LoadGameLevelViewModel("Cave", null),
                new LoadGameLevelViewModel("CAVE 1", new Ray1Settings(g.EngineVersion, World.Cave, 1, volume: vol)),
                new LoadGameLevelViewModel("CAVE 2", new Ray1Settings(g.EngineVersion, World.Cave, 2, volume: vol)),
                new LoadGameLevelViewModel("CAVE 3", new Ray1Settings(g.EngineVersion, World.Cave, 3, volume: vol)),
                new LoadGameLevelViewModel("CAVE 4", new Ray1Settings(g.EngineVersion, World.Cave, 4, volume: vol)),
                new LoadGameLevelViewModel("CAVE 5", new Ray1Settings(g.EngineVersion, World.Cave, 5, volume: vol)),
                new LoadGameLevelViewModel("CAVE 6", new Ray1Settings(g.EngineVersion, World.Cave, 6, volume: vol)),
                new LoadGameLevelViewModel("CAVE 7", new Ray1Settings(g.EngineVersion, World.Cave, 7, volume: vol)),
                new LoadGameLevelViewModel("CAVE 8", new Ray1Settings(g.EngineVersion, World.Cave, 8, volume: vol)),
                new LoadGameLevelViewModel("CAVE 9", new Ray1Settings(g.EngineVersion, World.Cave, 9, volume: vol)),
                new LoadGameLevelViewModel("CAVE 10", new Ray1Settings(g.EngineVersion, World.Cave, 10, volume: vol)),

                new LoadGameLevelViewModel("Cake", null),
                new LoadGameLevelViewModel("CAKE 1", new Ray1Settings(g.EngineVersion, World.Cake, 1, volume: vol)),
                new LoadGameLevelViewModel("CAKE 2", new Ray1Settings(g.EngineVersion, World.Cake, 2, volume: vol)),
                new LoadGameLevelViewModel("CAKE 3", new Ray1Settings(g.EngineVersion, World.Cake, 3, volume: vol)),
                new LoadGameLevelViewModel("CAKE 4", new Ray1Settings(g.EngineVersion, World.Cake, 4, volume: vol)),
                new LoadGameLevelViewModel("CAKE 5", new Ray1Settings(g.EngineVersion, World.Cake, 5, volume: vol)),
                new LoadGameLevelViewModel("CAKE 6", new Ray1Settings(g.EngineVersion, World.Cake, 6, volume: vol)),
                new LoadGameLevelViewModel("CAKE 7", new Ray1Settings(g.EngineVersion, World.Cake, 7, volume: vol)),
                new LoadGameLevelViewModel("CAKE 8", new Ray1Settings(g.EngineVersion, World.Cake, 8, volume: vol)),
                new LoadGameLevelViewModel("CAKE 9", new Ray1Settings(g.EngineVersion, World.Cake, 9, volume: vol)),
                new LoadGameLevelViewModel("CAKE 10", new Ray1Settings(g.EngineVersion, World.Cake, 10, volume: vol)),
            };
        }
    }
}