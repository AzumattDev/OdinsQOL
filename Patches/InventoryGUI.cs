using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    /*[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.RepairOneItem))]
    public static class InventoryGui_RepairOneItem_Transpiler
    {
        private static readonly MethodInfo method_EffectList_Create =
            AccessTools.Method(typeof(EffectList), nameof(EffectList.Create));

        private static readonly MethodInfo method_CreateNoop =
            AccessTools.Method(typeof(InventoryGui_RepairOneItem_Transpiler), nameof(CreateNoop));

        /// <summary>
        ///     Patches out the code that spawns an effect for each item repaired - when we repair multiple items, we only want
        ///     one effect, otherwise it looks and sounds bad. The patch for InventoryGui.UpdateRepair will spawn the effect
        ///     instead.
        /// </summary>
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            var il = instructions.ToList();

            if (OdinQOLplugin.AutoRepair.Value)
                // We look for a call to EffectList::Create and replace it with our own noop stub.
                for (var i = 0; i < il.Count; ++i)
                    if (il[i].Calls(method_EffectList_Create))
                    {
                        il[i].opcode = OpCodes.Call; // original is callvirt, so we need to tweak it
                        il[i].operand = method_CreateNoop;
                    }

            return il.AsEnumerable();
        }

        private static GameObject[] CreateNoop(Vector3 _0, Quaternion _1, Transform _2, float _3)
        {
            return null;
        }
    }*/

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

            CraftingStation? curr_crafting_station = Player.m_localPlayer.GetCurrentCraftingStation();

            if (curr_crafting_station != null)
            {
                int repair_count = 0;

                while (__instance.HaveRepairableItems())
                {
                    __instance.RepairOneItem();
                    ++repair_count;
                }

                if (repair_count > 0)
                    curr_crafting_station.m_repairItemDoneEffects.Create(curr_crafting_station.transform.position,
                        Quaternion.identity);
            }
        }
    }
    /* 
      /// <summary>
      /// Setting up deconstruct feature
      /// </summary>
      [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
      public static class InventoryGui_Awake_Patch
      {
          private static void Postfix(ref InventoryGui __instance)
          {
              if (OdinQOLplugin.Deconstruct.Value)
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
              if (OdinQOLplugin.Deconstruct.Value)
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
              if (OdinQOLplugin.Deconstruct.Value)
              {
                  Player localPlayer = Player.m_localPlayer;
  
                  // check for active tab
                  if (__instance.m_tabCraft.interactable.Equals(true) && __instance.m_tabUpgrade.interactable.Equals(true) && localPlayer != null)
                  {
                      Deconstruct.Deconstruct_UpdateRecipeList(ref localPlayer);
                      return false; // skip original update otherwise recipe list would be overwritten
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
              if (OdinQOLplugin.Deconstruct.Value)
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
              if (OdinQOLplugin.Deconstruct.Value)
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
              if (OdinQOLplugin.Deconstruct.Value)
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
              if (OdinQOLplugin.Deconstruct.Value)
              {
                  Deconstruct.SetDeconstructTab(true);
              }
          }
      }*/
}