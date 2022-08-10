using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OdinQOL.Configs;
using OdinQOL.Patches;
using OdinQOL.Patches.BiFrost;
using ServerSync;
using UnityEngine;

namespace OdinQOL
{
    public static class ImprovedBuildHudConfig
    {
        public static ConfigEntry<string> InventoryAmountFormat = null!;
        public static ConfigEntry<string> InventoryAmountColor = null!;
        public static ConfigEntry<string> CanBuildAmountFormat = null!;
        public static ConfigEntry<string> CanBuildAmountColor = null!;
    }

    class AcceptableShortcuts : AcceptableValueBase
    {
        public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
        {
        }

        public override object Clamp(object value) => value;
        public override bool IsValid(object value) => true;

        public override string ToDescriptionString() =>
            "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
    }

    class AcceptableCategories : AcceptableValueBase
    {
        public AcceptableCategories() : base(typeof(ItemDrop.ItemData.ItemType))
        {
        }

        public override object Clamp(object value) => value;
        public override bool IsValid(object value) => true;

        public override string ToDescriptionString() =>
            "# Acceptable categories: " + string.Join(", ", OdinQOLplugin.Categories.ToString());
    }

    [BepInPlugin(GUID, ModName, Version)]
    [BepInIncompatibility(
        "aedenthorn.CraftFromContainers")] // Since I have pulled this in and optimized a few things...make sure we don't overlap.
    /*[BepInIncompatibility(
        "com.jotunn.modsettings")] // It was exploited in the past to bypass ServerSync settings while it was in Jotunn. No telling that won't happen again even if only a short time.*/
    public partial class OdinQOLplugin : BaseUnityPlugin
    {
        public const string Version = "0.8.6";
        public const string ModName = "OdinPlusQOL";
        public const string GUID = "com.odinplusqol.mod";
        internal static readonly int windowId = 434343;
        internal static Assembly epicLootAssembly;
        public static OdinQOLplugin context;
        private const string ConfigFileName = GUID + ".cfg";

        private static readonly string ConfigFileFullPath =
            Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        public static ConfigEntry<bool> ModEnabled = null!;

        public static ConfigEntry<bool> filltoptobottom = null!;
        public static ConfigEntry<bool> Deconstruct = null!;
        public static ConfigEntry<bool> AutoRepair = null!;
        public static ConfigEntry<int> returnedpercent = null!;

        public static ConfigEntry<float> WeightReduction = null!;
        public static ConfigEntry<float> ItemStackMultiplier = null!;
        public static ConfigEntry<bool> NoTeleportPrevention = null!;

        //public static readonly IEnumerable<KeyCode> AllKeyCodes;

        public static ConfigSync configSync = new(GUID) { DisplayName = ModName, CurrentVersion = Version };
        internal static readonly List<Container> ContainerList = new();
        internal static readonly List<ConnectionParams> ContainerConnections = new();
        internal static GameObject ConnectionVfxPrefab = null!;
        private ConfigEntry<bool> _serverConfigLocked = null!;

        // ReSharper disable once InconsistentNaming
        public static readonly ManualLogSource QOLLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);


        public void Awake()
        {
            _serverConfigLocked = config("General", "Lock Configuration", true, "Lock Configuration");
            configSync.AddLockingConfigEntry(_serverConfigLocked);

            context = this;


            ModEnabled = config("General", "Enabled", true, "Enable the entire mod");

            WorldPatchConfigs.Generate();
            ChestSizeConfigs.Generate();
            CraftingConfigs.Generate();


            WeightReduction = config("Items", "Item Weight Increase", 1f,
                new ConfigDescription(
                    "Multiplier for your item weight. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%."));
            ItemStackMultiplier = config("Items", "Item Stack Increase", 1f,
                new ConfigDescription(
                    "Multiplier for your item stacks. Directly multiplies stack of the item by this value. (Stack of 50 with value of 10 here would turn into a stack of 500)"));
            NoTeleportPrevention = config("Items", "Disable Teleport check for items", false,
                new ConfigDescription("Disable Teleport check for items"));
            filltoptobottom = config("Items", "Fill your things top to bottom when moving from inv to chest", true,
                new ConfigDescription("Move your things top to bottom when changing from inv to chest"), false);
            AutoRepair = config("Items", "Auto repair your things when interacting with build station", true,
                new ConfigDescription("Auto repair your things when interacting with build station"), false);

            MapDetailConfigs.Generate();
            ClockPatchConfigs.Generate();
            GamePatchConfigs.Generate();
            SignPatchConfigs.Generate();
            AutoStoreConfigs.Generate();

            ClientPatches.ChatPlayerName =
                config(
                    "Names", "chatPlayerName", string.Empty,
                    "Override your player name shown in-game and in the chat box.", false);

            PlantGrowthConfigs.Generate();
            WNTPatchConfigs.Generate();
            SkillPatchConfigs.Generate();
            EPIConfigs.Generate();
            MoveableChestConfigs.Generate();
            InvDiscardConfigs.Generate();
            CFCConfigs.Generate();
            BiFrostConfigs.Generate();


            if (!ModEnabled.Value)
                return;
            CFC.CfcWasAllowed = !CFC.switchPrevent.Value;

            SignPatches.CurrentFont = SignPatches.GetFont(SignPatches.FontName.Value, 20);
            SignPatches.LastFontName = SignPatches.CurrentFont?.name;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            SetupWatcher();
            QuickAccessBar.Hotkeys = new[]
            {
                QuickAccessBar.HotKey1,
                QuickAccessBar.HotKey2,
                QuickAccessBar.HotKey3
            };
            QuickAccessBar.HotkeyTexts = new[]
            {
                QuickAccessBar.HotKey1Text,
                QuickAccessBar.HotKey2Text,
                QuickAccessBar.HotKey3Text
            };
        }

        private void Start()
        {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot"))
            {
                epicLootAssembly = Chainloader.PluginInfos["randyknapp.mods.epicloot"].Instance.GetType().Assembly;
                QOLLogger.LogDebug("Epic Loot found, providing compatibility");
            }

            Game.isModded = true;
        }

        private void Update()
        {
            if (Minimap.instance && Player.m_localPlayer != null && ZNet.instance != null)
                StartCoroutine(MapDetail.UpdateMap(false));
            if (Utilities.IgnoreKeyPresses())
                return;
            if (AutoStorePatch.StoreShortcut.Value.IsDown())
            {
                AutoStorePatch.PlayerTryStore();
            }

            if (Utilities.CheckKeyDown(AutoStorePatch.toggleKey.Value))
            {
                AutoStorePatch.isOn.Value = !AutoStorePatch.isOn.Value;
                Config.Save();
                Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                    string.Format(AutoStorePatch.toggleString.Value, AutoStorePatch.isOn.Value));
            }

            if (Utilities.IgnoreKeyPresses() || ToggleClockKeyOnPress.Value ||
                !PressedToggleKey())
                return;
            bool show = ShowingClock.Value;
            ShowingClock.Value = !show;
            Config.Save();
        }

        private void LateUpdate()
        {
            CFC.CfcWasAllowed = Utilities.AllowByKey();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;

            FileSystemWatcher serverWatcher = new(Paths.ConfigPath, BiFrostServers.ConfigFileName);
            serverWatcher.Changed += Utilities.ReadNewServers;
            serverWatcher.Created += Utilities.ReadNewServers;
            serverWatcher.Renamed += Utilities.ReadNewServers;
            serverWatcher.IncludeSubdirectories = true;
            serverWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            serverWatcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                QOLLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                QOLLogger.LogError($"There was an issue loading your {ConfigFileName}");
                QOLLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        internal ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        internal ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        public static int GetAvailableItems(string itemName)
        {
            Player? player = Player.m_localPlayer;
            if (player == null) return 0;

            int playerInventoryCount = player.GetInventory().CountItems(itemName);
            int containerCount = ContainerList.Sum(container => container.GetInventory().CountItems(itemName));
            return playerInventoryCount + containerCount;
        }

        internal static string GetCurrentTimeString()
        {
            if (!EnvMan.instance)
                return "";
            float fraction = EnvMan.instance.m_smoothDayFraction;

            int hour = (int)(fraction * 24);
            int minute = (int)((fraction * 24 - hour) * 60);
            int second = (int)(((fraction * 24 - hour) * 60 - minute) * 60);

            DateTime now = DateTime.Now;
            DateTime theTime = new(now.Year, now.Month, now.Day, hour, minute, second);
            int days = EnvMan.instance.GetCurrentDay();
            return GetCurrentTimeString(theTime, fraction, days);
        }

        public static string[] Categories =
        {
            "None",
            "Material",
            "Consumable",
            "OneHandedWeapon",
            "Bow",
            "Shield",
            "Helmet",
            "Chest",
            "Ammo",
            "Customization",
            "Legs",
            "Hands",
            "Trophie",
            "TwoHandedWeapon",
            "Torch",
            "Misc",
            "Shoulder",
            "Utility",
            "Tool",
            "Attach_Atgeir"
        };
    }
}