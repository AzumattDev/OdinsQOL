using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    public class ClientPatches
    {
        public static ConfigEntry<string> _chatPlayerName;

        private static bool _overridePlayerName;

        [HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
        static class Chat_SendText_Patch
        {
            static void Prefix(Chat __instance, Talker.Type type, string text)
            {
                _overridePlayerName = true;
            }

            static void Postfix(Chat __instance, Talker.Type type, string text)
            {
                _overridePlayerName = false;
            }
        }

        [HarmonyPatch(typeof(Chat), nameof(Chat.SendPing))]
        static class Chat_SendPing_Patch
        {
            static void Prefix(Chat __instance, Vector3 position)
            {
                _overridePlayerName = true;
            }

            static void Postfix(Chat __instance, Vector3 position)
            {
                _overridePlayerName = false;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetPlayerName))]
        static class Player_GetPlayerName_Patch
        {
            static void Postfix(Player __instance, ref string __result)
            {
                if (_overridePlayerName
                    && __instance == Player.m_localPlayer
                    && !string.IsNullOrEmpty(_chatPlayerName.Value))
                    __result =  _chatPlayerName.Value;
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.GetName))]
        static class PlayerProfile_GetName_Patch
        {
            static void Postfix(PlayerProfile __instance, ref string __result)
            {
                if (_overridePlayerName && !string.IsNullOrEmpty(_chatPlayerName.Value)) __result = _chatPlayerName.Value;
                
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
        static class Game_SpawnPlayer_Patch
        {
            static void Postfix(Game __instance, Vector3 spawnPoint, Player __result)
            {
                if (!string.IsNullOrEmpty(_chatPlayerName.Value))
                    __result.m_nview.GetZDO().Set("playerName", _chatPlayerName.Value);
            }
        }

        [HarmonyPatch(typeof(Ship), "Awake")]
        public static class shipfix
        {
            private static void Postfix(ref Ship __instance)
            {
                __instance.m_minWaterImpactForce = 100f;
            }
        }

        [HarmonyPatch(typeof(ItemStand), "Interact")]
        private class standFix
        {
            private static bool Prefix(Humanoid user, bool hold, ItemStand __instance)
            {
                if (PrivateArea.CheckAccess(__instance.transform.position)) return true;
                return false;
            }
        }


        [HarmonyPatch(typeof(ItemStand), "CanAttach")]
        public class ItemStand_CanAttach
        {
            public static void Postfix(ItemStand __instance, ref bool __result)
            {
                if (__instance.m_name == "$piece_itemstand") __result = true;
            }
        }

        [HarmonyPatch(typeof(ItemStand), "GetAttachPrefab")]
        public class ItemStand_GetAttachPrefab
        {
            public static void Postfix(GameObject item, ref GameObject __result)
            {
                if (__result == null)
                {
                    Collider? componentInChildren = item.transform.GetComponentInChildren<Collider>();
                    if ((bool)componentInChildren) __result = componentInChildren.transform.gameObject;
                }
            }
        }
    }
}