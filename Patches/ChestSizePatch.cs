using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMP_Mod.Patches
{
    class Container_Configs
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
       
        static void Postfix(Container __instance, ref Inventory ___m_inventory)
        {
           
            if (__instance == null || ___m_inventory == null || !__instance.transform.parent) return;

            string containerName = __instance.transform.parent.name;
            string inventoryName = ___m_inventory.m_name;
            ref int inventoryColumns = ref ___m_inventory.m_width;
            ref int inventoryRows = ref ___m_inventory.m_height;

            // Karve (small boat)
            // Use Contains because the actual name is "Karve (Clone)"
            if (containerName.Contains("Karve"))
            {
                inventoryRows = Container_Configs.KarveRow.Value;
                inventoryColumns = Container_Configs.KarveCol.Value;
            }
            // Longboat (Large boat)
            else if (containerName.Contains("VikingShip"))
            {
                inventoryRows = Container_Configs.LongRow.Value;
                inventoryColumns = Container_Configs.LongCol.Value;
            }
            // Cart (Wagon)
            else if (containerName.Contains("Cart"))
            {
                inventoryRows = Container_Configs.CartRow.Value;
                inventoryColumns = Container_Configs.CartCol.Value;
            }
            // Chests (containerName is _NetSceneRoot)
            else
            {
                // Personal chest
                if (inventoryName == "$piece_chestprivate")
                {
                    inventoryRows = Container_Configs.PersonalRow.Value;
                    inventoryColumns = Container_Configs.PersonalCol.Value;
                }
                // Wood chest
                else if (inventoryName == "$piece_chestwood")
                {
                    inventoryRows = Container_Configs.WoodRow.Value;
                    inventoryColumns = Container_Configs.WoodCol.Value;
                }
                // Iron chest
                else if (inventoryName == "$piece_chest")
                {
                    inventoryRows = Container_Configs.IronRow.Value;
                    inventoryColumns = IronCol.Value;
                }
            }

        }
    }
   }
}
