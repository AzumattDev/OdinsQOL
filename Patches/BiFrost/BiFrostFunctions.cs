/*using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OdinQOL.Patches.BiFrost;

public class BiFrostFunctions
{
    internal static void DestroyAll(GameObject thing)
    {
        Object.Destroy(thing.transform.Find("Join manually").gameObject);
        Object.Destroy(thing.transform.Find("FilterField").gameObject);
        Object.Destroy(thing.transform.Find("Refresh").gameObject);
        Object.Destroy(thing.transform.Find("FriendGames").gameObject);
        Object.Destroy(thing.transform.Find("PublicGames").gameObject);
        Object.Destroy(thing.transform.Find("Server help").gameObject);
        Object.Destroy(thing.transform.Find("Back").gameObject);
        Object.Destroy(thing.transform.Find("Join").gameObject);
    }


    internal static void PopulateServerList(GameObject linkpanel)
    {
        OdinQOLplugin.QOLLogger.LogDebug("POPULATE SERVER LIST");
        BiFrostSetupGui.MServerListElement = linkpanel.transform.Find("ServerList/ServerElement").gameObject;
        linkpanel.transform.Find("ServerList").gameObject.GetComponent<Image>().enabled = false;
        GameObject? listRoot = GameObject.Find("GuiRoot/GUI/StartGui/BiFrost/JoinPanel(Clone)/ServerList/ListRoot")
            .gameObject;
        listRoot.gameObject.transform.localScale = new Vector3(1, (float)0.8, 1);
        BiFrostSetupGui.MServerListRoot = listRoot
            .GetComponent<RectTransform>();
        listRoot.gameObject.GetComponent<RectTransform>().pivot =
            new Vector2(listRoot.gameObject.GetComponent<RectTransform>().pivot.x,
                1); // Literally here just because Valheim's UI forces scrollbar to halfway.
        BiFrostSetupGui.MServerCount = linkpanel.transform.Find("serverCount").gameObject.GetComponent<Text>();
        BiFrostSetupGui.MServerListBaseSize = BiFrostSetupGui.MServerListRoot.rect.height;
    }

    internal static void UpdateServerList()
    {
        OdinQOLplugin.QOLLogger.LogDebug("UPDATE SERVER LIST");
        BiFrostSetupGui.MServerList.Clear();
        if (BiFrostSetupGui.Connecting != null)
        {
            OdinQOLplugin.QOLLogger.LogDebug("Connecting not null");
            AbortConnect();
        }
        else if (BiFrostServers.entries.Count > 0)
        {
            foreach (BiFrostDefinition Definition in BiFrostServers.entries)
            {
                BiFrostSetupGui.MServerList.Add(Definition);
            }
        }
        else
        {
            OdinQOLplugin.QOLLogger.LogError("No servers defined");
            OdinQOLplugin.QOLLogger.LogError($"Please create this file {BiFrostServers.ConfigPath}");
        }

        BiFrostSetupGui.MServerList.Sort((Comparison<BiFrostDefinition>)((a, b) =>
            string.Compare(a?.serverName, b?.serverName, StringComparison.Ordinal)));
        if (BiFrostSetupGui.MJoinServer != null && !BiFrostSetupGui.MServerList.Contains(BiFrostSetupGui.MJoinServer))
        {
            OdinQOLplugin.QOLLogger.LogDebug(
                "Server list does not contain selected server, clearing selected server");
            BiFrostSetupGui.MJoinServer =
                BiFrostSetupGui.MServerList.Count <= 0 ? null : BiFrostSetupGui.MServerList[0];
        }

        UpdateServerListGui(false);
    }

    private static void UpdateServerListGui(bool centerSelection)
    {
        if (BiFrostSetupGui.MServerList.Count != BiFrostSetupGui.MServerListElements.Count)
        {
            OdinQOLplugin.QOLLogger.LogDebug("UPDATE SERVER LIST GUI");
            foreach (GameObject? serverListElement in BiFrostSetupGui.MServerListElements)
                Object.Destroy(serverListElement);
            BiFrostSetupGui.MServerListElements.Clear();
            BiFrostSetupGui.MServerListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Max(BiFrostSetupGui.MServerListBaseSize,
                    BiFrostSetupGui.MServerList.Count * BiFrostSetupGui.m_serverListElementStep));
            for (int index = 0; index < BiFrostSetupGui.MServerList.Count; ++index)
            {
                GameObject? gameObject = Object.Instantiate(BiFrostSetupGui.MServerListElement,
                    BiFrostSetupGui.MServerListRoot);
                gameObject.SetActive(true);
                ((gameObject.transform as RectTransform)!).anchoredPosition =
                    new Vector2(0.0f, index * -BiFrostSetupGui.m_serverListElementStep);
                gameObject.GetComponent<Button>().onClick.AddListener(OnSelectedServer);
                BiFrostSetupGui.MServerListElements.Add(gameObject);
                if (BiFrostSetupGui.MServerListElements.Count > 1)
                {
                    if (BiFrostSetupGui.MServerCount != null)
                        BiFrostSetupGui.MServerCount.text = BiFrostSetupGui.MServerListElements.Count + " Servers";
                }
                else
                {
                    if (BiFrostSetupGui.MServerCount != null)
                        BiFrostSetupGui.MServerCount.text = BiFrostSetupGui.MServerListElements.Count + " Server";
                }
            }
        }

        OdinQOLplugin.QOLLogger.LogDebug($"ServerList count: {BiFrostSetupGui.MServerList.Count}");
        for (int index = 0; index < BiFrostSetupGui.MServerList.Count; ++index)
        {
            BiFrostDefinition server = BiFrostSetupGui.MServerList[index];
            GameObject? serverListElement = BiFrostSetupGui.MServerListElements?[index];
            if (serverListElement == null) continue;
            serverListElement.GetComponentInChildren<Text>().text =
                index + 1 + ". " + server.serverName;
            serverListElement.GetComponentInChildren<UITooltip>().m_text = server.ToString();
            serverListElement.transform.Find("version").GetComponent<Text>().text = server.address;
            serverListElement.transform.Find("players").GetComponent<Text>().text = server.port.ToString();
            serverListElement.transform.Find("Private").gameObject
                .SetActive(server.password.Length > 1);
            serverListElement.transform.Find("PVP").gameObject
                .SetActive(server.ispvp);
            Transform target = serverListElement.transform.Find("selected");

            bool flag = BiFrostSetupGui.MJoinServer != null && BiFrostSetupGui.MJoinServer.Equals(server);
            target.gameObject.SetActive(flag);
        }
    }

    private static void Connect(BiFrostDefinition server)
    {
        OdinQOLplugin.QOLLogger.LogDebug("DO CONNECT");
        BiFrostSetupGui.Connecting = server;
        try
        {
            IPAddress address = IPAddress.Parse(server.address);
            if (!JoinServer(address, server.port))
            {
                BiFrostSetupGui.Connecting = null;
                OdinQOLplugin.QOLLogger.LogError("Server address was not valid");
            }
        }
        catch (FormatException)
        {
            OdinQOLplugin.QOLLogger.LogDebug("Resolving: " + server.address);
            try
            {
                BiFrostSetupGui.ResolveTask = Dns.GetHostEntryAsync(server.address);
                OdinQOLplugin.QOLLogger.LogDebug("Resolving after task: " +
                                                 BiFrostSetupGui.ResolveTask.Result.AddressList[0]);
            }
            catch (Exception)
            {
                OdinQOLplugin.QOLLogger.LogError(
                    $"You are trying to resolve the IP : {server.address}, but something is happening causing it to not work properly.");
            }

            if (BiFrostSetupGui.ResolveTask == null)
            {
                OdinQOLplugin.QOLLogger.LogError("Your resolve task was null, fix it you idiot");
                return;
            }

            if (BiFrostSetupGui.ResolveTask.IsFaulted)
            {
                OdinQOLplugin.QOLLogger.LogError($"Error resolving IP: {BiFrostSetupGui.ResolveTask.Exception}");
                OdinQOLplugin.QOLLogger.LogError(BiFrostSetupGui.ResolveTask.Exception?.InnerException != null
                    ? BiFrostSetupGui.ResolveTask.Exception.InnerException.Message
                    : BiFrostSetupGui.ResolveTask.Exception?.Message);
                BiFrostSetupGui.ResolveTask = null;
                BiFrostSetupGui.Connecting = null;
            }
            else if (BiFrostSetupGui.ResolveTask.IsCanceled)
            {
                OdinQOLplugin.QOLLogger.LogError($"Error CANCELED: {BiFrostSetupGui.ResolveTask.Result.HostName}");
                BiFrostSetupGui.ResolveTask = null;
                BiFrostSetupGui.Connecting = null;
            }
            else if (BiFrostSetupGui.ResolveTask.IsCompleted)
            {
                OdinQOLplugin.QOLLogger.LogDebug("COMPLETE: " + server.address);
                foreach (IPAddress address in BiFrostSetupGui.ResolveTask.Result.AddressList)
                {
                    OdinQOLplugin.QOLLogger.LogDebug($"Resolved Completed: {address}");
                    BiFrostSetupGui.ResolveTask = null;
                    if (!JoinServer(address, server.port))
                    {
                        BiFrostSetupGui.Connecting = null;
                        OdinQOLplugin.QOLLogger.LogError("Server DNS resolved to invalid address");
                    }

                    return;
                }
            }
            else
            {
                BiFrostSetupGui.ResolveTask = null;
                BiFrostSetupGui.Connecting = null;
                OdinQOLplugin.QOLLogger.LogError("Server DNS resolved to no valid addresses");
            }
        }
    }

    private static bool JoinServer(IPAddress address, ushort port)
    {
        string target = (address.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{address}]" : $"{address}") +
                        $":{port}";
        OdinQOLplugin.QOLLogger.LogDebug($"Server and Port passed into JoinServer: {target}");

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            address = address.MapToIPv6();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return false;
        }

        ZSteamMatchmaking.instance.m_joinAddr.SetIPv6(address.GetAddressBytes(), port);
        ZSteamMatchmaking.instance.m_haveJoinAddr = true;
        return true;
    }

    public static string? CurrentPass() => BiFrostSetupGui.Connecting?.password;

    public static void AbortConnect()
    {
        OdinQOLplugin.QOLLogger.LogDebug("ABORT CONNECT");
        BiFrostSetupGui.Connecting = null;
        BiFrostSetupGui.ResolveTask = null;
    }

    private static void OnSelectedServer()
    {
        OdinQOLplugin.QOLLogger.LogDebug("SELECTED SERVER");
        BiFrostSetupGui.MJoinServer =
            BiFrostSetupGui.MServerList[FindSelectedServer(EventSystem.current.currentSelectedGameObject)];
        Connect(new BiFrostDefinition
        {
            serverName = BiFrostSetupGui.MJoinServer.serverName, address = BiFrostSetupGui.MJoinServer.address,
            port = BiFrostSetupGui.MJoinServer.port,
            password = BiFrostSetupGui.MJoinServer.password
        });
        UpdateServerListGui(false);
    }

    private static int FindSelectedServer(Object button)
    {
        OdinQOLplugin.QOLLogger.LogDebug("FIND SELECTED");
        for (int index = 0; index < BiFrostSetupGui.MServerListElements.Count; ++index)
        {
            if (BiFrostSetupGui.MServerListElements[index] == button)
                return index;
        }

        return -1;
    }
}*/