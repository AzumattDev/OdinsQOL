using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches
{
    internal class WorldPatches
    {
        public static ConfigEntry<int> DungeonMaxRoomCount;
        public static ConfigEntry<int> DungoneMinRoomCount;
        public static ConfigEntry<int> CampRadiusMin;
        public static ConfigEntry<int> CampRadiusMax;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DungeonGenerator), "Generate", typeof(int), typeof(ZoneSystem.SpawnMode))]
        private static void ApplyGeneratorSettings(ref DungeonGenerator __instance)
        {
            __instance.m_minRooms = DungoneMinRoomCount.Value;
            __instance.m_maxRooms = DungeonMaxRoomCount.Value;
            __instance.m_campRadiusMin = CampRadiusMin.Value;
            __instance.m_campRadiusMax = CampRadiusMax.Value;
        }
    }
}