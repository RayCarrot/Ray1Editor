using System;
using System.Linq;
using BinarySerializer.Ray1;

namespace RayCarrot.Ray1Editor
{
    public static class Games
    {
        // TODO: Due to games being separated into game records and managers we could implement external loading through plugins, dynamically populating this array
        public static Game[] LoadedGames { get; } = new Game[]
        {
            // Rayman 1 (PC)
            new R1_Game("R1_PC_1_00", "Rayman 1 (PC - 1.00)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_1_00),
            new R1_Game("R1_PC_1_10", "Rayman 1 (PC - 1.10)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_1_10),
            new R1_Game("R1_PC_1_12", "Rayman 1 (PC - 1.12)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_1_12),
            new R1_Game("R1_PC_1_20", "Rayman 1 (PC - 1.20)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_1_20),
            new R1_Game("R1_PC_1_21", "Rayman 1 (PC - 1.21)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_1_21),
            new R1_Game("R1_PC_1_21_JP", "Rayman 1 (PC - 1.21 JP)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_1_21_JP),
            new R1_Game("R1_PC_Demo_1", "Rayman 1 (PC - Demo 1)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_Demo_1),
            new R1_Game("R1_PC_Demo_2", "Rayman 1 (PC - Demo 2)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC, "r1_pc", Ray1PCVersion.PC_Demo_2),

            // Rayman EDU/KIT/FAN (PC)
            new R1_Game("R1_EDU_PC", "Rayman Educational (PC)", () => new R1_EDU_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC_Edu, "edu"),
            new R1_Game("R1_QUI_PC", "Rayman Quiz (PC)", () => new R1_EDU_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC_Edu, "edu"),
            //new R1_Game("R1_KIT_PC", "Rayman Designer (PC)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC_Kit),
            //new R1_Game("R1_KIT_PC_Demo", "Rayman Gold (PC - Demo)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC_Kit),
            //new R1_Game("R1_FAN_PC", "Rayman by his Fans (PC)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC_Fan),
            //new R1_Game("R1_60N_PC", "Rayman 60 Levels (PC)", () => new R1_PC_GameManager(), GameModePathType.Directory, Ray1EngineVersion.PC_Fan),
        };

        public static Game FromID(string id) => LoadedGames.FirstOrDefault(x => x.ID == id) ?? throw new Exception($"Game with ID {id} not found");

        public record Game(string ID, string DisplayName, Func<GameManager> GetManager, GameModePathType PathType);
        public record R1_Game(string ID, string DisplayName, Func<GameManager> GetManager, GameModePathType PathType, Ray1EngineVersion EngineVersion, string NameTablesName, Ray1PCVersion PCVersion = Ray1PCVersion.None) : Game(ID, DisplayName, GetManager, PathType);
    }
}