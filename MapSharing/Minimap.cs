using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VMP_Mod.RPC;
using Object = UnityEngine.Object;

// ToDo add packet system to convey map markers
namespace VMP_Mod.GameClasses
{
    /// <summary>
    ///     Hooks base explore method
    /// </summary>
    [HarmonyPatch(typeof(Minimap))]
    public class HookExplore
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Minimap), "Explore", typeof(Vector3), typeof(float))]
        public static void call_Explore(object instance, Vector3 p, float radius)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///     Update exploration for all players
    /// </summary>
    [HarmonyPatch(typeof(Minimap), "UpdateExplore")]
    public static class ChangeMapBehavior
    {
        private static void Prefix(ref float dt, ref Player player, ref Minimap __instance, ref float ___m_exploreTimer,
            ref float ___m_exploreInterval)
        {
            if (!VMP_Modplugin.mapIsEnabled.Value) return;

            if (VMP_Modplugin.shareMapProgression.Value)
            {
                var explorerTime = ___m_exploreTimer;
                explorerTime += Time.deltaTime;
                if (explorerTime > ___m_exploreInterval)
                    if (ZNet.instance.m_players.Any())
                        foreach (var m_Player in ZNet.instance.m_players)
                            HookExplore.call_Explore(__instance, m_Player.m_position,
                                VMP_Modplugin.exploreRadius.Value);
            }

            // Always reveal for your own, we do this non the less to apply the potentially bigger exploreRadius
            HookExplore.call_Explore(__instance, player.transform.position, VMP_Modplugin.exploreRadius.Value);
        }
    }

    [HarmonyPatch(typeof(Minimap), "Awake")]
    public static class MinimapAwake
    {
        private static void Postfix()
        {
            if (ZNet.m_isServer && VMP_Modplugin.mapIsEnabled.Value && VMP_Modplugin.shareMapProgression.Value)
            {
                //Init map array
                MapSync.ServerMapData = new bool[Minimap.instance.m_textureSize * Minimap.instance.m_textureSize];

                //Load map data from disk
                MapSync.LoadMapDataFromDisk();

                //Start map data save timer
                VMP_Modplugin.mapSyncSaveTimer.Start();
            }
        }
    }

    public static class MapPinEditor_Patches
    {
        public static GameObject pinEditorPanel;
        public static AssetBundle mapPinBundle;
        public static Dropdown iconSelected;
        public static InputField pinName;
        public static Toggle sharePin;
        public static Vector3 pinPos;

        private static AssetBundle GetAssetBundleFromResources(string filename)
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(filename));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
        public static class Minimap_AddPin_Patch
        {
            public static List<Minimap.PinType> shareablePins;

            private static void Postfix(ref Minimap __instance, ref Minimap.PinData __result)
            {
                if (VMP_Modplugin.mapIsEnabled.Value && VMP_Modplugin.shareAllPins.Value)
                    if (shareablePins.Contains(__result.m_type))
                    {
                        if (__instance.m_mode != Minimap.MapMode.Large)
                            VMPMapPinSync.SendMapPinToServer(__result, true);
                        else
                            VMPMapPinSync.SendMapPinToServer(__result);
                    }
            }
        }

        [HarmonyPatch(typeof(Minimap), "Awake")]
        public static class MapPinEditor_Patches_Awake
        {
            public static void AddPin(ref Minimap __instance)
            {
                var pintype = iconSelected.value == 4 ? Minimap.PinType.Icon4 : (Minimap.PinType) iconSelected.value;
                var addedPin = __instance.AddPin(pinPos, pintype, pinName.text, true, false);
                if (VMP_Modplugin.shareablePins.Value && sharePin.isOn && !VMP_Modplugin.shareAllPins.Value)
                    VMPMapPinSync.SendMapPinToServer(addedPin);
                pinEditorPanel.SetActive(false);
                __instance.m_wasFocused = false;
            }

            private static void Postfix(ref Minimap __instance)
            {
                if (VMP_Modplugin.mapIsEnabled.Value)
                {
                    Minimap_AddPin_Patch.shareablePins = new List<Minimap.PinType>
                    {
                        Minimap.PinType.Icon0, Minimap.PinType.Icon1, Minimap.PinType.Icon2,
                        Minimap.PinType.Icon3, Minimap.PinType.Icon4
                    };
                    var iconPanelOld = GameObjectAssistant
                        .GetChildComponentByName<Transform>("IconPanel", __instance.m_largeRoot).gameObject;
                    for (var i = 0; i < 5; i++)
                        GameObjectAssistant.GetChildComponentByName<Transform>("Icon" + i, iconPanelOld).gameObject
                            .SetActive(false);
                    GameObjectAssistant.GetChildComponentByName<Transform>("Bkg", iconPanelOld).gameObject
                        .SetActive(false);
                    iconPanelOld.SetActive(false);
                    __instance.m_nameInput.gameObject.SetActive(false);
                    if (mapPinBundle == null) mapPinBundle = GetAssetBundleFromResources("map-pin-ui");
                    var pinEditorPanelParent = mapPinBundle.LoadAsset<GameObject>("MapPinEditor");
                    pinEditorPanel = Object.Instantiate(pinEditorPanelParent.transform.GetChild(0).gameObject);
                    pinEditorPanel.transform.SetParent(__instance.m_largeRoot.transform, false);
                    var image = pinEditorPanel.GetComponentInChildren<Image>();
                    image.gameObject.SetActive(false);
                    pinEditorPanel.SetActive(false);

                    pinName = pinEditorPanel.GetComponentInChildren<InputField>();
                    if (pinName != null)
                        Debug.Log("Pin Name loaded properly");
                    var theInstance = __instance;
                    GameObjectAssistant.GetChildComponentByName<Transform>("OK", pinEditorPanel).gameObject
                        .GetComponent<Button>().onClick.AddListener(delegate { AddPin(ref theInstance); });
                    GameObjectAssistant.GetChildComponentByName<Transform>("Cancel", pinEditorPanel).gameObject
                        .GetComponent<Button>().onClick.AddListener(delegate
                        {
                            Minimap.instance.m_wasFocused = false;
                            pinEditorPanel.SetActive(false);
                        });
                    iconSelected = pinEditorPanel.GetComponentInChildren<Dropdown>();
                    iconSelected.options.Clear();
                    var ind = 0;
                    var list = new List<string> {"Fire", "Home", "Hammer", "Circle", "Rune"};
                    foreach (var option in list)
                    {
                        iconSelected.options.Add(new Dropdown.OptionData(option, Minimap.instance.m_icons[ind].m_icon));
                        ind++;
                    }

                    if (iconSelected != null)
                        Debug.Log("Dropdown loaded properly");
                    sharePin = pinEditorPanel.GetComponentInChildren<Toggle>();
                    if (sharePin != null)
                        Debug.Log("Share pin loaded properly");
                    if (!VMP_Modplugin.shareablePins.Value || VMP_Modplugin.shareAllPins.Value)
                        sharePin.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(Minimap), "OnMapDblClick")]
        public static class MapPinEditor_Patches_OnMapDblClick
        {
            public static bool imageoff;

            private static bool Prefix(ref Minimap __instance)
            {
                if (VMP_Modplugin.mapIsEnabled.Value)
                {
                    if (pinEditorPanel == null)
                    {
                        Minimap_AddPin_Patch.shareablePins = new List<Minimap.PinType>
                        {
                            Minimap.PinType.Icon0, Minimap.PinType.Icon1, Minimap.PinType.Icon2,
                            Minimap.PinType.Icon3, Minimap.PinType.Icon4
                        };
                        var iconPanelOld = GameObjectAssistant
                            .GetChildComponentByName<Transform>("IconPanel", __instance.m_largeRoot).gameObject;
                        for (var i = 0; i < 5; i++)
                            GameObjectAssistant.GetChildComponentByName<Transform>("Icon" + i, iconPanelOld).gameObject
                                .SetActive(false);
                        GameObjectAssistant.GetChildComponentByName<Transform>("Bkg", iconPanelOld).gameObject
                            .SetActive(false);
                        iconPanelOld.SetActive(false);
                        __instance.m_nameInput.gameObject.SetActive(false);
                        if (mapPinBundle == null) mapPinBundle = GetAssetBundleFromResources("map-pin-ui");
                        var pinEditorPanelParent = mapPinBundle.LoadAsset<GameObject>("MapPinEditor");
                        pinEditorPanel = Object.Instantiate(pinEditorPanelParent.transform.GetChild(0).gameObject);
                        pinEditorPanel.transform.SetParent(__instance.m_largeRoot.transform, false);


                        pinName = pinEditorPanel.GetComponentInChildren<InputField>();
                        if (pinName != null)
                            Debug.Log("Pin Name loaded properly");
                        var theInstance = __instance;
                        GameObjectAssistant.GetChildComponentByName<Transform>("OK", pinEditorPanel).gameObject
                            .GetComponent<Button>().onClick.AddListener(delegate
                            {
                                MapPinEditor_Patches_Awake.AddPin(ref theInstance);
                            });
                        GameObjectAssistant.GetChildComponentByName<Transform>("Cancel", pinEditorPanel).gameObject
                            .GetComponent<Button>().onClick.AddListener(delegate
                            {
                                Minimap.instance.m_wasFocused = false;
                                pinEditorPanel.SetActive(false);
                            });
                        iconSelected = pinEditorPanel.GetComponentInChildren<Dropdown>();
                        iconSelected.options.Clear();
                        var ind = 0;
                        var list = new List<string> {"Fire", "Home", "Hammer", "Circle", "Rune"};
                        foreach (var option in list)
                        {
                            iconSelected.options.Add(new Dropdown.OptionData(option,
                                Minimap.instance.m_icons[ind].m_icon));
                            ind++;
                        }

                        if (iconSelected != null)
                            Debug.Log("Dropdown loaded properly");
                        sharePin = pinEditorPanel.GetComponentInChildren<Toggle>();
                        if (sharePin != null)
                            Debug.Log("Share pin loaded properly");
                        if (!VMP_Modplugin.shareablePins.Value || VMP_Modplugin.shareAllPins.Value)
                            sharePin.gameObject.SetActive(false);
                    }

                    if (!pinEditorPanel.activeSelf)
                    {
                        pinEditorPanel.SetActive(true);

                        if (imageoff == false)
                        {
                            var title = pinEditorPanel.transform.Find("Title");
                            var picture = title.GetComponentInChildren<Image>();
                            picture.gameObject.SetActive(false);
                            imageoff = true;
                        }
                    }

                    if (!pinName.isFocused) EventSystem.current.SetSelectedGameObject(pinName.gameObject);
                    pinPos = __instance.ScreenToWorldPoint(Input.mousePosition);
                    __instance.m_wasFocused = true;
                    //iconPanel.transform.localPosition = pinPos;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Minimap), "UpdateNameInput")]
        public static class MapPinEditor_Patches_UpdateNameInput
        {
            private static bool Prefix(ref Minimap __instance)
            {
                if (VMP_Modplugin.mapIsEnabled.Value)
                    // Break out of this unnecessary thing
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.InTextInput))]
        public static class MapPinEditor_InTextInput_Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (VMP_Modplugin.mapIsEnabled.Value)
                {
                    __result = Minimap.m_instance.m_mode == Minimap.MapMode.Large && Minimap.m_instance.m_wasFocused;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Update))]
        public static class MapPinEditor_Update_Patch
        {
            private static void Postfix(ref Minimap __instance)
            {
                if (VMP_Modplugin.mapIsEnabled.Value)
                    if (Minimap.InTextInput())
                    {
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            Minimap.instance.m_wasFocused = false;
                            pinEditorPanel.SetActive(false);
                        }
                        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            MapPinEditor_Patches_Awake.AddPin(ref __instance);
                        }
                    }
            }
        }
    }


    /// <summary>
    ///     Show boats and carts on map
    /// </summary>
    public class displayCartsAndBoatsOnMap
    {
        private static readonly Dictionary<ZDO, Minimap.PinData> customPins = new Dictionary<ZDO, Minimap.PinData>();
        private static readonly Dictionary<int, Sprite> icons = new Dictionary<int, Sprite>();
        private static readonly int CartHashcode = "Cart".GetStableHashCode();
        private static readonly int RaftHashcode = "Raft".GetStableHashCode();
        private static readonly int KarveHashcode = "Karve".GetStableHashCode();
        private static readonly int LongshipHashcode = "VikingShip".GetStableHashCode();
        private static readonly int hammerHashCode = "Hammer".GetStableHashCode();
        private static readonly float updateInterval = 5.0f;

        // clear dictionary if the user logs out
        [HarmonyPatch(typeof(Minimap), "OnDestroy")]
        public static class Minimap_OnDestroy_Patch
        {
            private static void Postfix()
            {
                customPins.Clear();
                icons.Clear();
            }
        }

        [HarmonyPatch(typeof(Minimap), "UpdateMap")]
        public static class Minimap_UpdateMap_Patch
        {
            private static float timeCounter = updateInterval;

            private static void FindIcons()
            {
                var hammer = ObjectDB.instance.m_itemByHash[hammerHashCode];
                if (!hammer)
                    return;
                var hammerDrop = hammer.GetComponent<ItemDrop>();
                if (!hammerDrop)
                    return;
                var hammerPieceTable = hammerDrop.m_itemData.m_shared.m_buildPieces;
                foreach (var piece in hammerPieceTable.m_pieces)
                {
                    var p = piece.GetComponent<Piece>();
                    icons.Add(p.name.GetStableHashCode(), p.m_icon);
                }
            }

            private static bool CheckPin(Minimap __instance, Player player, ZDO zdo, int hashCode, string pinName)
            {
                if (zdo.m_prefab != hashCode)
                    return false;

                Minimap.PinData customPin;
                var pinWasFound = customPins.TryGetValue(zdo, out customPin);

                // turn off associated pin if player controlled ship is in that position
                var controlledShip = player.GetControlledShip();
                if (controlledShip && Vector3.Distance(controlledShip.transform.position, zdo.m_position) < 0.01f)
                {
                    if (pinWasFound)
                    {
                        __instance.RemovePin(customPin);
                        customPins.Remove(zdo);
                    }

                    return true;
                }

                if (!pinWasFound)
                {
                    customPin = __instance.AddPin(zdo.m_position, Minimap.PinType.Death, pinName, false, false);

                    Sprite sprite;
                    if (icons.TryGetValue(hashCode, out sprite))
                        customPin.m_icon = sprite;

                    customPin.m_doubleSize = true;
                    customPins.Add(zdo, customPin);
                }
                else
                {
                    customPin.m_pos = zdo.m_position;
                }

                return true;
            }

            public static void Postfix(ref Minimap __instance, Player player, float dt, bool takeInput)
            {
                timeCounter += dt;

                if (timeCounter < updateInterval || !VMP_Modplugin.mapIsEnabled.Value ||
                    !VMP_Modplugin.displayCartsAndBoats.Value)
                    return;

                timeCounter -= updateInterval;

                if (icons.Count == 0)
                    FindIcons();

                // search zones for ships and carts
                foreach (var zdoarray in ZDOMan.instance.m_objectsBySector)
                    if (zdoarray != null)
                        foreach (var zdo in zdoarray)
                        {
                            if (CheckPin(__instance, player, zdo, CartHashcode, "Cart"))
                                continue;
                            if (CheckPin(__instance, player, zdo, RaftHashcode, "Raft"))
                                continue;
                            if (CheckPin(__instance, player, zdo, KarveHashcode, "Karve"))
                                continue;
                            if (CheckPin(__instance, player, zdo, LongshipHashcode, "Longship"))
                                continue;
                        }

                // clear pins for destroyed objects
                foreach (var pin in customPins)
                    if (!pin.Key.IsValid())
                    {
                        __instance.RemovePin(pin.Value);
                        customPins.Remove(pin.Key);
                    }
            }
        }
    }
}