using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OdinQOL.Patches
{
    internal class MoveableChestInventory
    {
        public static ConfigEntry<float> chestInventoryX;
        public static ConfigEntry<float> chestInventoryY;
        public static ConfigEntry<KeyCode> modKeyOneChestMove;
        public static ConfigEntry<KeyCode> modKeyTwoChestMove;
        private static Vector3 lastMousePos;

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
        private static class MoveableChest_InventoryGui_Update_Patch
        {
            private static void Postfix(InventoryGui __instance, Container ___m_currentContainer)
            {
                Vector3 mousePos = Input.mousePosition;
                if (!OdinQOLplugin.modEnabled.Value || !___m_currentContainer || !___m_currentContainer.IsOwner())
                {
                    lastMousePos = mousePos;
                    return;
                }


                if (chestInventoryX.Value < 0)
                    chestInventoryX.Value = __instance.m_container.anchorMin.x;
                if (chestInventoryY.Value < 0)
                    chestInventoryY.Value = __instance.m_container.anchorMin.y;

                __instance.m_container.anchorMin = new Vector2(chestInventoryX.Value, chestInventoryY.Value);
                __instance.m_container.anchorMax = new Vector2(chestInventoryX.Value, chestInventoryY.Value);


                if (lastMousePos == Vector3.zero)
                    lastMousePos = mousePos;


                PointerEventData eventData = new(EventSystem.current)
                {
                    position = lastMousePos
                };

                if (Utilities.CheckKeyHeldKeycode(modKeyOneChestMove.Value) &&
                    Utilities.CheckKeyHeldKeycode(modKeyTwoChestMove.Value))
                {
                    //Dbgl($"position {__instance.m_container.transform.parent.position}");

                    List<RaycastResult> raycastResults = new();
                    EventSystem.current.RaycastAll(eventData, raycastResults);

                    foreach (RaycastResult rcr in raycastResults)
                        if (rcr.gameObject.layer == LayerMask.NameToLayer("UI") && rcr.gameObject.name == "Bkg" &&
                            rcr.gameObject.transform.parent.name == "Container")
                        {
                            chestInventoryX.Value += (mousePos.x - lastMousePos.x) / Screen.width;
                            chestInventoryY.Value += (mousePos.y - lastMousePos.y) / Screen.height;
                        }
                }

                lastMousePos = mousePos;
            }
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InputText))]
        private static class InputText_Patch
        {
            private static bool Prefix(Terminal __instance)
            {
                if (!OdinQOLplugin.modEnabled.Value)
                    return true;
                string text = __instance.m_input.text;
                if (text.ToLower().Equals("movablechestinventory reset"))
                {
                    OdinQOLplugin.context.Config.Reload();
                    OdinQOLplugin.context.Config.Save();
                    Traverse.Create(__instance).Method("AddString", text).GetValue();
                    Traverse.Create(__instance).Method("AddString", "movable chest inventory config reloaded")
                        .GetValue();
                    return false;
                }

                return true;
            }
        }
    }
}