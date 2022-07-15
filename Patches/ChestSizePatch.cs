using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches
{
    internal class Container_Configs
    {
        public static ConfigEntry<bool> ContainerSectionOn;
        public static ConfigEntry<bool> ChestContainerControl;
        public static ConfigEntry<bool> ShipContainerControl;
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
        public static ConfigEntry<int> BMRow;
        public static ConfigEntry<int> BMCol;

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

                    // Personal chest
                    if (inventoryName == "$piece_chestprivate")
                    {
                        inventoryRows = PersonalRow.Value;
                        inventoryColumns = PersonalCol.Value;
                    }
                    // Wood chest
                    else if (inventoryName == "$piece_chestwood")
                    {
                        inventoryRows = WoodRow.Value;
                        inventoryColumns = WoodCol.Value;
                    }
                    // Iron chest
                    else if (inventoryName == "$piece_chest")
                    {
                        inventoryRows = IronRow.Value;
                        inventoryColumns = IronCol.Value;
                    }
                    // Blackmetal chest
                    else if (inventoryName == "$piece_chestblackmetal")
                    {
                        inventoryRows = BMRow.Value;
                        inventoryColumns = BMCol.Value;
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