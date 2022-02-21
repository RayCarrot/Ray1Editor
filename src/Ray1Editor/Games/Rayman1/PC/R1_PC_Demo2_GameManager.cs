using System.Collections.Generic;
using BinarySerializer.Ray1;

namespace Ray1Editor;

public class R1_PC_Demo2_GameManager : R1_PC_GameManager
{
    public override IEnumerable<LoadGameLevelViewModel> GetLevels(Games.Game game, string path)
    {
        var engineVersion = ((Games.R1_Game)game).EngineVersion;
        var pcVersion = ((Games.R1_Game)game).PCVersion;

        yield return new LoadGameLevelViewModel("Jungle", null);
        yield return new LoadGameLevelViewModel("Pink Plant Woods 1", new Ray1Settings(engineVersion, World.Jungle, 1, pcVersion));
        yield return new LoadGameLevelViewModel("Pink Plant Woods 2", new Ray1Settings(engineVersion, World.Jungle, 2, pcVersion));
        yield return new LoadGameLevelViewModel("Pink Plant Woods 3 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 3, pcVersion));
        yield return new LoadGameLevelViewModel("Pink Plant Woods 4", new Ray1Settings(engineVersion, World.Jungle, 4, pcVersion));
        yield return new LoadGameLevelViewModel("Anguish Lagoon 1", new Ray1Settings(engineVersion, World.Jungle, 5, pcVersion));
        yield return new LoadGameLevelViewModel("Anguish Lagoon 2 - Boss", new Ray1Settings(engineVersion, World.Jungle, 6, pcVersion));
        yield return new LoadGameLevelViewModel("Anguish Lagoon 3 - Bzzit Flight", new Ray1Settings(engineVersion, World.Jungle, 7, pcVersion));
        yield return new LoadGameLevelViewModel("Anguish Lagoon 4 - Betilla", new Ray1Settings(engineVersion, World.Jungle, 8, pcVersion));
        yield return new LoadGameLevelViewModel("The Swamps of Forgetfulness 1", new Ray1Settings(engineVersion, World.Jungle, 9, pcVersion));
        yield return new LoadGameLevelViewModel("(Bonus) Jungle 3", new Ray1Settings(engineVersion, World.Jungle, 20, pcVersion));
        yield return new LoadGameLevelViewModel("(Bonus) Jungle 4", new Ray1Settings(engineVersion, World.Jungle, 21, pcVersion));

        yield return new LoadGameLevelViewModel("Music", null);
        yield return new LoadGameLevelViewModel("Allegro Presto 1", new Ray1Settings(engineVersion, World.Music, 7, pcVersion));

        yield return new LoadGameLevelViewModel("Mountain", null);
        yield return new LoadGameLevelViewModel("Mr Stone's Peaks 2", new Ray1Settings(engineVersion, World.Mountain, 7, pcVersion));

        yield return new LoadGameLevelViewModel("Image", null);
        yield return new LoadGameLevelViewModel("Eraser Plains 1", new Ray1Settings(engineVersion, World.Image, 1, pcVersion));

        yield return new LoadGameLevelViewModel("Cave", null);
        yield return new LoadGameLevelViewModel("Eat at Joe's 2", new Ray1Settings(engineVersion, World.Cave, 4, pcVersion));
        yield return new LoadGameLevelViewModel("Mr Skops' Stalactites 2 - Boss", new Ray1Settings(engineVersion, World.Cave, 10, pcVersion));
    }
}