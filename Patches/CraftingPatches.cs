using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    internal class CraftingPatch
    {
        public enum SortType
        {
            Name,
            Weight,
            Amount,
            Value
        }

        public static ConfigEntry<bool> AlterWorkbench = null!;
        public static ConfigEntry<int> WorkbenchRange = null!;
        public static ConfigEntry<int> WorkbenchEnemySpawnRange = null!;
        public static ConfigEntry<bool> ChangeRoofBehavior = null!;
        public static ConfigEntry<int> MaxEntries = null!;
        public static ConfigEntry<SortType> sortType = null!;
        public static ConfigEntry<bool> SortAsc = null!;
        public static ConfigEntry<string> EntryString = null!;
        public static ConfigEntry<string> OverFlowText = null!;
        public static ConfigEntry<string> CapacityText = null!;
        public static ConfigEntry<bool> UseScrollWheel = null!;
        public static ConfigEntry<bool> ShowMenu = null!;
        public static ConfigEntry<string> ScrollModKey = null!;
        public static ConfigEntry<string> PrevHotKey = null!;
        public static ConfigEntry<string> NextHotKey = null!;
        public static ConfigEntry<int> WorkbenchAttachmentRange = null!;


        public static void ResizeChildEffectArea(MonoBehaviour parent, EffectArea.Type includedTypes, float newRadius)
        {
            if (parent == null) return;
            EffectArea? effectArea = parent.GetComponentInChildren<EffectArea>();
            if (effectArea == null) return;
            if ((effectArea.m_type & includedTypes) == 0) return;
            SphereCollider? collision = effectArea.GetComponent<SphereCollider>();
            if (collision != null) collision.radius = newRadius;
        }

        public static void SortByType(SortType type, List<ItemData> items, bool asc)
        {
            // combine
            SortByName(items, true);

            for (int i = 0; i < items.Count; i++)
                while (i < items.Count - 1 && items[i].m_stack < items[i].m_shared.m_maxStackSize &&
                       items[i + 1].m_shared.m_name == items[i].m_shared.m_name)
                {
                    int amount = Mathf.Min(items[i].m_shared.m_maxStackSize - items[i].m_stack, items[i + 1].m_stack);
                    items[i].m_stack += amount;
                    if (amount == items[i + 1].m_stack)
                        items.RemoveAt(i + 1);
                    else
                        items[i + 1].m_stack -= amount;
                }

            switch (type)
            {
                case SortType.Name:
                    SortByName(items, asc);
                    break;
                case SortType.Weight:
                    SortByWeight(items, asc);
                    break;
                case SortType.Value:
                    SortByValue(items, asc);
                    break;
                case SortType.Amount:
                    SortByAmount(items, asc);
                    break;
            }
        }

        public static void SortByName(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_shared.m_name == b.m_shared.m_name) return CompareInts(a.m_stack, b.m_stack, false);
                return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                    Localization.instance.Localize(b.m_shared.m_name), asc);
            });
        }

        public static void SortByWeight(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_shared.m_weight != b.m_shared.m_weight)
                    return CompareFloats(a.m_shared.m_weight, b.m_shared.m_weight, asc);
                if (a.m_shared.m_name == b.m_shared.m_name)
                    return CompareInts(a.m_stack, b.m_stack, false);
                return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                    Localization.instance.Localize(b.m_shared.m_name), true);
            });
        }

        public static void SortByValue(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_shared.m_value != b.m_shared.m_value)
                    return CompareInts(a.m_shared.m_value, b.m_shared.m_value, asc);
                if (a.m_shared.m_name == b.m_shared.m_name)
                    return CompareInts(a.m_stack, b.m_stack, false);
                return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                    Localization.instance.Localize(b.m_shared.m_name), true);
            });
        }

        public static void SortByAmount(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_stack == b.m_stack)
                    return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                        Localization.instance.Localize(b.m_shared.m_name), true);
                return CompareInts(a.m_stack, b.m_stack, asc);
            });
        }

        public static int CompareStrings(string a, string b, bool asc)
        {
            return asc
                ? string.Compare(a, b, StringComparison.Ordinal)
                : string.Compare(b, a, StringComparison.Ordinal);
        }

        public static int CompareFloats(float a, float b, bool asc)
        {
            return asc ? a.CompareTo(b) : b.CompareTo(a);
        }

        public static int CompareInts(float a, float b, bool asc)
        {
            return asc ? a.CompareTo(b) : b.CompareTo(a);
        }


        /// <summary>
        ///     Alter workbench range
        /// </summary>
        [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.Start))]
        public static class WorkbenchRangeIncrease
        {
            private static void Prefix(ref CraftingStation __instance, ref float ___m_rangeBuild,
                GameObject ___m_areaMarker)
            {
                if (!AlterWorkbench.Value || WorkbenchRange.Value <= 0) return;
                try
                {
                    ___m_rangeBuild = WorkbenchRange.Value;
                    ___m_areaMarker.GetComponent<CircleProjector>().m_radius = ___m_rangeBuild;

                    // Apply this change to the child GameObject's EffectArea collision.
                    // Various other systems query this collision instead of the PrivateArea radius for permissions (notably, enemy spawning).
                    ResizeChildEffectArea(__instance, EffectArea.Type.PlayerBase,
                        WorkbenchEnemySpawnRange.Value > 0 ? WorkbenchEnemySpawnRange.Value : WorkbenchRange.Value);
                }
                catch
                {
                    // is not a workbench
                }
            }
        }

        /// <summary>
        ///     Disable roof requirement on workbench
        /// </summary>
        [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.CheckUsable))]
        public static class WorkbenchRemoveRestrictions
        {
            private static bool Prefix(ref CraftingStation __instance, ref Player player, ref bool showMessage,
                ref bool __result)
            {
                if (AlterWorkbench.Value && ChangeRoofBehavior.Value) __instance.m_craftRequireRoof = false;

                return true;
            }
        }

        [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
        private static class GetHoverText_Patch
        {
            private static void Postfix(Container __instance, ref string __result)
            {
                if (__instance.m_checkGuardStone &&
                    !PrivateArea.CheckAccess(__instance.transform.position, 0f, false) ||
                    __instance.GetInventory().NrOfItems() == 0)
                    return;

                List<ItemData>? items = __instance.GetInventory().GetAllItems().Select(idd => new ItemData(idd))
                    .ToList();
                SortByType(sortType.Value, items, SortAsc.Value);
                int entries = 0;
                int amount = 0;
                string? name = "";

                if (CapacityText.Value.Trim().Length > 0)
                {
                    __result = __result.Replace("\n",
                        string.Format(CapacityText.Value, __instance.GetInventory().GetAllItems().Count,
                            __instance.GetInventory().GetWidth() * __instance.GetInventory().GetHeight()) + "\n");
                }

                for (int i = 0; i < items.Count; i++)
                {
                    if (MaxEntries.Value >= 0 && entries >= MaxEntries.Value)
                    {
                        if (OverFlowText.Value.Length > 0)
                            __result += "\n" + OverFlowText.Value;
                        break;
                    }

                    ItemData? item = items[i];

                    if (item.m_shared.m_name == name || name == "")
                    {
                        amount += item.m_stack;
                    }
                    else
                    {
                        __result += "\n" + string.Format(EntryString.Value, amount,
                            Localization.instance.Localize(name));
                        entries++;

                        amount = item.m_stack;
                    }

                    name = item.m_shared.m_name;
                    if (i == items.Count - 1)
                        __result += "\n" + string.Format(EntryString.Value, amount,
                            Localization.instance.Localize(name));
                }
            }
        }

        [HarmonyPatch(typeof(StationExtension), nameof(StationExtension.Awake))]
        static class StationExtension_Awake_Patch
        {
            static void Prefix(StationExtension __instance, ref float ___m_maxStationDistance)
            {
                if (AlterWorkbench.Value && WorkbenchRange.Value >=5)
                    ___m_maxStationDistance = WorkbenchAttachmentRange.Value;
            }
        }

        public class ItemData
        {
            public ItemDrop.ItemData.SharedData m_shared;
            public int m_stack;

            public ItemData(ItemDrop.ItemData idd)
            {
                m_shared = idd.m_shared;
                m_stack = idd.m_stack;
            }
        }
    }
}