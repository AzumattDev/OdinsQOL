using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    public class InventoryDiscard
    {
        public static ConfigEntry<KeyboardShortcut> hotKey;
        public static ConfigEntry<bool> discardInvEnabled;
        public static ConfigEntry<bool> returnUnknownResources;
        public static ConfigEntry<bool> returnEnchantedResources;
        public static ConfigEntry<float> returnResources;

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateItemDrag))]
        private static class UpdateItemDrag_Patch
        {
            private static void Postfix(InventoryGui __instance, ItemDrop.ItemData ___m_dragItem,
                Inventory ___m_dragInventory, int ___m_dragAmount, ref GameObject ___m_dragGo)
            {
                if (!discardInvEnabled.Value || !hotKey.Value.IsDown() || ___m_dragItem == null ||
                    !___m_dragInventory.ContainsItem(___m_dragItem))
                    return;

                OdinQOLplugin.QOLLogger.LogDebug(
                    $"Discarding {___m_dragAmount}/{___m_dragItem.m_stack} {___m_dragItem.m_dropPrefab.name}");

                if (returnResources.Value > 0)
                {
                    Recipe recipe = ObjectDB.instance.GetRecipe(___m_dragItem);

                    if (recipe != null && (returnUnknownResources.Value ||
                                           Player.m_localPlayer.IsRecipeKnown(___m_dragItem.m_shared.m_name)))
                    {
                        OdinQOLplugin.QOLLogger.LogDebug(
                            $"Recipe stack: {recipe.m_amount} num of stacks: {___m_dragAmount / recipe.m_amount}");


                        List<Piece.Requirement>? reqs = recipe.m_resources.ToList();

                        bool isMagic = false;
                        bool cancel = false;
                        if (OdinQOLplugin.epicLootAssembly != null && returnEnchantedResources.Value)
                            isMagic = (bool)OdinQOLplugin.epicLootAssembly.GetType("EpicLoot.ItemDataExtensions")
                                .GetMethod("IsMagic", BindingFlags.Public | BindingFlags.Static, null,
                                    new[] { typeof(ItemDrop.ItemData) }, null)?.Invoke(null, new[] { ___m_dragItem });
                        if (isMagic)
                        {
                            int rarity = (int)OdinQOLplugin.epicLootAssembly?.GetType("EpicLoot.ItemDataExtensions")
                                .GetMethod("GetRarity", BindingFlags.Public | BindingFlags.Static)
                                ?.Invoke(null, new[] { ___m_dragItem });
                            List<KeyValuePair<ItemDrop, int>> magicReqs =
                                (List<KeyValuePair<ItemDrop, int>>)OdinQOLplugin.epicLootAssembly
                                    ?.GetType("EpicLoot.Crafting.EnchantTabController")
                                    .GetMethod("GetEnchantCosts", BindingFlags.Public | BindingFlags.Static)
                                    ?.Invoke(null, new object[] { ___m_dragItem, rarity });
                            foreach (KeyValuePair<ItemDrop, int> kvp in magicReqs)
                            {
                                if (!returnUnknownResources.Value &&
                                    (ObjectDB.instance.GetRecipe(kvp.Key.m_itemData) &&
                                     !Player.m_localPlayer.IsRecipeKnown(kvp.Key.m_itemData.m_shared.m_name) ||
                                     !Player.m_localPlayer.m_knownMaterial.Contains(kvp.Key.m_itemData.m_shared
                                         .m_name)))
                                {
                                    Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                                        "You don't know all the recipes for this item's materials.");
                                    return;
                                }

                                reqs.Add(new Piece.Requirement
                                {
                                    m_amount = kvp.Value,
                                    m_resItem = kvp.Key
                                });
                            }
                        }

                        if (!cancel && ___m_dragAmount / recipe.m_amount > 0)
                            for (int i = 0; i < ___m_dragAmount / recipe.m_amount; i++)
                                foreach (Piece.Requirement req in reqs)
                                {
                                    int quality = ___m_dragItem.m_quality;
                                    for (int j = quality; j > 0; j--)
                                    {
                                        GameObject prefab = ObjectDB.instance.m_items.FirstOrDefault(item =>
                                            item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name ==
                                            req.m_resItem.m_itemData.m_shared.m_name)!;
                                        ItemDrop.ItemData newItem = prefab.GetComponent<ItemDrop>().m_itemData.Clone();
                                        int numToAdd = Mathf.RoundToInt(req.GetAmount(j) * returnResources.Value);
                                        OdinQOLplugin.QOLLogger.LogDebug(
                                            $"Returning {numToAdd}/{req.GetAmount(j)} {prefab.name}");
                                        while (numToAdd > 0)
                                        {
                                            int stack = Mathf.Min(req.m_resItem.m_itemData.m_shared.m_maxStackSize,
                                                numToAdd);
                                            numToAdd -= stack;

                                            if (Player.m_localPlayer.GetInventory().AddItem(prefab.name, stack,
                                                    req.m_resItem.m_itemData.m_quality,
                                                    req.m_resItem.m_itemData.m_variant,
                                                    0, "") == null)
                                            {
                                                Transform transform;
                                                ItemDrop component = Object.Instantiate(prefab,
                                                    (transform = Player.m_localPlayer.transform).position +
                                                    transform.forward +
                                                    transform.up,
                                                    transform.rotation).GetComponent<ItemDrop>();
                                                component.m_itemData = newItem;
                                                component.m_itemData.m_dropPrefab = prefab;
                                                component.m_itemData.m_stack = stack;
                                                component.Save();
                                            }
                                        }
                                    }
                                }
                    }
                }

                if (___m_dragAmount == ___m_dragItem.m_stack)
                {
                    Player.m_localPlayer.RemoveFromEquipQueue(___m_dragItem);
                    Player.m_localPlayer.UnequipItem(___m_dragItem, false);
                    ___m_dragInventory.RemoveItem(___m_dragItem);
                }
                else
                {
                    ___m_dragInventory.RemoveItem(___m_dragItem, ___m_dragAmount);
                }

                Object.Destroy(___m_dragGo);
                ___m_dragGo = null;
                __instance.UpdateCraftingPanel();
            }
        }
    }
}