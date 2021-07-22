using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace VMP_Mod.Patches
{
    public class ClientPatches
    {
        public static bool admin = false;

        public static string playerClass = "";

        public static bool Contains(string source, string toCheck, StringComparison comp)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var text = source.ToUpperInvariant();
            var value = toCheck.ToUpperInvariant();
            return text.Contains(value);
        }

        public static void SendModerationLog(string message)
        {
            if (ZRoutedRpc.instance != null && !ZRoutedRpc.instance.m_server)
                ZRoutedRpc.instance.InvokeRoutedRPC("ServerModerationLog", message);
        }

        public static void playerEqp(ref ItemDrop.ItemData item, ref Humanoid huma)
        {
            if (item != null && item.IsEquipable() && huma != null && huma.IsPlayer())
            {
                var player = (Player) huma;
                if (player.m_nview.IsValid() && Player.m_localPlayer != null)
                {
                    var text = item.m_crafterID == 0L
                        ? " : uncrafted : "
                        : " : crafted by : " + item.m_crafterName + " ";
                    SendModerationLog("Item Equipped:" + item.m_shared.m_name + Player.m_localPlayer.GetPlayerName());
                }
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


        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static class PlayerEquip
        {
            private static void Postfix(ItemDrop.ItemData item, bool triggerEquipEffects, ref Humanoid __instance)
            {
                playerEqp(ref item, ref __instance);
            }
        }


        [HarmonyPatch(typeof(Player), "ConsumeItem")]
        public static class ConsumeLog
        {
            private static void Postfix(Inventory inventory, ItemDrop.ItemData item, Player __instance)
            {
                if (__instance != null && item != null && item.m_shared != null && __instance.m_nview.IsValid())
                {
                    var text = item.m_crafterID == 0L
                        ? " : uncrafted : "
                        : " : crafted by : " + item.m_crafterName + " ";
                    SendModerationLog(__instance.GetPlayerName() + " : consume : " + item.m_shared.m_name + text);
                }
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


        [HarmonyPatch(typeof(WearNTear), "OnPlaced")]
        private class shipCreated
        {
            private static void Postfix(ref WearNTear __instance)
            {
                if ((bool) __instance)
                {
                    var component = __instance.gameObject.GetComponent<Ship>();
                    if ((bool) component && (bool) component.m_nview && component.m_nview.IsValid())
                    {
                        component.m_nview.GetZDO().Set("creatorName", Game.instance.GetPlayerProfile().GetName());
                        SendModerationLog("ship created " + component.gameObject.name + " " +
                                          Game.instance.GetPlayerProfile().GetName());
                    }

                    var craftings = __instance.gameObject.GetComponent<CraftingStation>();
                    if ((bool) craftings && (bool) craftings.m_nview && craftings.m_nview.IsValid())
                    {
                        craftings.m_nview.GetZDO().Set("creatorName", Game.instance.GetPlayerProfile().GetName());
                        SendModerationLog("Crafting station created " + craftings.gameObject.name + " " +
                                          Game.instance.GetPlayerProfile().GetName());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Ship), "OnDestroyed")]
        private class shipDestroyed
        {
            private static void Postfix(ref Ship __instance)
            {
                if (__instance.IsOwner() && (bool) __instance.m_nview && __instance.m_nview.IsValid())
                {
                    var list = new List<Player>();
                    Player.GetPlayersInRange(__instance.transform.position, 20f, list);
                    SendModerationLog("ship destroyed " + __instance.gameObject.name + " creator: " +
                                      __instance.m_nview.GetZDO().GetString("creatorName") + " around: " +
                                      string.Join(",", list.Select(p => p.GetPlayerName())));
                }
            }
        }

        [HarmonyPatch(typeof(Player), "StartShipControl")]
        private class shipControlled
        {
            private static void Postfix(ref ShipControlls shipControl)
            {
                SendModerationLog("ship controlled " + shipControl.GetShip().gameObject.name + " creator: " +
                                  shipControl.GetShip().m_nview.GetZDO().GetString("creatorName") + " player: " +
                                  Game.instance.GetPlayerProfile().GetName());
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
                    if ((bool) componentInChildren) __result = componentInChildren.transform.gameObject;
                }
            }
        }

        [HarmonyPatch(typeof(VisEquipment), "EnableEquipedEffects")]
        private class VariantColors
        {
            private static void Postfix(ref GameObject instance, ref VisEquipment __instance)
            {
                if ((bool) instance)
                {
                    var component = __instance.gameObject.GetComponent<Player>();
                    if (!(component != null))
                    {
                    }
                }
            }
        }
    }
}