using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VMP_Mod.EAQS
{
    internal class EAQS
    {
        public static ConfigEntry<bool> addEquipmentRow;
        public static ConfigEntry<bool> displayEquipmentRowSeparate;
        public static ConfigEntry<int> extraRows;

        public static ConfigEntry<string> helmetText;
        public static ConfigEntry<string> chestText;
        public static ConfigEntry<string> legsText;
        public static ConfigEntry<string> backText;
        public static ConfigEntry<string> utilityText;
        public static ConfigEntry<float> quickAccessScale;

        public static ConfigEntry<string> hotKey1;
        public static ConfigEntry<string> hotKey2;
        public static ConfigEntry<string> hotKey3;
        public static ConfigEntry<string> modKeyOne;
        public static ConfigEntry<string> modKeyTwo;

        public static ConfigEntry<string>[] hotkeys;

        public static ConfigEntry<float> quickAccessX;
        public static ConfigEntry<float> quickAccessY;

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
            var t = transform.Find("binding");
            if (!t) t = Object.Instantiate(elementPrefab.transform.Find("binding"), transform);
            t.GetComponent<Text>().enabled = true;
            t.GetComponent<Text>().text = value;
            if (center)
            {
                t.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 17);
                t.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, -10);
            }
        }

        private static bool IsEquipmentSlotFree(Inventory inventory, ItemDrop.ItemData item, out int which)
        {
            which = Array.IndexOf(typeEnums, item.m_shared.m_itemType);
            return which >= 0 && inventory.GetItemAt(which, inventory.GetHeight() - 1) == null;
        }

        private static bool IsAtEquipmentSlot(Inventory inventory, ItemDrop.ItemData item, out int which)
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
            var hudRoot = Hud.instance.transform.Find("hudroot");

            if (hudRoot.Find("QuickAccessBar")?.GetComponent<RectTransform>() != null)
            {
                if (quickAccessX.Value == 9999)
                    quickAccessX.Value = hudRoot.Find("healthpanel").GetComponent<RectTransform>().anchoredPosition.x -
                                         32;
                if (quickAccessY.Value == 9999)
                    quickAccessY.Value = hudRoot.Find("healthpanel").GetComponent<RectTransform>().anchoredPosition.y -
                                         870;

                hudRoot.Find("QuickAccessBar").GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(quickAccessX.Value, quickAccessY.Value);
                hudRoot.Find("QuickAccessBar").GetComponent<RectTransform>().localScale =
                    new Vector3(quickAccessScale.Value, quickAccessScale.Value, 1);
            }
        }

        [HarmonyPatch(typeof(Player), "Awake")]
        private static class Player_Awake_Patch
        {
            private static void Prefix(Player __instance, Inventory ___m_inventory)
            {
                VMP_Modplugin.Dbgl("Player_Awake");

                var height = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);

                AccessTools.FieldRefAccess<Inventory, int>(___m_inventory, "m_height") = height;
                __instance.m_tombstone.GetComponent<Container>().m_height = height;
            }
        }

        [HarmonyPatch(typeof(TombStone), "Awake")]
        private static class TombStone_Awake_Patch
        {
            private static void Prefix(TombStone __instance)
            {
                VMP_Modplugin.Dbgl("TombStone_Awake");

                var height = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);

                __instance.GetComponent<Container>().m_height = height;
                //AccessTools.FieldRefAccess<Inventory, int>(AccessTools.FieldRefAccess<Container, Inventory>(__instance.GetComponent<Container>(), "m_inventory"), "m_height") = height;
                VMP_Modplugin.Dbgl(
                    $"tombstone Awake {__instance.GetComponent<Container>().GetInventory()?.GetHeight()}");
            }
        }

        [HarmonyPatch(typeof(TombStone), "Interact")]
        private static class TombStone_Interact_Patch
        {
            private static void Prefix(TombStone __instance, Container ___m_container)
            {
                VMP_Modplugin.Dbgl("TombStone_Interact");

                var height = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);

                __instance.GetComponent<Container>().m_height = height;

                var t = Traverse.Create(___m_container);
                var dataString = t.Field("m_nview").GetValue<ZNetView>().GetZDO().GetString("items");
                if (string.IsNullOrEmpty(dataString)) return;
                var pkg = new ZPackage(dataString);
                t.Field("m_loading").SetValue(true);
                t.Field("m_inventory").GetValue<Inventory>().Load(pkg);
                t.Field("m_loading").SetValue(false);
                t.Field("m_lastRevision").SetValue(t.Field("m_nview").GetValue<ZNetView>().GetZDO().m_dataRevision);
                t.Field("m_lastDataString").SetValue(dataString);
            }
        }


        [HarmonyPatch(typeof(Inventory), "MoveInventoryToGrave")]
        private static class MoveInventoryToGrave_Patch
        {
            private static void Postfix(Inventory __instance, Inventory original)
            {
                VMP_Modplugin.Dbgl("MoveInventoryToGrave");

                VMP_Modplugin.Dbgl($"inv: {__instance.GetHeight()} orig: {original.GetHeight()}");
            }
        }


        [HarmonyPatch(typeof(Player), "Update")]
        private static class Player_Update_Patch
        {
            private static void Postfix(Player __instance, Inventory ___m_inventory)
            {
                var height = extraRows.Value + (addEquipmentRow.Value ? 5 : 4);

                AccessTools.FieldRefAccess<Inventory, int>(___m_inventory, "m_height") = height;
                __instance.m_tombstone.GetComponent<Container>().m_height = height;


                if (Utilities.IgnoreKeyPresses(true) || !addEquipmentRow.Value)
                    return;

                int which;
                if (Utilities.CheckKeyDown(hotKey1.Value))
                    which = 1;
                else if (Utilities.CheckKeyDown(hotKey2.Value))
                    which = 2;
                else if (Utilities.CheckKeyDown(hotKey3.Value))
                    which = 3;
                else return;

                var itemAt = ___m_inventory.GetItemAt(which + 4, ___m_inventory.GetHeight() - 1);
                if (itemAt != null) __instance.UseItem(null, itemAt, false);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "Update")]
        private static class InventoryGui_Update_Patch
        {
            private static void Postfix(InventoryGui __instance, InventoryGrid ___m_playerGrid, Animator ___m_animator)
            {
                if (!addEquipmentRow.Value || !Player.m_localPlayer)
                    return;

                var t = Traverse.Create(Player.m_localPlayer);

                var inv = Player.m_localPlayer.GetInventory();

                var items = inv.GetAllItems();

                var helmet = t.Field("m_helmetItem").GetValue<ItemDrop.ItemData>();
                var chest = t.Field("m_chestItem").GetValue<ItemDrop.ItemData>();
                var legs = t.Field("m_legItem").GetValue<ItemDrop.ItemData>();
                var back = t.Field("m_shoulderItem").GetValue<ItemDrop.ItemData>();
                var utility = t.Field("m_utilityItem").GetValue<ItemDrop.ItemData>();

                for (var i = 0; i < items.Count; i++)
                    //Dbgl($"{items[i].m_gridPos} {inv.GetHeight() - 1},0 {items[i] != helmet}");
                    if (IsAtEquipmentSlot(inv, items[i], out var which))
                    {
                        if ( // in right slot and equipped
                            which == 0 && items[i] == helmet ||
                            which == 1 && items[i] == chest ||
                            which == 2 && items[i] == legs ||
                            which == 3 && items[i] == back ||
                            which == 4 && items[i] == utility
                        )
                            continue;

                        if (which > -1 && items[i].m_shared.m_itemType == typeEnums[which] &&
                            equipItems[which] != items[i] &&
                            Player.m_localPlayer.EquipItem(items[i], false)) // in right slot and new
                            continue;

                        // in wrong slot or unequipped in slot or can't equip
                        var newPos = (Vector2i) typeof(Inventory)
                            .GetMethod("FindEmptySlot", BindingFlags.NonPublic | BindingFlags.Instance)
                            .Invoke(inv, new object[] {true});
                        if (newPos.x < 0 || newPos.y < 0 || newPos.y == inv.GetHeight() - 1)
                        {
                            Player.m_localPlayer.DropItem(inv, items[i], items[i].m_stack);
                        }
                        else
                        {
                            items[i].m_gridPos = newPos;
                            ___m_playerGrid.UpdateInventory(inv, Player.m_localPlayer, null);
                        }
                    }

                equipItems = new[] {helmet, chest, legs, back, utility};

                if (!___m_animator.GetBool("visible"))
                    return;

                var width = inv.GetWidth();
                var offset = width * (inv.GetHeight() - 1);

                if (helmet != null)
                    t.Field("m_helmetItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                        new Vector2i(offset % width, offset / width);
                offset++;

                if (chest != null)
                    t.Field("m_chestItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                        new Vector2i(offset % width, offset / width);
                offset++;

                if (legs != null)
                    t.Field("m_legItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                        new Vector2i(offset % width, offset / width);
                offset++;

                if (back != null)
                    t.Field("m_shoulderItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                        new Vector2i(offset % width, offset / width);
                offset++;

                if (utility != null)
                    t.Field("m_utilityItem").GetValue<ItemDrop.ItemData>().m_gridPos =
                        new Vector2i(offset % width, offset / width);


                __instance.m_player.Find("Bkg").GetComponent<RectTransform>().anchorMin = new Vector2(0,
                    (extraRows.Value + (addEquipmentRow.Value && !displayEquipmentRowSeparate.Value ? 1 : 0)) * -0.25f);

                if (displayEquipmentRowSeparate.Value && __instance.m_player.Find("EquipmentBkg") == null)
                {
                    var bkg = Object.Instantiate(__instance.m_player.Find("Bkg"), __instance.m_player);
                    bkg.SetAsFirstSibling();
                    bkg.name = "EquipmentBkg";
                    bkg.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                    bkg.GetComponent<RectTransform>().anchorMax = new Vector2(1.5f, 1);
                }
                else if (!displayEquipmentRowSeparate.Value && __instance.m_player.Find("EquipmentBkg"))
                {
                    Object.Destroy(__instance.m_player.Find("EquipmentBkg").gameObject);
                }
            }
        }


        [HarmonyPatch(typeof(InventoryGui), "UpdateInventory")]
        private static class UpdateInventory_Patch
        {
            private static void Postfix(InventoryGrid ___m_playerGrid)
            {
                if (!addEquipmentRow.Value)
                    return;

                var t = Traverse.Create(Player.m_localPlayer);

                var inv = Player.m_localPlayer.GetInventory();

                var offset = inv.GetWidth() * (inv.GetHeight() - 1);

                SetSlotText(helmetText.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++));
                SetSlotText(chestText.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++));
                SetSlotText(legsText.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++));
                SetSlotText(backText.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++));
                SetSlotText(utilityText.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++));
                SetSlotText(hotKey1.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++), false);
                SetSlotText(hotKey2.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++), false);
                SetSlotText(hotKey3.Value, ___m_playerGrid.m_gridRoot.transform.GetChild(offset++), false);

                if (displayEquipmentRowSeparate.Value)
                {
                    offset = inv.GetWidth() * (inv.GetHeight() - 1);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(678, 0);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(748, -35);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(678, -70);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(748, -105);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(678, -140);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(643, -210);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(713, -210);
                    ___m_playerGrid.m_gridRoot.transform.GetChild(offset++).GetComponent<RectTransform>()
                        .anchoredPosition = new Vector2(783, -210);
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "FindEmptySlot")]
        private static class FindEmptySlot_Patch
        {
            private static void Prefix(Inventory __instance, ref int ___m_height)
            {
                if (!addEquipmentRow.Value || !Player.m_localPlayer ||
                    __instance != Player.m_localPlayer.GetInventory())
                    return;
                VMP_Modplugin.Dbgl("FindEmptySlot");

                ___m_height--;
            }

            private static void Postfix(Inventory __instance, ref int ___m_height)
            {
                if (!addEquipmentRow.Value || !Player.m_localPlayer ||
                    __instance != Player.m_localPlayer.GetInventory())
                    return;

                ___m_height++;
            }
        }

        [HarmonyPatch(typeof(Inventory), "GetEmptySlots")]
        private static class GetEmptySlots_Patch
        {
            private static bool Prefix(Inventory __instance, ref int __result, List<ItemDrop.ItemData> ___m_inventory,
                int ___m_width, int ___m_height)
            {
                if (!addEquipmentRow.Value || __instance != Player.m_localPlayer.GetInventory())
                    return true;
                VMP_Modplugin.Dbgl("GetEmptySlots");
                var count = ___m_inventory.FindAll(i => i.m_gridPos.y < ___m_height - 1).Count;
                __result = (___m_height - 1) * ___m_width - count;
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), "HaveEmptySlot")]
        private static class HaveEmptySlot_Patch
        {
            private static bool Prefix(Inventory __instance, ref bool __result, List<ItemDrop.ItemData> ___m_inventory,
                int ___m_width, int ___m_height)
            {
                if (!addEquipmentRow.Value || __instance != Player.m_localPlayer.GetInventory())
                    return true;

                var count = ___m_inventory.FindAll(i => i.m_gridPos.y < ___m_height - 1).Count;

                __result = count < ___m_width * (___m_height - 1);
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), "AddItem", typeof(ItemDrop.ItemData))]
        private static class Inventory_AddItem_Patch
        {
            private static bool Prefix(Inventory __instance, ref bool __result, List<ItemDrop.ItemData> ___m_inventory,
                ItemDrop.ItemData item)
            {
                if (!addEquipmentRow.Value || !Player.m_localPlayer ||
                    __instance != Player.m_localPlayer.GetInventory())
                    return true;

                VMP_Modplugin.Dbgl("AddItem");

                if (IsEquipmentSlotFree(__instance, item, out var which))
                    item.m_gridPos = new Vector2i(which, __instance.GetHeight() - 1);
                else
                    return true;
                ___m_inventory.Add(item);
                Player.m_localPlayer.EquipItem(item, false);
                typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(__instance, new object[] { });
                __result = true;
                return false;
            }
        }


        [HarmonyPatch(typeof(Inventory), "AddItem", typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
        private static class Inventory_AddItem_Patch2
        {
            private static void Prefix(Inventory __instance, ref int ___m_width, ref int ___m_height, int x, int y)
            {
                if (x >= ___m_width) ___m_width = x + 1;
                if (y >= ___m_height) ___m_height = y + 1;
            }
        }

        [HarmonyPatch(typeof(Hud), "Awake")]
        private static class Hud_Awake_Patch
        {
            private static void Postfix(Hud __instance)
            {
                if (!addEquipmentRow.Value)
                    return;

                var newBar = Object.Instantiate(__instance.m_rootObject.transform.Find("HotKeyBar"));
                newBar.name = "QuickAccessBar";
                newBar.SetParent(__instance.m_rootObject.transform);
                newBar.GetComponent<RectTransform>().localPosition = Vector3.zero;
                var go = newBar.GetComponent<HotkeyBar>().m_elementPrefab;
                var qab = newBar.gameObject.AddComponent<QuickAccessBar>();
                qab.m_elementPrefab = go;
                elementPrefab = go;
                Object.Destroy(newBar.GetComponent<HotkeyBar>());
            }
        }

        [HarmonyPatch(typeof(Hud), "Update")]
        private static class Hud_Update_Patch
        {
            private static void Postfix(Hud __instance)
            {
                if (!addEquipmentRow.Value || Player.m_localPlayer == null)
                    return;

                var gameScale = GameObject.Find("GUI").GetComponent<CanvasScaler>().scaleFactor;

                var mousePos = Input.mousePosition;

                SetElementPositions();

                if (lastMousePos == Vector3.zero)
                    lastMousePos = mousePos;


                var hudRoot = Hud.instance.transform.Find("hudroot");


                if (Utilities.CheckKeyHeld(modKeyOne.Value) && Utilities.CheckKeyHeld(modKeyTwo.Value))
                {
                    var quickSlotsRect = Rect.zero;
                    if (hudRoot.Find("QuickAccessBar")?.GetComponent<RectTransform>() != null)
                        quickSlotsRect = new Rect(
                            hudRoot.Find("QuickAccessBar").GetComponent<RectTransform>().anchoredPosition.x * gameScale,
                            hudRoot.Find("QuickAccessBar").GetComponent<RectTransform>().anchoredPosition.y *
                            gameScale + Screen.height -
                            hudRoot.Find("QuickAccessBar").GetComponent<RectTransform>().sizeDelta.y * gameScale *
                            quickAccessScale.Value,
                            hudRoot.Find("QuickAccessBar").GetComponent<RectTransform>().sizeDelta.x * gameScale *
                            quickAccessScale.Value * (3 / 8f),
                            hudRoot.Find("QuickAccessBar").GetComponent<RectTransform>().sizeDelta.y * gameScale *
                            quickAccessScale.Value
                        );

                    if (quickSlotsRect.Contains(lastMousePos) &&
                        (currentlyDragging == "" || currentlyDragging == "QuickAccessBar"))
                    {
                        quickAccessX.Value += (mousePos.x - lastMousePos.x) / gameScale;
                        quickAccessY.Value += (mousePos.y - lastMousePos.y) / gameScale;
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

                lastMousePos = mousePos;
            }
        }
    }
}