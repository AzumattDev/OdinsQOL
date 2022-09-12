using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OdinQOL.Patches;

internal class AutoStorePatch
{
    public static ConfigEntry<float> dropRangeChests;
    public static ConfigEntry<float> dropRangePersonalChests;
    public static ConfigEntry<float> dropRangeReinforcedChests;
    public static ConfigEntry<float> dropRangeBlackmetalChests;
    public static ConfigEntry<float> dropRangeCustomChests;
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
    public static ConfigEntry<string> customChests;
    public static ConfigEntry<string> itemAllowTypesCustomChests;
    public static ConfigEntry<string> itemDisallowTypesCustomChests;
    public static ConfigEntry<string> itemAllowTypesBlackMetalChests;
    public static ConfigEntry<string> itemDisallowTypesCarts;
    public static ConfigEntry<string> itemAllowTypesCarts;
    public static ConfigEntry<string> itemDisallowTypesShips;
    public static ConfigEntry<string> itemAllowTypesShips;

    public static ConfigEntry<string> itemDisallowCategories;
    public static ConfigEntry<string> itemAllowCategories;
    public static ConfigEntry<string> itemDisallowCategoriesChests;
    public static ConfigEntry<string> itemAllowCategoriesChests;
    public static ConfigEntry<string> itemDisallowCategoriesPersonalChests;
    public static ConfigEntry<string> itemAllowCategoriesPersonalChests;
    public static ConfigEntry<string> itemDisallowCategoriesReinforcedChests;
    public static ConfigEntry<string> itemAllowCategoriesReinforcedChests;
    public static ConfigEntry<string> itemDisallowCategoriesBlackMetalChests;
    public static ConfigEntry<string> itemAllowCategoriesBlackMetalChests;
    public static ConfigEntry<string> itemDisallowCategoriesCustomChests;
    public static ConfigEntry<string> itemAllowCategoriesCustomChests;
    public static ConfigEntry<string> itemDisallowCategoriesCarts;
    public static ConfigEntry<string> itemAllowCategoriesCarts;
    public static ConfigEntry<string> itemDisallowCategoriesShips;
    public static ConfigEntry<string> itemAllowCategoriesShips;

    public static ConfigEntry<string> toggleKey;
    public static ConfigEntry<string> toggleString;

    public static ConfigEntry<bool> mustHaveItemToPull;
    public static ConfigEntry<bool> isOn;
    public static ConfigEntry<KeyboardShortcut> StoreShortcut; // This will be checked in the main update method.
    public static ConfigEntry<float> playerRange;

    private static bool DisallowItem(Container container, ItemDrop.ItemData item)
    {
        string? name = item.m_dropPrefab.name;
        string category = item.m_shared.m_itemType.ToString();
        if (itemAllowTypes.Value is { Length: > 0 } &&
            !itemAllowTypes.Value.Split(',').Contains(name, StringComparer.Ordinal))
            return true;
        if (itemDisallowTypes.Value.Split(',').Contains(name, StringComparer.Ordinal))
            return true;

        if (itemAllowCategories.Value is { Length: > 0 } &&
            !itemAllowCategories.Value.Split(',').Contains(category, StringComparer.Ordinal))
            return true;
        if (itemDisallowCategories.Value.Split(',').Contains(category, StringComparer.Ordinal))
            return true;

        if (mustHaveItemToPull.Value && !container.GetInventory().HaveItem(item.m_shared.m_name))
            return true;

        Ship? ship = container.gameObject.transform.parent?.GetComponent<Ship>();
        if (ship != null)
        {
            if (itemAllowTypesShips.Value is { Length: > 0 } &&
                !itemAllowTypesShips.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemDisallowTypesShips.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemAllowCategoriesShips.Value is { Length: > 0 } && !itemAllowCategoriesShips.Value.Split(',')
                    .Contains(category, StringComparer.Ordinal))
                return true;
            return itemDisallowCategoriesShips.Value.Split(',').Contains(category, StringComparer.Ordinal);
        }

        if (container.m_wagon)
        {
            if (itemAllowTypesCarts.Value is { Length: > 0 } &&
                !itemAllowTypesCarts.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemDisallowTypesCarts.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemAllowCategoriesCarts.Value is { Length: > 0 } && !itemAllowCategoriesCarts.Value.Split(',')
                    .Contains(category, StringComparer.Ordinal))
                return true;
            return itemDisallowCategoriesCarts.Value.Split(',').Contains(category, StringComparer.Ordinal);
        }

        if (container.name.StartsWith("piece_chest_wood", StringComparison.Ordinal))
        {
            if (itemAllowTypesChests.Value is { Length: > 0 } &&
                !itemAllowTypesChests.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemDisallowTypesChests.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemAllowCategoriesChests.Value is { Length: > 0 } && !itemAllowCategoriesChests.Value.Split(',')
                    .Contains(category, StringComparer.Ordinal))
                return true;
            return itemDisallowCategoriesChests.Value.Split(',').Contains(category, StringComparer.Ordinal);
        }

        if (container.name.StartsWith("piece_chest_private", StringComparison.Ordinal))
        {
            if (itemAllowTypesPersonalChests.Value is { Length: > 0 } && !itemAllowTypesPersonalChests.Value
                    .Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemDisallowTypesPersonalChests.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemAllowCategoriesPersonalChests.Value is { Length: > 0 } && !itemAllowCategoriesPersonalChests
                    .Value.Split(',').Contains(category, StringComparer.Ordinal))
                return true;
            return itemDisallowCategoriesPersonalChests.Value.Split(',').Contains(category, StringComparer.Ordinal);
        }

        if (container.name.StartsWith("piece_chest_blackmetal", StringComparison.Ordinal))
        {
            if (itemAllowTypesBlackMetalChests.Value is { Length: > 0 } && !itemAllowTypesBlackMetalChests.Value
                    .Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemDisallowTypesBlackMetalChests.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemAllowCategoriesBlackMetalChests.Value is { Length: > 0 } && !itemAllowCategoriesBlackMetalChests
                    .Value.Split(',').Contains(category, StringComparer.Ordinal))
                return true;
            return itemDisallowCategoriesBlackMetalChests.Value.Split(',')
                .Contains(category, StringComparer.Ordinal);
        }

        if (container.name.StartsWith("piece_chest", StringComparison.Ordinal))
        {
            if (itemAllowTypesReinforcedChests.Value is { Length: > 0 } && !itemAllowTypesReinforcedChests.Value
                    .Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemDisallowTypesReinforcedChests.Value.Split(',').Contains(name, StringComparer.Ordinal))
                return true;
            if (itemAllowCategoriesReinforcedChests.Value is { Length: > 0 } && !itemAllowCategoriesReinforcedChests
                    .Value.Split(',').Contains(category, StringComparer.Ordinal))
                return true;
            return itemDisallowCategoriesReinforcedChests.Value.Split(',')
                .Contains(category, StringComparer.Ordinal);
        }

        foreach (string s in customChests.Value.Split(','))
        {
            if (container.name.StartsWith(s, StringComparison.Ordinal))
            {
                if (itemAllowTypesCustomChests.Value is { Length: > 0 } && !itemAllowTypesCustomChests.Value
                        .Split(',').Contains(name, StringComparer.Ordinal))
                    return true;
                if (itemDisallowTypesCustomChests.Value.Split(',').Contains(name, StringComparer.Ordinal))
                    return true;
                if (itemAllowCategoriesCustomChests.Value is { Length: > 0 } && !itemAllowCategoriesCustomChests
                        .Value.Split(',').Contains(category, StringComparer.Ordinal))
                    return true;
                return itemDisallowCategoriesCustomChests.Value.Split(',')
                    .Contains(category, StringComparer.Ordinal);
            }
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
        if (container.name.StartsWith("piece_chest_blackmetal", StringComparison.Ordinal))
            return dropRangeBlackmetalChests.Value;
        foreach (string s in customChests.Value.Split(','))
        {
            if (container.name.StartsWith(s, StringComparison.Ordinal))
                return dropRangeCustomChests.Value;
        }

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
                    if (!item) continue;
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

    // ReSharper disable Unity.PerformanceAnalysis
    internal static void PlayerTryStore()
    {
        if (Player.m_localPlayer == null) return;

        OdinQOLplugin.QOLLogger.LogDebug(
            $"Trying to store items from player inventory.");
        int total = 0;
        int containers = 0;

        Vector3 position = Player.m_localPlayer.transform.position + Vector3.up;
        foreach (Collider collider in Physics.OverlapSphere(position, playerRange.Value,
                     LayerMask.GetMask("piece", "item", "piece_nonsolid", "vehicle")))
        {
            if (!collider)
                continue;

            Container c = collider.GetComponent<Container>();
            if (!c && collider.transform.parent)
                c = collider.transform.parent.GetComponent<Container>();
            if (!c && collider.transform.parent?.parent)
                c = collider.transform.parent.parent.GetComponent<Container>();
            if (!c || c.IsInUse() || !(bool)AccessTools.Method(typeof(Container), "CheckAccess")
                    .Invoke(c, new object[] { Player.m_localPlayer.GetPlayerID() })) continue;
            OdinQOLplugin.QOLLogger.LogDebug($"Storing in {c.name}.");
            List<ItemDrop.ItemData>? items = Player.m_localPlayer.GetInventory().GetAllItems();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                ItemDrop.ItemData? item = items[i];
                if (item.m_equiped)
                    continue;
                int originalAmount = item.m_stack;
                TryStoreItem(c, ref item);
                if (item.m_stack >= originalAmount) continue;
                total += originalAmount - item.m_stack;
                containers++;
                Player.m_localPlayer.GetInventory().RemoveItem(item, originalAmount - item.m_stack);
                OdinQOLplugin.QOLLogger.LogDebug(
                    $"Stored {originalAmount - item.m_stack} {item.m_shared.m_name} into {c.name}");
            }
        }

        if (total > 0)
            Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                $"Stored {total} items in {containers} containers");
    }
}