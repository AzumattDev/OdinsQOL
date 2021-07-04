﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VMP_Mod.Patches;

namespace VMP_Mod
{
    [BepInPlugin(VMP_Modplugin.GUID, VMP_Modplugin.ModName, VMP_Modplugin.Version)]
    public class VMP_Modplugin : BaseUnityPlugin
    {
        public const string Version = "0.0.1";
        public const string ModName = "VMP Mod";
        public const string GUID = "com.vmp.mod";
        public static readonly string VMP_DatadirectoryPath =
            Paths.BepInExRootPath + Path.DirectorySeparatorChar + "vmp-data";
        public static System.Timers.Timer mapSyncSaveTimer =
            new System.Timers.Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        public static ConfigEntry<bool> mapIsEnabled;
        public static ConfigEntry<bool> shareMapProgression;
        public static ConfigEntry<bool> shareablePins;
        public static ConfigEntry<bool> shareAllPins;
        public static ConfigEntry<bool> preventPlayerFromTurningOffPublicPosition;
        public static ConfigEntry<bool> displayCartsAndBoats;
        public static ConfigEntry<int> exploreRadius;
        private static bool wasAllowed;

        private static List<ConnectionParams> containerConnections = new List<ConnectionParams>();
        private static GameObject connectionVfxPrefab = null;

        public static ConfigEntry<bool> showGhostConnections;
        public static ConfigEntry<float> ghostConnectionStartOffset;
        public static ConfigEntry<float> ghostConnectionRemovalDelay;

        //public static ConfigEntry<bool> ignoreRangeInBuildArea;
        public static ConfigEntry<float> m_range;
        public static ConfigEntry<Color> flashColor;
        public static ConfigEntry<Color> unFlashColor;
        public static ConfigEntry<string> resourceString;
        public static ConfigEntry<string> pulledMessage;
        public static ConfigEntry<string> fuelDisallowTypes;
        public static ConfigEntry<string> oreDisallowTypes;

        public static ConfigEntry<string> pullItemsKey;
        public static ConfigEntry<string> preventModKey;
        public static ConfigEntry<string> fillAllModKey;
        public static ConfigEntry<bool> switchPrevent;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> nexusID;
        public static ConfigEntry<int> workbenchAttachmentRange;

        public static List<Container> containerList = new List<Container>();
        private static VMP_Modplugin context = null;

        public class ConnectionParams
        {
            public GameObject connection = null;
            public Vector3 stationPos;
        }

        public static void Dbgl(string str = "", bool pref = true)
        {

            if (isDebug.Value)
                Debug.Log((pref ? typeof(VMP_Modplugin).Namespace + " " : "") + str);

        }


            public void Awake()
        {


            shareMapProgression = Config.Bind<bool>("Maps", "Share Map Progress with others", true, "Share Map Progress with others");
            mapIsEnabled = Config.Bind<bool>("Maps", "Map Control enabled", true, "Map Control enabled");
            shareablePins = Config.Bind<bool>("Maps", "Share Pins", true, "Share pins with other players");
            shareAllPins = Config.Bind<bool>("Maps", "Share ALL pins with other players", true, "Share ALL pins with other players");
            preventPlayerFromTurningOffPublicPosition = Config.Bind<bool>("General", "IsDebug", true, "Show debug messages in log");
            displayCartsAndBoats = Config.Bind<bool>("Maps", "Display Boats/Carts", true, "Show Boats and carts on the map");
            exploreRadius = Config.Bind<int>("Maps", "NexusID", 40, "Explore radius addition");
            context = this;

            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Show debug messages in log");
            nexusID = Config.Bind<int>("General", "NexusID", 40, "Nexus mod ID for updates");
            
            m_range = Config.Bind<float>("General", "ContainerRange", 10f, "The maximum range from which to pull items from");
            resourceString = Config.Bind<string>("General", "ResourceCostString", "{0}/{1}", "String used to show required and available resources. {0} is replaced by how much is available, and {1} is replaced by how much is required. Set to nothing to leave it as default.");
            flashColor = Config.Bind<Color>("General", "FlashColor", Color.yellow, "Resource amounts will flash to this colour when coming from containers");
            unFlashColor = Config.Bind<Color>("General", "UnFlashColor", Color.white, "Resource amounts will flash from this colour when coming from containers (set both colors to the same color for no flashing)");
            pulledMessage = Config.Bind<string>("General", "PulledMessage", "Pulled items to inventory", "Message to show after pulling items to player inventory");
            fuelDisallowTypes = Config.Bind<string>("General", "FuelDisallowTypes", "RoundLog,FineWood", "Types of item to disallow as fuel (i.e. anything that is consumed), comma-separated.");
            oreDisallowTypes = Config.Bind<string>("General", "OreDisallowTypes", "RoundLog,FineWood", "Types of item to disallow as ore (i.e. anything that is transformed), comma-separated).");

            showGhostConnections = Config.Bind<bool>("Station Connections", "ShowConnections", false, "If true, will display connections to nearby workstations within range when building containers");
            ghostConnectionStartOffset = Config.Bind<float>("Station Connections", "ConnectionStartOffset", 1.25f, "Height offset for the connection VFX start position");
            ghostConnectionRemovalDelay = Config.Bind<float>("Station Connections", "ConnectionRemoveDelay", 0.05f, "");

            switchPrevent = Config.Bind<bool>("Hot Keys", "SwitchPrevent", false, "if true, holding down the PreventModKey modifier key will allow this mod's behaviour; if false, holding down the key will prevent it.");

            preventModKey = Config.Bind<string>("Hot Keys", "PreventModKey", "left alt", "Modifier key to toggle fuel and ore filling behaviour when down. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            pullItemsKey = Config.Bind<string>("Hot Keys", "PullItemsKey", "left ctrl", "Holding down this key while crafting or building will pull resources into your inventory instead of building. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            fillAllModKey = Config.Bind<string>("Hot Keys", "FillAllModKey", "left shift", "Modifier key to pull all available fuel or ore when down. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");


            MapDetails.detailsmodEnabled = Config.Bind<bool>("General", "Map Details Enabled", true, "Enable Map Details showing mod");

            MapDetails.showRange = Config.Bind<float>("Variables", "ShowRange", 50f, "Range in metres around player to show details");
            MapDetails.updateDelta = Config.Bind<float>("Variables", "UpdateDelta", 5f, "Distance in metres to move before automatically updating the map details");
            MapDetails.showBuildings = Config.Bind<bool>("Variables", "ShowBuildings", true, "Show building pieces");
            MapDetails.personalBuildingColor = Config.Bind<Color>("Variables", "PersonalBuildingColor", Color.green, "Color of one's own build pieces");
            MapDetails.otherBuildingColor = Config.Bind<Color>("Variables", "OtherBuildingColor", Color.red, "Color of other players' build pieces");
            MapDetails.unownedBuildingColor = Config.Bind<Color>("Variables", "UnownedBuildingColor", Color.yellow, "Color of npc build pieces");
            MapDetails.customPlayerColors = Config.Bind<string>("Variables", "CustomPlayerColors", "", "Custom color list, comma-separated. Use either <name>:<colorCode> pair entries or just <colorCode> entries. E.g. Erinthe:FF0000 or just FF0000. The latter will assign a color randomly to each connected peer.");

            Container_Configs.KarveRow = Config.Bind<int>("Containers", "Karve Rows", 2, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(2, 30)));
            Container_Configs.KarveCol = Config.Bind<int>("Containers", "Karve Columns", 2, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(2, 8)));
            Container_Configs.LongRow = Config.Bind<int>("Containers", "Longboat Rows", 3, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.LongCol = Config.Bind<int>("Containers", "Longboat Columns", 6, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.CartRow = Config.Bind<int>("Containers", "Cart Rows", 3, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.CartCol = Config.Bind<int>("Containers", "Cart Colums", 6, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.PersonalRow = Config.Bind<int>("Containers", "Personal Chest Rows", 2, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(2, 20)));
            Container_Configs.PersonalCol = Config.Bind<int>("Containers", "Personal Chest Colums", 3, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(3, 8)));
            Container_Configs.WoodRow = Config.Bind<int>("Containers", "Wood Chest Rows", 2, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(2,10)));
            Container_Configs.WoodCol = Config.Bind<int>("Containers", "Wood Chest Colums", 5, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(5, 8)));
            Container_Configs.IronRow = Config.Bind<int>("Containers", "Iron Chest Rows", 3, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(3, 20)));
            Container_Configs.IronCol = Config.Bind<int>("Containers", "Iron Chest Colums", 6, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(6, 8)));


            CraftingPatch.WorkbenchRange = Config.Bind<int>("WorkBench", "WorkBenchRange", 40, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.workbenchEnemySpawnRange = Config.Bind<int>("WorkBench", "WorkBenchRange (Playerbase size)", 40, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.AlterWorkBench = Config.Bind<bool>("WorkBench", "Change No Roof Behavior", true, "Show building pieces");
            workbenchAttachmentRange = Config.Bind<int>("WorkBench", "WorkBench Extension", 40, new ConfigDescription("Nexus mod ID for updates", new AcceptableValueRange<int>(5, 100)));

            ItemDropPatches.WeightReduction = Config.Bind<float>("Items", "Item Weight Increase", 1.25f, new ConfigDescription("Multiplier for your item weight", new AcceptableValueList<float>(0f, 10f)));
            ItemDropPatches.itemStackMultiplier = Config.Bind<int>("Items", "Item Stack Increase", 2, new ConfigDescription("Multiplier for your item stacks", new AcceptableValueList<int>(0, 10)));
            ItemDropPatches.NoTeleportPrevention = Config.Bind<bool>("Items", "Disable Teleport check for items", false, new ConfigDescription("Disable Teleport check for items"));
           
            
            if (!modEnabled.Value)
                return;

            wasAllowed = !switchPrevent.Value;



            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private void Update()
        {
            if (Minimap.instance && Player.m_localPlayer)
                StartCoroutine(MapDetails.UpdateMap(false));
        }

        private void LateUpdate()
        {
            wasAllowed = AllowByKey();
        }

        private static bool AllowByKey()
        {
            if (CheckKeyHeld(preventModKey.Value))
                return switchPrevent.Value;
            return !switchPrevent.Value;
        }

        private void OnDestroy()
        {
            StopConnectionEffects();
        }

        private static bool CheckKeyHeld(string value, bool req = true)
        {
            try
            {
                return Input.GetKey(value.ToLower());
            }
            catch
            {
                return !req;
            }
        }

        public static List<Container> GetNearbyContainers(Vector3 center)
        {
            List<Container> containers = new List<Container>();
            foreach (Container container in containerList)
            {
                if (container != null
                    && container.GetComponentInParent<Piece>() != null
                    && Player.m_localPlayer != null
                    && container?.transform != null
                    && container.GetInventory() != null
                    && (m_range.Value <= 0 || Vector3.Distance(center, container.transform.position) < m_range.Value)
                    && (!PrivateArea.CheckInPrivateArea(container.transform.position) || PrivateArea.CheckAccess(container.transform.position, 0f, true))
                    && (!container.m_checkGuardStone || PrivateArea.CheckAccess(container.transform.position, 0f, false, false))
                    && Traverse.Create(container).Method("CheckAccess", new object[] { Player.m_localPlayer.GetPlayerID() }).GetValue<bool>() && !container.IsInUse())
                {
                    //container.GetComponent<ZNetView>()?.ClaimOwnership();
                    Traverse.Create(container).Method("Load").GetValue();
                    containers.Add(container);
                }
            }
            return containers;
        }

        [HarmonyPatch(typeof(Container), "Awake")]
        static class Container_Awake_Patch
        {
            static void Postfix(Container __instance, ZNetView ___m_nview)
            {
                context.StartCoroutine(AddContainer(__instance, ___m_nview));
            }

        }

        public static IEnumerator AddContainer(Container container, ZNetView nview)
        {
            yield return null;
            try
            {
                //Dbgl($"Checking {container.name} {nview != null} {nview?.GetZDO() != null} {nview?.GetZDO()?.GetLong("creator".GetStableHashCode(), 0L)}");
                if (container.GetInventory() != null && nview?.GetZDO() != null && (container.name.StartsWith("piece_") || container.name.StartsWith("Container") || nview.GetZDO().GetLong("creator".GetStableHashCode(), 0L) != 0))
                {
                    //Dbgl($"Adding {container.name}");
                    containerList.Add(container);
                }
            }
            catch
            {

            }
            yield break;
        }


        [HarmonyPatch(typeof(Container), "OnDestroyed")]
        static class Container_OnDestroyed_Patch
        {
            static void Prefix(Container __instance)
            {
                containerList.Remove(__instance);

            }
        }


        [HarmonyPatch(typeof(InventoryGui), "Update")]
        static class InventoryGui_Update_Patch
        {
            static void Prefix(InventoryGui __instance, Animator ___m_animator)
            {
                if (Player.m_localPlayer && wasAllowed != AllowByKey() && ___m_animator.GetBool("visible"))
                    Traverse.Create(__instance).Method("UpdateCraftingPanel", new object[] { false }).GetValue();
            }
        }

        [HarmonyPatch(typeof(Fireplace), "Interact")]
        static class Fireplace_Interact_Patch
        {
            static bool Prefix(Fireplace __instance, Humanoid user, bool hold, ref bool __result, ZNetView ___m_nview)
            {
                __result = true;
                bool pullAll = CheckKeyHeld(fillAllModKey.Value);
                Inventory inventory = user.GetInventory();

                if (!AllowByKey() || hold || inventory == null || (inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name) && !pullAll))
                    return true;

                if (!___m_nview.HasOwner())
                {
                    ___m_nview.ClaimOwnership();
                }


                if (pullAll && inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name))
                {
                    int amount = (int)Mathf.Min(__instance.m_maxFuel - Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel", 0f)), inventory.CountItems(__instance.m_fuelItem.m_itemData.m_shared.m_name));
                    inventory.RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                    typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(inventory, new object[] { });
                    for (int i = 0; i < amount; i++)
                        ___m_nview.InvokeRPC("AddFuel", new object[] { });

                    user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[] { __instance.m_fuelItem.m_itemData.m_shared.m_name }), 0, null);

                    __result = false;
                }

                if (!inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name) && Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel", 0f)) < __instance.m_maxFuel)
                {
                    List<Container> nearbyContainers = GetNearbyContainers(__instance.transform.position);

                    foreach (Container c in nearbyContainers)
                    {
                        ItemDrop.ItemData item = c.GetInventory().GetItem(__instance.m_fuelItem.m_itemData.m_shared.m_name);
                        if (item != null && Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel", 0f)) < __instance.m_maxFuel)
                        {
                            if (fuelDisallowTypes.Value.Split(',').Contains(item.m_dropPrefab.name))
                            {
                                Dbgl($"container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name} but it's forbidden by config");
                                continue;
                            }

                            int amount = pullAll ? (int)Mathf.Min(__instance.m_maxFuel - Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel", 0f)), item.m_stack) : 1;

                            Dbgl($"container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name}, taking {amount}");

                            c.GetInventory().RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                            typeof(Container).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c, new object[] { });
                            //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });

                            if (__result)
                                user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[] { __instance.m_fuelItem.m_itemData.m_shared.m_name }), 0, null);

                            for (int i = 0; i < amount; i++)
                                ___m_nview.InvokeRPC("AddFuel", new object[] { });

                            __result = false;

                            if (!pullAll || Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel", 0f)) >= __instance.m_maxFuel)
                                return false;
                        }
                    }
                }
                return __result;
            }
        }


        [HarmonyPatch(typeof(CookingStation), "FindCookableItem")]
        static class CookingStation_FindCookableItem_Patch
        {
            static void Postfix(CookingStation __instance, ref ItemDrop.ItemData __result)
            {
                Dbgl($"looking for cookable");

                if (!AllowByKey() || __result != null || !((bool)typeof(CookingStation).GetMethod("IsFireLit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { })) || ((int)typeof(CookingStation).GetMethod("GetFreeSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { })) == -1)
                    return;

                Dbgl($"missing cookable in player inventory");


                List<Container> nearbyContainers = GetNearbyContainers(__instance.transform.position);

                foreach (CookingStation.ItemConversion itemConversion in __instance.m_conversion)
                {
                    foreach (Container c in nearbyContainers)
                    {
                        ItemDrop.ItemData item = c.GetInventory().GetItem(itemConversion.m_from.m_itemData.m_shared.m_name);
                        if (item != null)
                        {
                            if (oreDisallowTypes.Value.Split(',').Contains(item.m_dropPrefab.name))
                            {
                                Dbgl($"container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name} but it's forbidden by config");
                                continue;
                            }

                            Dbgl($"container at {c.transform.position} has {item.m_stack} {item.m_dropPrefab.name}, taking one");
                            __result = item;
                            c.GetInventory().RemoveItem(itemConversion.m_from.m_itemData.m_shared.m_name, 1);
                            typeof(Container).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c, new object[] { });
                            //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });
                            return;
                        }
                    }
                }
            }

        }
        [HarmonyPatch(typeof(Smelter), "UpdateHoverTexts")]
        static class Smelter_UpdateHoverTexts_Patch
        {
            static void Postfix(Smelter __instance)
            {
                if (fillAllModKey.Value?.Length > 0)
                {
                    if (__instance.m_addOreSwitch?.m_hoverText != null)
                        __instance.m_addOreSwitch.m_hoverText += $"\n[<color=yellow><b>{fillAllModKey.Value}+$KEY_Use</b></color>] {__instance.m_addOreTooltip} max";
                    if (__instance.m_addWoodSwitch?.m_hoverText != null)
                        __instance.m_addWoodSwitch.m_hoverText += $"\n[<color=yellow><b>{fillAllModKey.Value}+$KEY_Use</b></color>] $piece_smelter_add max";
                }

            }
        }


        [HarmonyPatch(typeof(Smelter), "OnAddOre")]
        static class Smelter_OnAddOre_Patch
        {
            static bool Prefix(Smelter __instance, Humanoid user, ItemDrop.ItemData item, ZNetView ___m_nview)
            {
                bool pullAll = CheckKeyHeld(fillAllModKey.Value);
                if ((!AllowByKey() && !pullAll) || item != null || Traverse.Create(__instance).Method("GetQueueSize").GetValue<int>() >= __instance.m_maxOre)
                    return true;

                Inventory inventory = user.GetInventory();


                foreach (Smelter.ItemConversion itemConversion in __instance.m_conversion)
                {
                    if (inventory.HaveItem(itemConversion.m_from.m_itemData.m_shared.m_name) && !pullAll)
                        return true;
                }

                Dictionary<string, int> added = new Dictionary<string, int>();

                List<Container> nearbyContainers = GetNearbyContainers(__instance.transform.position);

                foreach (Smelter.ItemConversion itemConversion in __instance.m_conversion)
                {
                    if (Traverse.Create(__instance).Method("GetQueueSize").GetValue<int>() >= __instance.m_maxOre || (added.Any() && !pullAll))
                        break;

                    string name = itemConversion.m_from.m_itemData.m_shared.m_name;
                    if (pullAll && inventory.HaveItem(name))
                    {
                        ItemDrop.ItemData newItem = inventory.GetItem(name);

                        if (oreDisallowTypes.Value.Split(',').Contains(newItem.m_dropPrefab.name))
                        {
                            Dbgl($"player has {newItem.m_stack} {newItem.m_dropPrefab.name} but it's forbidden by config");
                            continue;
                        }

                        int amount = pullAll ? Mathf.Min(__instance.m_maxOre - Traverse.Create(__instance).Method("GetQueueSize").GetValue<int>(), inventory.CountItems(name)) : 1;

                        if (!added.ContainsKey(name))
                            added[name] = 0;
                        added[name] += amount;

                        inventory.RemoveItem(itemConversion.m_from.m_itemData.m_shared.m_name, amount);
                        //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(inventory, new object[] { });

                        for (int i = 0; i < amount; i++)
                            ___m_nview.InvokeRPC("AddOre", new object[] { newItem.m_dropPrefab.name });

                        user.Message(MessageHud.MessageType.TopLeft, $"$msg_added {amount} {name}", 0, null);
                        if (Traverse.Create(__instance).Method("GetQueueSize").GetValue<int>() >= __instance.m_maxOre)
                            break;
                    }

                    foreach (Container c in nearbyContainers)
                    {
                        ItemDrop.ItemData newItem = c.GetInventory().GetItem(name);
                        if (newItem != null)
                        {
                            if (oreDisallowTypes.Value.Split(',').Contains(newItem.m_dropPrefab.name))
                            {
                                Dbgl($"container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name} but it's forbidden by config");
                                continue;
                            }
                            int amount = pullAll ? (int)Mathf.Min(__instance.m_maxOre - Traverse.Create(__instance).Method("GetQueueSize").GetValue<int>(), c.GetInventory().CountItems(name)) : 1;

                            if (!added.ContainsKey(name))
                                added[name] = 0;
                            added[name] += amount;

                            Dbgl($"container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name}, taking {amount}");

                            c.GetInventory().RemoveItem(name, amount);
                            typeof(Container).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c, new object[] { });
                            //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });

                            for (int i = 0; i < amount; i++)
                                ___m_nview.InvokeRPC("AddOre", new object[] { newItem.m_dropPrefab.name });

                            user.Message(MessageHud.MessageType.TopLeft, $"$msg_added {amount} {name}", 0, null);

                            if (Traverse.Create(__instance).Method("GetQueueSize").GetValue<int>() >= __instance.m_maxOre || !pullAll)
                                break;
                        }
                    }
                }

                if (!added.Any())
                    user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems", 0, null);
                else
                {
                    List<string> outAdded = new List<string>();
                    foreach (var kvp in added)
                    {
                        outAdded.Add($"$msg_added {kvp.Value} {kvp.Key}");
                    }
                    user.Message(MessageHud.MessageType.Center, string.Join("\n", outAdded), 0, null);
                }

                return false;
            }
        }


        [HarmonyPatch(typeof(Smelter), "OnAddFuel")]
        static class Smelter_OnAddFuel_Patch
        {
            static bool Prefix(Smelter __instance, ref bool __result, ZNetView ___m_nview, Humanoid user, ItemDrop.ItemData item)
            {
                bool pullAll = CheckKeyHeld(fillAllModKey.Value);
                Inventory inventory = user.GetInventory();
                if ((!AllowByKey() && !pullAll) || item != null || inventory == null || (inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name) && !pullAll))
                    return true;

                __result = true;

                int added = 0;

                if (((float)typeof(Smelter).GetMethod("GetFuel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { })) > __instance.m_maxFuel - 1)
                {
                    user.Message(MessageHud.MessageType.Center, "$msg_itsfull", 0, null);
                    __result = false;
                    return false;
                }

                if (pullAll && inventory.HaveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name))
                {
                    int amount = (int)Mathf.Min(__instance.m_maxFuel - ((float)typeof(Smelter).GetMethod("GetFuel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { })), inventory.CountItems(__instance.m_fuelItem.m_itemData.m_shared.m_name));
                    inventory.RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                    //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(inventory, new object[] { });
                    for (int i = 0; i < amount; i++)
                        ___m_nview.InvokeRPC("AddFuel", new object[] { });

                    added += amount;

                    user.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_fireadding", new string[] { __instance.m_fuelItem.m_itemData.m_shared.m_name }), 0, null);

                    __result = false;
                }

                List<Container> nearbyContainers = GetNearbyContainers(__instance.transform.position);

                foreach (Container c in nearbyContainers)
                {
                    ItemDrop.ItemData newItem = c.GetInventory().GetItem(__instance.m_fuelItem.m_itemData.m_shared.m_name);
                    if (newItem != null)
                    {
                        if (fuelDisallowTypes.Value.Split(',').Contains(newItem.m_dropPrefab.name))
                        {
                            Dbgl($"container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name} but it's forbidden by config");
                            continue;
                        }
                        int amount = pullAll ? (int)Mathf.Min(__instance.m_maxFuel - ((float)typeof(Smelter).GetMethod("GetFuel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { })), newItem.m_stack) : 1;

                        Dbgl($"container at {c.transform.position} has {newItem.m_stack} {newItem.m_dropPrefab.name}, taking {amount}");

                        c.GetInventory().RemoveItem(__instance.m_fuelItem.m_itemData.m_shared.m_name, amount);
                        typeof(Container).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c, new object[] { });
                        //typeof(Inventory).GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c.GetInventory(), new object[] { });

                        for (int i = 0; i < amount; i++)
                            ___m_nview.InvokeRPC("AddFuel", new object[] { });

                        added += amount;

                        user.Message(MessageHud.MessageType.TopLeft, "$msg_added " + __instance.m_fuelItem.m_itemData.m_shared.m_name, 0, null);

                        __result = false;

                        if (!pullAll || Mathf.CeilToInt(___m_nview.GetZDO().GetFloat("fuel", 0f)) >= __instance.m_maxFuel)
                            return false;
                    }
                }

                if (added == 0)
                    user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems", 0, null);
                else
                    user.Message(MessageHud.MessageType.Center, $"$msg_added {added} {__instance.m_fuelItem.m_itemData.m_shared.m_name}", 0, null);

                return __result;
            }
        }

        // fix flashing red text, add amounts

        [HarmonyPatch(typeof(InventoryGui), "SetupRequirement")]
        static class InventoryGui_SetupRequirement_Patch
        {
            static void Postfix(InventoryGui __instance, Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality)
            {
                if (!AllowByKey())
                    return;
                if (req.m_resItem != null)
                {
                    int invAmount = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);
                    int amount = req.GetAmount(quality);
                    if (amount <= 0)
                    {
                        return;
                    }
                    Text text = elementRoot.transform.Find("res_amount").GetComponent<Text>();
                    if (invAmount < amount)
                    {
                        List<Container> nearbyContainers = GetNearbyContainers(Player.m_localPlayer.transform.position);
                        foreach (Container c in nearbyContainers)
                            invAmount += c.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);

                        if (invAmount >= amount)
                            text.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? flashColor.Value : unFlashColor.Value);
                    }
                    if (resourceString.Value.Trim().Length > 0)
                        text.text = string.Format(resourceString.Value, invAmount, amount);
                    else
                        text.text = amount.ToString();
                    text.resizeTextForBestFit = true;
                }
            }
        }

        //[HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
        static class InventoryGui_UpdateRecipeList_Patch
        {
            static void Postfix(InventoryGui __instance, List<GameObject> ___m_recipeList)
            {
                if (!AllowByKey() || ___m_recipeList.Count == 0)
                    return;
                foreach (GameObject go in ___m_recipeList)
                {
                }
            }
        }



        [HarmonyPatch(typeof(Player), "HaveRequirements", new Type[] { typeof(Piece.Requirement[]), typeof(bool), typeof(int) })]
        static class HaveRequirements_Patch
        {
            static void Postfix(Player __instance, ref bool __result, Piece.Requirement[] resources, bool discover, int qualityLevel, HashSet<string> ___m_knownMaterial)
            {
                if (__result || discover || !AllowByKey())
                    return;
                List<Container> nearbyContainers = GetNearbyContainers(__instance.transform.position);

                foreach (Piece.Requirement requirement in resources)
                {
                    if (requirement.m_resItem)
                    {
                        int amount = requirement.GetAmount(qualityLevel);
                        int invAmount = __instance.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                        if (invAmount < amount)
                        {
                            foreach (Container c in nearbyContainers)
                                invAmount += c.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                            if (invAmount < amount)
                                return;
                        }
                    }
                }
                __result = true;
            }
        }

        [HarmonyPatch(typeof(Player), "HaveRequirements", new Type[] { typeof(Piece), typeof(Player.RequirementMode) })]
        static class HaveRequirements_Patch2
        {
            static void Postfix(Player __instance, ref bool __result, Piece piece, Player.RequirementMode mode, HashSet<string> ___m_knownMaterial, Dictionary<string, int> ___m_knownStations)
            {
                if (__result || __instance?.transform?.position == null || !AllowByKey())
                    return;

                //bool ignoreRange = false;

                if (piece.m_craftingStation)
                {
                    if (mode == Player.RequirementMode.IsKnown || mode == Player.RequirementMode.CanAlmostBuild)
                    {
                        if (!___m_knownStations.ContainsKey(piece.m_craftingStation.m_name))
                        {
                            return;
                        }
                    }
                    else if (!CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, __instance.transform.position))
                    {
                        return;
                    }
                    //else
                    //    ignoreRange = ignoreRangeInBuildArea.Value;
                }
                if (piece.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(piece.m_dlc))
                {
                    return;
                }

                List<Container> nearbyContainers = GetNearbyContainers(__instance.transform.position);

                foreach (Piece.Requirement requirement in piece.m_resources)
                {
                    if (requirement.m_resItem && requirement.m_amount > 0)
                    {
                        if (mode == Player.RequirementMode.IsKnown)
                        {
                            if (!___m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
                            {
                                return;
                            }
                        }
                        else if (mode == Player.RequirementMode.CanAlmostBuild)
                        {
                            if (!__instance.GetInventory().HaveItem(requirement.m_resItem.m_itemData.m_shared.m_name))
                            {
                                bool hasItem = false;
                                foreach (Container c in nearbyContainers)
                                {
                                    if (c.GetInventory().HaveItem(requirement.m_resItem.m_itemData.m_shared.m_name))
                                    {
                                        hasItem = true;
                                        break;
                                    }
                                }
                                if (!hasItem)
                                    return;
                            }
                        }
                        else if (mode == Player.RequirementMode.CanBuild && __instance.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) < requirement.m_amount)
                        {
                            int hasItems = __instance.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                            foreach (Container c in nearbyContainers)
                            {
                                try
                                {
                                    hasItems += c.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                                    if (hasItems >= requirement.m_amount)
                                    {
                                        break;
                                    }
                                }
                                catch { }
                            }
                            if (hasItems < requirement.m_amount)
                                return;
                        }
                    }
                }
                __result = true;
            }
        }

        [HarmonyPatch(typeof(Player), "ConsumeResources")]
        static class ConsumeResources_Patch
        {
            static bool Prefix(Player __instance, Piece.Requirement[] requirements, int qualityLevel)
            {
                if (!AllowByKey())
                    return true;

                Inventory pInventory = __instance.GetInventory();
                List<Container> nearbyContainers = GetNearbyContainers(__instance.transform.position);
                foreach (Piece.Requirement requirement in requirements)
                {
                    if (requirement.m_resItem)
                    {
                        int totalRequirement = requirement.GetAmount(qualityLevel);
                        if (totalRequirement <= 0)
                            continue;

                        string reqName = requirement.m_resItem.m_itemData.m_shared.m_name;
                        int totalAmount = pInventory.CountItems(reqName);
                        Dbgl($"have {totalAmount}/{totalRequirement} {reqName} in player inventory");
                        pInventory.RemoveItem(reqName, Math.Min(totalAmount, totalRequirement));

                        if (totalAmount < totalRequirement)
                        {
                            foreach (Container c in nearbyContainers)
                            {
                                Inventory cInventory = c.GetInventory();
                                int thisAmount = Mathf.Min(cInventory.CountItems(reqName), totalRequirement - totalAmount);

                                Dbgl($"Container at {c.transform.position} has {cInventory.CountItems(reqName)}");

                                if (thisAmount == 0)
                                    continue;


                                for (int i = 0; i < cInventory.GetAllItems().Count; i++)
                                {
                                    ItemDrop.ItemData item = cInventory.GetItem(i);
                                    if (item.m_shared.m_name == reqName)
                                    {
                                        Dbgl($"Got stack of {item.m_stack} {reqName}");
                                        int stackAmount = Mathf.Min(item.m_stack, totalRequirement - totalAmount);
                                        if (stackAmount == item.m_stack)
                                            cInventory.RemoveItem(item);
                                        else
                                            item.m_stack -= stackAmount;

                                        totalAmount += stackAmount;
                                        Dbgl($"total amount is now {totalAmount}/{totalRequirement} {reqName}");

                                        if (totalAmount >= totalRequirement)
                                            break;
                                    }
                                }
                                c.GetType().GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c, new object[] { });
                                cInventory.GetType().GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cInventory, new object[] { });

                                if (totalAmount >= totalRequirement)
                                {
                                    Dbgl($"consumed enough {reqName}");
                                    break;
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        static class UpdatePlacementGhost_Patch
        {
            static void Postfix(Player __instance, bool flashGuardStone)
            {
                if (!showGhostConnections.Value)
                {
                    return;
                }

                FieldInfo placementGhostField = typeof(Player).GetField("m_placementGhost", BindingFlags.Instance | BindingFlags.NonPublic);
                GameObject placementGhost = placementGhostField != null ? (GameObject)placementGhostField.GetValue(__instance) : null;
                if (placementGhost == null)
                {
                    return;
                }

                Container ghostContainer = placementGhost.GetComponent<Container>();
                if (ghostContainer == null)
                {
                    return;
                }

                FieldInfo allStationsField = typeof(CraftingStation).GetField("m_allStations", BindingFlags.Static | BindingFlags.NonPublic);
                List<CraftingStation> allStations = allStationsField != null ? (List<CraftingStation>)allStationsField.GetValue(null) : null;

                if (connectionVfxPrefab == null)
                {
                    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                    {
                        if (go.name == "vfx_ExtensionConnection")
                        {
                            connectionVfxPrefab = go;
                            break;
                        }
                    }
                }

                if (connectionVfxPrefab == null)
                {
                    return;
                }

                if (allStations != null)
                {
                    bool bAddedConnections = false;
                    foreach (CraftingStation station in allStations)
                    {
                        int connectionIndex = ConnectionExists(station);
                        bool connectionAlreadyExists = connectionIndex != -1;

                        if (Vector3.Distance(station.transform.position, placementGhost.transform.position) < m_range.Value)
                        {
                            bAddedConnections = true;

                            Vector3 connectionStartPos = station.GetConnectionEffectPoint();
                            Vector3 connectionEndPos = placementGhost.transform.position + Vector3.up * ghostConnectionStartOffset.Value;

                            ConnectionParams tempConnection = null;
                            if (!connectionAlreadyExists)
                            {
                                tempConnection = new ConnectionParams();
                                tempConnection.stationPos = station.GetConnectionEffectPoint();
                                tempConnection.connection = UnityEngine.Object.Instantiate<GameObject>(connectionVfxPrefab, connectionStartPos, Quaternion.identity);
                            }
                            else
                            {
                                tempConnection = containerConnections[connectionIndex];
                            }

                            if (tempConnection.connection != null)
                            {
                                Vector3 vector3 = connectionEndPos - connectionStartPos;
                                Quaternion quaternion = Quaternion.LookRotation(vector3.normalized);
                                tempConnection.connection.transform.position = connectionStartPos;
                                tempConnection.connection.transform.rotation = quaternion;
                                tempConnection.connection.transform.localScale = new Vector3(1f, 1f, vector3.magnitude);
                            }

                            if (!connectionAlreadyExists)
                            {
                                containerConnections.Add(tempConnection);
                            }
                        }
                        else if (connectionAlreadyExists)
                        {
                            UnityEngine.Object.Destroy((UnityEngine.Object)containerConnections[connectionIndex].connection);
                            containerConnections.RemoveAt(connectionIndex);
                        }
                    }

                    if (bAddedConnections && context != null)
                    {
                        context.CancelInvoke("StopConnectionEffects");
                        context.Invoke("StopConnectionEffects", ghostConnectionRemovalDelay.Value);
                    }
                }
            }
        }

        public static int ConnectionExists(CraftingStation station)
        {
            foreach (ConnectionParams c in containerConnections)
            {
                if (Vector3.Distance(c.stationPos, station.GetConnectionEffectPoint()) < 0.1f)
                {
                    return containerConnections.IndexOf(c);
                }
            }

            return -1;
        }

        public void StopConnectionEffects()
        {
            if (containerConnections.Count > 0)
            {
                foreach (ConnectionParams c in containerConnections)
                {
                    UnityEngine.Object.Destroy((UnityEngine.Object)c.connection);
                }
            }

            containerConnections.Clear();
        }

        [HarmonyPatch(typeof(InventoryGui), "OnCraftPressed")]
        static class DoCrafting_Patch
        {
            static bool Prefix(InventoryGui __instance, KeyValuePair<Recipe, ItemDrop.ItemData> ___m_selectedRecipe, ItemDrop.ItemData ___m_craftUpgradeItem)
            {
                if (!AllowByKey() || !CheckKeyHeld(pullItemsKey.Value) || ___m_selectedRecipe.Key == null)
                    return true;

                int qualityLevel = (___m_craftUpgradeItem != null) ? (___m_craftUpgradeItem.m_quality + 1) : 1;
                if (qualityLevel > ___m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_maxQuality)
                {
                    return true;
                }
                Dbgl($"pulling resources to player inventory for crafting item {___m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_name}");

                PullResources(Player.m_localPlayer, ___m_selectedRecipe.Key.m_resources, qualityLevel);
                return false;
            }

        }

        [HarmonyPatch(typeof(Player), "UpdatePlacement")]
        static class UpdatePlacement_Patch
        {
            static bool Prefix(Player __instance, bool takeInput, float dt, PieceTable ___m_buildPieces, GameObject ___m_placementGhost)
            {

                if (!AllowByKey() || !CheckKeyHeld(pullItemsKey.Value) || !__instance.InPlaceMode() || !takeInput || Hud.IsPieceSelectionVisible())
                {
                    return true;
                }
                if (ZInput.GetButtonDown("Attack") || ZInput.GetButtonDown("JoyPlace"))
                {
                    Piece selectedPiece = ___m_buildPieces.GetSelectedPiece();
                    if (selectedPiece != null)
                    {
                        if (selectedPiece.m_repairPiece)
                            return true;
                        if (___m_placementGhost != null)
                        {
                            int placementStatus = (int)typeof(Player).GetField("m_placementStatus", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                            if (placementStatus == 0)
                            {
                                Dbgl($"pulling resources to player inventory for piece {selectedPiece.name}");
                                PullResources(__instance, selectedPiece.m_resources, 0);
                            }
                        }
                    }
                    return false;
                }
                return true;
            }
        }

        private static void PullResources(Player player, Piece.Requirement[] resources, int qualityLevel)
        {
            Inventory pInventory = Player.m_localPlayer.GetInventory();
            List<Container> nearbyContainers = GetNearbyContainers(Player.m_localPlayer.transform.position);
            foreach (Piece.Requirement requirement in resources)
            {
                if (requirement.m_resItem)
                {
                    int totalRequirement = requirement.GetAmount(qualityLevel);
                    if (totalRequirement <= 0)
                        continue;

                    string reqName = requirement.m_resItem.m_itemData.m_shared.m_name;
                    int totalAmount = 0;
                    //Dbgl($"have {totalAmount}/{totalRequirement} {reqName} in player inventory");

                    foreach (Container c in nearbyContainers)
                    {
                        Inventory cInventory = c.GetInventory();
                        int thisAmount = Mathf.Min(cInventory.CountItems(reqName), totalRequirement - totalAmount);

                        Dbgl($"Container at {c.transform.position} has {cInventory.CountItems(reqName)}");

                        if (thisAmount == 0)
                            continue;


                        for (int i = 0; i < cInventory.GetAllItems().Count; i++)
                        {
                            ItemDrop.ItemData item = cInventory.GetItem(i);
                            if (item.m_shared.m_name == reqName)
                            {
                                Dbgl($"Got stack of {item.m_stack} {reqName}");
                                int stackAmount = Mathf.Min(item.m_stack, totalRequirement - totalAmount);

                                if (!pInventory.HaveEmptySlot())
                                    stackAmount = Math.Min(Traverse.Create(pInventory).Method("FindFreeStackSpace", new object[] { item.m_shared.m_name }).GetValue<int>(), stackAmount);

                                Dbgl($"Sending {stackAmount} {reqName} to player");

                                ItemDrop.ItemData sendItem = item.Clone();
                                sendItem.m_stack = stackAmount;

                                pInventory.AddItem(sendItem);

                                if (stackAmount == item.m_stack)
                                    cInventory.RemoveItem(item);
                                else
                                    item.m_stack -= stackAmount;

                                totalAmount += stackAmount;
                                Dbgl($"total amount is now {totalAmount}/{totalRequirement} {reqName}");

                                if (totalAmount >= totalRequirement)
                                    break;
                            }
                        }
                        c.GetType().GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(c, new object[] { });
                        cInventory.GetType().GetMethod("Changed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cInventory, new object[] { });

                        if (totalAmount >= totalRequirement)
                        {
                            Dbgl($"pulled enough {reqName}");
                            break;
                        }
                    }
                }
                if (pulledMessage.Value?.Length > 0)
                    player.Message(MessageHud.MessageType.Center, pulledMessage.Value, 0, null);
            }
        }
        static public bool HaveRequiredItemCount(Player player, Piece piece, Player.RequirementMode mode, Inventory inventory, HashSet<string> knownMaterial)
        {
            List<Container> nearbyContainers = GetNearbyContainers(player.transform.position);

            foreach (Piece.Requirement resource in piece.m_resources)
            {
                if (resource.m_resItem && resource.m_amount > 0)
                {
                    switch (mode)
                    {
                        case Player.RequirementMode.CanBuild:
                            int inInventory = inventory.CountItems(resource.m_resItem.m_itemData.m_shared.m_name);
                            int itemCount = inInventory;
                            if (itemCount < resource.m_amount)
                            {
                                bool enoughInContainers = false;
                                foreach (Container c in nearbyContainers)
                                {
                                    try
                                    {
                                        itemCount += c.GetInventory().CountItems(resource.m_resItem.m_itemData.m_shared.m_name);
                                        if (itemCount >= resource.m_amount)
                                        {
                                            enoughInContainers = true;
                                            break;
                                        }
                                    }
                                    catch { }
                                }

                                if (!enoughInContainers)
                                {
                                    return false;
                                }
                            }
                            continue;
                        case Player.RequirementMode.IsKnown:
                            if (!knownMaterial.Contains(resource.m_resItem.m_itemData.m_shared.m_name))
                            {
                                return false;
                            }
                            continue;
                        case Player.RequirementMode.CanAlmostBuild:
                            if (!inventory.HaveItem(resource.m_resItem.m_itemData.m_shared.m_name))
                            {
                                bool enoughInContainers = false;
                                foreach (Container c in nearbyContainers)
                                {
                                    if (c.GetInventory().HaveItem(resource.m_resItem.m_itemData.m_shared.m_name))
                                    {
                                        enoughInContainers = true;
                                        break;
                                    }
                                }

                                if (!enoughInContainers)
                                {
                                    return false;
                                }
                            }
                            continue;
                        default:
                            continue;
                    }
                }
            }

            return true;
        }

        //[HarmonyPatch(typeof(Player), "HaveRequirements", new Type[] { typeof(Piece), typeof(Player.RequirementMode) })]
        static class HaveRequirements_Patch2_broken
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var codes = new List<CodeInstruction>(instructions);

                //Dbgl($"");
                //Dbgl($"#############################################################");
                //Dbgl($"######## ORIGINAL INSTRUCTIONS - {codes.Count} ########");
                //Dbgl($"#############################################################");
                //Dbgl($"");
                //for (var j = 0; j < codes.Count; j++)
                //{
                //    CodeInstruction instrPr = codes[j];
                //
                //    Dbgl($"{j} {instrPr}");
                //}
                //Dbgl($"#############################################################");
                //Dbgl($"");

                for (var i = 0; i < codes.Count; i++)
                {
                    CodeInstruction instr = codes[i];

                    //Dbgl($"{i} {instr}");

                    if (instr.opcode == OpCodes.Callvirt)
                    {
                        String instrString = instr.ToString();
                        if (instrString.Contains("IsDLCInstalled"))
                        {
                            int targetRetIndex = -1;
                            for (var j = i + 1; j < i + 5; j++)           // find 'ret' instruction within next 5
                            {
                                //Dbgl($"v{j} {codes[j].ToString()}");

                                if (codes[j].opcode == OpCodes.Ret)
                                {
                                    targetRetIndex = j;
                                    break;
                                }
                            }

                            if (targetRetIndex != -1)
                            {
                                int targetIndex = targetRetIndex + 1;
                                List<Label> labels = codes[targetIndex].labels;
                                //Dbgl($">>> Removing instructions in range: {targetIndex}-{codes.Count}");
                                codes.RemoveRange(targetIndex, codes.Count - targetIndex);

                                //Dbgl($"### Instructions after removal: START");
                                //
                                //for (var j = targetIndex - 1; j < codes.Count; j++)
                                //{
                                //    CodeInstruction instrPr = codes[j];
                                //
                                //    Dbgl($"{j} {instrPr}");
                                //}
                                //Dbgl($"### Instructions after removal: END");

                                // Parameter - this (Player player)
                                CodeInstruction loadThis = new CodeInstruction(OpCodes.Ldarg_0);
                                loadThis.labels = labels;
                                codes.Add(loadThis);
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");

                                // Parameter - Piece piece
                                codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");

                                // Parameter - Player.RequirementMode mode
                                codes.Add(new CodeInstruction(OpCodes.Ldarg_2));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");

                                // Parameter - Inventory inventory
                                codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");
                                codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Humanoid), "m_inventory")));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");

                                // Parameter - HashSet<string> knownMaterial
                                //FieldInfo materialField = typeof(Player).GetField("m_skills", BindingFlags.Instance | BindingFlags.NonPublic);
                                codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");
                                codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Player), "m_knownMaterial")));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");

                                // Call static function - CraftFromContainers.BepInExPlugin.HaveRequiredItemCount()
                                codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VMP_Modplugin), "HaveRequiredItemCount")));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count-1].ToString()}");

                                // return
                                codes.Add(new CodeInstruction(OpCodes.Ret));
                                //Dbgl($">>> Inserting instruction to the end:: { codes[codes.Count - 1].ToString()}");
                                break;
                            }
                            else
                            {
                                Dbgl($">>> FAILED to find targeted code for HaveRequirements transpiler patch!!! Mod will not work!");
                            }
                        }
                    }
                }

                //Dbgl($"");
                //Dbgl($"#############################################################");
                //Dbgl($"######## MODIFIED INSTRUCTIONS - {codes.Count} ########");
                //Dbgl($"#############################################################");
                //Dbgl($"");
                //
                //for (var j = 0; j < codes.Count; j++)
                //{
                //    CodeInstruction instrPr = codes[j];
                //
                //    Dbgl($"{j} {instrPr}");
                //}

                return codes;
            }
        }

        [HarmonyPatch(typeof(Console), "InputText")]
        static class InputText_Patch
        {
            static bool Prefix(Console __instance)
            {
                if (!modEnabled.Value)
                    return true;
                string text = __instance.m_input.text;
                if (text.ToLower().Equals("craftfromcontainers reset"))
                {
                    context.Config.Reload();
                    context.Config.Save();
                    Traverse.Create(__instance).Method("AddString", new object[] { text }).GetValue();
                    Traverse.Create(__instance).Method("AddString", new object[] { "Craft From Containers config reloaded" }).GetValue();
                    return false;
                }
                return true;
            }
        }

    }
}
