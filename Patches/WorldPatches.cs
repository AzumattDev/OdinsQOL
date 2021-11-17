using HarmonyLib;

namespace OdinQOL.Patches
{
    internal class WorldPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate", typeof(int), typeof(ZoneSystem.SpawnMode))]
        private static void ApplyGeneratorSettings(ref DungeonGenerator __instance)
        {
            __instance.m_minRooms = OdinQOLplugin.DungoneMinRoomCount.Value;
            __instance.m_maxRooms = OdinQOLplugin.DungeonMaxRoomCount.Value;
            __instance.m_campRadiusMin = OdinQOLplugin.CampRadiusMin.Value;
            __instance.m_campRadiusMax = OdinQOLplugin.CampRadiusMax.Value;
        }
    }
}