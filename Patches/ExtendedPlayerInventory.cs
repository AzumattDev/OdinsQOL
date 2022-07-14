using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static OdinQOL.Patches.QuickAccessBar;
using static OdinQOL.OdinQOLplugin;
using Object = UnityEngine.Object;

namespace OdinQOL.Patches
{
    internal class ExtendedPlayerInventory
    {
        private static GameObject elementPrefab;

        private static readonly ItemDrop.ItemData.ItemType[] typeEnums =
        {
            ItemDrop.ItemData.ItemType.Helmet,
            ItemDrop.ItemData.ItemType.Chest,
            ItemDrop.ItemData.ItemType.Legs,
            ItemDrop.ItemData.ItemType.Shoulder,
            ItemDrop.ItemData.ItemType.Utility
        };

        private static ItemDrop.ItemData[] equipItems = new ItemDrop.ItemData[5];

        private static Vector3 lastMousePos;
        private static string currentlyDragging;

        public static void SetSlotText(string value, Transform transform, bool center = true)
        {
            Transform transform1 = transform.Find("binding");
            if (!transform1)
                transform1 = Object.Instantiate(elementPrefab.transform.Find("binding"), transform);
            transform1.GetComponent<Text>().enabled = true;
            transform1.GetComponent<Text>().text = value;
            if (!center)
                return;
            transform1.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 17f);
            transform1.GetComponent<RectTransform>().anchoredPosition = new Vector2(30f, -10f);
        }

        private static bool IsEquipmentSlotFree(
            Inventory inventory,
            ItemDrop.ItemData item,
            out int which)
        {
            which = Array.IndexOf(typeEnums, item.m_shared.m_itemType);
            return which >= 0 && inventory.GetItemAt(which, inventory.GetHeight() - 1) == null;
        }

        private static bool IsAtEquipmentSlot(
            Inventory inventory,
            ItemDrop.ItemData item,
            out int which)
        {
            if (!addEquipmentRow.Value || item.m_gridPos.x > 4 || item.m_gridPos.y < inventory.GetHeight() - 1)
            {
                which = -1;
                return false;
            }

            which = item.m_gridPos.x;
            return true;
        }

        private static void SetElementPositions()
        {
            Transform transform = Hud.instance.transform.Find("hudroot");
            if (!(transform.Find("QuickAccessBar")?.GetComponent<RectTransform>() != null))
                return;
            if (quickAccessX.Value == 9999.0)
                quickAccessX.Value = transform.Find("healthpanel").GetComponent<RectTransform>().anchoredPosition.x -
                                     32f;
            if (quickAccessY.Value == 9999.0)
                quickAccessY.Value = transform.Find("healthpanel").GetComponent<RectTransform>().anchoredPosition.y -
                                     870f;
            transform.Find("QuickAccessBar").GetComponent<RectTransform>().anchoredPosition =
                new Vector2(quickAccessX.Value, quickAccessY.Value);
            transform.Find("QuickAccessBar").GetComponent<RectTransform>().localScale =
                new Vector3(quickAccessScale.Value, quickAccessScale.Value, 1f);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        private static class Player_Awake_Patch
        {
            private static void Prefix(Player __instance, Inventory ___m_inventory)
            {
                if (!modEnabled.Value)
                    return;
                QOLLogger.LogDebug("Player_Awake");

                int height = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);
                __instance.m_inventory.m_height = height;
                __instance.m_tombstone.GetComponent<Container>().m_height = height;
            }
        }

        [HarmonyPatch(typeof(TombStone), nameof(TombStone.Awake))]
        private static class TombStone_Awake_Patch
        {
            private static void Prefix(TombStone __instance)
            {
                if (!modEnabled.Value)
                    return;
                QOLLogger.LogDebug("TombStone_Awake");

                int height = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);

                __instance.GetComponent<Container>().m_height = height;
                //AccessTools.FieldRefAccess<Inventory, int>(AccessTools.FieldRefAccess<Container, Inventory>(__instance.GetComponent<Container>(), "m_inventory"), "m_height") = height;
                //OdinQOLplugin.QOLLogger.LogDebug($"tombstone Awake {__instance.GetComponent<Container>().GetInventory()?.GetHeight()}");
            }
        }

        [HarmonyPatch(typeof(TombStone), nameof(TombStone.Interact))]
        private static class TombStone_Interact_Patch
        {
            private static void Prefix(TombStone __instance, Container ___m_container)
            {
                if (!modEnabled.Value)
                    return;
                QOLLogger.LogDebug("TombStone_Interact");
                int num = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);
                __instance.GetComponent<Container>().m_height = num;
                Traverse traverse = Traverse.Create(___m_container);
                string base64String = traverse.Field("m_nview").GetValue<ZNetView>().GetZDO().GetString("items");
                if (string.IsNullOrEmpty(base64String))
                    return;
                ZPackage pkg = new(base64String);
                traverse.Field("m_loading").SetValue(true);
                traverse.Field("m_inventory").GetValue<Inventory>().Load(pkg);
                traverse.Field("m_loading").SetValue(false);
                traverse.Field("m_lastRevision")
                    .SetValue(traverse.Field("m_nview").GetValue<ZNetView>().GetZDO().m_dataRevision);
                traverse.Field("m_lastDataString").SetValue(base64String);
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveInventoryToGrave))]
        private static class MoveInventoryToGrave_Patch
        {
            private static void Postfix(Inventory __instance, Inventory original)
            {
                if (!modEnabled.Value)
                    return;
                QOLLogger.LogDebug("MoveInventoryToGrave");

                QOLLogger.LogDebug($"inv: {__instance.GetHeight()} orig: {original.GetHeight()}");
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        private static class Player_Update_Patch
        {
            private static void Postfix(Player __instance, Inventory ___m_inventory)
            {
                if (!modEnabled.Value)
                    return;
                int height = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);
                AccessTools.FieldRefAccess<Inventory, int>(___m_inventory, "m_height") = height;
                __instance.m_tombstone.GetComponent<Container>().m_height = height;
                if (Utilities.IgnoreKeyPresses(true) || !addEquipmentRow.Value)
                    return;
                int num2;
                if (Utilities.CheckKeyDownKeycode(hotKey1.Value))
                {
                    num2 = 1;
                }
                else if (Utilities.CheckKeyDownKeycode(hotKey2.Value))
                {
                    num2 = 2;
                }
                else
                {
                    if (!Utilities.CheckKeyDownKeycode(hotKey3.Value))
                        return;
                    num2 = 3;
                }

                ItemDrop.ItemData itemAt = ___m_inventory.GetItemAt(num2 + 4, ___m_inventory.GetHeight() - 1);
                if (itemAt == null)
                    return;
                __instance.UseItem(null, itemAt, false);
            }

            private static void CreateTombStone()
            {
                QOLLogger.LogDebug(string.Format("height {0}",
                    Player.m_localPlayer.m_tombstone.GetComponent<Container>().m_height));
                GameObject gameObject = Object.Instantiate(Player.m_localPlayer.m_tombstone,
                    Player.m_localPlayer.GetCenterPoint(), Player.m_localPlayer.transform.rotation);
                TombStone component = gameObject.GetComponent<TombStone>();
                QOLLogger.LogDebug(string.Format("height {0}",
                    gameObject.GetComponent<Container>().m_height));
                QOLLogger.LogDebug(string.Format("inv height {0}",
                    gameObject.GetComponent<Container>().GetInventory().GetHeight()));
                QOLLogger.LogDebug(string.Format("inv slots {0}",
                    gameObject.GetComponent<Container>().GetInventory().GetEmptySlots()));
                for (int index = 0;
                     index < gameObject.GetComponent<Container>().GetInventory().GetEmptySlots();
                     ++index)
                    gameObject.GetComponent<Container>().GetInventory().AddItem("SwordBronze", 1, 1, 0, 0L, "");
                QOLLogger.LogDebug(string.Format("no items: {0}",
                    gameObject.GetComponent<Container>().GetInventory().NrOfItems()));
                PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
                component.Setup(playerProfile.GetName(), playerProfile.GetPlayerID());
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
        private static class InventoryGui_Update_Patch
        {
            private static void Postfix(
                InventoryGui __instance,
                InventoryGrid ___m_playerGrid,
                Animator ___m_animator)
            {
                if (!modEnabled.Value || !Player.m_localPlayer)
                    return;
                if (addEquipmentRow.Value)
                {
                    Traverse traverse = Traverse.Create(Player.m_localPlayer);
                    Inventory inventory = Player.m_localPlayer.GetInventory();
                    List<ItemDrop.ItemData> allItems = inventory.GetAllItems();
                    ItemDrop.ItemData itemData1 = traverse.Field("m_helmetItem").GetValue<ItemDrop.ItemData>();
                    ItemDrop.ItemData itemData2 = traverse.Field("m_chestItem").GetValue<ItemDrop.ItemData>();
                    ItemDrop.ItemData itemData3 = traverse.Field("m_legItem").GetValue<ItemDrop.ItemData>();
                    ItemDrop.ItemData itemData4 = traverse.Field("m_shoulderItem").GetValue<ItemDrop.ItemData>();
                    ItemDrop.ItemData itemData5 = traverse.Field("m_utilityItem").GetValue<ItemDrop.ItemData>();
                    int width = inventory.GetWidth();
                    int num1 = width * (inventory.GetHeight() - 1);
                    if (itemData1 != null)
                        traverse.Field("m_helmetItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                            new Vector2i(num1 % width, num1 / width);
                    int num2 = num1 + 1;
                    if (itemData2 != null)
                        traverse.Field("m_chestItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                            new Vector2i(num2 % width, num2 / width);
                    int num3 = num2 + 1;
                    if (itemData3 != null)
                        traverse.Field("m_legItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                            new Vector2i(num3 % width, num3 / width);
                    int num4 = num3 + 1;
                    if (itemData4 != null)
                        traverse.Field("m_shoulderItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                            new Vector2i(num4 % width, num4 / width);
                    int num5 = num4 + 1;
                    if (itemData5 != null)
                        traverse.Field("m_utilityItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                            new Vector2i(num5 % width, num5 / width);
                    for (int index = 0; index < allItems.Count; ++index)
                    {
                        int which;
                        if (IsAtEquipmentSlot(inventory, allItems[index], out which) &&
                            (which != 0 || allItems[index] != itemData1) &&
                            (which != 1 || allItems[index] != itemData2) &&
                            (which != 2 || allItems[index] != itemData3) &&
                            (which != 3 || allItems[index] != itemData4) &&
                            (which != 4 || allItems[index] != itemData5) && (which <= -1 ||
                                                                             allItems[index].m_shared.m_itemType !=
                                                                             typeEnums[which] ||
                                                                             equipItems[which] == allItems[index] ||
                                                                             !Player.m_localPlayer.EquipItem(
                                                                                 allItems[index], false)))
                        {
                            Vector2i vector2i = (Vector2i)typeof(Inventory)
                                .GetMethod("FindEmptySlot", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(
                                    inventory, new object[1]
                                    {
                                        true
                                    });
                            if (vector2i.x < 0 || vector2i.y < 0 || vector2i.y == inventory.GetHeight() - 1)
                            {
                                Player.m_localPlayer.DropItem(inventory, allItems[index], allItems[index].m_stack);
                            }
                            else
                            {
                                allItems[index].m_gridPos = vector2i;
                                ___m_playerGrid.UpdateInventory(inventory, Player.m_localPlayer, null);
                            }
                        }
                    }

                    equipItems = new ItemDrop.ItemData[5]
                    {
                        itemData1,
                        itemData2,
                        itemData3,
                        itemData4,
                        itemData5
                    };
                }

                if (!___m_animator.GetBool("visible"))
                    return;
                __instance.m_player.Find("Bkg").GetComponent<RectTransform>().anchorMin = new Vector2(0.0f,
                    (extraRows.Value + (!addEquipmentRow.Value || displayEquipmentRowSeparate.Value ? 0 : 1)) * -0.25f);
                if (addEquipmentRow.Value)
                {
                    if (displayEquipmentRowSeparate.Value && __instance.m_player.Find("EquipmentBkg") == null)
                    {
                        Transform transform = Object.Instantiate(__instance.m_player.Find("Bkg"), __instance.m_player);
                        transform.SetAsFirstSibling();
                        transform.name = "EquipmentBkg";
                        transform.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.0f);
                        transform.GetComponent<RectTransform>().anchorMax = new Vector2(1.5f, 1f);
                    }
                    else if (!displayEquipmentRowSeparate.Value &&
                             __instance.m_player.Find("EquipmentBkg"))
                    {
                        Object.Destroy(__instance.m_player.Find("EquipmentBkg").gameObject);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateInventory))]
        private static class UpdateInventory_Patch
        {
            private static void Postfix(InventoryGrid ___m_playerGrid)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value)
                    return;
                try
                {
                    Traverse.Create(Player.m_localPlayer);
                    Inventory inventory = Player.m_localPlayer.GetInventory();
                    int num1 = inventory.GetWidth() * (inventory.GetHeight() - 1);
                    string str1 = helmetText.Value;
                    Transform transform1 = ___m_playerGrid.m_gridRoot.transform;
                    int index1 = num1;
                    int num2 = index1 + 1;
                    Transform child1 = transform1.GetChild(index1);
                    SetSlotText(str1, child1);
                    string str2 = chestText.Value;
                    Transform transform2 = ___m_playerGrid.m_gridRoot.transform;
                    int index2 = num2;
                    int num3 = index2 + 1;
                    Transform child2 = transform2.GetChild(index2);
                    SetSlotText(str2, child2);
                    string str3 = legsText.Value;
                    Transform transform3 = ___m_playerGrid.m_gridRoot.transform;
                    int index3 = num3;
                    int num4 = index3 + 1;
                    Transform child3 = transform3.GetChild(index3);
                    SetSlotText(str3, child3);
                    string str4 = backText.Value;
                    Transform transform4 = ___m_playerGrid.m_gridRoot.transform;
                    int index4 = num4;
                    int num5 = index4 + 1;
                    Transform child4 = transform4.GetChild(index4);
                    SetSlotText(str4, child4);
                    string str5 = utilityText.Value;
                    Transform transform5 = ___m_playerGrid.m_gridRoot.transform;
                    int index5 = num5;
                    int num6 = index5 + 1;
                    Transform child5 = transform5.GetChild(index5);
                    SetSlotText(str5, child5);
                    string? str6 = hotKey1.Value.ToString();
                    Transform transform6 = ___m_playerGrid.m_gridRoot.transform;
                    int index6 = num6;
                    int num7 = index6 + 1;
                    Transform child6 = transform6.GetChild(index6);
                    SetSlotText(str6, child6, false);
                    string? str7 = hotKey2.Value.ToString();
                    Transform transform7 = ___m_playerGrid.m_gridRoot.transform;
                    int index7 = num7;
                    int num8 = index7 + 1;
                    Transform child7 = transform7.GetChild(index7);
                    SetSlotText(str7, child7, false);
                    string? str8 = hotKey3.Value.ToString();
                    Transform transform8 = ___m_playerGrid.m_gridRoot.transform;
                    int index8 = num8;
                    int num9 = index8 + 1;
                    Transform child8 = transform8.GetChild(index8);
                    SetSlotText(str8, child8, false);
                    if (!displayEquipmentRowSeparate.Value)
                        return;
                    int num10 = inventory.GetWidth() * (inventory.GetHeight() - 1);
                    Transform transform9 = ___m_playerGrid.m_gridRoot.transform;
                    int index9 = num10;
                    int num11 = index9 + 1;
                    transform9.GetChild(index9).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(678f, 0.0f);
                    Transform transform10 = ___m_playerGrid.m_gridRoot.transform;
                    int index10 = num11;
                    int num12 = index10 + 1;
                    transform10.GetChild(index10).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(748f, -35f);
                    Transform transform11 = ___m_playerGrid.m_gridRoot.transform;
                    int index11 = num12;
                    int num13 = index11 + 1;
                    transform11.GetChild(index11).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(678f, -70f);
                    Transform transform12 = ___m_playerGrid.m_gridRoot.transform;
                    int index12 = num13;
                    int num14 = index12 + 1;
                    transform12.GetChild(index12).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(748f, -105f);
                    Transform transform13 = ___m_playerGrid.m_gridRoot.transform;
                    int index13 = num14;
                    int num15 = index13 + 1;
                    transform13.GetChild(index13).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(678f, -140f);
                    Transform transform14 = ___m_playerGrid.m_gridRoot.transform;
                    int index14 = num15;
                    int num16 = index14 + 1;
                    transform14.GetChild(index14).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(643f, -210f);
                    Transform transform15 = ___m_playerGrid.m_gridRoot.transform;
                    int index15 = num16;
                    int num17 = index15 + 1;
                    transform15.GetChild(index15).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(713f, -210f);
                    Transform transform16 = ___m_playerGrid.m_gridRoot.transform;
                    int index16 = num17;
                    num9 = index16 + 1;
                    transform16.GetChild(index16).GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(783f, -210f);
                }
                catch (Exception ex)
                {
                    QOLLogger.LogDebug($"Exception in EPI Update Inventory: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindEmptySlot))]
        private static class FindEmptySlot_Patch
        {
            private static void Prefix(Inventory __instance, ref int ___m_height)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value || !Player.m_localPlayer ||
                    __instance != Player.m_localPlayer.GetInventory())
                    return;
                QOLLogger.LogDebug("FindEmptySlot");
                --___m_height;
            }

            private static void Postfix(Inventory __instance, ref int ___m_height)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value || !Player.m_localPlayer ||
                    __instance != Player.m_localPlayer.GetInventory())
                    return;
                ++___m_height;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetEmptySlots))]
        private static class GetEmptySlots_Patch
        {
            private static bool Prefix(
                Inventory __instance,
                ref int __result,
                List<ItemDrop.ItemData> ___m_inventory,
                int ___m_width,
                int ___m_height)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value || __instance != Player.m_localPlayer.GetInventory())
                    return true;
                QOLLogger.LogDebug("GetEmptySlots");
                int count = ___m_inventory.FindAll((Predicate<ItemDrop.ItemData>)(i => i.m_gridPos.y < ___m_height - 1))
                    .Count;
                __result = (___m_height - 1) * ___m_width - count;
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveEmptySlot))]
        private static class HaveEmptySlot_Patch
        {
            private static bool Prefix(
                Inventory __instance,
                ref bool __result,
                List<ItemDrop.ItemData> ___m_inventory,
                int ___m_width,
                int ___m_height)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value || __instance != Player.m_localPlayer.GetInventory())
                    return true;
                int count = ___m_inventory.FindAll((Predicate<ItemDrop.ItemData>)(i => i.m_gridPos.y < ___m_height - 1))
                    .Count;
                __result = count < ___m_width * (___m_height - 1);
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData))]
        private static class Inventory_AddItem_Patch1
        {
            private static bool Prefix(
                Inventory __instance,
                ref bool __result,
                List<ItemDrop.ItemData> ___m_inventory,
                ItemDrop.ItemData item)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value || !Player.m_localPlayer ||
                    __instance != Player.m_localPlayer.GetInventory())
                    return true;
                QOLLogger.LogDebug("AddItem");
                int which;
                if (!IsEquipmentSlotFree(__instance, item, out which))
                    return true;
                item.m_gridPos = new Vector2i(which, __instance.GetHeight() - 1);
                ___m_inventory.Add(item);
                Player.m_localPlayer.EquipItem(item, false);
                typeof(Inventory).GetMethod("Changed", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(__instance, new object[0]);
                __result = true;
                return false;
            }
        }


        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData), typeof(int), typeof(int),
            typeof(int))]
        private static class Inventory_AddItem_Patch2
        {
            private static void Prefix(
                Inventory __instance,
                ref int ___m_width,
                ref int ___m_height,
                int x,
                int y)
            {
                if (!modEnabled.Value)
                    return;
                if (x >= ___m_width)
                    ___m_width = x + 1;
                if (y < ___m_height)
                    return;
                ___m_height = y + 1;
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
        private static class Hud_Awake_Patch
        {
            private static void Postfix(Hud __instance)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value)
                    return;
                Transform transform = Object.Instantiate(__instance.m_rootObject.transform.Find("HotKeyBar"),
                    __instance.m_rootObject.transform, true);
                transform.name = "QuickAccessBar";
                transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
                GameObject elementPrefab = transform.GetComponent<HotkeyBar>().m_elementPrefab;
                transform.gameObject.AddComponent<QuickAccessBar>().m_elementPrefab = elementPrefab;
                elementPrefab = elementPrefab;
                Object.Destroy(transform.GetComponent<HotkeyBar>());
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.Update))]
        private static class Hud_Update_Patch
        {
            private static void Postfix(Hud __instance)
            {
                if (!modEnabled.Value || !addEquipmentRow.Value || Player.m_localPlayer == null)
                    return;
                float scaleFactor = GameObject.Find("LoadingGUI").GetComponent<CanvasScaler>().scaleFactor;
                Vector3 mousePosition = Input.mousePosition;
                if (!modEnabled.Value)
                {
                    lastMousePos = mousePosition;
                }
                else
                {
                    SetElementPositions();
                    if (lastMousePos == Vector3.zero)
                        lastMousePos = mousePosition;
                    Transform transform = Hud.instance.transform.Find("hudroot");
                    if (Utilities.CheckKeyHeldKeycode(modKeyOne.Value) &&
                        Utilities.CheckKeyHeldKeycode(modKeyTwo.Value))
                    {
                        Rect rect = Rect.zero;
                        if (transform.Find("QuickAccessBar")?.GetComponent<RectTransform>() != null)
                            rect = new Rect(
                                transform.Find("QuickAccessBar").GetComponent<RectTransform>().anchoredPosition.x *
                                scaleFactor,
                                (float)(transform.Find("QuickAccessBar").GetComponent<RectTransform>().anchoredPosition
                                            .y * (double)scaleFactor + Screen.height -
                                        transform.Find("QuickAccessBar").GetComponent<RectTransform>().sizeDelta.y *
                                        (double)scaleFactor * quickAccessScale.Value),
                                (float)(transform.Find("QuickAccessBar").GetComponent<RectTransform>().sizeDelta.x *
                                        (double)scaleFactor * quickAccessScale.Value * 0.375),
                                transform.Find("QuickAccessBar").GetComponent<RectTransform>().sizeDelta.y *
                                scaleFactor * quickAccessScale.Value);
                        if (rect.Contains(lastMousePos) &&
                            (currentlyDragging == "" || currentlyDragging == "QuickAccessBar"))
                        {
                            quickAccessX.Value += (mousePosition.x - lastMousePos.x) / scaleFactor;
                            quickAccessY.Value += (mousePosition.y - lastMousePos.y) / scaleFactor;
                            currentlyDragging = "QuickAccessBar";
                        }
                        else
                        {
                            currentlyDragging = "";
                        }
                    }
                    else
                    {
                        currentlyDragging = "";
                    }

                    lastMousePos = mousePosition;
                }
            }
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InputText))]
        private static class InputText_Patch
        {
            private static bool Prefix(Terminal __instance)
            {
                if (!modEnabled.Value)
                    return true;
                string text = __instance.m_input.text;
                if (!text.ToLower().Equals(typeof(OdinQOLplugin).Namespace.ToLower() + " reset"))
                    return true;
                context.Config.Reload();
                context.Config.Save();
                __instance.AddString(text);
                __instance.AddString(context.Info.Metadata.Name + " config reloaded");
                return false;
            }
        }
    }
}