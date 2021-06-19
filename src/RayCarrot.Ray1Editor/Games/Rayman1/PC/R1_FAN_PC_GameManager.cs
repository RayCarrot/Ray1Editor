using System.Collections.Generic;
using BinarySerializer.Ray1;

namespace RayCarrot.Ray1Editor
{
    public class R1_FAN_PC_GameManager : R1_KIT_PC_GameManager
    {
        public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path)
        {
            var g = (Games.R1_Game)game;

            // Get the default volume to use
            var vol = GetDefaultVolume(path);

            return new LoadGameLevelViewModel[]
            {
                new LoadGameLevelViewModel("Jungle", null),
                new LoadGameLevelViewModel("The O Pond", new Ray1Settings(g.EngineVersion, World.Jungle, 1, volume: vol)),
                new LoadGameLevelViewModel("Good Pickin'", new Ray1Settings(g.EngineVersion, World.Jungle, 2, volume: vol)),
                new LoadGameLevelViewModel("The Living Forest", new Ray1Settings(g.EngineVersion, World.Jungle, 3, volume: vol)),
                new LoadGameLevelViewModel("The enchanted forest", new Ray1Settings(g.EngineVersion, World.Jungle, 4, volume: vol)),
                new LoadGameLevelViewModel("In the thick of things", new Ray1Settings(g.EngineVersion, World.Jungle, 5, volume: vol)),
                new LoadGameLevelViewModel("Water Lily Way", new Ray1Settings(g.EngineVersion, World.Jungle, 6, volume: vol)),
                new LoadGameLevelViewModel("Lost in the Jungle", new Ray1Settings(g.EngineVersion, World.Jungle, 7, volume: vol)),

                new LoadGameLevelViewModel("Music", null),
                new LoadGameLevelViewModel("The Magic Flutes", new Ray1Settings(g.EngineVersion, World.Music, 1, volume: vol)),
                new LoadGameLevelViewModel("A Symphony of Drums", new Ray1Settings(g.EngineVersion, World.Music, 2, volume: vol)),
                new LoadGameLevelViewModel("A trumpet fanfare", new Ray1Settings(g.EngineVersion, World.Music, 3, volume: vol)),
                new LoadGameLevelViewModel("Musical Rambles", new Ray1Settings(g.EngineVersion, World.Music, 4, volume: vol)),
                new LoadGameLevelViewModel("The Wrong Note", new Ray1Settings(g.EngineVersion, World.Music, 5, volume: vol)),

                new LoadGameLevelViewModel("Mountain", null),
                new LoadGameLevelViewModel("Watch out for the peaks", new Ray1Settings(g.EngineVersion, World.Mountain, 1, volume: vol)),
                new LoadGameLevelViewModel("The Illuminated Heights", new Ray1Settings(g.EngineVersion, World.Mountain, 2, volume: vol)),
                new LoadGameLevelViewModel("The Blue Mountain", new Ray1Settings(g.EngineVersion, World.Mountain, 3, volume: vol)),
                new LoadGameLevelViewModel("Ting Mountain", new Ray1Settings(g.EngineVersion, World.Mountain, 4, volume: vol)),
                new LoadGameLevelViewModel("Heart of the Mountain", new Ray1Settings(g.EngineVersion, World.Mountain, 5, volume: vol)),
                new LoadGameLevelViewModel("An avalanche of tings", new Ray1Settings(g.EngineVersion, World.Mountain, 6, volume: vol)),
                new LoadGameLevelViewModel("Hell mountain", new Ray1Settings(g.EngineVersion, World.Mountain, 7, volume: vol)),
                new LoadGameLevelViewModel("Slides and Spikes", new Ray1Settings(g.EngineVersion, World.Mountain, 8, volume: vol)),
                new LoadGameLevelViewModel("Danger at the top", new Ray1Settings(g.EngineVersion, World.Mountain, 9, volume: vol)),
                new LoadGameLevelViewModel("Sliders, Away!", new Ray1Settings(g.EngineVersion, World.Mountain, 10, volume: vol)),

                new LoadGameLevelViewModel("Image", null),
                new LoadGameLevelViewModel("The Forgotten Pencils", new Ray1Settings(g.EngineVersion, World.Image, 1, volume: vol)),
                new LoadGameLevelViewModel("Tangled up in Colors", new Ray1Settings(g.EngineVersion, World.Image, 2, volume: vol)),
                new LoadGameLevelViewModel("Rubbers !", new Ray1Settings(g.EngineVersion, World.Image, 3, volume: vol)),
                new LoadGameLevelViewModel("The drawing lesson", new Ray1Settings(g.EngineVersion, World.Image, 4, volume: vol)),
                new LoadGameLevelViewModel("Punaise !", new Ray1Settings(g.EngineVersion, World.Image, 5, volume: vol)),
                new LoadGameLevelViewModel("Don't Despair", new Ray1Settings(g.EngineVersion, World.Image, 6, volume: vol)),
                new LoadGameLevelViewModel("High Flyer", new Ray1Settings(g.EngineVersion, World.Image, 7, volume: vol)),

                new LoadGameLevelViewModel("Cave", null),
                new LoadGameLevelViewModel("Meandering Peaks", new Ray1Settings(g.EngineVersion, World.Cave, 1, volume: vol)),
                new LoadGameLevelViewModel("Ting Fishin'", new Ray1Settings(g.EngineVersion, World.Cave, 2, volume: vol)),
                new LoadGameLevelViewModel("Up, up and away!", new Ray1Settings(g.EngineVersion, World.Cave, 3, volume: vol)),
                new LoadGameLevelViewModel("A Cave full of Surprises", new Ray1Settings(g.EngineVersion, World.Cave, 4, volume: vol)),
                new LoadGameLevelViewModel("The Forbidden Valley", new Ray1Settings(g.EngineVersion, World.Cave, 5, volume: vol)),

                new LoadGameLevelViewModel("Cake", null),
                new LoadGameLevelViewModel("Peanut Perfume", new Ray1Settings(g.EngineVersion, World.Cake, 1, volume: vol)),
                new LoadGameLevelViewModel("Catch it if you can", new Ray1Settings(g.EngineVersion, World.Cake, 2, volume: vol)),
                new LoadGameLevelViewModel("Yummy, munch, slurp!", new Ray1Settings(g.EngineVersion, World.Cake, 3, volume: vol)),
                new LoadGameLevelViewModel("Bumbs Galore!", new Ray1Settings(g.EngineVersion, World.Cake, 4, volume: vol)),
                new LoadGameLevelViewModel("In the guts of the Cream", new Ray1Settings(g.EngineVersion, World.Cake, 5, volume: vol)),
                new LoadGameLevelViewModel("Supercopter Race", new Ray1Settings(g.EngineVersion, World.Cake, 6, volume: vol)),
            };
        }

        public override IEnumerable<EditorFieldViewModel> GetEditorLevelAttributeFields(GameData gameData)
        {
            foreach (var field in base.GetEditorLevelAttributeFields(gameData))
                yield return field;

            var data = (R1_PC_GameData)gameData;
            var profile = data.ProfileDefines;
         
            // TODO: Add fields for the profile defines
        }
    }
}