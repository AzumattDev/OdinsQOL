using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HarmonyLib;

namespace vrp.Patches
{
    internal class ServerPatches
    {
        [HarmonyPatch(typeof(ZNet), "Awake")]
        public static class Znetawakfix
        {
            private static void Postfix()
            {
                if (ZNet.instance.IsServer())
                {
                    fresh = new SyncedList(Utils.GetSaveDataPath() + "/fresh.txt", "List fresh players ID|name ONE per line");
                }
            }
        }

        [HarmonyPatch(typeof(ZDOMan))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(int) })]
        public static class AddCustomeRPCHere
        {
            private static void Postfix()
            {
                ZRoutedRpc.instance.Register<string>("ServerModerationLog", RPC_ModerationLog);
                ZRoutedRpc.instance.Register<string>("ServerAddFresh", RPC_AddFresh);
            }
        }

        [HarmonyPatch(typeof(ZDOMan), "AddPeer")]
        public static class PeerAdded
        {
            private static void Postfix(ZNetPeer netPeer, ZDOMan __instance)
            {
                RPC_ModerationLog(__instance.m_myid, netPeer.m_socket.GetHostName() + "|" + netPeer.m_playerName + " Connected");
            }
        }

        [HarmonyPatch(typeof(ZDOMan), "RemovePeer")]
        public static class PeerRemoved
        {
            private static void Postfix(ZNetPeer netPeer, ZDOMan __instance)
            {
                RPC_ModerationLog(__instance.m_myid, netPeer.m_socket.GetHostName() + "|" + netPeer.m_playerName + " Disconnected");
            }
        }

        [HarmonyPatch(typeof(ZNet), "IsAllowed")]
        public static class CharWhitelist
        {
            private static bool Prefix(ref bool __result, string hostName, string playerName, ZNet __instance)
            {
                string text = hostName + "|" + playerName;
                __result = !__instance.m_bannedList.Contains(hostName) && !__instance.m_bannedList.Contains(playerName) && (__instance.m_permittedList.Count() <= 0 || __instance.m_permittedList.Contains(text));
                if (!__result)
                {
                    RPC_ModerationLog(0L, "IsAllowed failed: " + text);
                }
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
                    {
                        foreach (ZNetPeer item in __instance.m_peers.ToList())
                        {
                            if (item.IsReady() && !item.m_characterID.IsNone())
                            {
                                string text = item.m_socket.GetHostName() + "|" + item.m_playerName;
                                ZDO zDO = __instance.m_zdoMan.GetZDO(item.m_characterID);
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
                        }
                    }
                    foreach (string item2 in timers.Keys.ToList())
                    {
                        timers[item2] -= 1;
                        if (timers[item2] < 0)
                        {
                            timers.Remove(item2);
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(ZNet), "SaveWorldThread")]
        public static class saferSaveWorldThread
        {
            private static bool Prefix(ZNet __instance)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    string dBPath = ZNet.m_world.GetDBPath();
                    string text = dBPath + ".new";
                    string text2 = dBPath + ".old" + now.ToString("HH");
                    Directory.CreateDirectory(ZNet.m_world.m_worldSavePath);
                    FileStream fileStream = File.Create(text);
                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                    binaryWriter.Write(Version.m_worldVersion);
                    binaryWriter.Write(__instance.m_netTime);
                    __instance.m_zdoMan.SaveAsync(binaryWriter);
                    ZoneSystem.instance.SaveASync(binaryWriter);
                    RandEventSystem.instance.SaveAsync(binaryWriter);
                    binaryWriter.Flush();
                    fileStream.Flush(flushToDisk: true);
                    binaryWriter.Close();
                    binaryWriter.Dispose();
                    fileStream.Close();
                    fileStream.Dispose();
                    Thread.Sleep(1000);
                    if (File.Exists(dBPath))
                    {
                        if (File.Exists(text2))
                        {
                            File.Delete(text2);
                            Thread.Sleep(1000);
                        }
                        File.Move(dBPath, text2);
                        Thread.Sleep(1000);
                    }
                    File.Move(text, dBPath);
                    ZNet.m_world.SaveWorldMetaData();
                    ZLog.Log("World saved ( " + (DateTime.Now - now).TotalMilliseconds + "ms )");
                }
                catch (Exception ex)
                {
                    ZLog.Log("SaveWorldThread EXCEPTION");
                    ZLog.LogError(ex.Message);
                    ZLog.LogError(ex.StackTrace);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(World), "SaveWorldMetaData")]
        public static class saferSaveWorldMetaData
        {
            private static bool Prefix(World __instance)
            {
                try
                {
                    Directory.CreateDirectory(__instance.m_worldSavePath);
                    ZPackage zPackage = new ZPackage();
                    zPackage.Write(Version.m_worldVersion);
                    zPackage.Write(__instance.m_name);
                    zPackage.Write(__instance.m_seedName);
                    zPackage.Write(__instance.m_seed);
                    zPackage.Write(__instance.m_uid);
                    zPackage.Write(__instance.m_worldGenVersion);
                    DateTime now = DateTime.Now;
                    string metaPath = __instance.GetMetaPath();
                    string text = metaPath + ".new";
                    string text2 = metaPath + ".old" + now.ToString("HH");
                    byte[] array = zPackage.GetArray();
                    FileStream fileStream = File.Create(text);
                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                    binaryWriter.Write(array.Length);
                    binaryWriter.Write(array);
                    binaryWriter.Flush();
                    fileStream.Flush(flushToDisk: true);
                    binaryWriter.Close();
                    binaryWriter.Dispose();
                    fileStream.Close();
                    fileStream.Dispose();
                    if (File.Exists(metaPath))
                    {
                        if (File.Exists(text2))
                        {
                            File.Delete(text2);
                            Thread.Sleep(1000);
                        }
                        File.Move(metaPath, text2);
                        Thread.Sleep(1000);
                    }
                    File.Move(text, metaPath);
                }
                catch (Exception ex)
                {
                    ZLog.Log("SaveWorldMetaData EXCEPTION");
                    ZLog.LogError(ex.Message);
                    ZLog.LogError(ex.StackTrace);
                }
                return false;
            }
        }

        public static SyncedList fresh;


        public static Dictionary<string, int> timers = new Dictionary<string, int>();

        public static void RPC_AddFresh(long sender, string playername)
        {
            ZLog.Log("RPC_AddFresh " + playername);
            if (ZNet.instance != null && ZNet.instance.IsServer() && !string.IsNullOrWhiteSpace(playername))
            {
                ZNetPeer peerByPlayerName = ZNet.instance.GetPeerByPlayerName(playername);
                if (peerByPlayerName?.IsReady() ?? false)
                {
                    string text = peerByPlayerName.m_socket.GetHostName() + "|" + peerByPlayerName.m_playerName;
                    fresh.Add(text);
                    RPC_ModerationLog(0L, text + " added to fresh list");
                }
            }
        }

        public static void RPC_ModerationLog(long sender, string msg)
        {
            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                string path = Utils.GetSaveDataPath() + "/PlayerAuditLog.txt";
                using StreamWriter streamWriter = new StreamWriter(path, append: true);
                streamWriter.WriteLine(DateTime.Now.ToUniversalTime().ToString() + " " + msg);
            }
        }
    }
}
