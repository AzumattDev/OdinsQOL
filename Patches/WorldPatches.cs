using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches
{
    internal class WorldPatches
    {
        public static ConfigEntry<bool> ChangeDungeons = null!;
        public static ConfigEntry<bool> ChangeCamps = null!;
        public static ConfigEntry<int> DungeonMaxRoomCount = null!;
        public static ConfigEntry<int> DungoneMinRoomCount = null!;
        public static ConfigEntry<int> CampRadiusMin = null!;
        public static ConfigEntry<int> CampRadiusMax = null!;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Generate), typeof(int),
            typeof(ZoneSystem.SpawnMode))]
        private static void ApplyGeneratorSettings(ref DungeonGenerator __instance)
        {
            if (ChangeDungeons.Value)
            {
                __instance.m_minRooms = DungoneMinRoomCount.Value;
                __instance.m_maxRooms = DungeonMaxRoomCount.Value;
            }
            if (!ChangeCamps.Value) return;
            __instance.m_campRadiusMin = CampRadiusMin.Value;
            __instance.m_campRadiusMax = CampRadiusMax.Value;
        }
    }
}