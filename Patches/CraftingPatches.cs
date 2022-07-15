using System;
using System.Collections.Generic;
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

        public static ConfigEntry<int> WorkbenchRange;
        public static ConfigEntry<int> workbenchEnemySpawnRange;
        public static ConfigEntry<bool> AlterWorkBench;
        public static ConfigEntry<int> maxEntries;
        public static ConfigEntry<SortType> sortType;
        public static ConfigEntry<bool> sortAsc;
        public static ConfigEntry<string> entryString;
        public static ConfigEntry<string> overFlowText;
        public static ConfigEntry<string> capacityText;
        public static ConfigEntry<bool> useScrollWheel;
        public static ConfigEntry<bool> showMenu;
        public static ConfigEntry<string> scrollModKey;
        public static ConfigEntry<string> prevHotKey;
        public static ConfigEntry<string> nextHotKey;
        public static ConfigEntry<int> workbenchAttachmentRange;


        public static void ResizeChildEffectArea(MonoBehaviour parent, EffectArea.Type includedTypes, float newRadius)
        {
            if (parent != null)
            {
                EffectArea? effectArea = parent.GetComponentInChildren<EffectArea>();
                if (effectArea != null)
                    if ((effectArea.m_type & includedTypes) != 0)
                    {
                        SphereCollider? collision = effectArea.GetComponent<SphereCollider>();
                        if (collision != null) collision.radius = newRadius;
                    }
            }
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
                if (a.m_shared.m_weight == b.m_shared.m_weight)
                {
                    if (a.m_shared.m_name == b.m_shared.m_name)
                        return CompareInts(a.m_stack, b.m_stack, false);
                    return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                        Localization.instance.Localize(b.m_shared.m_name), true);
                }

                return CompareFloats(a.m_shared.m_weight, b.m_shared.m_weight, asc);
            });
        }

        public static void SortByValue(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_shared.m_value == b.m_shared.m_value)
                {
                    if (a.m_shared.m_name == b.m_shared.m_name)
                        return CompareInts(a.m_stack, b.m_stack, false);
                    return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                        Localization.instance.Localize(b.m_shared.m_name), true);
                }

                return CompareInts(a.m_shared.m_value, b.m_shared.m_value, asc);
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
            if (asc)
                return string.Compare(a, b, StringComparison.Ordinal);
            return string.Compare(b, a, StringComparison.Ordinal);
        }

        public static int CompareFloats(float a, float b, bool asc)
        {
            if (asc)
                return a.CompareTo(b);
            return b.CompareTo(a);
        }

        public static int CompareInts(float a, float b, bool asc)
        {
            if (asc)
                return a.CompareTo(b);
            return b.CompareTo(a);
        }


        /// <summary>
        ///     Alter workbench range
        /// </summary>
        [HarmonyPatch(typeof(CraftingStation), "Start")]
        public static class WorkbenchRangeIncrease
        {
            private static void Prefix(ref CraftingStation __instance, ref float ___m_rangeBuild,
                GameObject ___m_areaMarker)
            {
                if (AlterWorkBench.Value && WorkbenchRange.Value > 0)
                    try
                    {
                        ___m_rangeBuild = WorkbenchRange.Value;
                        ___m_areaMarker.GetComponent<CircleProjector>().m_radius = ___m_rangeBuild;
                        float scaleIncrease = (WorkbenchRange.Value - 20f) / 20f * 100f;
                        ___m_areaMarker.gameObject.transform.localScale =
                            new Vector3(scaleIncrease / 100, 1f, scaleIncrease / 100);

                        // Apply this change to the child GameObject's EffectArea collision.
                        // Various other systems query this collision instead of the PrivateArea radius for permissions (notably, enemy spawning).
                        ResizeChildEffectArea(__instance, EffectArea.Type.PlayerBase,
                            workbenchEnemySpawnRange.Value > 0 ? workbenchEnemySpawnRange.Value : WorkbenchRange.Value);
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
        [HarmonyPatch(typeof(CraftingStation), "CheckUsable")]
        public static class WorkbenchRemoveRestrictions
        {
            private static bool Prefix(ref CraftingStation __instance, ref Player player, ref bool showMessage,
                ref bool __result)
            {
                if (AlterWorkBench.Value) __instance.m_craftRequireRoof = false;

                return true;
            }
        }

        [HarmonyPatch(typeof(Container), "GetHoverText")]
        private static class GetHoverText_Patch
        {
            private static void Postfix(Container __instance, ref string __result)
            {
                if (__instance.m_checkGuardStone &&
                    !PrivateArea.CheckAccess(__instance.transform.position, 0f, false) ||
                    __instance.GetInventory().NrOfItems() == 0)
                    return;

                List<ItemData>? items = new();
                foreach (ItemDrop.ItemData? idd in __instance.GetInventory().GetAllItems())
                    items.Add(new ItemData(idd));
                SortByType(SortType.Value, items, sortAsc.Value);
                int entries = 0;
                int amount = 0;
                string? name = "";

                if (capacityText.Value.Trim().Length > 0)
                {
                    __result = __result.Replace("\n",
                        string.Format(capacityText.Value, __instance.GetInventory().GetAllItems().Count,
                            __instance.GetInventory().GetWidth() * __instance.GetInventory().GetHeight()) + "\n");
                }

                for (int i = 0; i < items.Count; i++)
                {
                    if (maxEntries.Value >= 0 && entries >= maxEntries.Value)
                    {
                        if (overFlowText.Value.Length > 0)
                            __result += "\n" + overFlowText.Value;
                        break;
                    }

                    ItemData? item = items[i];

                    if (item.m_shared.m_name == name || name == "")
                    {
                        amount += item.m_stack;
                    }
                    else
                    {
                        __result += "\n" + string.Format(entryString.Value, amount,
                            Localization.instance.Localize(name));
                        entries++;

                        amount = item.m_stack;
                    }

                    name = item.m_shared.m_name;
                    if (i == items.Count - 1)
                        __result += "\n" + string.Format(entryString.Value, amount,
                            Localization.instance.Localize(name));
                }
            }
        }

        [HarmonyPatch(typeof(StationExtension), nameof(StationExtension.Awake))]
        static class StationExtension_Awake_Patch
        {
            static void Prefix(StationExtension __instance, ref float ___m_maxStationDistance)
            {
                if (CraftingPatch.AlterWorkBench.Value)
                    ___m_maxStationDistance = CraftingPatch.workbenchAttachmentRange.Value;
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