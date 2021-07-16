using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;

namespace VMP_Mod.Patches
{
    /// <summary>
    ///     Makes all items fill inventories top to bottom instead of just tools and weapons
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "TopFirst")]
    public static class Inventory_TopFirst_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            if (VMP_Modplugin.filltoptobottom.Value)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), "IsTeleportable")]
    public static class noItemTeleportPrevention
    {
        private static void Postfix(ref bool __result)
        {
            if (VMP_Modplugin.NoTeleportPrevention.Value)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
    public static class ItemDrop_Awake_Patch
    {
        private static void Prefix(ref ItemDrop __instance)
        {
            if (VMP_Modplugin.itemStackMultiplier.Value > 0)
            {
                __instance.m_itemData.m_shared.m_weight =
                    VMP_Modplugin.applyModifierValue(__instance.m_itemData.m_shared.m_weight,
                        VMP_Modplugin.WeightReduction.Value);

                if (__instance.m_itemData.m_shared.m_maxStackSize > 1)
                    if (VMP_Modplugin.itemStackMultiplier.Value >= 1)
                        __instance.m_itemData.m_shared.m_maxStackSize = (int) VMP_Modplugin.applyModifierValue(
                            __instance.m_itemData.m_shared.m_maxStackSize, VMP_Modplugin.itemStackMultiplier.Value);
            }
        }
    }

    public static class Inventory_NearbyChests_Cache
    {
        public static List<Container> chests = new List<Container>();
        public static readonly Stopwatch delta = new Stopwatch();
    }

    /// <summary>
    ///     When merging another inventory, try to merge items with existing stacks.
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "MoveAll")]
    public static class Inventory_MoveAll_Patch
    {
        private static void Prefix(ref Inventory __instance, ref Inventory fromInventory)
        {
            var list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
            foreach (var otherItem in list)
                if (otherItem.m_shared.m_maxStackSize > 1)
                    foreach (var myItem in __instance.m_inventory)
                        if (myItem.m_shared.m_name == otherItem.m_shared.m_name &&
                            myItem.m_quality == otherItem.m_quality)
                        {
                            var itemsToMove = Math.Min(myItem.m_shared.m_maxStackSize - myItem.m_stack,
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