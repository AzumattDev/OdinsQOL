using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    /*Replace IL stuff above with H&H compat code */
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.RepairOneItem))]
    public static class InventoryGui_RepairOneItem_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction>? il = instructions.ToList();
            if (OdinQOLplugin.AutoRepair.Value) il.RemoveRange(52, 11);

            return il.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRepair))]
    public static class InventoryGui_UpdateRepair_Patch
    {
        /// <summary>
        ///     When we're in a state where the InventoryGui is open and we have items available to repair,
        ///     and we have an active crafting station, this patch is responsible for repairing all items
        ///     that can be repaired and then spawning one instance of the repair effect if at least one item
        ///     has been repaired.
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(InventoryGui __instance)
        {
            if (!OdinQOLplugin.AutoRepair.Value) return;

            CraftingStation? currCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();

            if (currCraftingStation == null) return;
            int repairCount = 0;

            while (__instance.HaveRepairableItems())
            {
                __instance.RepairOneItem();
                ++repairCount;
            }

            if (repairCount > 0)
                currCraftingStation.m_repairItemDoneEffects.Create(currCraftingStation.transform.position,
                    Quaternion.identity);
        }
    }
}