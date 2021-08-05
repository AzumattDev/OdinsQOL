using HarmonyLib;

namespace VMP_Mod.Patches
{
    internal class WorldPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate", typeof(int), typeof(ZoneSystem.SpawnMode))]
        private static void ApplyGeneratorSettings(ref DungeonGenerator __instance)
        {
            __instance.m_minRooms = 40;
            __instance.m_maxRooms = 70;
            __instance.m_campRadiusMin = 40f;
            __instance.m_campRadiusMax = 60f;
        }
    }
}