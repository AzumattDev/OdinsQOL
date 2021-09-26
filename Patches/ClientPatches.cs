using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    public class ClientPatches
    {
        public static ConfigEntry<string> _chatPlayerName;

        private static bool _overridePlayerName;

        internal static void ChatSendTextPrefix(On.Chat.orig_SendText orig, Chat self, Talker.Type type, string text)
        {
            _overridePlayerName = true;
            orig(self, type, text);
            _overridePlayerName = false;
        }

        public static void ChatSendPingPrefix(On.Chat.orig_SendPing orig, Chat self, Vector3 position)
        {
            _overridePlayerName = true;
            orig(self, position);
            _overridePlayerName = false;
        }

        public static string PlayerGetPlayerNamePrefix(On.Player.orig_GetPlayerName orig, Player self)
        {
            if (_overridePlayerName
                && self == Player.m_localPlayer
                && !string.IsNullOrEmpty(_chatPlayerName.Value))
                return _chatPlayerName.Value;

            return orig(self);
        }

        public static string PlayerProfileGetNamePrefix(On.PlayerProfile.orig_GetName orig, PlayerProfile self)
        {
            if (_overridePlayerName && !string.IsNullOrEmpty(_chatPlayerName.Value)) return _chatPlayerName.Value;

            return orig(self);
        }

        public static Player GameSpawnPlayerPostfix(On.Game.orig_SpawnPlayer orig, Game self, Vector3 spawnPoint)
        {
            var player = orig(self, spawnPoint);

            if (!string.IsNullOrEmpty(_chatPlayerName.Value))
                player.m_nview.GetZDO().Set("playerName", _chatPlayerName.Value);

            return player;
        }

        [HarmonyPatch(typeof(Ship), "Awake")]
        public static class shipfix
        {
            private static void Postfix(ref Ship __instance)
            {
                __instance.m_minWaterImpactForce = 100f;
            }
        }

        [HarmonyPatch(typeof(ZSteamMatchmaking))]
        [HarmonyPatch("GetServers")]
        private class PatchUpdateServerListGui
        {
            private static void Postfix(ref List<ServerData> allServers)
            {
                var serverData = new ServerData
                {
                    m_host = "3.17.85.9",
                    m_name = "<color=#6600cc>HelHeim</color>",
                    m_password = false,
                    m_players = 999,
                    m_port = 2456,
                    m_steamHostID = 0uL,
                    m_steamHostAddr = default
                };
                serverData.m_steamHostAddr.ParseString(serverData.m_host + ":" + serverData.m_port);
                serverData.m_upnp = true;
                serverData.m_version = "";
                allServers.Insert(0, serverData);

                var serverNo2 = new ServerData
                {
                    m_host = "23.23.255.113",
                    m_name = "<color=#ff0000>HoulGate</color>",
                    m_password = false,
                    m_players = 999,
                    m_port = 2456,
                    m_steamHostID = 0uL,
                    m_steamHostAddr = default
                };
                serverNo2.m_steamHostAddr.ParseString(serverNo2.m_host + ":" + serverNo2.m_port);
                serverNo2.m_upnp = true;
                serverNo2.m_version = "";
                allServers.Insert(0, serverNo2);
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
                    var componentInChildren = item.transform.GetComponentInChildren<Collider>();
                    if ((bool)componentInChildren) __result = componentInChildren.transform.gameObject;
                }
            }
        }
    }
}