using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using ServerSync;
using VMP_Mod.Patches;
using VMP_Mod.RPC;
using VMP_Mod.EAQS;

namespace VMP_Mod
{
    public static class ImprovedBuildHudConfig
    {
        public static ConfigEntry<string> InventoryAmountFormat;
        public static ConfigEntry<string> InventoryAmountColor;
        public static ConfigEntry<string> CanBuildAmountFormat;
        public static ConfigEntry<string> CanBuildAmountColor;
    }
    [BepInPlugin(VMP_Modplugin.GUID, VMP_Modplugin.ModName, VMP_Modplugin.Version)]
    public class VMP_Modplugin : BaseUnityPlugin
    {
        public const string Version = "0.0.2";
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

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<int> workbenchAttachmentRange;

        public static ConfigEntry<bool> filltoptobottom;
        public static ConfigEntry<bool> Deconstruct;
        public static ConfigEntry<bool> AutoRepair;
        public static ConfigEntry<int> returnedpercent;

        public static ConfigEntry<float> WeightReduction;
        public static ConfigEntry<float> itemStackMultiplier;
        public static ConfigEntry<bool> NoTeleportPrevention;
        public static ConfigEntry<bool> iHaveArrivedOnSpawn;

        public static List<Container> containerList = new List<Container>();
        private static VMP_Modplugin context = null;
        public static ServerSync.ConfigSync configSync = new ServerSync.ConfigSync(GUID) { DisplayName = ModName, CurrentVersion = Version };
        private ConfigEntry<bool> serverConfigLocked;
        private static List<Container> _cachedContainers;

        public static bool CraftFromContainersInstalledAndActive;
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

            Deconstruct = config<bool>("Items", "Allow deconstruction of items in crafting menu", true, new ConfigDescription("Deconstructing crafting items for return of mats"), true );
            AutoRepair = config<bool>("Items", "Auto repair your things when interacting with build station", true, new ConfigDescription("Auto repair your things when interacting with build station"), true );
            returnedpercent = config<int>("Items", "Percent of item materials you would recieve back from deconstruction", 100, new ConfigDescription("Perecent of item mats you get back from deconstructin tab"), true );


            MapDetail.showRange = config<float>("Map Details", "ShowRange", 50f, "Range in metres around player to show details",true);
            MapDetail.updateDelta = config<float>("Map Details", "UpdateDelta", 5f, "Distance in metres to move before automatically updating the map details", true);
            MapDetail.showBuildings = config<bool>("Map Details", "ShowBuildings", true, "Show building pieces", true);
            MapDetail.personalBuildingColor = Config.Bind<Color>("Map Details", "PersonalBuildingColor", Color.green, "Color of one's own build pieces");
            MapDetail.otherBuildingColor = Config.Bind<Color>("Map Details", "OtherBuildingColor", Color.red, "Color of other players' build pieces");
            MapDetail.unownedBuildingColor = Config.Bind<Color>("Map Details", "UnownedBuildingColor", Color.yellow, "Color of npc build pieces");
            MapDetail.customPlayerColors = Config.Bind<string>("Map Details", "CustomPlayerColors", "", "Custom color list, comma-separated. Use either <name>:<colorCode> pair entries or just <colorCode> entries. E.g. Erinthe:FF0000 or just FF0000. The latter will assign a color randomly to each connected peer.");



            CraftingPatch.maxEntries = Config.Bind<int>("Show Chest Contents", "MaxEntries", -1, "Max number of entries to show");
            CraftingPatch.sortType = Config.Bind<CraftingPatch.SortType>("Show Chest Contents", "SortType", CraftingPatch.SortType.Value, "Type by which to sort entries.");
            CraftingPatch.sortAsc = Config.Bind<bool>("Show Chest Contents", "SortAsc", false, "Sort ascending?");
            CraftingPatch.entryString = Config.Bind<string>("Show Chest Contents", "EntryText", "<color=#AAAAAAFF>{0} {1}</color>", "Entry text. {0} is replaced by the total amount, {1} is replaced by the item name.");
            CraftingPatch.overFlowText = Config.Bind<string>("Show Chest Contents", "OverFlowText", "<color=#AAAAAAFF>...</color>", "Overflow text if more items than max entries.");

            iHaveArrivedOnSpawn = config<bool>("Game", "I have arrived disable", true, new ConfigDescription("Auto repair your things when interacting with build station"), true);


            GamePatches.DisableGuardianAnimation = config<bool>("Game", "I have arrived disable", true, new ConfigDescription("Auto repair your things when interacting with build station"), true);
            GamePatches.SkipTuts = config<bool>("Game", "Skip Tuts", true, new ConfigDescription("Auto repair your things when interacting with build station"), true);
            GamePatches.reequipItemsAfterSwimming = config<bool>("Game", "Re Equip after Swimming", true, new ConfigDescription("Auto repair your things when interacting with build station"), true);
            GamePatches.enableAreaRepair = config<bool>("Game", "Area Repair", true, new ConfigDescription("Auto repair your things when interacting with build station"), true);
            GamePatches.areaRepairRadius = config<int>("Game", "Area Repair Radius", -1, "Max number of entries to show", true);
            GamePatches.baseMegingjordBuff = config<int>("Game", "Base Meginjord Buff", -1, "Max number of entries to show", true);
            GamePatches.honeyProductionSpeed = config<int>("Game", "Honey Speed", -1, "Max number of entries to show", true);
            GamePatches.maximumHoneyPerBeehive = config<int>("Game", "Honey Count Per Hive", -1, "Max number of entries to show", true);
            GamePatches.maxPlayers = config<int>("Server", "Max Player Count", -1, "Max number of Players to allow", true);

            GamePatches.StaminaIsEnabled = config<bool>("Player", "Stamina alterations enabled", false, "Stamina alterations enabled", true);
            GamePatches.dodgeStaminaUsage = config<float>("Player", "Dodge Stamina Usage", 1f, "Dodge Stamina Usage", true);
            GamePatches.encumberedStaminaDrain = config<float>("Player", "Encumbered Stamina drain", 1f, "Encumbered Stamina drain", true);
            GamePatches.sneakStaminaDrain = config<float>("Player", "Sneak Stamina Drain", 1f, "Sneak stamina drain", true);
            GamePatches.runStaminaDrain = config<float>("Player", "Run Stamina Drain", 1f, "Run Stamina Drain", true);
            GamePatches.staminaRegenDelay = config<float>("Player", "Delay before stamina regeneration starts", 1f, "Delay before stamina regeneration starts", true);
            GamePatches.staminaRegen = config<float>("Player", "Stamina regen factor", 1f, "Stamina regen factor", true);
            GamePatches.swimStaminaDrain = config<float>("Player", "Stamina drain from swin", 1f, "Stamina drain from swim", true);
            GamePatches.jumpStaminaDrain = config<float>("Player", "Jump stamina drain factor", 1f, "Stamina drain factor for jumping", true);
            GamePatches.baseAutoPickUpRange = config<float>("Player", "Auto pickup range adjustment", 2f, "Auto pickup range adjustment", true);
            GamePatches.disableCameraShake = config<float>("Player", "Cam shake factor", 0, "Cam Shake factor", true);
            GamePatches.baseMaximumWeight = config<float>("Player", "Base maximum weight addition for player", 350f, "Base max weight addition for player", true);
            GamePatches.maximumPlacementDistance = config<float>("General", "Build distance alteration", 15, "Build Distance alteration", true);
            GamePatches.savePlayerProfileInterval = Config.Bind("Global","savePlayerProfileInterval",300,"Interval (in seconds) for how often to save the player profile. Game default (and maximum) is 1200s.");
            GamePatches.setLogoutPointOnSave = Config.Bind("Global","setLogoutPointOnSave", true, "Sets your logout point to your current position when the mod performs a save.");
            GamePatches.showMessageOnModSave = Config.Bind("Global","saveMessageOnModSave", true, "Show a message (in the middle of your screen) when the mod tries to save.");




            ImprovedBuildHudConfig.InventoryAmountFormat = Config.Bind("Building HUD", "Inventory Amount Format", "({0})", "Format for the amount of items in the player inventory to show after the required amount. Uses standard C# format rules. Leave empty to hide altogether.");
            ImprovedBuildHudConfig.InventoryAmountColor = Config.Bind("Building HUD", "Inventory Amount Color", "lightblue", "Color to set the inventory amount after the requirement amount. Leave empty to set no color. You can use the #XXXXXX hex color format.");
            ImprovedBuildHudConfig.CanBuildAmountFormat = Config.Bind("Building HUD", "Can Build Amount Color", "({0})", "Format for the amount of times you can build the currently selected item with your current inventory. Uses standard C# format rules. Leave empty to hide altogether.");
            ImprovedBuildHudConfig.CanBuildAmountColor = Config.Bind("Building HUD", "Can Build Amount Color", "white", "Color to set the can-build amount. Leave empty to set no color. You can use the #XXXXXX hex color format.");

            SignPatches.signScale = Config.Bind<Vector3>("Signs", "SignScale", new Vector3(1, 1, 1), "Sign scale (w,h,d)");
            SignPatches.textPositionOffset = Config.Bind<Vector2>("Signs", "TextPositionOffset", new Vector2(0, 0), "Default font size");
            SignPatches.useRichText = Config.Bind<bool>("Signs", "UseRichText", true, "Enable rich text");
            SignPatches.fontName = Config.Bind<string>("Signs", "FontName", "AveriaSerifLibre-Bold", "Font name");


            AutoStorePatch.dropRangeChests = Config.Bind<float>("Auto Storage", "DropRangeChests", 5f, "The maximum range to pull dropped items");
            AutoStorePatch.dropRangePersonalChests = Config.Bind<float>("Auto Storage", "DropRangePersonalChests", 5f, "The maximum range to pull dropped items");
            AutoStorePatch.dropRangeReinforcedChests = Config.Bind<float>("Auto Storage", "DropRangeReinforcedChests", 5f, "The maximum range to pull dropped items");
            AutoStorePatch.dropRangeCarts = Config.Bind<float>("Auto Storage", "DropRangeCarts", 5f, "The maximum range to pull dropped items");
            AutoStorePatch.dropRangeShips = Config.Bind<float>("Auto Storage", "DropRangeShips", 5f, "The maximum range to pull dropped items");
            AutoStorePatch.itemDisallowTypes = Config.Bind<string>("Auto Storage", "ItemDisallowTypes", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypes = Config.Bind<string>("Auto Storage", "ItemAllowTypes", "", "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypes");
            AutoStorePatch.itemDisallowTypesChests = Config.Bind<string>("Auto Storage", "ItemDisallowTypesChests", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesChests = Config.Bind<string>("Auto Storage", "ItemAllowTypesChests", "", "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesChests");
            AutoStorePatch.itemDisallowTypesPersonalChests = Config.Bind<string>("Auto Storage", "ItemDisallowTypesPersonalChests", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesPersonalChests = Config.Bind<string>("Auto Storage", "ItemAllowTypesPersonalChests", "", "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesPersonalChests");
            AutoStorePatch.itemDisallowTypesReinforcedChests = Config.Bind<string>("Auto Storage", "ItemDisallowTypesReinforcedChests", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesReinforcedChests = Config.Bind<string>("General", "ItemAllowTypesReinforcedChests", "", "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesReinforcedChests");
            AutoStorePatch.itemDisallowTypesCarts = Config.Bind<string>("Auto Storage", "ItemDisallowTypesCarts", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesCarts = Config.Bind<string>("Auto Storage", "ItemAllowTypesCarts", "", "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesCarts");
            AutoStorePatch.itemDisallowTypesShips = Config.Bind<string>("Auto Storage", "ItemDisallowTypesShips", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesShips = Config.Bind<string>("Auto Storage", "ItemAllowTypesShips", "", "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesShips");
            AutoStorePatch.toggleString = Config.Bind<string>("Auto Storage", "ToggleString", "Auto Pull: {0}", "Text to show on toggle. {0} is replaced with true/false");
            AutoStorePatch.toggleKey = Config.Bind<string>("Auto Storage", "ToggleKey", "", "Key to toggle behaviour. Leave blank to disable the toggle key.");
            AutoStorePatch.mustHaveItemToPull = Config.Bind<bool>("Auto Storage", "MustHaveItemToPull", false, "If true, a container must already have at least one of the item to pull.");
            AutoStorePatch.isOn = Config.Bind<bool>("Auto Storage", "IsOn", true, "Behaviour is currently on or not");


            EAQS.EAQS.extraRows = Config.Bind<int>("EAQS", "ExtraRows", 0, "Number of extra ordinary rows.");
            EAQS.EAQS.addEquipmentRow = Config.Bind<bool>("EAQS", "AddEquipmentRow", true, "Add special row for equipped items and quick slots.");
            EAQS.EAQS.displayEquipmentRowSeparate = Config.Bind<bool>("EAQS", "DisplayEquipmentRowSeparate", true, "Display equipment and quickslots in their own area.");

            EAQS.EAQS.helmetText = Config.Bind<string>("EAQS", "HelmetText", "Head", "Text to show for helmet slot.");
            EAQS.EAQS.chestText = Config.Bind<string>("EAQS", "ChestText", "Chest", "Text to show for chest slot.");
            EAQS.EAQS.legsText = Config.Bind<string>("EAQS", "LegsText", "Legs", "Text to show for legs slot.");
            EAQS.EAQS.backText = Config.Bind<string>("EAQS", "BackText", "Back", "Text to show for back slot.");
            EAQS.EAQS.utilityText = Config.Bind<string>("EAQS", "UtilityText", "Utility", "Text to show for utility slot.");

            EAQS.EAQS.quickAccessScale = Config.Bind<float>("EAQS", "QuickAccessScale", 1, "Scale of quick access bar.");

            EAQS.EAQS.hotKey1 = Config.Bind<string>("EAQS", "HotKey1", "z", "Hotkey 1 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            EAQS.EAQS.hotKey2 = Config.Bind<string>("EAQS", "HotKey2", "x", "Hotkey 2 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            EAQS.EAQS.hotKey3 = Config.Bind<string>("EAQS", "HotKey3", "c", "Hotkey 3 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");

            EAQS.EAQS.modKeyOne = Config.Bind<string>("EAQS", "ModKey1", "mouse 0", "First modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.");
            EAQS.EAQS.modKeyTwo = Config.Bind<string>("EAQS", "ModKey2", "left ctrl", "Second modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.");

            EAQS.EAQS.quickAccessX = Config.Bind<float>("EAQS", "quickAccessX", 9999, "Current X of Quick Slots");
            EAQS.EAQS.quickAccessY = Config.Bind<float>("EAQS", "quickAccessY", 9999, "Current Y of Quick Slots");
            
            
            PlantGrowth.displayGrowth = config<bool>("PlantGrowth", "DisplayGrowth", true, "Display growth progress in hover text");
            PlantGrowth.plantAnywhere = config<bool>("PlantGrowth", "PlantAnywhere", false, "Don't require cultivated ground to plant anything");
            PlantGrowth.ignoreBiome = config<bool>("PlantGrowth", "IgnoreBiome", false, "Allow planting anything in any biome.");
            PlantGrowth.ignoreSun = config<bool>("PlantGrowth", "IgnoreSun", false, "Allow planting under roofs.");
            PlantGrowth.preventPlantTooClose = config<bool>("PlantGrowth", "PreventPlantTooClose", true, "Prevent plants from being planted if they are too close together to grow.");
            PlantGrowth.preventDestroyIfCantGrow = config<bool>("PlantGrowth", "PreventDestroyIfCantGrow", false, "Prevent destruction of plants that normally are destroyed if they can't grow.");
            PlantGrowth.growthTimeMultTree = config<float>("PlantGrowth", "GrowthTimeMultTree", 1f, "Multiply time taken to grow by this amount.");
            PlantGrowth.growRadiusMultTree = config<float>("PlantGrowth", "GrowthRadiusMultTree", 1f, "Multiply required space to grow by this amount.");
            PlantGrowth.minScaleMultTree = config<float>("PlantGrowth", "MinScaleMultTree", 1f, "Multiply minimum size by this amount.");
            PlantGrowth.maxScaleMultTree = config<float>("PlantGrowth", "MaxScaleMultTree", 1f, "Multiply maximum size by this amount.");
            PlantGrowth.growthTimeMultPlant = config<float>("PlantGrowth", "GrowthTimeMultPlant", 1f, "Multiply time taken to grow by this amount.");
            PlantGrowth.growRadiusMultPlant = config<float>("PlantGrowth", "GrowthRadiusMultPlant", 1f, "Multiply required space to grow by this amount.");
            PlantGrowth.minScaleMultPlant = config<float>("PlantGrowth", "MinScaleMultPlant", 1f, "Multiply minimum size by this amount.");
            PlantGrowth.maxScaleMultPlant = config<float>("PlantGrowth", "MaxScaleMultPlant", 1f, "Multiply maximum size by this amount.");

            if (!modEnabled.Value)
                return;

            EAQS.EAQS.hotkeys = new ConfigEntry<string>[]
            {
                EAQS.EAQS.hotKey1,
                EAQS.EAQS.hotKey2,
                EAQS.EAQS.hotKey3,
            };

            if (!Directory.Exists(VMP_DatadirectoryPath))
            {
                Directory.CreateDirectory(VMP_DatadirectoryPath);
            }
            SignPatches.currentFont = SignPatches.GetFont(SignPatches.fontName.Value, 20);
            SignPatches.lastFontName = SignPatches.currentFont?.name;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
        private void OnDestroy()
        {
            CraftFromContainersInstalledAndActive = false;

        }

        private void LateUpdate()
        {
            if (CraftFromContainersInstalledAndActive)
            {
                if (_cachedContainers != null)
                {
                    _cachedContainers.Clear();
                    _cachedContainers = null;
                }
            }
        }
        private void Update()
        {
            if (Utils.IgnoreKeyPresses())
                return;
            if (CheckKeyDown(AutoStorePatch.toggleKey.Value))
            {
                AutoStorePatch.isOn.Value = !AutoStorePatch.isOn.Value;
                Config.Save();
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, string.Format(AutoStorePatch.toggleString.Value, AutoStorePatch.isOn.Value), 0, null);
            }

            if (Minimap.instance && Player.m_localPlayer && ZNet.instance != null)
                StartCoroutine(MapDetail.UpdateMap(false));
        }
        private static bool CheckKeyDown(string value)
        {
            try
            {
                return Input.GetKeyDown(value.ToLower());
            }
            catch
            {
                return false;
            }
        }
        public static int GetAvailableItems(string itemName)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return 0;
            }

            var playerInventoryCount = player.GetInventory().CountItems(itemName);
            var containerCount = 0;

            /*if (CraftFromContainersInstalledAndActive)
            {
                if (_cachedContainers == null)
                {
                    _cachedContainers = CraftFromContainers.BepInExPlugin.GetNearbyContainers(player.transform.position);
                }
                foreach (var container in _cachedContainers)
                {
                    containerCount += container.GetInventory().CountItems(itemName);
                }
            }*/

            return playerInventoryCount + containerCount;
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
        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        public static class Game_Start_Patch
        {
            private static void Prefix()
            {
                ZRoutedRpc.instance.Register("VMPMapSync", new Action<long, ZPackage>(MapSync.RPC_VMPMapSync)); //Map Sync
                ZRoutedRpc.instance.Register("VMPMapPinSync", new Action<long, ZPackage>(VMPMapPinSync.RPC_VMPMapPinSync)); //Map Pin Sync
                ZRoutedRpc.instance.Register("VMPAck", new Action<long>(VMPAck.RPC_VPlusAck)); //Ack
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
