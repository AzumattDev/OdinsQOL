using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace OdinQOL.Patches
{
    internal class ServerPatches
    {
        public static Dictionary<string, int> timers = new();


        public static void RPC_ModerationLog(long sender, string msg)
        {
            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                string? path = Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + "/PlayerAuditLog.txt";
                using StreamWriter? streamWriter = new StreamWriter(path, true);
                streamWriter.WriteLine(DateTime.Now.ToUniversalTime() + " " + msg);
            }
        }

        [HarmonyPatch(typeof(ZDOMan))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new[] { typeof(int) })]
        public static class AddCustomeRPCHere
        {
            private static void Postfix()
            {
                ZRoutedRpc.instance.Register<string>("ServerModerationLog", RPC_ModerationLog);
            }
        }

        [HarmonyPatch(typeof(ZDOMan), "AddPeer")]
        public static class PeerAdded
        {
            private static void Postfix(ZNetPeer netPeer, ZDOMan __instance)
            {
                RPC_ModerationLog(__instance.m_myid,
                    netPeer.m_socket.GetHostName() + "|" + netPeer.m_playerName + " Connected");
            }
        }

        [HarmonyPatch(typeof(ZDOMan), "RemovePeer")]
        public static class PeerRemoved
        {
            private static void Postfix(ZNetPeer netPeer, ZDOMan __instance)
            {
                RPC_ModerationLog(__instance.m_myid,
                    netPeer.m_socket.GetHostName() + "|" + netPeer.m_playerName + " Disconnected");
            }
        }

        /*[HarmonyPatch(typeof(ZNet), "IsAllowed")]
        public static class CharWhitelist
        {
            private static bool Prefix(ref bool __result, string hostName, string playerName, ZNet __instance)
            {
                var text = hostName + "|" + playerName;
                __result = !__instance.m_bannedList.Contains(hostName) &&
                           !__instance.m_bannedList.Contains(playerName) && (__instance.m_permittedList.Count() <= 0 ||
                                                                             __instance.m_permittedList.Contains(text));
                if (!__result) RPC_ModerationLog(0L, "IsAllowed failed: " + text);
                return false;
            }
        }

        [HarmonyPatch(typeof(ZNet), "CheckWhiteList")]
        public static class CheckCharWhitelist
        {
            private static bool Prefix(ZNet __instance)
            {
                if (__instance.m_permittedList != null && __instance.m_peers != null)
                {
                    if (__instance.m_permittedList.Count() > 0 && __instance.m_peers.Count > 0)
                        foreach (var item in __instance.m_peers.ToList())
                            if (item.IsReady() && !item.m_characterID.IsNone())
                            {
                                var text = item.m_socket.GetHostName() + "|" + item.m_playerName;
                                var zDO = __instance.m_zdoMan.GetZDO(item.m_characterID);
                                if (zDO != null && zDO.GetBool("dead") && !timers.ContainsKey(text))
                                {
                                    RPC_ModerationLog(0L, text + "DEAD");
                                    timers.Add(text, 5);
                                }

                                if (!__instance.m_permittedList.Contains(text))
                                {
                                    ZLog.Log("Kicking player not in permitted list " + text);
                                    RPC_ModerationLog(0L, "Kicking player not in permitted list " + text);
                                    __instance.InternalKick(item);
                                }
                            }

                    foreach (var item2 in timers.Keys.ToList())
                    {
                        timers[item2] -= 1;
                        if (timers[item2] < 0) timers.Remove(item2);
                    }
                }

                return false;
            }
        }*/
    }
}