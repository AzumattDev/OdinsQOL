using BepInEx.Configuration;
using HarmonyLib;

namespace VMP_Mod.Patches
{
    internal class Container_Configs
    {
        public static ConfigEntry<int> KarveRow;
        public static ConfigEntry<int> KarveCol;
        public static ConfigEntry<int> LongRow;
        public static ConfigEntry<int> LongCol;
        public static ConfigEntry<int> CartRow;
        public static ConfigEntry<int> CartCol;
        public static ConfigEntry<int> PersonalRow;
        public static ConfigEntry<int> PersonalCol;
        public static ConfigEntry<int> WoodRow;
        public static ConfigEntry<int> WoodCol;
        public static ConfigEntry<int> IronRow;
        public static ConfigEntry<int> IronCol;

        [HarmonyPatch(typeof(Container), "Awake")]
        public static class Container_Awake_Patch
        {
            private static void Postfix(Container __instance, ref Inventory ___m_inventory)
            {
                if (__instance == null || ___m_inventory == null || !__instance.transform.parent) return;

                var containerName = __instance.transform.parent.name;
                var chestContainerNames = __instance.gameObject.name;
                var inventoryName = ___m_inventory.m_name;
                ref var inventoryColumns = ref ___m_inventory.m_width;
                ref var inventoryRows = ref ___m_inventory.m_height;

                // Karve (small boat)
                // Use Contains because the actual name is "Karve (Clone)"
                if (containerName.Contains("Karve"))
                {
                    inventoryRows = KarveRow.Value;
                    inventoryColumns = KarveCol.Value;
                }
                // Longboat (Large boat)
                else if (containerName.Contains("VikingShip"))
                {
                    inventoryRows = LongRow.Value;
                    inventoryColumns = LongCol.Value;
                }
                // Cart (Wagon)
                else if (containerName.Contains("Cart"))
                {
                    inventoryRows = CartRow.Value;
                    inventoryColumns = CartCol.Value;
                }
                // Chests (containerName is _NetSceneRoot)
                else
                {
                    // Personal chest
                    if (inventoryName == "$piece_chest_private" || chestContainerNames.Contains("$piece_chest_private"))
                    {
                        inventoryRows = PersonalRow.Value;
                        inventoryColumns = PersonalCol.Value;
                    }
                    // Wood chest
                    else if (inventoryName == "$piece_chest_wood" || chestContainerNames.Contains("piece_chest_wood"))
                    {
                        inventoryRows = WoodRow.Value;
                        inventoryColumns = WoodCol.Value;
                    }
                    // Iron chest
                    else if (inventoryName == "$piece_chest" || chestContainerNames.Contains("piece_chest"))
                    {
                        inventoryRows = IronRow.Value;
                        inventoryColumns = IronCol.Value;
                    }
                }
            }
        }
        
    }
}