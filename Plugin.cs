using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VMP_Mod.Patches;

namespace VMP_Mod
{
    [BepInPlugin(VMP_Modplugin.GUID, VMP_Modplugin.ModName, VMP_Modplugin.Version)]
    public class VMP_Modplugin : BaseUnityPlugin
    {
        private static Harmony _harmony;
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

        public void Awake()
        {


            shareMapProgression = Config.Bind<bool>("Maps", "Share Map Progress with others", true, "Share Map Progress with others");
            mapIsEnabled = Config.Bind<bool>("Maps", "Map Control enabled", true, "Map Control enabled");
            shareablePins = Config.Bind<bool>("Maps", "Share Pins", true, "Share pins with other players");
            shareAllPins = Config.Bind<bool>("Maps", "Share ALL pins with other players", true, "Share ALL pins with other players");
            preventPlayerFromTurningOffPublicPosition = Config.Bind<bool>("General", "IsDebug", true, "Show debug messages in log");
            displayCartsAndBoats = Config.Bind<bool>("Maps", "Display Boats/Carts", true, "Show Boats and carts on the map");
            exploreRadius = Config.Bind<int>("Maps", "NexusID", 40, "Explore radius addition");




            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }


    }
}
