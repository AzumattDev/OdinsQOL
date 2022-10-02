using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OdinQOL.Patches.BiFrost;

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnSelelectCharacterBack))]
internal class BiFrostPatchCharacterBack
{
    private static void Postfix()
    {
        if (!BiFrostSetupGui.BF)
        {
            return;
        }

        BiFrostFunctions.AbortConnect();
    }
}

[HarmonyPatch(typeof(ZSteamMatchmaking), nameof(ZSteamMatchmaking.OnJoinServerFailed))]
internal class BiFrostPatchConnectFailed
{
    private static void Postfix()
    {
        if (!BiFrostSetupGui.BF)
        {
            return;
        }

        JoinServerFailed();
    }

    private static void JoinServerFailed()
    {
        OdinQOLplugin.QOLLogger.LogError("Server connection failed");
        BiFrostSetupGui.Connecting = null;
    }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_ClientHandshake))]
internal class BiFrostPatchPasswordPrompt
{
    private static bool Prefix(ZNet __instance, ZRpc rpc, bool needPassword, string serverPasswordSalt)
    {
        if (BiFrost.ShowPasswordPrompt.Value) return true;
        string? str = BiFrostFunctions.CurrentPass();
        if (string.IsNullOrEmpty(str)) return true;
        ZNet.m_serverPasswordSalt = serverPasswordSalt;
        if (needPassword)
        {
            OdinQOLplugin.QOLLogger.LogDebug($"Authenticating with saved password...{str}");
            __instance.m_connectingDialog.gameObject.SetActive(false);
            __instance.SendPeerInfo(rpc, str);
            return false;
        }

        OdinQOLplugin.QOLLogger.LogDebug("Server didn't want password?");
        return true;
    }
}

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.SetupGui))]
internal class BiFrostSetupGui
{
    public static GameObject BF = null!;
    public static GameObject BFRootGo = null!;
    public static readonly List<BiFrostDefinition> MServerList = new();
    public static BiFrostDefinition? MJoinServer = new();
    public static readonly List<GameObject> MServerListElements = new();
    public static Text? MServerCount;
    public static GameObject MServerListElement = new();
    public static float m_serverListElementStep = 28f;
    public static RectTransform MServerListRoot = new();
    public static float MServerListBaseSize;

    public static Task<IPHostEntry>? ResolveTask;
    public static BiFrostDefinition? Connecting;

    private static void Postfix(FejdStartup __instance)
    {
        Connecting = null;
        foreach (GameObject serverListElement in MServerListElements)
            Object.DestroyImmediate(serverListElement);
        MServerListElements.Clear();

        BiFrostServers.Init();
        if (BiFrost.DisableBiFrost.Value) return;

        BFRootGo = new GameObject("BiFrost");
        BFRootGo.AddComponent<RectTransform>();
        BFRootGo.AddComponent<DragControl>();
        BFRootGo.transform.SetParent(GameObject.Find("GuiRoot/GUI/StartGui").transform);

        BF = Object.Instantiate(GameObject.Find("GUI/StartGui/StartGame/Panel/JoinPanel").gameObject,
            BFRootGo.transform);
        Object.DestroyImmediate(BF.gameObject.GetComponent<TabHandler>());
        Object.DestroyImmediate(BF.gameObject.GetComponent<ServerList>());
        Object.DestroyImmediate(BF.gameObject.GetComponent<UIGamePad>());
        BF.transform.SetParent(BFRootGo.transform);
        BF.gameObject.transform.localScale = new Vector3((float)0.85, (float)0.85, (float)0.85);
        BFRootGo.transform.position =
            new Vector2(BiFrost.UIAnchor.Value.x, BiFrost.UIAnchor.Value.y);


        if (!BF.activeSelf)
            BF.SetActive(true);


        /* Set Mod Text */
        BF.transform.Find("topic").GetComponent<Text>().text = "Bifröst";

        try
        {
            BiFrostFunctions.DestroyAll(BF);
        }
        catch (Exception e)
        {
            OdinQOLplugin.QOLLogger.LogError(e);
            throw;
        }

        BiFrostFunctions.PopulateServerList(BF);
        BiFrostFunctions.UpdateServerList();
    }
}

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.LoadMainScene))]
static class FejdStartup_LoadMainScene_Patch
{
    static void Postfix(FejdStartup __instance)
    {
        if (BiFrost.DisableBiFrost.Value) return;
        if (BiFrostSetupGui.BF.activeSelf)
            BiFrostSetupGui.BF.SetActive(false);
    }
}