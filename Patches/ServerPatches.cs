using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace OdinQOL.Patches
{
    internal class ServerPatches
    {
        public static Dictionary<string, int> Timers = new();


        public static void RPC_ModerationLog(long sender, string msg)
        {
            if (ZNet.instance == null || !ZNet.instance.IsServer()) return;
            string? path = Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + "/PlayerAuditLog.txt";
            using StreamWriter? streamWriter = new(path, true);
            streamWriter.WriteLine(DateTime.Now.ToUniversalTime() + " " + msg);
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

        [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.AddPeer))]
        public static class PeerAdded
        {
            private static void Postfix(ZNetPeer netPeer, ZDOMan __instance)
            {
                RPC_ModerationLog(__instance.m_myid,
                    netPeer.m_socket.GetHostName() + "|" + netPeer.m_playerName + " Connected");
            }
        }

        [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.RemovePeer))]
        public static class PeerRemoved
        {
            private static void Postfix(ZNetPeer netPeer, ZDOMan __instance)
            {
                RPC_ModerationLog(__instance.m_myid,
                    netPeer.m_socket.GetHostName() + "|" + netPeer.m_playerName + " Disconnected");
            }
        }
    }
}