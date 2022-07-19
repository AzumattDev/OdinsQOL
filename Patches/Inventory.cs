using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;

namespace OdinQOL.Patches
{
    /// <summary>
    ///     Makes all items fill inventories top to bottom instead of just tools and weapons
    /// </summary>
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.TopFirst))]
    public static class Inventory_TopFirst_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            if (!OdinQOLplugin.filltoptobottom.Value) return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
    public static class NoItemTeleportPrevention
    {
        private static void Postfix(ref bool __result)
        {
            if (OdinQOLplugin.NoTeleportPrevention.Value)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
    public static class ItemDrop_Awake_Patch
    {
        private static void Prefix(ref ItemDrop __instance)
        {
            if (!(OdinQOLplugin.itemStackMultiplier.Value > 0)) return;
            __instance.m_itemData.m_shared.m_weight =
                Utilities.ApplyModifierValue(__instance.m_itemData.m_shared.m_weight,
                    OdinQOLplugin.WeightReduction.Value);

            if (__instance.m_itemData.m_shared.m_maxStackSize <= 1) return;
            if (OdinQOLplugin.itemStackMultiplier.Value >= 1)
            {
                __instance.m_itemData.m_shared.m_maxStackSize *= (int)OdinQOLplugin.itemStackMultiplier.Value;
            }
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.SetStack))]
    static class ItemDrop_SetStack_Patch
    {
        static void Prefix(ItemDrop __instance)
        {
            if (__instance.m_itemData.m_shared.m_maxStackSize <= 1) return;
            if (OdinQOLplugin.itemStackMultiplier.Value >= 1)
            {
                __instance.m_itemData.m_shared.m_maxStackSize *= (int)OdinQOLplugin.itemStackMultiplier.Value;
            }
        }
    }


    /// <summary>
    ///     When merging another inventory, try to merge items with existing stacks.
    /// </summary>
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll))]
    public static class Inventory_MoveAll_Patch
    {
        private static void Prefix(ref Inventory __instance, ref Inventory fromInventory)
        {
            List<ItemDrop.ItemData>? list = new(fromInventory.GetAllItems());
            foreach (ItemDrop.ItemData? otherItem in list.Where(otherItem => otherItem.m_shared.m_maxStackSize > 1))
            foreach (ItemDrop.ItemData? myItem in __instance.m_inventory)
                if (myItem.m_shared.m_name == otherItem.m_shared.m_name &&
                    myItem.m_quality == otherItem.m_quality)
                {
                    int itemsToMove = Math.Min(myItem.m_shared.m_maxStackSize - myItem.m_stack,
                        otherItem.m_stack);
                    myItem.m_stack += itemsToMove;
                    if (otherItem.m_stack == itemsToMove)
                    {
                        fromInventory.RemoveItem(otherItem);
                        break;
                    }

                    otherItem.m_stack -= itemsToMove;
                }
        }
    }
}