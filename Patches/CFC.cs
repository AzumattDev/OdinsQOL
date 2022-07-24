using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace OdinQOL.Patches;

public class CFC
{
    // Container Patches
    [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
    static class ContainerAwakePatch
    {
        static void Postfix(Container __instance, ZNetView ___m_nview)
        {
            if (CFCEnabled.Value)
                Utilities.AddContainer(__instance, ___m_nview);
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.OnDestroyed))]
    static class ContainerOnDestroyedPatch
    {
        static void Prefix(Container __instance)
        {
            if (CFCEnabled.Value)
                OdinQOLplugin.ContainerList.Remove(__instance);
        }
    }


    // Cooking Station Patches
    [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnAddFuelSwitch))]
    static class CookingStationOnAddFuelSwitchPatch
    {
        static bool Prefix(CookingStation __instance, ref bool __result, Humanoid user, ItemDrop.ItemData item,
            ZNetView ___m_nview)
        {
            OdinQOLplugin.QOLLogger.LogDebug($"(CookingStationOnAddFuelSwitchPatch) Looking for fuel");

            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || !Utilities.AllowByKey() || item != null ||
                __instance.GetFuel() > __instance.m_maxFuel - 1 ||
                user.GetInventory().HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name))
                return true;

            OdinQOLplugin.QOLLogger.LogDebug(
                $"(CookingStationOnAddFuelSwitchPatch) Missing fuel in player inventory");


            List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);

            foreach (Container c in nearbyContainers)
            {
                ItemDrop.ItemData fuelItem = c.GetInventory().GetItem(__instance.m_fuelItem.m_itemData.m_shared.m_name);
                if (fuelItem == null) continue;
                if (((IList)CFCFuelDisallowTypes.Value.Split(',')).Contains(fuelItem.m_dropPrefab.name))
                {
                    OdinQOLplugin.QOLLogger.LogDebug(
                        $"(CookingStationOnAddFuelSwitchPatch) Container at {c.transform.position} has {fuelItem.m_stack} {fuelItem.m_dropPrefab.name} but it's forbidden by config");
                    continue;
                }

                OdinQOLplugin.QOLLogger.LogDebug(
                    $"(CookingStationOnAddFuelSwitchPatch) Container at {c.transform.position} has {fuelItem.m_stack} {fuelItem.m_dropPrefab.name}, taking one");
                c.GetInventory().RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, 1);
                c.Save();
                //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });
                user.Message(MessageHud.MessageType.Center,
                    "$msg_added " + __instance.m_fuelItem.m_itemData.m_shared.m_name);
                ___m_nview.InvokeRPC("AddFuel", Array.Empty<object>());
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.FindCookableItem))]
    static class CookingStationFindCookableItemPatch
    {
        static void Postfix(CookingStation __instance, ref ItemDrop.ItemData __result)
        {
            OdinQOLplugin.QOLLogger.LogDebug($"(CookingStationFindCookableItemPatch) Looking for cookable");

            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || !Utilities.AllowByKey() || __result != null ||
                (__instance.m_requireFire && !__instance.IsFireLit() || __instance.GetFreeSlot() == -1))
                return;

            OdinQOLplugin.QOLLogger.LogDebug(
                $"(CookingStationFindCookableItemPatch) Missing cookable in player inventory");


            List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);

            foreach (CookingStation.ItemConversion itemConversion in __instance.m_conversion)
            {
                foreach (Container c in nearbyContainers)
                {
                    ItemDrop.ItemData item = c.GetInventory().GetItem(itemConversion.m_from.m_itemData.m_shared.m_name);
                    if (item == null) continue;
                    if (((IList)CFCOreDisallowTypes.Value.Split(',')).Contains(item.m_dropPrefab.name))
                    {
                        OdinQOLplugin.QOLLogger.LogDebug(
                            $"(CookingStationFindCookableItemPatch) Container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name} but it's forbidden by config");
                        continue;
                    }

                    OdinQOLplugin.QOLLogger.LogDebug(
                        $"(CookingStationFindCookableItemPatch) Container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name}, taking one");
                    __result = item;
                    c.GetInventory().RemoveItem(itemConversion.m_from.m_itemData.m_shared.m_name, 1);
                    c.Save();
                    //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });
                    return;
                }
            }
        }
    }

    // Fireplace Patches

    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Interact))]
    static class FireplaceInteractPatch
    {
        static bool Prefix(Fireplace __instance, Humanoid user, bool hold, ref bool __result, ZNetView ___m_nview)
        {
            __result = true;
            bool pullAll = Input.GetKey(fillAllModKey.Value.MainKey); // Used to be fillAllModKey.Value.IsPressed(); something is wrong with KeyboardShortcuts always returning false
            Inventory inventory = user.GetInventory();

            if (!Utilities.AllowByKey() || hold || inventory == null ||
                (inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name) && !pullAll))
                return true;

            if (!___m_nview.HasOwner())
            {
                ___m_nview.ClaimOwnership();
            }


            if (pullAll && inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name))
            {
                int amount =
                    (int)Mathf.Min(__instance.m_maxFuel - Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel")),
                        inventory.CountItems(__instance.m_fuelItem.m_itemData.m_shared.m_name));
                inventory.RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                inventory.Changed();
                for (int i = 0; i < amount; i++)
                    ___m_nview.InvokeRPC("AddFuel");

                user.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$msg_fireadding",
                        __instance.m_fuelItem.m_itemData.m_shared.m_name));

                __result = false;
            }

            if (!inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name) &&
                Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel")) < __instance.m_maxFuel)
            {
                List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);

                foreach (Container c in nearbyContainers)
                {
                    ItemDrop.ItemData item = c.GetInventory().GetItem(__instance.m_fuelItem.m_itemData.m_shared.m_name);
                    if (item != null && Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel")) < __instance.m_maxFuel)
                    {
                        if (((IList)CFCFuelDisallowTypes.Value.Split(',')).Contains(item.m_dropPrefab
                                .name))
                        {
                            OdinQOLplugin.QOLLogger.LogDebug(
                                $"(FireplaceInteractPatch) Container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name} but it's forbidden by config");
                            continue;
                        }

                        int amount = pullAll
                            ? (int)Mathf.Min(
                                __instance.m_maxFuel - Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel")),
                                item.m_stack)
                            : 1;
                        OdinQOLplugin.QOLLogger.LogDebug($"Pull ALL is {pullAll}");
                        OdinQOLplugin.QOLLogger.LogDebug(
                            $"(FireplaceInteractPatch) Container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name}, taking {amount}");

                        c.GetInventory().RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                        c.Save();
                        //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });

                        if (__result)
                            user.Message(MessageHud.MessageType.Center,
                                Localization.instance.Localize("$msg_fireadding",
                                    __instance.m_fuelItem.m_itemData.m_shared.m_name));

                        for (int i = 0; i < amount; i++)
                            ___m_nview.InvokeRPC("AddFuel");

                        __result = false;

                        if (!pullAll || Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel")) >= __instance.m_maxFuel)
                            return false;
                    }
                }
            }

            return __result;
        }
    }


    // Inventory Patches

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
    static class InventoryGuiUpdatePatch
    {
        static void Prefix(InventoryGui __instance, Animator ___m_animator)
        {
            if (!CFCEnabled.Value) return;
            if (Player.m_localPlayer && CfcWasAllowed != Utilities.AllowByKey() &&
                ___m_animator.GetBool("visible"))
            {
                __instance.UpdateCraftingPanel();
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
    static class InventoryGuiSetupRequirementPatch
    {
        static void Postfix(InventoryGui __instance, Transform elementRoot, Piece.Requirement req, Player player,
            bool craft, int quality)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || !Utilities.AllowByKey())
                return;
            if (req.m_resItem == null) return;
            int invAmount = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);
            int amount = req.GetAmount(quality);
            if (amount <= 0)
            {
                return;
            }

            Text text = elementRoot.transform.Find("res_amount").GetComponent<Text>();
            if (invAmount < amount)
            {
                List<Container> nearbyContainers =
                    Utilities.GetNearbyContainers(Player.m_localPlayer.transform.position);
                invAmount +=
                    nearbyContainers.Sum(c => c.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name));

                if (invAmount >= amount)
                    text.color = ((Mathf.Sin(Time.time * 10f) > 0f)
                        ? flashColor.Value
                        : unFlashColor.Value);
            }

            text.text = resourceString.Value.Trim().Length > 0
                ? string.Format(resourceString.Value, invAmount, amount)
                : amount.ToString();
            text.resizeTextForBestFit = true;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCraftPressed))]
    static class InventoryGuiOnCraftPressedPatch
    {
        static bool Prefix(InventoryGui __instance, KeyValuePair<Recipe, ItemDrop.ItemData> ___m_selectedRecipe,
            ItemDrop.ItemData ___m_craftUpgradeItem)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || !Utilities.AllowByKey() ||
                !pullItemsKey.Value.IsPressed() || ___m_selectedRecipe.Key == null)
                return true;

            int qualityLevel = (___m_craftUpgradeItem != null) ? (___m_craftUpgradeItem.m_quality + 1) : 1;
            if (qualityLevel > ___m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_maxQuality)
            {
                return true;
            }

            OdinQOLplugin.QOLLogger.LogDebug(
                $"(InventoryGuiOnCraftPressedPatch) Pulling resources to player inventory for crafting item {___m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_name}");
            Utilities.PullResources(Player.m_localPlayer, ___m_selectedRecipe.Key.m_resources, qualityLevel);

            return false;
        }
    }


    // Player Patches
    [HarmonyPatch]
    public static class HaveRequirementsPatch
    {
        public static MethodBase TargetMethod() => AccessTools.DeclaredMethod(typeof(Player),
            nameof(Player.HaveRequirements), new[] { typeof(Piece.Requirement[]), typeof(bool), typeof(int) });

        static void Postfix(Player __instance, ref bool __result, Piece.Requirement[] resources, bool discover,
            int qualityLevel, HashSet<string> ___m_knownMaterial)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || __result || discover ||
                !Utilities.AllowByKey())
                return;
            List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);
            foreach (Piece.Requirement requirement in resources)
            {
                if (!requirement.m_resItem) continue;
                int amount = requirement.GetAmount(qualityLevel);
                int invAmount = __instance.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                if (invAmount >= amount) continue;
                invAmount += nearbyContainers.Sum(c =>
                    c.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name));
                if (invAmount < amount)
                    return;
            }

            __result = true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements), typeof(Piece), typeof(Player.RequirementMode))]
    static class HaveRequirementsPatch2
    {
        static void Postfix(Player __instance, ref bool __result, Piece piece, Player.RequirementMode mode,
            HashSet<string> ___m_knownMaterial, Dictionary<string, int> ___m_knownStations)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || __result ||
                __instance?.transform?.position == null ||
                !Utilities.AllowByKey())
                return;

            if (piece.m_craftingStation)
            {
                if (mode is Player.RequirementMode.IsKnown or Player.RequirementMode.CanAlmostBuild)
                {
                    if (!___m_knownStations.ContainsKey(piece.m_craftingStation.m_name))
                    {
                        return;
                    }
                }
                else if (!CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name,
                             __instance.transform.position))
                {
                    return;
                }
            }

            if (piece.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(piece.m_dlc))
            {
                return;
            }

            List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);

            foreach (Piece.Requirement requirement in piece.m_resources)
            {
                if (requirement.m_resItem && requirement.m_amount > 0)
                {
                    switch (mode)
                    {
                        case Player.RequirementMode.IsKnown
                            when !___m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name):
                            return;
                        case Player.RequirementMode.CanAlmostBuild when __instance.GetInventory()
                            .HaveItem(requirement.m_resItem.m_itemData.m_shared.m_name):
                            continue;
                        case Player.RequirementMode.CanAlmostBuild:
                        {
                            bool hasItem = nearbyContainers.Any(c =>
                                c.GetInventory().HaveItem(requirement.m_resItem.m_itemData.m_shared.m_name));

                            if (!hasItem)
                                return;
                            break;
                        }
                        case Player.RequirementMode.CanBuild
                            when __instance.GetInventory()
                                     .CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) <
                                 requirement.m_amount:
                        {
                            int hasItems = __instance.GetInventory()
                                .CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                            foreach (Container c in nearbyContainers)
                            {
                                try
                                {
                                    hasItems += c.GetInventory()
                                        .CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                                    if (hasItems >= requirement.m_amount)
                                    {
                                        break;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                            if (hasItems < requirement.m_amount)
                                return;
                            break;
                        }
                    }
                }
            }

            __result = true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources))]
    static class ConsumeResourcesPatch
    {
        static bool Prefix(Player __instance, Piece.Requirement[] requirements, int qualityLevel)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || !Utilities.AllowByKey())
                return true;

            Inventory pInventory = __instance.GetInventory();
            List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);
            foreach (Piece.Requirement requirement in requirements)
            {
                if (!requirement.m_resItem) continue;
                int totalRequirement = requirement.GetAmount(qualityLevel);
                if (totalRequirement <= 0)
                    continue;

                string reqName = requirement.m_resItem.m_itemData.m_shared.m_name;
                int totalAmount = pInventory.CountItems(reqName);
                OdinQOLplugin.QOLLogger.LogDebug(
                    $"(ConsumeResourcesPatch) Have {totalAmount}/{totalRequirement} {reqName} in player inventory");
                pInventory.RemoveItem(reqName, Math.Min(totalAmount, totalRequirement));

                if (totalAmount >= totalRequirement) continue;
                foreach (Container c in nearbyContainers)
                {
                    Inventory cInventory = c.GetInventory();
                    int thisAmount = Mathf.Min(cInventory.CountItems(reqName), totalRequirement - totalAmount);

                    OdinQOLplugin.QOLLogger.LogDebug(
                        $"(ConsumeResourcesPatch) Container at {c.transform.position} has {cInventory.CountItems(reqName)}");

                    if (thisAmount == 0)
                        continue;


                    for (int i = 0; i < cInventory.GetAllItems().Count; i++)
                    {
                        ItemDrop.ItemData item = cInventory.GetItem(i);
                        if (item.m_shared.m_name != reqName) continue;
                        OdinQOLplugin.QOLLogger.LogDebug(
                            $"(ConsumeResourcesPatch) Got stack of {item.m_stack} {reqName}");
                        int stackAmount = Mathf.Min(item.m_stack, totalRequirement - totalAmount);
                        if (stackAmount == item.m_stack)
                            cInventory.RemoveItem(item);
                        else
                            item.m_stack -= stackAmount;

                        totalAmount += stackAmount;
                        OdinQOLplugin.QOLLogger.LogDebug(
                            $"(ConsumeResourcesPatch) Total amount is now {totalAmount}/{totalRequirement} {reqName}");

                        if (totalAmount >= totalRequirement)
                            break;
                    }

                    c.Save();
                    cInventory.Changed();

                    if (totalAmount < totalRequirement) continue;
                    OdinQOLplugin.QOLLogger.LogDebug($"(ConsumeResourcesPatch) Consumed enough {reqName}");
                    break;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    static class UpdatePlacementGhostPatch
    {
        static void Postfix(Player __instance, bool flashGuardStone)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || !showGhostConnections.Value)
            {
                return;
            }


            GameObject? placementGhost =
                __instance.m_placementGhost != null ? __instance.m_placementGhost : null;
            if (placementGhost == null)
            {
                return;
            }

            Container ghostContainer = placementGhost.GetComponent<Container>();
            if (ghostContainer == null)
            {
                return;
            }

            if (OdinQOLplugin.ConnectionVfxPrefab == null)
            {
                return;
            }

            if (CraftingStation.m_allStations == null) return;
            bool bAddedConnections = false;
            foreach (CraftingStation station in CraftingStation.m_allStations)
            {
                int connectionIndex = Utilities.ConnectionExists(station);
                bool connectionAlreadyExists = connectionIndex != -1;

                if (Vector3.Distance(station.transform.position, placementGhost.transform.position) <
                    mRange.Value)
                {
                    bAddedConnections = true;

                    Vector3 connectionStartPos = station.GetConnectionEffectPoint();
                    Vector3 connectionEndPos = placementGhost.transform.position +
                                               Vector3.up * ghostConnectionStartOffset.Value;

                    ConnectionParams tempConnection;
                    if (!connectionAlreadyExists)
                    {
                        tempConnection = new ConnectionParams
                        {
                            StationPos = station.GetConnectionEffectPoint(),
                            Connection = UnityEngine.Object.Instantiate(
                                OdinQOLplugin.ConnectionVfxPrefab,
                                connectionStartPos, Quaternion.identity)
                        };
                    }
                    else
                    {
                        tempConnection = OdinQOLplugin.ContainerConnections[connectionIndex];
                    }

                    if (tempConnection.Connection != null)
                    {
                        Vector3 vector3 = connectionEndPos - connectionStartPos;
                        Quaternion quaternion = Quaternion.LookRotation(vector3.normalized);
                        tempConnection.Connection.transform.position = connectionStartPos;
                        tempConnection.Connection.transform.rotation = quaternion;
                        tempConnection.Connection.transform.localScale = new Vector3(1f, 1f, vector3.magnitude);
                    }

                    if (!connectionAlreadyExists)
                    {
                        OdinQOLplugin.ContainerConnections.Add(tempConnection);
                    }
                }
                else if (connectionAlreadyExists)
                {
                    UnityEngine.Object.Destroy(
                        OdinQOLplugin.ContainerConnections[connectionIndex].Connection);
                    OdinQOLplugin.ContainerConnections.RemoveAt(connectionIndex);
                }
            }

            if (!bAddedConnections || OdinQOLplugin.context == null) return;
            OdinQOLplugin.context.CancelInvoke("StopConnectionEffects");
            OdinQOLplugin.context.Invoke("StopConnectionEffects",
                ghostConnectionRemovalDelay.Value);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
    static class UpdatePlacementPatch
    {
        static bool Prefix(Player __instance, bool takeInput, float dt, PieceTable ___m_buildPieces,
            GameObject ___m_placementGhost)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || !Utilities.AllowByKey() ||
                !pullItemsKey.Value.IsPressed() || !__instance.InPlaceMode() ||
                !takeInput || Hud.IsPieceSelectionVisible())
            {
                return true;
            }

            if (!ZInput.GetButtonDown("Attack") && !ZInput.GetButtonDown("JoyPlace")) return true;
            Piece selectedPiece = ___m_buildPieces.GetSelectedPiece();
            if (selectedPiece == null) return false;
            if (selectedPiece.m_repairPiece)
                return true;
            if (___m_placementGhost == null) return false;
            Player.PlacementStatus placementStatus = __instance.m_placementStatus;
            if (placementStatus != 0) return false;
            OdinQOLplugin.QOLLogger.LogDebug(
                $"(UpdatePlacementPatch) Pulling resources to player inventory for piece {selectedPiece.name}");
            Utilities.PullResources(__instance, selectedPiece.m_resources, 0);

            return false;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.InPlaceMode))]
    static class Player_InPlaceMode_Patch
    {
        static void Postfix(Player __instance, ref bool __result)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value) return;
            if (__result == false)
            {
                Utilities.StopConnectionEffects();
            }
        }
    }

    // Smelter Patches

    [HarmonyPatch(typeof(Smelter), nameof(Smelter.UpdateHoverTexts))]
    static class SmelterUpdateHoverTextsPatch
    {
        static void Postfix(Smelter __instance)
        {
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value)
                return;

            if (fillAllModKey.Value.MainKey == KeyCode.None) return;
            if (__instance.m_addWoodSwitch)
                __instance.m_addWoodSwitch.m_hoverText +=
                    $"\n[<color=yellow><b>{fillAllModKey.Value} + $KEY_Use</b></color>] $piece_smelter_add (Inventory & Nearby Containers)";
            if (!__instance.m_addOreSwitch)
                return;
            Switch addOreSwitch = __instance.m_addOreSwitch;
            addOreSwitch.m_hoverText +=
                $"\n[<color=yellow><b>{fillAllModKey.Value} + $KEY_Use</b></color>] {__instance.m_addOreTooltip} (Inventory & Nearby Containers)";
        }
    }

    [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddOre))]
    static class SmelterOnAddOrePatch
    {
        static bool Prefix(Smelter __instance, Humanoid user, ItemDrop.ItemData item, ZNetView ___m_nview)
        {
            bool pullAll = Input.GetKey(fillAllModKey.Value.MainKey); // Used to be fillAllModKey.Value.IsPressed(); something is wrong with KeyboardShortcuts always returning false
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || (!Utilities.AllowByKey() && !pullAll) ||
                item != null ||
                __instance.GetQueueSize() >= __instance.m_maxOre)
                return true;

            Inventory inventory = user.GetInventory();


            if (__instance.m_conversion.Any(itemConversion =>
                    inventory.HaveItem(itemConversion.m_from.m_itemData.m_shared.m_name) && !pullAll))
            {
                return true;
            }

            Dictionary<string, int> added = new();

            List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);

            foreach (Smelter.ItemConversion itemConversion in __instance.m_conversion)
            {
                if (__instance.GetQueueSize() >= __instance.m_maxOre ||
                    (added.Any() && !pullAll))
                    break;

                string name = itemConversion.m_from.m_itemData.m_shared.m_name;
                if (pullAll && inventory.HaveItem(name))
                {
                    ItemDrop.ItemData newItem = inventory.GetItem(name);

                    if (CFCOreDisallowTypes.Value.Split(',').Contains(newItem.m_dropPrefab.name))
                    {
                        OdinQOLplugin.QOLLogger.LogDebug(
                            $"(SmelterOnAddOrePatch) Player has {newItem.m_stack} {newItem.m_dropPrefab.name} but it's forbidden by config");
                        continue;
                    }

                    int amount = pullAll
                        ? Mathf.Min(
                            __instance.m_maxOre - __instance.GetQueueSize(),
                            inventory.CountItems(name))
                        : 1;

                    if (!added.ContainsKey(name))
                        added[name] = 0;
                    added[name] += amount;

                    inventory.RemoveItem(itemConversion.m_from.m_itemData.m_shared.m_name, amount);
                    //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(inventory, new object[] { });

                    for (int i = 0; i < amount; i++)
                        ___m_nview.InvokeRPC("AddOre", newItem.m_dropPrefab.name);

                    user.Message(MessageHud.MessageType.TopLeft, $"$msg_added {amount} {name}");
                    if (__instance.GetQueueSize() >= __instance.m_maxOre)
                        break;
                }

                foreach (Container c in nearbyContainers)
                {
                    ItemDrop.ItemData newItem = c.GetInventory().GetItem(name);
                    if (newItem == null) continue;
                    if (CFCOreDisallowTypes.Value.Split(',').Contains(newItem.m_dropPrefab.name))
                    {
                        OdinQOLplugin.QOLLogger.LogDebug(
                            $"(SmelterOnAddOrePatch) Container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name} but it's forbidden by config");
                        continue;
                    }

                    int amount = pullAll
                        ? Mathf.Min(
                            __instance.m_maxOre - __instance.GetQueueSize(),
                            c.GetInventory().CountItems(name))
                        : 1;

                    if (!added.ContainsKey(name))
                        added[name] = 0;
                    added[name] += amount;
                    OdinQOLplugin.QOLLogger.LogError($"Pull ALL is {pullAll}");
                    OdinQOLplugin.QOLLogger.LogDebug(
                        $"(SmelterOnAddOrePatch) Container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name}, taking {amount}");

                    c.GetInventory().RemoveItem(name, amount);
                    c.Save();
                    //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });

                    for (int i = 0; i < amount; i++)
                        ___m_nview.InvokeRPC("AddOre", newItem.m_dropPrefab.name);

                    user.Message(MessageHud.MessageType.TopLeft, $"$msg_added {amount} {name}");

                    if (__instance.GetQueueSize() >= __instance.m_maxOre ||
                        !pullAll)
                        break;
                }
            }

            if (!added.Any())
                user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems");
            else
            {
                List<string> outAdded = added.Select(kvp => $"$msg_added {kvp.Value} {kvp.Key}").ToList();

                user.Message(MessageHud.MessageType.Center, string.Join("\n", outAdded));
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddFuel))]
    static class SmelterOnAddFuelPatch
    {
        static bool Prefix(Smelter __instance, ref bool __result, ZNetView ___m_nview, Humanoid user,
            ItemDrop.ItemData item)
        {
            bool pullAll = Input.GetKey(fillAllModKey.Value.MainKey); // Used to be fillAllModKey.Value.IsPressed(); something is wrong with KeyboardShortcuts always returning false
            Inventory inventory = user.GetInventory();
            if (!CFCEnabled.Value || !OdinQOLplugin.ModEnabled.Value || (!Utilities.AllowByKey() && !pullAll) ||
                item != null ||
                inventory == null ||
                (inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name) && !pullAll))
                return true;

            __result = true;

            int added = 0;

            if (__instance.GetFuel() > __instance.m_maxFuel - 1)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                __result = false;
                return false;
            }

            if (pullAll && inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name))
            {
                int amount = (int)Mathf.Min(
                    __instance.m_maxFuel - __instance.GetFuel(),
                    inventory.CountItems(__instance.m_fuelItem.m_itemData.m_shared.m_name));
                inventory.RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(inventory, new object[] { });
                for (int i = 0; i < amount; i++)
                    ___m_nview.InvokeRPC("AddFuel");

                added += amount;

                user.Message(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize("$msg_fireadding",
                        __instance.m_fuelItem.m_itemData.m_shared.m_name));

                __result = false;
            }

            List<Container> nearbyContainers = Utilities.GetNearbyContainers(__instance.transform.position);

            foreach (Container c in nearbyContainers)
            {
                ItemDrop.ItemData newItem = c.GetInventory().GetItem(__instance.m_fuelItem.m_itemData.m_shared.m_name);
                if (newItem == null) continue;
                if (CFCFuelDisallowTypes.Value.Split(',').Contains(newItem.m_dropPrefab.name))
                {
                    OdinQOLplugin.QOLLogger.LogDebug(
                        $"(SmelterOnAddFuelPatch) Container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name} but it's forbidden by config");
                    continue;
                }

                OdinQOLplugin.QOLLogger.LogError($"Pull ALL is {pullAll}");
                int amount = pullAll
                    ? (int)Mathf.Min(
                        __instance.m_maxFuel - __instance.GetFuel(), newItem.m_stack)
                    : 1;

                OdinQOLplugin.QOLLogger.LogDebug(
                    $"(SmelterOnAddFuelPatch) Container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name}, taking {amount}");

                c.GetInventory().RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                c.Save();
                //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });

                for (int i = 0; i < amount; i++)
                    ___m_nview.InvokeRPC("AddFuel");

                added += amount;

                user.Message(MessageHud.MessageType.TopLeft,
                    "$msg_added " + __instance.m_fuelItem.m_itemData.m_shared.m_name);

                __result = false;

                if (!pullAll || Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel")) >= __instance.m_maxFuel)
                    return false;
            }

            user.Message(MessageHud.MessageType.Center,
                added == 0
                    ? "$msg_noprocessableitems"
                    : $"$msg_added {added} {__instance.m_fuelItem.m_itemData.m_shared.m_name}");

            return __result;
        }
    }

    internal static bool CfcWasAllowed;
    public static ConfigEntry<bool> showGhostConnections = null!;
    public static ConfigEntry<float> ghostConnectionStartOffset = null!;
    public static ConfigEntry<float> ghostConnectionRemovalDelay = null!;
    public static ConfigEntry<float> mRange = null!;
    public static ConfigEntry<Color> flashColor = null!;
    public static ConfigEntry<Color> unFlashColor = null!;
    public static ConfigEntry<string> resourceString = null!;
    public static ConfigEntry<string> pulledMessage = null!;
    public static ConfigEntry<string> CFCFuelDisallowTypes = null!;
    public static ConfigEntry<string> CFCOreDisallowTypes = null!;
    public static ConfigEntry<KeyboardShortcut> pullItemsKey = null!;
    public static ConfigEntry<KeyboardShortcut> preventModKey = null!;
    public static ConfigEntry<KeyboardShortcut> fillAllModKey = null!;
    public static ConfigEntry<bool> switchPrevent = null!;
    public static ConfigEntry<bool> CFCEnabled = null!;
}