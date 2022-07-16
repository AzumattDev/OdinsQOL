using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OdinQOL.Patches
{
    internal class AutoStorePatch
    {
        public static ConfigEntry<float> dropRangeChests;
        public static ConfigEntry<float> dropRangePersonalChests;
        public static ConfigEntry<float> dropRangeReinforcedChests;
        public static ConfigEntry<float> dropRangeCarts;
        public static ConfigEntry<float> dropRangeShips;
        public static ConfigEntry<string> itemDisallowTypes;
        public static ConfigEntry<string> itemAllowTypes;
        public static ConfigEntry<string> itemDisallowTypesChests;
        public static ConfigEntry<string> itemAllowTypesChests;
        public static ConfigEntry<string> itemDisallowTypesPersonalChests;
        public static ConfigEntry<string> itemAllowTypesPersonalChests;
        public static ConfigEntry<string> itemDisallowTypesReinforcedChests;
        public static ConfigEntry<string> itemAllowTypesReinforcedChests;
        public static ConfigEntry<string> itemDisallowTypesBlackMetalChests;
        public static ConfigEntry<string> itemAllowTypesBlackMetalChests;
        public static ConfigEntry<string> itemDisallowTypesCarts;
        public static ConfigEntry<string> itemAllowTypesCarts;
        public static ConfigEntry<string> itemDisallowTypesShips;
        public static ConfigEntry<string> itemAllowTypesShips;

        public static ConfigEntry<string> toggleKey;
        public static ConfigEntry<string> toggleString;

        public static ConfigEntry<bool> mustHaveItemToPull;
        public static ConfigEntry<bool> isOn;

        private static bool DisallowItem(Container container, ItemDrop.ItemData item)
        {
            string? name = item.m_dropPrefab.name;
            if (itemAllowTypes.Value is { Length: > 0 } &&
                !itemAllowTypes.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemDisallowTypes.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;

            if (mustHaveItemToPull.Value && !container.GetInventory().HaveItem(item.m_shared.m_name))
                return true;

            Ship? ship = container.gameObject.transform.parent?.GetComponent<Ship>();
            if (ship != null)
            {
                if (itemAllowTypesShips.Value != null && itemAllowTypesShips.Value.Length > 0 &&
                    !itemAllowTypesShips.Value.Split(',').Contains(name))
                    return true;
                return itemDisallowTypesShips.Value.Split(',').Contains(name);
            }

            if (container.m_wagon)
            {
                if (itemAllowTypesCarts.Value != null && itemAllowTypesCarts.Value.Length > 0 &&
                    !itemAllowTypesCarts.Value.Split(',').Contains(name))
                    return true;
                return itemDisallowTypesCarts.Value.Split(',').Contains(name);
            }

            if (container.name.StartsWith("piece_chest_wood", StringComparison.Ordinal))
            {
                if (itemAllowTypesChests.Value != null && itemAllowTypesChests.Value.Length > 0 &&
                    !itemAllowTypesChests.Value.Split(',').Contains(name))
                    return true;
                return itemDisallowTypesChests.Value.Split(',').Contains(name);
            }

            if (container.name.StartsWith("piece_chest_private", StringComparison.Ordinal))
            {
                if (itemAllowTypesPersonalChests.Value != null && itemAllowTypesPersonalChests.Value.Length > 0 &&
                    !itemAllowTypesPersonalChests.Value.Split(',').Contains(name))
                    return true;
                return itemDisallowTypesPersonalChests.Value.Split(',').Contains(name);
            }

            if (container.name.StartsWith("piece_chest_blackmetal", StringComparison.Ordinal))
            {
                if (itemAllowTypesBlackMetalChests.Value != null && itemAllowTypesBlackMetalChests.Value.Length > 0 &&
                    !itemAllowTypesBlackMetalChests.Value.Split(',').Contains(name))
                    return true;
                return itemDisallowTypesBlackMetalChests.Value.Split(',').Contains(name);
            }

            if (container.name.StartsWith("piece_chest", StringComparison.Ordinal))
            {
                if (itemAllowTypesReinforcedChests.Value != null && itemAllowTypesReinforcedChests.Value.Length > 0 &&
                    !itemAllowTypesReinforcedChests.Value.Split(',').Contains(name))
                    return true;
                return itemDisallowTypesReinforcedChests.Value.Split(',').Contains(name);
            }

            return true;
        }

        private static float ContainerRange(Container container)
        {
            if (container.GetInventory() == null)
                return -1f;

            Ship? ship = container.gameObject.transform.parent?.GetComponent<Ship>();
            if (ship != null)
                return dropRangeShips.Value;
            if (container.m_wagon)
                return dropRangeCarts.Value;
            if (container.name.StartsWith("piece_chest_wood", StringComparison.Ordinal))
                return dropRangeChests.Value;
            if (container.name.StartsWith("piece_chest_private", StringComparison.Ordinal))
                return dropRangePersonalChests.Value;
            if (container.name.StartsWith("piece_chest", StringComparison.Ordinal))
                return dropRangeReinforcedChests.Value;
            return -1f;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.CheckForChanges))]
        private static class Container_CheckForChanges_Patch
        {
            private static void Postfix(Container __instance, ZNetView ___m_nview)
            {
                if (!isOn.Value || ___m_nview == null || ___m_nview.GetZDO() == null)
                    return;

                Vector3 position = __instance.transform.position + Vector3.up;
                foreach (Collider? collider in Physics.OverlapSphere(position, ContainerRange(__instance),
                             LayerMask.GetMask("item")))
                    if (collider?.attachedRigidbody)
                    {
                        ItemDrop? item = collider.attachedRigidbody.GetComponent<ItemDrop>();
                        OdinQOLplugin.QOLLogger.LogDebug($"Nearby item name: {item.m_itemData.m_dropPrefab.name}");

                        if (item?.GetComponent<ZNetView>()?.IsValid() != true ||
                            !item.GetComponent<ZNetView>().IsOwner())
                            continue;
                        if (!TryStoreItem(__instance, ref item.m_itemData)) continue;
                        item.Save();
                        if (item.GetComponent<ZNetView>() == null)
                            Object.DestroyImmediate(item.gameObject);
                        else
                            ZNetScene.instance.Destroy(item.gameObject);
                    }
            }
        }

        private static bool TryStoreItem(Container __instance, ref ItemDrop.ItemData item)
        {
            if (DisallowItem(__instance, item))
                return false;

            OdinQOLplugin.QOLLogger.LogDebug(
                $"Auto storing {item.m_dropPrefab.name} from ground");
            bool changed = false;
            while (item.m_stack > 1 && __instance.GetInventory().CanAddItem(item, 1))
            {
                changed = true;
                item.m_stack--;
                ItemDrop.ItemData newItem = item.Clone();
                newItem.m_stack = 1;
                __instance.GetInventory().AddItem(newItem);
            }

            if (item.m_stack == 1 && __instance.GetInventory().CanAddItem(item, 1))
            {
                ItemDrop.ItemData newItem = item.Clone();
                item.m_stack = 0;
                __instance.GetInventory().AddItem(newItem);
                changed = true;
            }

            if (changed)
                __instance.Save();

            return changed;
        }
    }
}