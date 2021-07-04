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
        public static ConfigEntry<int> nexusID;
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

            serverConfigLocked = config("General", "Lock Configuration", false, "Lock Configuration");
            configSync.AddLockingConfigEntry<bool>(serverConfigLocked);
            shareMapProgression = config<bool>("Maps", "Share Map Progress with others", true, "Share Map Progress with others");
            mapIsEnabled = config<bool>("Maps", "Map Control enabled", true, "Map Control enabled");
            shareablePins = config<bool>("Maps", "Share Pins", true, "Share pins with other players");
            shareAllPins = config<bool>("Maps", "Share ALL pins with other players", true, "Share ALL pins with other players");
            preventPlayerFromTurningOffPublicPosition = config<bool>("General", "IsDebug", true, "Show debug messages in log");
            displayCartsAndBoats = config<bool>("Maps", "Display Boats/Carts", true, "Show Boats and carts on the map");
            exploreRadius = config<int>("Maps", "NexusID", 40, "Explore radius addition");
            context = this;

            modEnabled = config<bool>("General", "Enabled", true, "Enable this mod");
            isDebug = config<bool>("General", "IsDebug", true, "Show debug messages in log");
            nexusID = config<int>("General", "NexusID", 40, "Nexus mod ID for updates");

            Container_Configs.KarveRow = config<int>("Containers", "Karve Rows", 2, new ConfigDescription("Rows for Karve", new AcceptableValueRange<int>(2, 30)));
            Container_Configs.KarveCol = config<int>("Containers", "Karve Columns", 2, new ConfigDescription("Columns for Karve", new AcceptableValueRange<int>(2, 8)));
            Container_Configs.LongRow = config<int>("Containers", "Longboat Rows", 3, new ConfigDescription("Rows for longboat", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.LongCol = config<int>("Containers", "Longboat Columns", 6, new ConfigDescription("Columns for longboat", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.CartRow = config<int>("Containers", "Cart Rows", 3, new ConfigDescription("Rows for Cart", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.CartCol = config<int>("Containers", "Cart Colums", 6, new ConfigDescription("Columns for Cart", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.PersonalRow = config<int>("Containers", "Personal Chest Rows", 2, new ConfigDescription("Personal Chest Rows", new AcceptableValueRange<int>(2, 20)));
            Container_Configs.PersonalCol = config<int>("Containers", "Personal Chest Colums", 3, new ConfigDescription("Personal Chest Colums", new AcceptableValueRange<int>(3, 8)));
            Container_Configs.WoodRow = config<int>("Containers", "Wood Chest Rows", 2, new ConfigDescription("Wood Chest Rows", new AcceptableValueRange<int>(2, 10)));
            Container_Configs.WoodCol = config<int>("Containers", "Wood Chest Colums", 5, new ConfigDescription("Wood Chest Colums", new AcceptableValueRange<int>(5, 8)));
            Container_Configs.IronRow = config<int>("Containers", "Iron Chest Rows", 3, new ConfigDescription("Iron Chest Rows", new AcceptableValueRange<int>(3, 20)));
            Container_Configs.IronCol = config<int>("Containers", "Iron Chest Colums", 6, new ConfigDescription("Iron Chest Colums", new AcceptableValueRange<int>(6, 8)));


            CraftingPatch.WorkbenchRange = config<int>("WorkBench", "WorkBenchRange", 40, new ConfigDescription("Range you can build from workbench in meters", new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.workbenchEnemySpawnRange = config<int>("WorkBench", "WorkBenchRange (Playerbase size)", 40, new ConfigDescription("Workbench PlayerBase radius, this is how far away enemies spawn", new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.AlterWorkBench = config<bool>("WorkBench", "Change No Roof Behavior", true, "Show building pieces");
            workbenchAttachmentRange = config<int>("WorkBench", "WorkBench Extension", 40, new ConfigDescription("Range for workbench extensions", new AcceptableValueRange<int>(5, 100)));

            WeightReduction = config<float>("Items", "Item Weight Increase", 1.25f, new ConfigDescription("Multiplier for your item weight"));
            itemStackMultiplier = config<float>("Items", "Item Stack Increase", 2f, new ConfigDescription("Multiplier for your item stacks"));
            NoTeleportPrevention = config<bool>("Items", "Disable Teleport check for items", false, new ConfigDescription("Disable Teleport check for items"));
            filltoptobottom = config<bool>("Items", "Fill your things top to bottom when moving from inv to chest", true, new ConfigDescription("Move your things top to bottom when changing from inv to chest"));
            Playerinvrow = config<int>("Items", "Player Inventory row count", 12, new ConfigDescription("Player row count for inventory", new AcceptableValueRange<int>(4, 20)));
            Deconstruct = config<bool>("Items", "Allow deconstruction of items in crafting menu", true, new ConfigDescription("Deconstructing crafting items for return of mats"));
            AutoRepair = config<bool>("Items", "Auto repair your things when interacting with build station", true, new ConfigDescription("Auto repair your things when interacting with build station"));
            returnedpercent = config<int>("Items", "Percent of item materials you would recieve back from deconstruction", 2, new ConfigDescription("Perecent of item mats you get back from deconstructin tab"));



            if (!modEnabled.Value)
                return;

            if (!Directory.Exists(VMP_DatadirectoryPath))
            {
                Directory.CreateDirectory(VMP_DatadirectoryPath);
            }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
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
