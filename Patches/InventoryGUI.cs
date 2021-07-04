using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace VMP_Mod.GameClasses
{

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.RepairOneItem))]
    public static class InventoryGui_RepairOneItem_Transpiler
    {
        private static MethodInfo method_EffectList_Create = AccessTools.Method(typeof(EffectList), nameof(EffectList.Create));
        private static MethodInfo method_CreateNoop = AccessTools.Method(typeof(InventoryGui_RepairOneItem_Transpiler), nameof(InventoryGui_RepairOneItem_Transpiler.CreateNoop));

        /// <summary>
        /// Patches out the code that spawns an effect for each item repaired - when we repair multiple items, we only want
        /// one effect, otherwise it looks and sounds bad. The patch for InventoryGui.UpdateRepair will spawn the effect instead.
        /// </summary>
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            

            List<CodeInstruction> il = instructions.ToList();

            if (VMP_Modplugin.AutoRepair.Value)
            {
                // We look for a call to EffectList::Create and replace it with our own noop stub.
                for (int i = 0; i < il.Count; ++i)
                {
                    if (il[i].Calls(method_EffectList_Create))
                    {
                        il[i].opcode = OpCodes.Call; // original is callvirt, so we need to tweak it
                        il[i].operand = method_CreateNoop;
                    }
                }
            }

            return il.AsEnumerable();
        }

        private static GameObject[] CreateNoop(Vector3 _0, Quaternion _1, Transform _2, float _3)
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRepair))]
    public static class InventoryGui_UpdateRepair_Patch
    {
        /// <summary>
        /// When we're in a state where the InventoryGui is open and we have items available to repair,
        /// and we have an active crafting station, this patch is responsible for repairing all items
        /// that can be repaired and then spawning one instance of the repair effect if at least one item
        /// has been repaired.
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(InventoryGui __instance)
        {
            if (!VMP_Modplugin.AutoRepair.Value) return;

            CraftingStation curr_crafting_station = Player.m_localPlayer.GetCurrentCraftingStation();

            if (curr_crafting_station != null)
            {
                int repair_count = 0;

                while (__instance.HaveRepairableItems())
                {
                    __instance.RepairOneItem();
                    ++repair_count;
                }

                if (repair_count > 0)
                {
                    curr_crafting_station.m_repairItemDoneEffects.Create(curr_crafting_station.transform.position, Quaternion.identity, null, 1.0f);
                }
            }
        }
    }
    
    /// <summary>
    /// Setting up deconstruct feature
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    public static class InventoryGui_Awake_Patch
    {
        private static void Postfix(ref InventoryGui __instance)
        {
            if (VMP_Modplugin.Deconstruct.Value)
            {
                Deconstruct.Setup(ref __instance);
            }
        }
    }

    /// <summary>
    /// Setting deconstruct tab state on update crafting panel
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateCraftingPanel))]
    public static class InventoryGui_UpdateCraftingPanel_Patch
    {
        private static void Postfix(ref InventoryGui __instance)
        {
            if (VMP_Modplugin.Deconstruct.Value)
            {
                Player localPlayer = Player.m_localPlayer;

                CraftingStation currentCraftingStation = localPlayer.GetCurrentCraftingStation();

                // don't show deconstruct tab if player isn't at a crafting station and doesn't have cheats enabled or is at a crafting station without deconstruct function
                if ((currentCraftingStation == null && !localPlayer.NoCostCheat()) || Deconstruct.nonDestructableCraftingStations.Any(x => x.Equals(currentCraftingStation?.m_name)))
                {
                    Deconstruct.m_tabDeconstruct.interactable = true;
                    Deconstruct.m_tabDeconstruct.gameObject.SetActive(false);
                }
                else
                {
                    Deconstruct.m_tabDeconstruct.gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// Updating recipe list for deconstruct if deconstruct is enabled
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipeList))]
    public static class InventoryGui_UpdateRecipeList_Patch
    {
        private static bool Prefix(ref InventoryGui __instance, List<Recipe> recipes)
        {
            if (VMP_Modplugin.Deconstruct.Value)
            {
                Player localPlayer = Player.m_localPlayer;

                // check for active tab
                if (__instance.m_tabCraft.interactable.Equals(true) && __instance.m_tabUpgrade.interactable.Equals(true) && localPlayer != null)
                {
                    Deconstruct.Deconstruct_UpdateRecipeList(ref localPlayer);
                    return false; // skip
                                  //
                                  //
                                  //
                                  //
                                  // g original update otherwise recipe list would be overwritten
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Updating recipe for deconstruct if deconstruct is enabled
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe))]
    public static class InventoryGui_UpdateRecipe_Patch
    {
        private static bool Prefix(ref InventoryGui __instance)
        {
            if (VMP_Modplugin.Deconstruct.Value)
            {
                Player localPlayer = Player.m_localPlayer;

                // check for active tab
                if (__instance.m_tabCraft.interactable.Equals(true) && __instance.m_tabUpgrade.interactable.Equals(true) && localPlayer != null)
                {
                    Deconstruct.Deconstruct_UpdateRecipe(ref localPlayer, Time.deltaTime);
                    return false; // skipping original update otherwise recipe would be overwritten
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Accounting for deconstruct tab in craft tab press
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCraftPressed))]
    public static class InventoryGui_OnCraftPressed_Patch
    {
        private static bool Prefix(ref InventoryGui __instance)
        {
            if (VMP_Modplugin.Deconstruct.Value)
            {
                if (Deconstruct.InDeconstructTab())
                {
                    Deconstruct.OnDeconstructPressed();
                    return false; // skipping original so that we only run deconstruct button function
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Accounting for deconstruct tab in craft tab press
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTabCraftPressed))]
    public static class InventoryGui_OnTabCraftPressed_Patch
    {
        private static void Prefix(ref InventoryGui __instance)
        {
            if (VMP_Modplugin.Deconstruct.Value)
            {
                Deconstruct.SetDeconstructTab(true);
            }
        }
    }

    /// <summary>
    /// Accounting for deconstruct tab in upgrade tab press
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTabUpgradePressed))]
    public static class InventoryGui_OnTabUpgradePressed_Patch
    {
        private static void Prefix(ref InventoryGui __instance)
        {
            if (VMP_Modplugin.Deconstruct.Value)
            {
                Deconstruct.SetDeconstructTab(true);
            }
        }
    }

    public static class Inventory_NearbyChests_Cache
    {
        public static List<Container> chests = new List<Container>();
        public static readonly Stopwatch delta = new Stopwatch();
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
    public static class InventoryGui_SetupRequirement_Patch
    {
        private static bool Prefix(Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality, ref bool __result)
        {
            Image component = elementRoot.transform.Find("res_icon").GetComponent<Image>();
            Text component2 = elementRoot.transform.Find("res_name").GetComponent<Text>();
            Text component3 = elementRoot.transform.Find("res_amount").GetComponent<Text>();
            UITooltip component4 = elementRoot.GetComponent<UITooltip>();

            if (req.m_resItem != null)
            {
                component.gameObject.SetActive(true);
                component2.gameObject.SetActive(true);
                component3.gameObject.SetActive(true);
                component.sprite = req.m_resItem.m_itemData.GetIcon();
                component.color = Color.white;
                component4.m_text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
                component2.text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
                int num = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);
                int amount = req.GetAmount(quality);

                if (amount <= 0)
                {
                    InventoryGui.HideRequirement(elementRoot);
                    __result = false;
                    return false;
                }

               
                    Stopwatch delta;

                    GameObject pos = player.GetCurrentCraftingStation()?.gameObject;
                    if (!pos)
                    {
                        pos = player.gameObject;
                        delta = Inventory_NearbyChests_Cache.delta;
                    }
                    else
                        delta = GameObjectAssistant.GetStopwatch(pos);

                    int lookupInterval = 5 * 1000;
                    if (!delta.IsRunning || delta.ElapsedMilliseconds > lookupInterval)
                    {
                        Inventory_NearbyChests_Cache.chests = InventoryAssistant.GetNearbyChests(pos, 30);
                        delta.Restart();
                    }
                    num += InventoryAssistant.GetItemAmountInItemList(InventoryAssistant.GetNearbyChestItemsByContainerList(Inventory_NearbyChests_Cache.chests), req.m_resItem.m_itemData);
                

                component3.text = num + "/" + amount.ToString();

                if (num < amount)
                {
                    component3.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : Color.white);
                }
                else
                {
                    component3.color = Color.white;
                }

                component3.fontSize = 14;
                if (component3.text.Length > 5)
                {
                    component3.fontSize -= component3.text.Length - 5;
                }
            }

            __result = true;
            return false;
        }
    }
}
