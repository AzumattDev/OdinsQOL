using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches
{
    public class ConnectionPanel
    {
        /* Connect Panel */
        public static ConfigEntry<bool> ServerAdditionToggle;
        public static ConfigEntry<string> ServerIPs;
        public static ConfigEntry<string> ServerNames;
        public static ConfigEntry<string> ServerPorts;

        [HarmonyPatch(typeof(ZSteamMatchmaking))]
        [HarmonyPatch("GetServers")]
        private class PatchUpdateServerListGui
        {
            private static void Postfix(ref List<ServerData> allServers)
            {
                if (!ServerAdditionToggle.Value) return;
                string[] serversArray = ServerIPs.Value.Trim().Split(',').ToArray();
                string[] serversNamesArray = ServerNames.Value.Trim().Split(',').ToArray();
                string[] serversPortsArray = ServerPorts.Value.Trim().Split(',').ToArray();
                int i = 0;
                if (serversArray.Length == serversNamesArray.Length &&
                    serversArray.Length == serversPortsArray.Length)
                    try
                    {
                        foreach (string? serv in serversArray)
                        {
                            ServerData? serverData = new ServerData();
                            serverData.m_host = serv;
                            serverData.m_name = serversNamesArray[i];
                            serverData.m_password = false;
                            serverData.m_players = 999;
                            serverData.m_port = int.Parse(serversPortsArray[i]);
                            serverData.m_steamHostID = 0uL;
                            serverData.m_steamHostAddr = default;
                            serverData.m_steamHostAddr.ParseString(serverData.m_host + ":" + serverData.m_port);
                            serverData.m_upnp = true;
                            serverData.m_version = "";
                            allServers.Insert(0, serverData);
                            ++i;
                        }
                    }
                    catch (Exception exception)
                    {
                        OdinQOLplugin.DbglError(
                            $"There was an issue adding your server listing to the menu. Please check your [Connection Panel] section in the config file for correct length and format {exception}");
                    }
                else
                    OdinQOLplugin.DbglError(
                        "Server IPs, Ports, or Names are not the same length or in an incorrect format. Please Check your [Connection Panel] section in the config and fix the issue.");
            }
        }
    }
}