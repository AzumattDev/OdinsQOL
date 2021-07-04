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

            Container_Configs.KarveRow = Config.Bind<int>("Containers", "Karve Rows", 2, new ConfigDescription("Rows for Karve", new AcceptableValueRange<int>(2, 30)));
            Container_Configs.KarveCol = Config.Bind<int>("Containers", "Karve Columns", 2, new ConfigDescription("Columns for Karve", new AcceptableValueRange<int>(2, 8)));
            Container_Configs.LongRow = Config.Bind<int>("Containers", "Longboat Rows", 3, new ConfigDescription("Rows for longboat", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.LongCol = Config.Bind<int>("Containers", "Longboat Columns", 6, new ConfigDescription("Columns for longboat", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.CartRow = Config.Bind<int>("Containers", "Cart Rows", 3, new ConfigDescription("Rows for Cart", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.CartCol = Config.Bind<int>("Containers", "Cart Colums", 6, new ConfigDescription("Columns for Cart", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.PersonalRow = Config.Bind<int>("Containers", "Personal Chest Rows", 2, new ConfigDescription("Personal Chest Rows", new AcceptableValueRange<int>(2, 20)));
            Container_Configs.PersonalCol = Config.Bind<int>("Containers", "Personal Chest Colums", 3, new ConfigDescription("Personal Chest Colums", new AcceptableValueRange<int>(3, 8)));
            Container_Configs.WoodRow = Config.Bind<int>("Containers", "Wood Chest Rows", 2, new ConfigDescription("Wood Chest Rows", new AcceptableValueRange<int>(2, 10)));
            Container_Configs.WoodCol = Config.Bind<int>("Containers", "Wood Chest Colums", 5, new ConfigDescription("Wood Chest Colums", new AcceptableValueRange<int>(5, 8)));
            Container_Configs.IronRow = Config.Bind<int>("Containers", "Iron Chest Rows", 3, new ConfigDescription("Iron Chest Rows", new AcceptableValueRange<int>(3, 20)));
            Container_Configs.IronCol = Config.Bind<int>("Containers", "Iron Chest Colums", 6, new ConfigDescription("Iron Chest Colums", new AcceptableValueRange<int>(6, 8)));


            CraftingPatch.WorkbenchRange = Config.Bind<int>("WorkBench", "WorkBenchRange", 40, new ConfigDescription("Range you can build from workbench in meters", new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.workbenchEnemySpawnRange = Config.Bind<int>("WorkBench", "WorkBenchRange (Playerbase size)", 40, new ConfigDescription("Workbench PlayerBase radius, this is how far away enemies spawn", new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.AlterWorkBench = Config.Bind<bool>("WorkBench", "Change No Roof Behavior", true, "Show building pieces");
            workbenchAttachmentRange = Config.Bind<int>("WorkBench", "WorkBench Extension", 40, new ConfigDescription("Range for workbench extensions", new AcceptableValueRange<int>(5, 100)));

            ItemDropPatches.WeightReduction = Config.Bind<float>("Items", "Item Weight Increase", 1.25f, new ConfigDescription("Multiplier for your item weight", new AcceptableValueList<float>(0f, 10f)));
            ItemDropPatches.itemStackMultiplier = Config.Bind<int>("Items", "Item Stack Increase", 2, new ConfigDescription("Multiplier for your item stacks", new AcceptableValueList<int>(0, 10)));
            ItemDropPatches.NoTeleportPrevention = Config.Bind<bool>("Items", "Disable Teleport check for items", false, new ConfigDescription("Disable Teleport check for items"));
            filltoptobottom = Config.Bind<bool>("Items", "Fill your things top to bottom when moving from inv to chest", true, new ConfigDescription("Move your things top to bottom when changing from inv to chest"));
            Playerinvrow = Config.Bind<int>("Items", "Player Inventory row count", 12, new ConfigDescription("Player row count for inventory", new AcceptableValueRange<int>(4, 20)));
            Deconstruct = Config.Bind<bool>("Items", "Allow deconstruction of items in crafting menu", true, new ConfigDescription("Deconstructing crafting items for return of mats"));
            AutoRepair = Config.Bind<bool>("Items", "Auto repair your things when interacting with build station", true, new ConfigDescription("Auto repair your things when interacting with build station"));
            returnedpercent = Config.Bind<int>("Items", "Percent of item materials you would recieve back from deconstruction", 2, new ConfigDescription("Perecent of item mats you get back from deconstructin tab"));



            if (!modEnabled.Value)
                return;

            if (!Directory.Exists(VMP_DatadirectoryPath))
            {
                Directory.CreateDirectory(VMP_DatadirectoryPath);
            }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }



    }
}
