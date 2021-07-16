using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HarmonyLib;

namespace VMP_Mod.Patches
{
    internal class ServerPatches
    {
        public static Dictionary<string, int> timers = new Dictionary<string, int>();


        public static void RPC_ModerationLog(long sender, string msg)
        {
            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                var path = Utils.GetSaveDataPath() + "/PlayerAuditLog.txt";
                using var streamWriter = new StreamWriter(path, true);
                streamWriter.WriteLine(DateTime.Now.ToUniversalTime() + " " + msg);
            }
        }

        [HarmonyPatch(typeof(ZDOMan))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new[] {typeof(int)})]
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

        [HarmonyPatch(typeof(ZNet), "IsAllowed")]
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
        }

        [HarmonyPatch(typeof(ZNet), "SaveWorldThread")]
        public static class saferSaveWorldThread
        {
            private static bool Prefix(ZNet __instance)
            {
                try
                {
                    var now = DateTime.Now;
                    var dBPath = ZNet.m_world.GetDBPath();
                    var text = dBPath + ".new";
                    var text2 = dBPath + ".old" + now.ToString("HH");
                    Directory.CreateDirectory(ZNet.m_world.m_worldSavePath);
                    var fileStream = File.Create(text);
                    var binaryWriter = new BinaryWriter(fileStream);
                    binaryWriter.Write(Version.m_worldVersion);
                    binaryWriter.Write(__instance.m_netTime);
                    __instance.m_zdoMan.SaveAsync(binaryWriter);
                    ZoneSystem.instance.SaveASync(binaryWriter);
                    RandEventSystem.instance.SaveAsync(binaryWriter);
                    binaryWriter.Flush();
                    fileStream.Flush(true);
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
                    var zPackage = new ZPackage();
                    zPackage.Write(Version.m_worldVersion);
                    zPackage.Write(__instance.m_name);
                    zPackage.Write(__instance.m_seedName);
                    zPackage.Write(__instance.m_seed);
                    zPackage.Write(__instance.m_uid);
                    zPackage.Write(__instance.m_worldGenVersion);
                    var now = DateTime.Now;
                    var metaPath = __instance.GetMetaPath();
                    var text = metaPath + ".new";
                    var text2 = metaPath + ".old" + now.ToString("HH");
                    var array = zPackage.GetArray();
                    var fileStream = File.Create(text);
                    var binaryWriter = new BinaryWriter(fileStream);
                    binaryWriter.Write(array.Length);
                    binaryWriter.Write(array);
                    binaryWriter.Flush();
                    fileStream.Flush(true);
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
    }
}