using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches
{
    internal class Container_Configs
    {
        public static ConfigEntry<bool> ContainerSectionOn = null!;
        public static ConfigEntry<bool> ChestContainerControl = null!;
        public static ConfigEntry<bool> ShipContainerControl = null!;
        public static ConfigEntry<int> KarveRow = null!;
        public static ConfigEntry<int> KarveCol = null!;
        public static ConfigEntry<int> LongRow = null!;
        public static ConfigEntry<int> LongCol = null!;
        public static ConfigEntry<int> CartRow = null!;
        public static ConfigEntry<int> CartCol = null!;
        public static ConfigEntry<int> PersonalRow = null!;
        public static ConfigEntry<int> PersonalCol = null!;
        public static ConfigEntry<int> WoodRow = null!;
        public static ConfigEntry<int> WoodCol = null!;
        public static ConfigEntry<int> IronRow = null!;
        public static ConfigEntry<int> IronCol = null!;
        public static ConfigEntry<int> BmRow = null!;
        public static ConfigEntry<int> BmCol = null!;

        [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
        public static class Container_Awake_Patch
        {
            private static void Postfix(Container __instance, ref Inventory ___m_inventory)
            {
                if (!ContainerSectionOn.Value) return;

                if (__instance == null || ___m_inventory == null || !__instance.transform.parent)
                {
                    // is chest
                    if (!ChestContainerControl.Value) return;
                    if (___m_inventory == null) return;

                    string inventoryName = ___m_inventory.m_name;
                    ref int inventoryColumns = ref ___m_inventory.m_width;
                    ref int inventoryRows = ref ___m_inventory.m_height;

                    switch (inventoryName)
                    {
                        // Personal chest
                        case "$piece_chestprivate":
                            inventoryRows = PersonalRow.Value;
                            inventoryColumns = PersonalCol.Value;
                            break;
                        // Wood chest
                        case "$piece_chestwood":
                            inventoryRows = WoodRow.Value;
                            inventoryColumns = WoodCol.Value;
                            break;
                        // Iron chest
                        case "$piece_chest":
                            inventoryRows = IronRow.Value;
                            inventoryColumns = IronCol.Value;
                            break;
                        // Blackmetal chest
                        case "$piece_chestblackmetal":
                            inventoryRows = BmRow.Value;
                            inventoryColumns = BmCol.Value;
                            break;
                    }
                }
                else
                {
                    // is not chest
                    if (!ShipContainerControl.Value) return;
                    string containerName = __instance.transform.parent.name;
                    string inventoryName = ___m_inventory.m_name;
                    ref int inventoryColumns = ref ___m_inventory.m_width;
                    ref int inventoryRows = ref ___m_inventory.m_height;

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
                }

                ;
            }
        }
    }
}