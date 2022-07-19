using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OdinQOL.Patches
{
    internal class MoveableChestInventory
    {
        public static ConfigEntry<float> ChestInventoryX = null!;
        public static ConfigEntry<float> ChestInventoryY = null!;
        public static ConfigEntry<KeyCode> ModKeyOneChestMove = null!;
        public static ConfigEntry<KeyCode> ModKeyTwoChestMove = null!;
        private static Vector3 lastMousePos;

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
        private static class MoveableChest_InventoryGui_Update_Patch
        {
            private static void Postfix(InventoryGui __instance, Container ___m_currentContainer)
            {
                Vector3 mousePos = Input.mousePosition;
                if (!OdinQOLplugin.ModEnabled.Value || !___m_currentContainer || !___m_currentContainer.IsOwner())
                {
                    lastMousePos = mousePos;
                    return;
                }


                if (ChestInventoryX.Value < 0)
                    ChestInventoryX.Value = __instance.m_container.anchorMin.x;
                if (ChestInventoryY.Value < 0)
                    ChestInventoryY.Value = __instance.m_container.anchorMin.y;

                __instance.m_container.anchorMin = new Vector2(ChestInventoryX.Value, ChestInventoryY.Value);
                __instance.m_container.anchorMax = new Vector2(ChestInventoryX.Value, ChestInventoryY.Value);


                if (lastMousePos == Vector3.zero)
                    lastMousePos = mousePos;


                PointerEventData eventData = new(EventSystem.current)
                {
                    position = lastMousePos
                };

                if (Utilities.CheckKeyHeldKeycode(ModKeyOneChestMove.Value) &&
                    Utilities.CheckKeyHeldKeycode(ModKeyTwoChestMove.Value))
                {
                    List<RaycastResult> raycastResults = new();
                    EventSystem.current.RaycastAll(eventData, raycastResults);

                    foreach (RaycastResult rcr in raycastResults.Where(rcr =>
                                 rcr.gameObject.layer == LayerMask.NameToLayer("UI") && rcr.gameObject.name == "Bkg" &&
                                 rcr.gameObject.transform.parent.name == "Container"))
                    {
                        ChestInventoryX.Value += (mousePos.x - lastMousePos.x) / Screen.width;
                        ChestInventoryY.Value += (mousePos.y - lastMousePos.y) / Screen.height;
                    }
                }

                lastMousePos = mousePos;
            }
        }
    }
}