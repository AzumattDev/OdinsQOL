using BepInEx;
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
using ServerSync;
using VMP_Mod.Patches;

namespace VMP_Mod
{
    [BepInPlugin(VMP_Modplugin.GUID, VMP_Modplugin.ModName, VMP_Modplugin.Version)]
    public class VMP_Modplugin : BaseUnityPlugin
    {
        public const string Version = "0.0.1";
        public const string ModName = "VMP Mod";
        public const string GUID = "com.vmp.mod";
        public static readonly string VMP_DatadirectoryPath = Paths.BepInExRootPath + "/vmp-data/";
        public static System.Timers.Timer mapSyncSaveTimer =
            new System.Timers.Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        public static ConfigEntry<bool> mapIsEnabled;
        public static ConfigEntry<bool> shareMapProgression;
        public static ConfigEntry<bool> shareablePins;
        public static ConfigEntry<bool> shareAllPins;
        public static ConfigEntry<bool> preventPlayerFromTurningOffPublicPosition;
        public static ConfigEntry<bool> displayCartsAndBoats;
        public static ConfigEntry<int> exploreRadius;

        private static List<ConnectionParams> containerConnections = new List<ConnectionParams>();
        private static GameObject connectionVfxPrefab = null;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> workbenchAttachmentRange;

        public static ConfigEntry<bool> filltoptobottom;
        public static ConfigEntry<int> Playerinvrow;
        public static ConfigEntry<bool> Deconstruct;
        public static ConfigEntry<bool> AutoRepair;
        public static ConfigEntry<int> returnedpercent;

        public static ConfigEntry<float> WeightReduction;
        public static ConfigEntry<float> itemStackMultiplier;
        public static ConfigEntry<bool> NoTeleportPrevention;

        public static List<Container> containerList = new List<Container>();
        private static VMP_Modplugin context = null;
        public static ServerSync.ConfigSync configSync = new ServerSync.ConfigSync(GUID) { DisplayName = ModName, CurrentVersion = Version };
        private ConfigEntry<bool> serverConfigLocked;

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
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);


        public void Awake()
        {

            serverConfigLocked = config("General", "Lock Configuration", true, "Lock Configuration", true );
            configSync.AddLockingConfigEntry<bool>(serverConfigLocked);
            shareMapProgression = config<bool>("Maps", "Share Map Progress with others", false, "Share Map Progress with others", true );
            mapIsEnabled = config<bool>("Maps", "Map Control enabled", true, "Map Control enabled", true );
            shareablePins = config<bool>("Maps", "Share Pins", true, "Share pins with other players", true );
            shareAllPins = config<bool>("Maps", "Share ALL pins with other players", false, "Share ALL pins with other players", true );
            preventPlayerFromTurningOffPublicPosition = config<bool>("General", "IsDebug", true, "Show debug messages in log", true );
            displayCartsAndBoats = config<bool>("Maps", "Display Boats/Carts", true, "Show Boats and carts on the map", true );
            exploreRadius = config<int>("Maps", "NexusID", 40, "Explore radius addition", true );
            context = this;

            modEnabled = config<bool>("General", "Enabled", true, "Enable this mod", true );
            isDebug = config<bool>("General", "IsDebug", true, "Show debug messages in log", true );

            Container_Configs.KarveRow = config<int>("Containers", "Karve Rows", 2, new ConfigDescription("Rows for Karve", new AcceptableValueRange<int>(2, 30)), true );
            Container_Configs.KarveCol = config<int>("Containers", "Karve Columns", 2, new ConfigDescription("Columns for Karve", new AcceptableValueRange<int>(2, 8)), true );
            Container_Configs.LongRow = config<int>("Containers", "Longboat Rows", 3, new ConfigDescription("Rows for longboat", new AcceptableValueRange<int>(3, 30)), true );
            Container_Configs.LongCol = config<int>("Containers", "Longboat Columns", 6, new ConfigDescription("Columns for longboat", new AcceptableValueRange<int>(6, 8)), true );
            Container_Configs.CartRow = config<int>("Containers", "Cart Rows", 3, new ConfigDescription("Rows for Cart", new AcceptableValueRange<int>(3, 30)), true );
            Container_Configs.CartCol = config<int>("Containers", "Cart Colums", 6, new ConfigDescription("Columns for Cart", new AcceptableValueRange<int>(6, 8)), true );
            Container_Configs.PersonalRow = config<int>("Containers", "Personal Chest Rows", 2, new ConfigDescription("Personal Chest Rows", new AcceptableValueRange<int>(2, 20)), true );
            Container_Configs.PersonalCol = config<int>("Containers", "Personal Chest Colums", 3, new ConfigDescription("Personal Chest Colums", new AcceptableValueRange<int>(3, 8)), true );
            Container_Configs.WoodRow = config<int>("Containers", "Wood Chest Rows", 2, new ConfigDescription("Wood Chest Rows", new AcceptableValueRange<int>(2, 10)), true );
            Container_Configs.WoodCol = config<int>("Containers", "Wood Chest Colums", 5, new ConfigDescription("Wood Chest Colums", new AcceptableValueRange<int>(5, 8)), true );
            Container_Configs.IronRow = config<int>("Containers", "Iron Chest Rows", 3, new ConfigDescription("Iron Chest Rows", new AcceptableValueRange<int>(3, 20)), true );
            Container_Configs.IronCol = config<int>("Containers", "Iron Chest Colums", 6, new ConfigDescription("Iron Chest Colums", new AcceptableValueRange<int>(6, 8)), true );


            CraftingPatch.WorkbenchRange = config<int>("WorkBench", "WorkBenchRange", 40, new ConfigDescription("Range you can build from workbench in meters", new AcceptableValueRange<int>(6, 650)), true );
            CraftingPatch.workbenchEnemySpawnRange = config<int>("WorkBench", "WorkBenchRange (Playerbase size)", 40, new ConfigDescription("Workbench PlayerBase radius, this is how far away enemies spawn", new AcceptableValueRange<int>(6, 650)), true );
            CraftingPatch.AlterWorkBench = config<bool>("WorkBench", "Change No Roof Behavior", true, "Show building pieces", true );
            workbenchAttachmentRange = config<int>("WorkBench", "WorkBench Extension", 40, new ConfigDescription("Range for workbench extensions", new AcceptableValueRange<int>(5, 100)), true );

            WeightReduction = config<float>("Items", "Item Weight Increase", 1.25f, new ConfigDescription("Multiplier for your item weight"), true );
            itemStackMultiplier = config<float>("Items", "Item Stack Increase", 2f, new ConfigDescription("Multiplier for your item stacks"), true );
            NoTeleportPrevention = config<bool>("Items", "Disable Teleport check for items", false, new ConfigDescription("Disable Teleport check for items"), true );
            filltoptobottom = config<bool>("Items", "Fill your things top to bottom when moving from inv to chest", true, new ConfigDescription("Move your things top to bottom when changing from inv to chest"), true );
            Playerinvrow = config<int>("Items", "Player Inventory row count", 12, new ConfigDescription("Player row count for inventory", new AcceptableValueRange<int>(4, 20)), true );
            Deconstruct = config<bool>("Items", "Allow deconstruction of items in crafting menu", true, new ConfigDescription("Deconstructing crafting items for return of mats"), true );
            AutoRepair = config<bool>("Items", "Auto repair your things when interacting with build station", true, new ConfigDescription("Auto repair your things when interacting with build station"), true );
            returnedpercent = config<int>("Items", "Percent of item materials you would recieve back from deconstruction", 2, new ConfigDescription("Perecent of item mats you get back from deconstructin tab"), true );


            MapDetail.showRange = config<float>("Variables", "ShowRange", 50f, "Range in metres around player to show details",true);
            MapDetail.updateDelta = config<float>("Variables", "UpdateDelta", 5f, "Distance in metres to move before automatically updating the map details", true);
            MapDetail.showBuildings = config<bool>("Variables", "ShowBuildings", true, "Show building pieces", true);
            MapDetail.personalBuildingColor = Config.Bind<Color>("Variables", "PersonalBuildingColor", Color.green, "Color of one's own build pieces");
            MapDetail.otherBuildingColor = Config.Bind<Color>("Variables", "OtherBuildingColor", Color.red, "Color of other players' build pieces");
            MapDetail.unownedBuildingColor = Config.Bind<Color>("Variables", "UnownedBuildingColor", Color.yellow, "Color of npc build pieces");
            MapDetail.customPlayerColors = Config.Bind<string>("Variables", "CustomPlayerColors", "", "Custom color list, comma-separated. Use either <name>:<colorCode> pair entries or just <colorCode> entries. E.g. Erinthe:FF0000 or just FF0000. The latter will assign a color randomly to each connected peer.");



            CraftingPatch.maxEntries = Config.Bind<int>("General", "MaxEntries", -1, "Max number of entries to show");
            CraftingPatch.sortType = Config.Bind<CraftingPatch.SortType>("General", "SortType", CraftingPatch.SortType.Value, "Type by which to sort entries.");
            CraftingPatch.sortAsc = Config.Bind<bool>("General", "SortAsc", false, "Sort ascending?");
            CraftingPatch.entryString = Config.Bind<string>("General", "EntryText", "<color=#AAAAAAFF>{0} {1}</color>", "Entry text. {0} is replaced by the total amount, {1} is replaced by the item name.");
            CraftingPatch.overFlowText = Config.Bind<string>("General", "OverFlowText", "<color=#AAAAAAFF>...</color>", "Overflow text if more items than max entries.");


            if (!modEnabled.Value)
                return;

            if (!Directory.Exists(VMP_DatadirectoryPath))
            {
                Directory.CreateDirectory(VMP_DatadirectoryPath);
            }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }


        private void Update()
        {
            if (Minimap.instance && Player.m_localPlayer && ZNet.instance != null)
                StartCoroutine(MapDetail.UpdateMap(false));
        }


        [HarmonyPatch(typeof(Player), "PlacePiece")]
        static class Player_PlacePiece_Patch
        {
            static void Postfix(bool __result)
            {

                context.StartCoroutine(MapDetail.UpdateMap(true));
            }
        }
        [HarmonyPatch(typeof(Player), "RemovePiece")]
        static class Player_RemovePiece_Patch
        {
            static void Postfix(bool __result)
            {

                context.StartCoroutine(MapDetail.UpdateMap(true));
            }
        }
        
        public static float applyModifierValue(float targetValue, float value)
        {

            if (value <= -100)
                value = -100;

            float newValue = targetValue;

            if (value >= 0)
            {
                newValue = targetValue + ((targetValue / 100) * value);
            }
            else
            {
                newValue = targetValue - ((targetValue / 100) * (value * -1));
            }

            return newValue;
        }


    }
}
