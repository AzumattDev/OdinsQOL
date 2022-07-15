using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class WorldPatchConfigs
{
    internal static void Generate()
    {
        WorldPatches.DungeonMaxRoomCount = OdinQOLplugin.context.config("Dungeon", "Max Room Count", 20,
            "This is the max number of rooms placed by dungeon gen higher numbers will cause lag");

        WorldPatches.DungoneMinRoomCount = OdinQOLplugin.context.config("Dungeon", "Min Room Count", 5,
            "This is the Min number of rooms placed by dungeon gen higher numbers will cause lag");

        WorldPatches.CampRadiusMin =
            OdinQOLplugin.context.config("Dungeon", "Camp Radius Min", 5,
                "This is the minimum radius for goblin camps");

        WorldPatches.CampRadiusMax =
            OdinQOLplugin.context.config("Dungeon", "Camp Radius Max", 15,
                "This is the maximum radius for goblin camps");
    }
}