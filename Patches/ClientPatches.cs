using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace VMP_Mod
{
	public class ClientPatches
	{

		[HarmonyPatch(typeof(Ship), "Awake")]
		public static class shipfix
		{
			private static void Postfix(ref Ship __instance)
			{
				__instance.m_minWaterImpactForce = 100f;
			}
		}

		[HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
		public static class DamageWard
		{
			private static bool Prefix(long sender, HitData hit, WearNTear __instance)
			{
				bool result = true;
				if (hit != null && hit.GetAttacker() != null && hit.GetAttacker().IsPlayer() && __instance.gameObject != null)
				{
					Player player = (Player)hit.GetAttacker();
					if (__instance.gameObject.name.Contains("piece_chest") || __instance.gameObject.name.Contains("guard_stone"))
					{
						foreach (PrivateArea allArea in PrivateArea.m_allAreas)
						{
							if (allArea.IsEnabled() && allArea.IsInside(hit.m_point, 1f))
							{
								if (player.GetPlayerName().ToUpper().Contains("ADMIN"))
								{
									result = true;
								}
								else if (allArea.m_piece.m_creator != player.GetPlayerID() && !allArea.IsPermitted(player.GetPlayerID()))
								{
									allArea.FlashShield(flashConnected: false);
									result = false;
								}
							}
						}
					}
				}
				return result;
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

		[HarmonyPatch(typeof(Humanoid), "UnequipItem")]
		public static class PlayerUnEquip
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
					string text = ((item.m_crafterID == 0L) ? " : uncrafted : " : (" : crafted by : " + item.m_crafterName + " "));
					SendModerationLog(__instance.GetPlayerName() + " : consume : " + item.m_shared.m_name + text);
				}
			}
		}


		[HarmonyPatch(typeof(Player), "SetIntro")]
		public static class SpawnLog
		{
			private static void Postfix(bool intro, Player __instance)
			{
				if (!ZNet.instance.IsServer() && __instance != null && __instance.m_nview.IsValid() && ZRoutedRpc.instance != null)
				{
					ZLog.LogWarning("FRESH " + __instance.GetPlayerName());
					SendModerationLog(__instance.GetPlayerName() + " : FRESH CHARACTER");
					ZRoutedRpc.instance.InvokeRoutedRPC("ServerAddFresh", __instance.GetPlayerName());
				}
			}
		}

        [HarmonyPatch(typeof(ZSteamMatchmaking))]
        [HarmonyPatch("GetServers")]
        private class PatchUpdateServerListGui
        {
            private static void Postfix(ref List<ServerData> allServers)
            {
                ServerData serverData = new ServerData();
                serverData.m_host = "23.23.255.113";
                serverData.m_name = "VMP A PVE Server";
                serverData.m_password = false;
                serverData.m_players = 100;
                serverData.m_port = 2456;
                serverData.m_steamHostID = 0uL;
                serverData.m_steamHostAddr = default(SteamNetworkingIPAddr);
                serverData.m_steamHostAddr.ParseString(serverData.m_host + ":" + serverData.m_port);
                serverData.m_upnp = true;
                serverData.m_version = "";
                allServers.Insert(0, serverData);
            }
        }


		[HarmonyPatch(typeof(WearNTear), "OnPlaced")]
		private class shipCreated
		{
			private static void Postfix(ref WearNTear __instance)
			{
				if ((bool)__instance)
				{
					Ship component = __instance.gameObject.GetComponent<Ship>();
					if ((bool)component && (bool)component.m_nview && component.m_nview.IsValid())
					{
						component.m_nview.GetZDO().Set("creatorName", Game.instance.GetPlayerProfile().GetName());
						SendModerationLog("ship created " + component.gameObject.name + " " + Game.instance.GetPlayerProfile().GetName());
					}
				}
			}
		}

		[HarmonyPatch(typeof(Ship), "OnDestroyed")]
		private class shipDestroyed
		{
			private static void Postfix(ref Ship __instance)
			{
				if (__instance.IsOwner() && (bool)__instance.m_nview && __instance.m_nview.IsValid())
				{
					List<Player> list = new List<Player>();
					Player.GetPlayersInRange(__instance.transform.position, 20f, list);
					SendModerationLog("ship destroyed " + __instance.gameObject.name + " creator: " + __instance.m_nview.GetZDO().GetString("creatorName") + " around: " + string.Join(",", list.Select((Player p) => p.GetPlayerName())));
				}
			}
		}

		[HarmonyPatch(typeof(Player), "StartShipControl")]
		private class shipControlled
		{
			private static void Postfix(ref ShipControlls shipControl)
			{
				SendModerationLog("ship controlled " + shipControl.GetShip().gameObject.name + " creator: " + shipControl.GetShip().m_nview.GetZDO().GetString("creatorName") + " player: " + Game.instance.GetPlayerProfile().GetName());
			}
		}




		[HarmonyPatch(typeof(ItemStand), "CanAttach")]
		public class ItemStand_CanAttach
		{
			public static void Postfix(ItemStand __instance, ref bool __result)
			{
				if (__instance.m_name == "$piece_itemstand")
				{
					__result = true;
				}
			}
		}

		[HarmonyPatch(typeof(ItemStand), "GetAttachPrefab")]
		public class ItemStand_GetAttachPrefab
		{
			public static void Postfix(GameObject item, ref GameObject __result)
			{
				if (__result == null)
				{
					Collider componentInChildren = item.transform.GetComponentInChildren<Collider>();
					if ((bool)componentInChildren)
					{
						__result = componentInChildren.transform.gameObject;
					}
				}
			}
		}

		[HarmonyPatch(typeof(VisEquipment), "EnableEquipedEffects")]
		private class VariantColors
		{
			private static void Postfix(ref GameObject instance, ref VisEquipment __instance)
			{
				if ((bool)instance)
				{
					Player component = __instance.gameObject.GetComponent<Player>();
					if (!(component != null))
					{
					}
				}
			}
		}

		public static bool admin = false;

		public static string playerClass = "";

		public static bool Contains(string source, string toCheck, StringComparison comp)
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			string text = source.ToUpperInvariant();
			string value = toCheck.ToUpperInvariant();
			return text.Contains(value);
		}

		public static void SendModerationLog(string message)
		{
			if (ZRoutedRpc.instance != null && !ZRoutedRpc.instance.m_server)
			{
				ZRoutedRpc.instance.InvokeRoutedRPC("ServerModerationLog", message);
			}
		}

		public static void playerEqp(ref ItemDrop.ItemData item, ref Humanoid huma)
		{
			if (item != null && item.IsEquipable() && huma != null && huma.IsPlayer())
			{
				Player player = (Player)huma;
				if (player.m_nview.IsValid() && Player.m_localPlayer != null)
				{
					string text = ((item.m_crafterID == 0L) ? " : uncrafted : " : (" : crafted by : " + item.m_crafterName + " "));
				}
			}
		}

	}
}
