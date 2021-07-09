using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace VMP_Mod.Patches
{
	internal class ListPatches
	{
		[HarmonyPatch(typeof(ZNet), "SendPlayerList")]
		public static class playerlistall
		{
			private static bool Prefix(ref ZNet __instance)
			{
				__instance.UpdatePlayerList();
				if (__instance.m_peers.Count > 0)
				{
					ZPackage zPackage = new ZPackage();
					zPackage.Write(__instance.m_players.Count);
					foreach (ZNet.PlayerInfo player in __instance.m_players)
					{
						zPackage.Write(player.m_name);
						zPackage.Write(player.m_host);
						zPackage.Write(player.m_characterID);
						zPackage.Write(player.m_publicPosition);
						zPackage.Write(player.m_position);
					}
					foreach (ZNetPeer peer in __instance.m_peers)
					{
						if (peer.IsReady())
						{
							peer.m_rpc.Invoke("PlayerList", zPackage);
						}
					}
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(ZNet), "UpdatePlayerList")]
		public static class updateplayerlistall
		{
			private static bool Prefix(ref ZNet __instance)
			{
				__instance.m_players.Clear();
				ZNet.PlayerInfo playerInfo;
				if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
				{
					playerInfo = default(ZNet.PlayerInfo);
					playerInfo.m_name = Game.instance.GetPlayerProfile().GetName();
					playerInfo.m_host = "";
					playerInfo.m_characterID = __instance.m_characterID;
					playerInfo.m_publicPosition = __instance.m_publicReferencePosition;
					playerInfo.m_position = __instance.m_referencePosition;
					ZNet.PlayerInfo item = playerInfo;
					__instance.m_players.Add(item);
				}
				foreach (ZNetPeer peer in __instance.m_peers)
				{
					if (peer.IsReady())
					{
						playerInfo = default(ZNet.PlayerInfo);
						playerInfo.m_characterID = peer.m_characterID;
						playerInfo.m_name = peer.m_playerName;
						playerInfo.m_host = peer.m_socket.GetHostName();
						playerInfo.m_publicPosition = peer.m_publicRefPos;
						playerInfo.m_position = peer.m_refPos;
						ZNet.PlayerInfo item2 = playerInfo;
						__instance.m_players.Add(item2);
					}
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(ZNet), "RPC_PlayerList")]
		public class PatchRPCPlayerlist
		{
			private static bool Prefix(ref ZRpc rpc, ref ZPackage pkg, ref ZNet __instance)
			{
				__instance.m_players.Clear();
				int num = pkg.ReadInt();
				for (int i = 0; i < num; i++)
				{
					ZNet.PlayerInfo playerInfo = default(ZNet.PlayerInfo);
					playerInfo.m_name = pkg.ReadString();
					playerInfo.m_host = pkg.ReadString();
					playerInfo.m_characterID = pkg.ReadZDOID();
					playerInfo.m_publicPosition = pkg.ReadBool();
					playerInfo.m_position = pkg.ReadVector3();
					ZNet.PlayerInfo item = playerInfo;
					__instance.m_players.Add(item);
				}
				return false;
			}
		}
	}
}
