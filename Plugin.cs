using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using UnityEngine.EventSystems;
using VMP_Mod.MapSharing;
using VMP_Mod.Patches;
using static VMP_Mod.Patches.SignPatches;

namespace VMP_Mod
{
    public static class ImprovedBuildHudConfig
    {
        public static ConfigEntry<string> InventoryAmountFormat;
        public static ConfigEntry<string> InventoryAmountColor;
        public static ConfigEntry<string> CanBuildAmountFormat;
        public static ConfigEntry<string> CanBuildAmountColor;
    }

    [BepInPlugin(GUID, ModName, Version)]
    public partial class VMP_Modplugin : BaseUnityPlugin
    {
        public const string Version = "0.0.9";
        public const string ModName = "VMP Mod";
        public const string GUID = "com.vmp.mod";
        private static readonly int windowId = 434343;

        public static VMP_Modplugin context;


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

        public static ConfigSync configSync = new ConfigSync(GUID) {DisplayName = ModName, CurrentVersion = Version};
        private static List<Container> _cachedContainers;

        public static bool CraftFromContainersInstalledAndActive;
        private ConfigEntry<bool> serverConfigLocked;


        public void Awake()
        {
            serverConfigLocked = config("General", "Lock Configuration", true, "Lock Configuration");
            configSync.AddLockingConfigEntry(serverConfigLocked);
            shareMapProgression = config("Maps", "Share Map Progress with others", false,
                "Share Map Progress with others");
            mapIsEnabled = config("Maps", "Map Control enabled", true, "Map Control enabled");
            shareablePins = config("Maps", "Share Pins", true, "Share pins with other players");
            shareAllPins = config("Maps", "Share ALL pins with other players", false,
                "Share ALL pins with other players");
            preventPlayerFromTurningOffPublicPosition =
                config("General", "IsDebug", true, "Show debug messages in log");
            displayCartsAndBoats = config("Maps", "Display Boats/Carts", true, "Show Boats and carts on the map");
            exploreRadius = config("Maps", "NexusID", 40, "Explore radius addition");


            modEnabled = config("General", "Enabled", true, "Enable this mod");
            isDebug = config("General", "IsDebug", true, "Show debug messages in log");

            Container_Configs.KarveRow = config("Containers", "Karve Rows", 2,
                new ConfigDescription("Rows for Karve", new AcceptableValueRange<int>(2, 30)));
            Container_Configs.KarveCol = config("Containers", "Karve Columns", 2,
                new ConfigDescription("Columns for Karve", new AcceptableValueRange<int>(2, 8)));
            Container_Configs.LongRow = config("Containers", "Longboat Rows", 3,
                new ConfigDescription("Rows for longboat", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.LongCol = config("Containers", "Longboat Columns", 6,
                new ConfigDescription("Columns for longboat", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.CartRow = config("Containers", "Cart Rows", 3,
                new ConfigDescription("Rows for Cart", new AcceptableValueRange<int>(3, 30)));
            Container_Configs.CartCol = config("Containers", "Cart Colums", 6,
                new ConfigDescription("Columns for Cart", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.PersonalRow = config("Containers", "Personal Chest Rows", 2,
                new ConfigDescription("Personal Chest Rows", new AcceptableValueRange<int>(2, 20)));
            Container_Configs.PersonalCol = config("Containers", "Personal Chest Colums", 3,
                new ConfigDescription("Personal Chest Colums", new AcceptableValueRange<int>(3, 8)));
            Container_Configs.WoodRow = config("Containers", "Wood Chest Rows", 2,
                new ConfigDescription("Wood Chest Rows", new AcceptableValueRange<int>(2, 10)));
            Container_Configs.WoodCol = config("Containers", "Wood Chest Colums", 5,
                new ConfigDescription("Wood Chest Colums", new AcceptableValueRange<int>(5, 8)));
            Container_Configs.IronRow = config("Containers", "Iron Chest Rows", 3,
                new ConfigDescription("Iron Chest Rows", new AcceptableValueRange<int>(3, 20)));
            Container_Configs.IronCol = config("Containers", "Iron Chest Colums", 6,
                new ConfigDescription("Iron Chest Colums", new AcceptableValueRange<int>(6, 8)));


            CraftingPatch.WorkbenchRange = config("WorkBench", "WorkBenchRange", 40,
                new ConfigDescription("Range you can build from workbench in meters",
                    new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.workbenchEnemySpawnRange = config("WorkBench", "WorkBenchRange (Playerbase size)", 40,
                new ConfigDescription("Workbench PlayerBase radius, this is how far away enemies spawn",
                    new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.AlterWorkBench = config("WorkBench", "Change No Roof Behavior", true, "Show building pieces");
            workbenchAttachmentRange = config("WorkBench", "WorkBench Extension", 40,
                new ConfigDescription("Range for workbench extensions", new AcceptableValueRange<int>(5, 100)));

            CraftingPatch.useScrollWheel =
                Config.Bind("Settings", "ScrollWheel", true, "Use scroll wheel to switch filter");
            CraftingPatch.showMenu = Config.Bind("Settings", "ShowMenu", true, "Show filter menu on hover");
            CraftingPatch.scrollModKey = Config.Bind("Settings", "ScrollModKey", "",
                "Modifer key to allow scroll wheel change. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            CraftingPatch.categoryFile =
                Config.Bind("Settings", "CategoryFile", "categories.json", "Category file name");
            CraftingPatch.prevHotKey = Config.Bind("Settings", "HotKeyPrev", "",
                "Hotkey to switch to previous filter. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            CraftingPatch.nextHotKey = Config.Bind("Settings", "HotKeyNext", "",
                "Hotkey to switch to next filter. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            CraftingPatch.assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                typeof(VMP_Modplugin).Namespace);


            WeightReduction = config("Items", "Item Weight Increase", 1.25f,
                new ConfigDescription("Multiplier for your item weight"));
            itemStackMultiplier = config("Items", "Item Stack Increase", 2f,
                new ConfigDescription("Multiplier for your item stacks"));
            NoTeleportPrevention = config("Items", "Disable Teleport check for items", false,
                new ConfigDescription("Disable Teleport check for items"));
            filltoptobottom = config("Items", "Fill your things top to bottom when moving from inv to chest", true,
                new ConfigDescription("Move your things top to bottom when changing from inv to chest"));

            Deconstruct = config("Items", "Allow deconstruction of items in crafting menu", true,
                new ConfigDescription("Deconstructing crafting items for return of mats"));
            AutoRepair = config("Items", "Auto repair your things when interacting with build station", true,
                new ConfigDescription("Auto repair your things when interacting with build station"));
            returnedpercent = config("Items", "Percent of item materials you would recieve back from deconstruction",
                100, new ConfigDescription("Perecent of item mats you get back from deconstructin tab"));


            MapDetail.showRange = config("Map Details", "ShowRange", 50f,
                "Range in metres around player to show details");
            MapDetail.updateDelta = config("Map Details", "UpdateDelta", 5f,
                "Distance in metres to move before automatically updating the map details");
            MapDetail.showBuildings = config("Map Details", "ShowBuildings", true, "Show building pieces");
            MapDetail.personalBuildingColor = Config.Bind("Map Details", "PersonalBuildingColor", Color.green,
                "Color of one's own build pieces");
            MapDetail.otherBuildingColor = Config.Bind("Map Details", "OtherBuildingColor", Color.red,
                "Color of other players' build pieces");
            MapDetail.unownedBuildingColor = Config.Bind("Map Details", "UnownedBuildingColor", Color.yellow,
                "Color of npc build pieces");
            MapDetail.customPlayerColors = Config.Bind("Map Details", "CustomPlayerColors", "",
                "Custom color list, comma-separated. Use either <name>:<colorCode> pair entries or just <colorCode> entries. E.g. Erinthe:FF0000 or just FF0000. The latter will assign a color randomly to each connected peer.");

            toggleClockKeyMod = Config.Bind("Clock", "ShowClockKeyMod", "",
                "Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            toggleClockKey = Config.Bind("Clock", "ShowClockKey", "home",
                "Key used to toggle the clock display. use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            clockLocationString = Config.Bind("Clock", "ClockLocationString", "50%,3%",
                "Location on the screen to show the clock (x,y) or (x%,y%)");

            LoadConfig();

            CraftingPatch.maxEntries =
                Config.Bind("Show Chest Contents", "MaxEntries", -1, "Max number of entries to show");
            CraftingPatch.sortType = Config.Bind("Show Chest Contents", "SortType", CraftingPatch.SortType.Value,
                "Type by which to sort entries.");
            CraftingPatch.sortAsc = Config.Bind("Show Chest Contents", "SortAsc", false, "Sort ascending?");
            CraftingPatch.entryString = Config.Bind("Show Chest Contents", "EntryText",
                "<color=#AAAAAAFF>{0} {1}</color>",
                "Entry text. {0} is replaced by the total amount, {1} is replaced by the item name.");
            CraftingPatch.overFlowText = Config.Bind("Show Chest Contents", "OverFlowText",
                "<color=#AAAAAAFF>...</color>", "Overflow text if more items than max entries.");

            iHaveArrivedOnSpawn = config("Game", "I have arrived disable", true,
                new ConfigDescription("Auto repair your things when interacting with build station"));


            GamePatches.DisableGuardianAnimation = config("Game", "I have arrived disable", true,
                new ConfigDescription("Auto repair your things when interacting with build station"));
            GamePatches.SkipTuts = config("Game", "Skip Tuts", true,
                new ConfigDescription("Auto repair your things when interacting with build station"));
            GamePatches.reequipItemsAfterSwimming = config("Player", "Re Equip after Swimming", true,
                new ConfigDescription("Auto repair your things when interacting with build station"));
            GamePatches.enableAreaRepair = config("Player", "Area Repair", true,
                new ConfigDescription("Auto repair your things when interacting with build station"));
            GamePatches.areaRepairRadius = config("Player", "Area Repair Radius", 15, "Max number of entries to show");
            GamePatches.baseMegingjordBuff =
                config("Player", "Base Meginjord Buff", 150, "Max number of entries to show");
            GamePatches.honeyProductionSpeed = config("Game", "Honey Speed", -1, "Max number of entries to show");
            GamePatches.maximumHoneyPerBeehive =
                config("Game", "Honey Count Per Hive", 4, "Max number of entries to show");
            GamePatches.maxPlayers = config("Server", "Max Player Count", 50, "Max number of Players to allow");

            GamePatches.StaminaIsEnabled =
                config("Player", "Stamina alterations enabled", false, "Stamina alterations enabled");
            GamePatches.dodgeStaminaUsage = config("Player", "Dodge Stamina Usage", 1f, "Dodge Stamina Usage");
            GamePatches.encumberedStaminaDrain =
                config("Player", "Encumbered Stamina drain", 1f, "Encumbered Stamina drain");
            GamePatches.sneakStaminaDrain = config("Player", "Sneak Stamina Drain", 1f, "Sneak stamina drain");
            GamePatches.runStaminaDrain = config("Player", "Run Stamina Drain", 1f, "Run Stamina Drain");
            GamePatches.staminaRegenDelay = config("Player", "Delay before stamina regeneration starts", 1f,
                "Delay before stamina regeneration starts");
            GamePatches.staminaRegen = config("Player", "Stamina regen factor", 1f, "Stamina regen factor");
            GamePatches.swimStaminaDrain = config("Player", "Stamina drain from swin", 1f, "Stamina drain from swim");
            GamePatches.jumpStaminaDrain =
                config("Player", "Jump stamina drain factor", 1f, "Stamina drain factor for jumping");
            GamePatches.baseAutoPickUpRange =
                config("Player", "Auto pickup range adjustment", 2f, "Auto pickup range adjustment");
            GamePatches.disableCameraShake = config<float>("Player", "Cam shake factor", 0, "Cam Shake factor");
            GamePatches.baseMaximumWeight = config("Player", "Base maximum weight addition for player", 350f,
                "Base max weight addition for player");
            GamePatches.maximumPlacementDistance = config<float>("WorkBench", "Build distance alteration", 15,
                "Build Distance alteration");

            ImprovedBuildHudConfig.InventoryAmountFormat = Config.Bind("Building HUD", "Inventory Amount Format",
                "({0})",
                "Format for the amount of items in the player inventory to show after the required amount. Uses standard C# format rules. Leave empty to hide altogether.");
            ImprovedBuildHudConfig.InventoryAmountColor = Config.Bind("Building HUD", "Inventory Amount Color",
                "lightblue",
                "Color to set the inventory amount after the requirement amount. Leave empty to set no color. You can use the #XXXXXX hex color format.");
            ImprovedBuildHudConfig.CanBuildAmountFormat = Config.Bind("Building HUD", "Can Build Amount Color", "({0})",
                "Format for the amount of times you can build the currently selected item with your current inventory. Uses standard C# format rules. Leave empty to hide altogether.");
            ImprovedBuildHudConfig.CanBuildAmountColor = Config.Bind("Building HUD", "Can Build Amount Color", "white",
                "Color to set the can-build amount. Leave empty to set no color. You can use the #XXXXXX hex color format.");

            signScale = Config.Bind("Signs", "SignScale", new Vector3(1, 1, 1), "Sign scale (w,h,d)");
            textPositionOffset =
                Config.Bind("Signs", "TextPositionOffset", new Vector2(0, 0), "Default font size");
            useRichText = Config.Bind("Signs", "UseRichText", true, "Enable rich text");
            fontName = Config.Bind("Signs", "FontName", "AveriaSerifLibre-Bold", "Font name");


            AutoStorePatch.dropRangeChests = Config.Bind("Auto Storage", "DropRangeChests", 5f,
                "The maximum range to pull dropped items");
            AutoStorePatch.dropRangePersonalChests = Config.Bind("Auto Storage", "DropRangePersonalChests", 5f,
                "The maximum range to pull dropped items");
            AutoStorePatch.dropRangeReinforcedChests = Config.Bind("Auto Storage", "DropRangeReinforcedChests", 5f,
                "The maximum range to pull dropped items");
            AutoStorePatch.dropRangeCarts = Config.Bind("Auto Storage", "DropRangeCarts", 5f,
                "The maximum range to pull dropped items");
            AutoStorePatch.dropRangeShips = Config.Bind("Auto Storage", "DropRangeShips", 5f,
                "The maximum range to pull dropped items");
            AutoStorePatch.itemDisallowTypes = Config.Bind("Auto Storage", "ItemDisallowTypes", "",
                "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypes = Config.Bind("Auto Storage", "ItemAllowTypes", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypes");
            AutoStorePatch.itemDisallowTypesChests = Config.Bind("Auto Storage", "ItemDisallowTypesChests", "",
                "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesChests = Config.Bind("Auto Storage", "ItemAllowTypesChests", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesChests");
            AutoStorePatch.itemDisallowTypesPersonalChests = Config.Bind("Auto Storage",
                "ItemDisallowTypesPersonalChests", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesPersonalChests = Config.Bind("Auto Storage", "ItemAllowTypesPersonalChests",
                "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesPersonalChests");
            AutoStorePatch.itemDisallowTypesReinforcedChests = Config.Bind("Auto Storage",
                "ItemDisallowTypesReinforcedChests", "", "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesReinforcedChests = Config.Bind("Auto Storage",
                "ItemAllowTypesReinforcedChests", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesReinforcedChests");
            AutoStorePatch.itemDisallowTypesCarts = Config.Bind("Auto Storage", "ItemDisallowTypesCarts", "",
                "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesCarts = Config.Bind("Auto Storage", "ItemAllowTypesCarts", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesCarts");
            AutoStorePatch.itemDisallowTypesShips = Config.Bind("Auto Storage", "ItemDisallowTypesShips", "",
                "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypesShips = Config.Bind("Auto Storage", "ItemAllowTypesShips", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesShips");
            AutoStorePatch.toggleString = Config.Bind("Auto Storage", "ToggleString", "Auto Pull: {0}",
                "Text to show on toggle. {0} is replaced with true/false");
            AutoStorePatch.toggleKey = Config.Bind("Auto Storage", "ToggleKey", "",
                "Key to toggle behaviour. Leave blank to disable the toggle key.");
            AutoStorePatch.mustHaveItemToPull = Config.Bind("Auto Storage", "MustHaveItemToPull", false,
                "If true, a container must already have at least one of the item to pull.");
            AutoStorePatch.isOn = Config.Bind("Auto Storage", "IsOn", true, "Behaviour is currently on or not");


            EAQS.EAQS.extraRows = Config.Bind<int>("EAQS", "ExtraRows", 0, "Number of extra ordinary rows.");
            EAQS.EAQS.addEquipmentRow = Config.Bind<bool>("EAQS", "AddEquipmentRow", true, "Add special row for equipped items and quick slots.");
            EAQS.EAQS.displayEquipmentRowSeparate = Config.Bind<bool>("EAQS", "DisplayEquipmentRowSeparate", true, "Display equipment and quickslots in their own area.");

            EAQS.EAQS.helmetText = Config.Bind("EAQS", "HelmetText", "Head", "Text to show for helmet slot.");
            EAQS.EAQS.chestText = Config.Bind("EAQS", "ChestText", "Chest", "Text to show for chest slot.");
            EAQS.EAQS.legsText = Config.Bind("EAQS", "LegsText", "Legs", "Text to show for legs slot.");
            EAQS.EAQS.backText = Config.Bind("EAQS", "BackText", "Back", "Text to show for back slot.");
            EAQS.EAQS.utilityText = Config.Bind("EAQS", "UtilityText", "Utility", "Text to show for utility slot.");

            EAQS.EAQS.quickAccessScale = Config.Bind<float>("EAQS", "QuickAccessScale", 1, "Scale of quick access bar.");

            EAQS.EAQS.hotKey1 = Config.Bind("EAQS", "HotKey1", "z", "Hotkey 1 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            EAQS.EAQS.hotKey2 = Config.Bind("EAQS", "HotKey2", "x", "Hotkey 2 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");
            EAQS.EAQS.hotKey3 = Config.Bind("EAQS", "HotKey3", "c", "Hotkey 3 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html");

            EAQS.EAQS.modKeyOne = Config.Bind<string>("EAQS", "ModKey1", "mouse 0", "First modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.");
            EAQS.EAQS.modKeyTwo = Config.Bind<string>("EAQS", "ModKey2", "left ctrl", "Second modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.");

            EAQS.EAQS.quickAccessX = Config.Bind<float>("EAQS", "quickAccessX", 9999, "Current X of Quick Slots");
            EAQS.EAQS.quickAccessY = Config.Bind<float>("EAQS", "quickAccessY", 9999, "Current Y of Quick Slots");

            ClientPatches._chatPlayerName =
                Config.Bind<string>(
                    "Names", "chatPlayerName", string.Empty, "Override your player name shown in-game and in the chat box.");

            PlantGrowth.displayGrowth =
                config("PlantGrowth", "DisplayGrowth", true, "Display growth progress in hover text");
            PlantGrowth.plantAnywhere = config("PlantGrowth", "PlantAnywhere", false,
                "Don't require cultivated ground to plant anything");
            PlantGrowth.ignoreBiome =
                config("PlantGrowth", "IgnoreBiome", false, "Allow planting anything in any biome.");
            PlantGrowth.ignoreSun = config("PlantGrowth", "IgnoreSun", false, "Allow planting under roofs.");
            PlantGrowth.preventPlantTooClose = config("PlantGrowth", "PreventPlantTooClose", true,
                "Prevent plants from being planted if they are too close together to grow.");
            PlantGrowth.preventDestroyIfCantGrow = config("PlantGrowth", "PreventDestroyIfCantGrow", false,
                "Prevent destruction of plants that normally are destroyed if they can't grow.");
            PlantGrowth.growthTimeMultTree = config("PlantGrowth", "GrowthTimeMultTree", 1f,
                "Multiply time taken to grow by this amount.");
            PlantGrowth.growRadiusMultTree = config("PlantGrowth", "GrowthRadiusMultTree", 1f,
                "Multiply required space to grow by this amount.");
            PlantGrowth.minScaleMultTree = config("PlantGrowth", "MinScaleMultTree", 1f,
                "Multiply minimum size by this amount.");
            PlantGrowth.maxScaleMultTree = config("PlantGrowth", "MaxScaleMultTree", 1f,
                "Multiply maximum size by this amount.");
            PlantGrowth.growthTimeMultPlant = config("PlantGrowth", "GrowthTimeMultPlant", 1f,
                "Multiply time taken to grow by this amount.");
            PlantGrowth.growRadiusMultPlant = config("PlantGrowth", "GrowthRadiusMultPlant", 1f,
                "Multiply required space to grow by this amount.");
            PlantGrowth.minScaleMultPlant = config("PlantGrowth", "MinScaleMultPlant", 1f,
                "Multiply minimum size by this amount.");
            PlantGrowth.maxScaleMultPlant = config("PlantGrowth", "MaxScaleMultPlant", 1f,
                "Multiply maximum size by this amount.");

            WearNTear_Patches.NoWeatherDam = config("WearNTear_Patches", "No Weather Damgae to bldgs", true,
                "No Weather Damgae to bldgs");
            WearNTear_Patches.DisableStructintegrity = config("WearNTear_Patches",
                "Disable Structural Integrety system", true, "Disable Structural Integrety system");
            WearNTear_Patches.DisableBoatDamage =
                config("WearNTear_Patches", "Disable Boat Damage", true, "Disable Boat Damage");
            WearNTear_Patches.NoPlayerStructDam = config("WearNTear_Patches", "No Damgae to player bldgs", true,
                "No Damgae to player bldgs");

            WearNTear_Patches.StructuralIntegritywood = config<float>("WearNTear_Patches", "Wood Structural Integrity",
                100, "Wood Structural Integrity");
            WearNTear_Patches.StructuralIntegritystone = config<float>("WearNTear_Patches",
                "Stone Structural Integrity", 100, "Stone Structural Integrity");
            WearNTear_Patches.StructuralIntegrityiron = config<float>("WearNTear_Patches", "Iron Structural Integrity",
                100, "Iron Structural Integrity");
            WearNTear_Patches.StructuralIntegrityhardWood = config<float>("WearNTear_Patches",
                "Hardwood Structural Integrity", 100, "Hardwood Structural Integrity");

            SkillPatches.ChangeSkills =
                config("Skills", "Change the skill gain factor", false, "Change skill gain factor");
            SkillPatches.experienceGainedNotifications = Config.Bind("Skills",
                "Display notifications for skills gained", false, "Display notifications for skills gained");
            SkillPatches.swordskill = config("Skills", "Sword Skill gain factor", 0f, "Sword skill gain factor");
            SkillPatches.kniveskill = config("Skills", "Knives Skill gain factor", 0f, "Knives skill gain factor");
            SkillPatches.clubskill = config("Skills", "Clubs Skill gain factor", 0f, "Clubs skill gain factor");
            SkillPatches.polearmskill = config("Skills", "Polearm Skill gain factor", 0f, "Polearm skill gain factor");
            SkillPatches.spearskill = config("Skills", "Spear Skill gain factor", 0f, "Spear skill gain factor");
            SkillPatches.blockskill = config("Skills", "Block Skill gain factor", 0f, "Block skill gain factor");
            SkillPatches.axeskill = config("Skills", "Axe Skill gain factor", 0f, "Axe skill gain factor");
            ;
            SkillPatches.bowskill = config("Skills", "Bow Skill gain factor", 0f, "Bow skill gain factor");
            SkillPatches.unarmed = config("Skills", "Unarmed Skill gain factor", 0f, "SwoUnarmedrd skill gain factor");
            SkillPatches.pickaxe = config("Skills", "Pickaxe Skill gain factor", 0f, "Pickaxe skill gain factor");
            SkillPatches.woodcutting =
                config("Skills", "WoodCutting Skill gain factor", 0f, "WoodCutting skill gain factor");
            SkillPatches.jump = config("Skills", "Jump Skill gain factor", 0f, "Jump skill gain factor");
            SkillPatches.run = config("Skills", "Run Skill gain factor", 0f, "Run skill gain factor");
            SkillPatches.sneak = config("Skills", "Sneak Skill gain factor", 0f, "Sneak skill gain factor");
            SkillPatches.swim = config("Skills", "Swim Skill gain factor", 0f, "Swim skill gain factor");
            SkillPatches.deathPenaltyMultiplier = config("Skills", "Death Penalty Factor Multiplier", 0f,
                "Death Penalty Factor Multiplier");

            if (!modEnabled.Value)
                return;

            EAQS.EAQS.hotkeys = new ConfigEntry<string>[]
            {
                EAQS.EAQS.hotKey1,
                EAQS.EAQS.hotKey2,
                EAQS.EAQS.hotKey3,
            };

            if (!Directory.Exists(VMP_DatadirectoryPath)) Directory.CreateDirectory(VMP_DatadirectoryPath);
            currentFont = GetFont(fontName.Value, 20);
            lastFontName = currentFont?.name;
            CraftingPatch.LoadCategories();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            if (ZNet.m_isServer && shareMapProgression.Value)
            {
                mapSyncSaveTimer.AutoReset = true;
                mapSyncSaveTimer.Elapsed += (sender, args) => MapSync.SaveMapDataToDisk();
            }
            
            On.Chat.SendText += ClientPatches.ChatSendTextPrefix;
            On.Chat.SendPing += ClientPatches.ChatSendPingPrefix;

            On.Player.GetPlayerName += ClientPatches.PlayerGetPlayerNamePrefix;
            On.PlayerProfile.GetName += ClientPatches.PlayerProfileGetNamePrefix;

            On.Game.SpawnPlayer += ClientPatches.GameSpawnPlayerPostfix;
        }

        private void Update()
        {
            if (Utilities.IgnoreKeyPresses())
                return;
            if (CheckKeyDown(AutoStorePatch.toggleKey.Value))
            {
                AutoStorePatch.isOn.Value = !AutoStorePatch.isOn.Value;
                Config.Save();
                Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                    string.Format(AutoStorePatch.toggleString.Value, AutoStorePatch.isOn.Value));
            }

            if (Minimap.instance && Player.m_localPlayer && ZNet.instance != null)
                StartCoroutine(MapDetail.UpdateMap(false));

            if (!modEnabled.Value || !Player.m_localPlayer || !InventoryGui.IsVisible() ||
                !Player.m_localPlayer.GetCurrentCraftingStation() && !Player.m_localPlayer.NoCostCheat())
            {
                CraftingPatch.lastCategoryIndex = 0;
                CraftingPatch.UpdateDropDown(false);
                return;
            }

            if (!InventoryGui.instance.InCraftTab())
            {
                CraftingPatch.UpdateDropDown(false);
                return;
            }

            var hover = false;

            var mousePos = Input.mousePosition;

            if (CraftingPatch.lastMousePos == Vector3.zero)
                CraftingPatch.lastMousePos = mousePos;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = CraftingPatch.lastMousePos
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            foreach (var rcr in raycastResults)
                if (rcr.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    if (rcr.gameObject.name == "Craft")
                    {
                        hover = true;
                        if (CraftingPatch.tabCraftPressed == 0)
                        {
                            if (CraftingPatch.useScrollWheel.Value &&
                                Utilities.CheckKeyHeld(CraftingPatch.scrollModKey.Value, false) &&
                                Input.mouseScrollDelta.y != 0) CraftingPatch.SwitchFilter(Input.mouseScrollDelta.y < 0);
                            if (CraftingPatch.showMenu.Value) CraftingPatch.UpdateDropDown(true);
                        }
                    }
                    else if (CraftingPatch.dropDownList.Contains(rcr.gameObject))
                    {
                        hover = true;
                    }
                }

            if (!hover)
            {
                if (CraftingPatch.tabCraftPressed > 0)
                    CraftingPatch.tabCraftPressed--;
                CraftingPatch.UpdateDropDown(false);
            }

            if (Utilities.CheckKeyDown(CraftingPatch.prevHotKey.Value))
                CraftingPatch.SwitchFilter(false);
            else if (Utilities.CheckKeyDown(CraftingPatch.nextHotKey.Value)) CraftingPatch.SwitchFilter(true);

            CraftingPatch.lastMousePos = Input.mousePosition;
            if (Utilities.IgnoreKeyPresses() || toggleClockKeyOnPress.Value || !PressedToggleKey())
                return;

            var show = showingClock.Value;
            showingClock.Value = !show;
            Config.Save();
        }

        private void LateUpdate()
        {
            if (CraftFromContainersInstalledAndActive)
                if (_cachedContainers != null)
                {
                    _cachedContainers.Clear();
                    _cachedContainers = null;
                }
        }

        private void OnDestroy()
        {
            CraftFromContainersInstalledAndActive = false;
        }

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug.Value)
                Debug.Log((pref ? typeof(VMP_Modplugin).Namespace + " " : "") + str);
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            var configEntry = Config.Bind(group, name, value, description);

            var syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
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
            if (player == null) return 0;

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

        public static float ApplyModifierValue(float targetValue, float value)
        {
            if (value <= -100)
                value = -100;

            float newValue;

            if (value >= 0)
                newValue = targetValue + targetValue / 100 * value;
            else
                newValue = targetValue - targetValue / 100 * (value * -1);

            return newValue;
        }

        private string GetCurrentTimeString()
        {
            if (!EnvMan.instance)
                return "";
            var fraction = (float) typeof(EnvMan)
                .GetField("m_smoothDayFraction", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(EnvMan.instance);

            var hour = (int) (fraction * 24);
            var minute = (int) ((fraction * 24 - hour) * 60);
            var second = (int) (((fraction * 24 - hour) * 60 - minute) * 60);

            var now = DateTime.Now;
            var theTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
            var days = Traverse.Create(EnvMan.instance).Method("GetCurrentDay").GetValue<int>();
            return GetCurrentTimeString(theTime, fraction, days);
        }


        [HarmonyPatch(typeof(Player), "PlacePiece")]
        private static class Player_PlacePiece_Patch
        {
            private static void Postfix(bool __result)
            {
                context.StartCoroutine(MapDetail.UpdateMap(true));
            }
        }

        [HarmonyPatch(typeof(Player), "RemovePiece")]
        private static class Player_RemovePiece_Patch
        {
            private static void Postfix(bool __result)
            {
                context.StartCoroutine(MapDetail.UpdateMap(true));
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        public static class Game_Start_Patch
        {
            private static void Prefix()
            {
                ZRoutedRpc.instance.Register("VMPMapSync",
                    new Action<long, ZPackage>(MapSync.RPC_VMPMapSync)); //Map Sync
                ZRoutedRpc.instance.Register("VMPMapPinSync",
                    new Action<long, ZPackage>(VmpMapPinSync.RPC_VMPMapPinSync)); //Map Pin Sync
                ZRoutedRpc.instance.Register("VMPAck", VMPAck.RPC_VMPAck); //Ack
            }
        }
    }
}