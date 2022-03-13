using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using CraftFromContainers;
using HarmonyLib;
using OdinQOL.MapSharing;
using OdinQOL.Patches;
using ServerSync;
using UnityEngine;
using static OdinQOL.Patches.SignPatches;

namespace OdinQOL
{
    public static class ImprovedBuildHudConfig
    {
        public static ConfigEntry<string> InventoryAmountFormat;
        public static ConfigEntry<string> InventoryAmountColor;
        public static ConfigEntry<string> CanBuildAmountFormat;
        public static ConfigEntry<string> CanBuildAmountColor;
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

    [BepInPlugin(GUID, ModName, Version)]
    public partial class OdinQOLplugin : BaseUnityPlugin
    {
        public const string Version = "0.4.0";
        public const string ModName = "OdinPlusQOL";
        public const string GUID = "com.odinplusqol.mod";
        private static readonly int windowId = 434343;
        internal static Assembly epicLootAssembly;
        public static OdinQOLplugin context;


        public static readonly string OdinQOL_DatadirectoryPath = Paths.BepInExRootPath + "/odinplus-data/";

        public static System.Timers.Timer mapSyncSaveTimer =
            new(TimeSpan.FromMinutes(5).TotalMilliseconds);

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

        public static ConfigEntry<int> DungeonMaxRoomCount;
        public static ConfigEntry<int> DungoneMinRoomCount;
        public static ConfigEntry<int> CampRadiusMin;
        public static ConfigEntry<int> CampRadiusMax;

        public static readonly IEnumerable<KeyCode> AllKeyCodes;

        public static ConfigSync configSync = new(GUID) { DisplayName = ModName, CurrentVersion = Version };
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
            shareablePins = config("Maps", "Share Pins", false, "Share pins with other players");
            shareAllPins = config("Maps", "Share ALL pins with other players", false,
                "Share ALL pins with other players");
            /*preventPlayerFromTurningOffPublicPosition =
                config("General", "IsDebug", true, "Show debug messages in log");*/
            displayCartsAndBoats = config("Maps", "Display Boats/Carts", true, "Show Boats and carts on the map");
            exploreRadius = config("Maps", "exploreRadius", 40, "Explore radius addition");


            DungeonMaxRoomCount = config("Dungeon", "Max Room Count", 20,
                "This is the max number of rooms placed by dungeon gen higher numbers will cause lag");

            DungoneMinRoomCount = config("Dungeon", "Min Room Count", 5,
                "This is the Min number of rooms placed by dungeon gen higher numbers will cause lag");

            CampRadiusMin = config("Dungeon", "Camp Radius Min", 5, "This is the minimum radius for goblin camps");

            CampRadiusMax = config("Dungeon", "Camp Radius Max", 15, "This is the maximum radius for goblin camps");

            modEnabled = config("General", "Enabled", true, "Enable the entire mod");
            isDebug = config("General", "IsDebug", false, "Show debug messages in log");

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
            Container_Configs.CartCol = config("Containers", "Cart Columns", 6,
                new ConfigDescription("Columns for Cart", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.PersonalRow = config("Containers", "Personal Chest Rows", 2,
                new ConfigDescription("Personal Chest Rows", new AcceptableValueRange<int>(2, 20)));
            Container_Configs.PersonalCol = config("Containers", "Personal Chest Columns", 3,
                new ConfigDescription("Personal Chest Columns", new AcceptableValueRange<int>(3, 8)));
            Container_Configs.WoodRow = config("Containers", "Wood Chest Rows", 2,
                new ConfigDescription("Wood Chest Rows", new AcceptableValueRange<int>(2, 10)));
            Container_Configs.WoodCol = config("Containers", "Wood Chest Columns", 5,
                new ConfigDescription("Wood Chest Columns", new AcceptableValueRange<int>(5, 8)));
            Container_Configs.IronRow = config("Containers", "Iron Chest Rows", 3,
                new ConfigDescription("Iron Chest Rows", new AcceptableValueRange<int>(3, 20)));
            Container_Configs.IronCol = config("Containers", "Iron Chest Columns", 6,
                new ConfigDescription("Iron Chest Columns", new AcceptableValueRange<int>(6, 8)));
            Container_Configs.BMRow = config("Containers", "Blackmetal Chest Rows", 4,
                new ConfigDescription("Blackmetal Chest Rows", new AcceptableValueRange<int>(3, 20)));
            Container_Configs.BMCol = config("Containers", "Blackmetal Chest Columns", 8,
                new ConfigDescription("Blackmetal Chest Columns", new AcceptableValueRange<int>(6, 8)));


            CraftingPatch.WorkbenchRange = config("WorkBench", "WorkBenchRange", 20,
                new ConfigDescription("Range you can build from workbench in meters",
                    new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.workbenchEnemySpawnRange = config("WorkBench", "WorkBenchRange (Playerbase size)", 20,
                new ConfigDescription("Workbench PlayerBase radius, this is how far away enemies spawn",
                    new AcceptableValueRange<int>(6, 650)));
            CraftingPatch.AlterWorkBench = config("WorkBench", "Change No Roof Behavior", true, "Show building pieces");
            workbenchAttachmentRange = config("WorkBench", "WorkBench Extension", 5,
                new ConfigDescription("Range for workbench extensions", new AcceptableValueRange<int>(5, 100)));


            WeightReduction = config("Items", "Item Weight Increase", 1f,
                new ConfigDescription("Multiplier for your item weight"));
            itemStackMultiplier = config("Items", "Item Stack Increase", 1f,
                new ConfigDescription("Multiplier for your item stacks"));
            NoTeleportPrevention = config("Items", "Disable Teleport check for items", false,
                new ConfigDescription("Disable Teleport check for items"));
            filltoptobottom = config("Items", "Fill your things top to bottom when moving from inv to chest", true,
                new ConfigDescription("Move your things top to bottom when changing from inv to chest"), false);

            /*Deconstruct = config("Items", "Allow deconstruction of items in crafting menu", true,
                new ConfigDescription("Deconstructing crafting items for return of mats"));*/
            AutoRepair = config("Items", "Auto repair your things when interacting with build station", true,
                new ConfigDescription("Auto repair your things when interacting with build station"), false);
            /*returnedpercent = config("Items", "Percent of item materials you would recieve back from deconstruction",
                100, new ConfigDescription("Perecent of item mats you get back from deconstructin tab"));*/

            MapDetail.MapDetailOn = config("Map Details", "MapDetail On", true,
                "Toggle this whole section off/on");
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
            MapDetail.customPlayerColors = config("Map Details", "CustomPlayerColors", "",
                "Custom color list, comma-separated. Use either <name>:<colorCode> pair entries or just <colorCode> entries. E.g. Erinthe:FF0000 or just FF0000. The latter will assign a color randomly to each connected peer.");

            toggleClockKeyMod = config("Clock", "ShowClockKeyMod", "",
                "Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
                false);
            toggleClockKey = config("Clock", "ShowClockKey", "home",
                "Key used to toggle the clock display. use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
                false);
            clockLocationString = config("Clock", "ClockLocationString", "50%,3%",
                "Location on the screen to show the clock (x,y) or (x%,y%)", false);

            LoadConfig();

            CraftingPatch.maxEntries =
                config("Show Chest Contents", "MaxEntries", -1,
                    "Max number of entries to show (-1 means show all)", false);
            CraftingPatch.sortType = config("Show Chest Contents", "SortType", CraftingPatch.SortType.Value,
                "Type by which to sort entries.", false);
            CraftingPatch.sortAsc = config("Show Chest Contents", "SortAsc", false, "Sort ascending?", false);
            CraftingPatch.entryString = config("Show Chest Contents", "EntryText",
                "<color=#FFFFAAFF>{0}</color> <color=#AAFFAAFF>{1}</color>",
                "Entry text. {0} is replaced by the total amount, {1} is replaced by the item name.", false);
            CraftingPatch.overFlowText = config("Show Chest Contents", "OverFlowText",
                "<color=#AAAAAAFF>...</color>", "Overflow text if more items than max entries.", false);
            CraftingPatch.capacityText = config("General", "CapacityText", "<color=#FFFFAAFF> {0}/{1}</color>",
                "Text to show capacity. {0} is replaced by number of full slots, {1} is replaced by total slots.",
                false);

            iHaveArrivedOnSpawn = config("Game", "I have arrived disable", true,
                new ConfigDescription("Disable the I have arrived message"));

            GamePatches.buildInsideProtectedLocations = config("Game", "BuildInProtectedLocations", false,
                new ConfigDescription("Allow Building Inside Protected Locations"));
            GamePatches.craftingDuration = config("Game", "Change Crafting Duration", .25f,
                new ConfigDescription("Change Crafting Duration time."), false);
            GamePatches.DisableGuardianAnimation = config("Game", "Disable Guardian Animation", true,
                new ConfigDescription("Disable Guardian Animation for the players"));
            GamePatches.SkipTuts = config("Game", "Skip Tuts", true,
                new ConfigDescription("Skip Tutorials"), false);
            GamePatches.reequipItemsAfterSwimming = config("Player", "Re Equip after Swimming", true,
                new ConfigDescription("Re-equip Items After Swimming"), false);
            GamePatches.enableAreaRepair = config("Player", "Area Repair", true,
                new ConfigDescription("Automatically repair build pieces within the repair radius"));
            GamePatches.areaRepairRadius =
                config("Player", "Area Repair Radius", 15, "Area Repair Radius for build pieces");
            GamePatches.baseMegingjordBuff =
                config("Player", "Base Meginjord Buff", 150, "Meginjord buff amount (Base)");
            GamePatches.honeyProductionSpeed = config("Game", "Honey Speed", 3600, "Honey Production Speed");
            GamePatches.maximumHoneyPerBeehive =
                config("Game", "Honey Count Per Hive", 4, "Honey Count Per Hive");
            GamePatches.maxPlayers =
                config("Server", "Max Player Count", 50, "Max number of Players to allow in a server");

            GamePatches.StaminaIsEnabled =
                config("Player", "Stamina alterations enabled", false,
                    "Stamina alterations enabled" + Environment.NewLine +
                    "Note: These are not percent drains. They are direct drain values.");
            GamePatches.dodgeStaminaUsage = config("Player", "Dodge Stamina Usage", 10f, "Dodge Stamina Usage");
            GamePatches.encumberedStaminaDrain =
                config("Player", "Encumbered Stamina drain", 10f, "Encumbered Stamina drain");
            GamePatches.sneakStaminaDrain = config("Player", "Sneak Stamina Drain", 5f, "Sneak stamina drain");
            GamePatches.runStaminaDrain = config("Player", "Run Stamina Drain", 10f, "Run Stamina Drain");
            GamePatches.staminaRegenDelay = config("Player", "Delay before stamina regeneration starts", 1f,
                "Delay before stamina regeneration starts");
            GamePatches.staminaRegen = config("Player", "Stamina regen factor", 5f, "Stamina regen factor");
            GamePatches.swimStaminaDrain = config("Player", "Stamina drain from swim", 5f, "Stamina drain from swim");
            GamePatches.jumpStaminaDrain =
                config("Player", "Jump stamina drain factor", 10f, "Stamina drain factor for jumping");
            GamePatches.baseAutoPickUpRange =
                config("Player", "Auto pickup range adjustment", 2f, "Auto pickup range adjustment");
            GamePatches.disableCameraShake = config<float>("Player", "Cam shake factor", 0, "Cam Shake factor", false);
            GamePatches.baseMaximumWeight = config("Player", "Base maximum weight addition for player", 350f,
                "Base max weight addition for player");
            GamePatches.maximumPlacementDistance = config<float>("WorkBench", "Build distance alteration", 15,
                "Build Distance  (Maximum Placement Distance)");

            ImprovedBuildHudConfig.InventoryAmountFormat = config("Building HUD", "Inventory Amount Format",
                "({0})",
                "Format for the amount of items in the player inventory to show after the required amount. Uses standard C# format rules. Leave empty to hide altogether.");
            ImprovedBuildHudConfig.InventoryAmountColor = config("Building HUD", "Inventory Amount Color",
                "lightblue",
                "Color to set the inventory amount after the requirement amount. Leave empty to set no color. You can use the #XXXXXX hex color format.",
                false);
            ImprovedBuildHudConfig.CanBuildAmountFormat = config("Building HUD", "Can Build Amount Color", "({0})",
                "Format for the amount of times you can build the currently selected item with your current inventory. Uses standard C# format rules. Leave empty to hide altogether.",
                false);
            ImprovedBuildHudConfig.CanBuildAmountColor = config("Building HUD", "Can Build Amount Color", "white",
                "Color to set the can-build amount. Leave empty to set no color. You can use the #XXXXXX hex color format.",
                false);

            signScale = config("Signs", "SignScale", new Vector3(1, 1, 1), "Sign scale (w,h,d)");
            textPositionOffset =
                config("Signs", "TextPositionOffset", new Vector2(0, 0), "Default font size");
            useRichText = config("Signs", "UseRichText", true, "Enable rich text");
            fontName = config("Signs", "FontName", "AveriaSerifLibre-Bold", "Font name", false);


            AutoStorePatch.dropRangeChests = config("Auto Storage", "DropRangeChests", 5f,
                "The maximum range to pull dropped items for Chests (Default chest)");
            AutoStorePatch.dropRangePersonalChests = config("Auto Storage", "DropRangePersonalChests", 5f,
                "The maximum range to pull dropped items for Personal chests");
            AutoStorePatch.dropRangeReinforcedChests = config("Auto Storage", "DropRangeReinforcedChests", 5f,
                "The maximum range to pull dropped items for Re-inforced Chests");
            AutoStorePatch.dropRangeCarts = config("Auto Storage", "DropRangeCarts", 5f,
                "The maximum range to pull dropped items for Carts");
            AutoStorePatch.dropRangeShips = config("Auto Storage", "DropRangeShips", 5f,
                "The maximum range to pull dropped items for Ships");
            AutoStorePatch.itemDisallowTypes = config("Auto Storage", "ItemDisallowTypes", "",
                "Types of item to disallow pulling for, comma-separated.");
            AutoStorePatch.itemAllowTypes = config("Auto Storage", "ItemAllowTypes", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypes");
            AutoStorePatch.itemDisallowTypesChests = config("Auto Storage", "ItemDisallowTypesChests", "",
                "Types of item to disallow pulling for, comma-separated. (For Chests)");
            AutoStorePatch.itemAllowTypesChests = config("Auto Storage", "ItemAllowTypesChests", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesChests");
            AutoStorePatch.itemDisallowTypesPersonalChests = config("Auto Storage",
                "ItemDisallowTypesPersonalChests", "",
                "Types of item to disallow pulling for, comma-separated. (For Personal Chests)");
            AutoStorePatch.itemAllowTypesPersonalChests = config("Auto Storage", "ItemAllowTypesPersonalChests",
                "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesPersonalChests");
            AutoStorePatch.itemDisallowTypesReinforcedChests = config("Auto Storage",
                "ItemDisallowTypesReinforcedChests", "",
                "Types of item to disallow pulling for, comma-separated. (For ReinforcedChests)");
            AutoStorePatch.itemAllowTypesReinforcedChests = config("Auto Storage",
                "ItemAllowTypesReinforcedChests", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesReinforcedChests");
            AutoStorePatch.itemDisallowTypesCarts = config("Auto Storage", "ItemDisallowTypesCarts", "",
                "Types of item to disallow pulling for, comma-separated. (For Carts)");
            AutoStorePatch.itemAllowTypesCarts = config("Auto Storage", "ItemAllowTypesCarts", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesCarts");
            AutoStorePatch.itemDisallowTypesShips = config("Auto Storage", "ItemDisallowTypesShips", "",
                "Types of item to disallow pulling for, comma-separated. (For Ships)");
            AutoStorePatch.itemAllowTypesShips = config("Auto Storage", "ItemAllowTypesShips", "",
                "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesShips");
            AutoStorePatch.toggleString = config("Auto Storage", "ToggleString", "Auto Pull: {0}",
                "Text to show on toggle. {0} is replaced with true/false");
            AutoStorePatch.toggleKey = config("Auto Storage", "ToggleKey", "",
                "Key to toggle behaviour. Leave blank to disable the toggle key.", false);
            AutoStorePatch.mustHaveItemToPull = config("Auto Storage", "MustHaveItemToPull", false,
                "If true, a container must already have at least one of the item to pull.");
            AutoStorePatch.isOn =
                config("Auto Storage", "AutoStorageIsOn", false, "Behaviour is currently on or not");


            ClientPatches._chatPlayerName =
                config(
                    "Names", "chatPlayerName", string.Empty,
                    "Override your player name shown in-game and in the chat box.", false);

            PlantGrowth.displayGrowth =
                config("PlantGrowth", "DisplayGrowth", true, "Display growth progress in hover text", false);
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

            WearNTear_Patches.NoWeatherDam = config("WearNTear_Patches", "No Weather Damage to buildings", false,
                "No Weather Damage to buildings");
            WearNTear_Patches.DisableStructintegrity = config("WearNTear_Patches",
                "Disable Structural Integrity system", false, "Disable Structural Integrity system");
            WearNTear_Patches.DisableBoatDamage =
                config("WearNTear_Patches", "Disable Boat Damage", false, "Disable Boat Damage");
            WearNTear_Patches.NoPlayerStructDam = config("WearNTear_Patches", "No Damage to player buildings", false,
                "No Damage to player buildings");

            WearNTear_Patches.StructuralIntegritywood = config("WearNTear_Patches", "Wood Structural Integrity",
                1f, "Wood Structural Integrity");
            WearNTear_Patches.StructuralIntegritystone = config("WearNTear_Patches",
                "Stone Structural Integrity", 1f, "Stone Structural Integrity");
            WearNTear_Patches.StructuralIntegrityiron = config("WearNTear_Patches", "Iron Structural Integrity",
                1f, "Iron Structural Integrity");
            WearNTear_Patches.StructuralIntegrityhardWood = config("WearNTear_Patches",
                "Hardwood Structural Integrity", 1f, "Hardwood Structural Integrity");

            SkillPatches.ChangeSkills =
                config("Skills", "Change the skill gain factor", false, "Change skill gain factor");
            SkillPatches.experienceGainedNotifications = config("Skills",
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
            SkillPatches.unarmed = config("Skills", "Unarmed Skill gain factor", 0f, "Unarmed skill gain factor");
            SkillPatches.pickaxe = config("Skills", "Pickaxe Skill gain factor", 0f, "Pickaxe skill gain factor");
            SkillPatches.woodcutting =
                config("Skills", "WoodCutting Skill gain factor", 0f, "WoodCutting skill gain factor");
            SkillPatches.jump = config("Skills", "Jump Skill gain factor", 0f, "Jump skill gain factor");
            SkillPatches.run = config("Skills", "Run Skill gain factor", 0f, "Run skill gain factor");
            SkillPatches.sneak = config("Skills", "Sneak Skill gain factor", 0f, "Sneak skill gain factor");
            SkillPatches.swim = config("Skills", "Swim Skill gain factor", 0f, "Swim skill gain factor");
            SkillPatches.deathPenaltyMultiplier = config("Skills", "Death Penalty Factor Multiplier", 0f,
                "Death Penalty Factor Multiplier");
            /* Extended Player Inventory Config options */
            QuickAccessBar.extraRows = config("Extended Inventory", "ExtraRows", 0,
                "Number of extra ordinary rows. (This can cause overlap with chest GUI, make sure you hold CTRL (the default key) and drag to desired position)");
            QuickAccessBar.addEquipmentRow = config("Extended Inventory", "AddEquipmentRow", false,
                "Add special row for equipped items and quick slots. (IF YOU ARE USING RANDY KNAPPS EAQs KEEP THIS VALUE OFF)");
            QuickAccessBar.displayEquipmentRowSeparate = config("Extended Inventory", "DisplayEquipmentRowSeparate",
                false,
                "Display equipment and quickslots in their own area. (IF YOU ARE USING RANDY KNAPPS EAQs KEEP THIS VALUE OFF)");

            QuickAccessBar.helmetText = config("Extended Inventory", "HelmetText", "Head",
                "Text to show for helmet slot.", false);
            QuickAccessBar.chestText = config("Extended Inventory", "ChestText", "Chest",
                "Text to show for chest slot.", false);
            QuickAccessBar.legsText = config("Extended Inventory", "LegsText", "Legs",
                "Text to show for legs slot.", false);
            QuickAccessBar.backText = config("Extended Inventory", "BackText", "Back",
                "Text to show for back slot.", false);
            QuickAccessBar.utilityText = config("Extended Inventory", "UtilityText", "Utility",
                "Text to show for utility slot.", false);

            QuickAccessBar.quickAccessScale = config("Extended Inventory", "QuickAccessScale", 1f,
                "Scale of quick access bar. ", false);

            QuickAccessBar.hotKey1 = config("Extended Inventory", "HotKey1", KeyCode.Z,
                "Hotkey 1 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", false);
            QuickAccessBar.hotKey2 = config("Extended Inventory", "HotKey2", KeyCode.X,
                "Hotkey 2 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", false);
            QuickAccessBar.hotKey3 = config("Extended Inventory", "HotKey3", KeyCode.C,
                "Hotkey 3 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", false);

            QuickAccessBar.modKeyOne = config("Extended Inventory", "ModKey1", KeyCode.Mouse0,
                "First modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.",
                false);
            QuickAccessBar.modKeyTwo = config("Extended Inventory", "ModKey2", KeyCode.LeftControl,
                "Second modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.",
                false);

            QuickAccessBar.quickAccessX = config("Extended Inventory", "quickAccessX", 9999f,
                "Current X of Quick Slots (Not Synced with server)", false);
            QuickAccessBar.quickAccessY = config("Extended Inventory", "quickAccessY", 9999f,
                "Current Y of Quick Slots (Not Synced with server)", false);

            /* Moveable Chest Inventory */
            MoveableChestInventory.chestInventoryX = config("General", "ChestInventoryX", -1f,
                "Current X of chest (Not Synced with server)", false);
            MoveableChestInventory.chestInventoryY = config("General", "ChestInventoryY", -1f,
                "Current Y of chest (Not Synced with server)", false);
            MoveableChestInventory.modKeyOneChestMove = config("General", "ModKeyOne", KeyCode.Mouse0,
                "First modifier key (to move the container). Use https://docs.unity3d.com/Manual/class-InputManager.html format.",
                false);
            MoveableChestInventory.modKeyTwoChestMove = config("General", "ModKeyTwo", KeyCode.LeftControl,
                "Second modifier key (to move the container). Use https://docs.unity3d.com/Manual/class-InputManager.html format.",
                false);

            /* Connect Panel */
            ConnectionPanel.ServerAdditionToggle = config("Connection Panel", "Enable Connection Panel", false,
                "This option, if enabled, will add the servers listed below to the Join Game panel on the main menu.",
                false);
            ConnectionPanel.ServerIPs = config("Connection Panel", "This is the IP for your server",
                "111.111.111.11,222.222.222.22", "This is the IP for your server. Separate each option by a comma.",
                false);
            ConnectionPanel.ServerNames = config("Connection Panel", "Name of the server",
                "<color=#6600cc>TEST EXAMPLE</color>, Test Example 2",
                "This is how your server shows in the list, can use colors. Separate each option by a comma.", false);
            ConnectionPanel.ServerPorts = config("Connection Panel",
                "The Port For your Server. Separate each option by a comma.", "28200,28300", "Port For server", false);


            /* Discard Items in Inventory */
            InventoryDiscard.discardInvEnabled =
                config("Inventory Discard", "Enabled", false, "Enable Inventory Discard Section");
            InventoryDiscard.hotKey = config("Inventory Discard", "DiscardHotkey", new KeyboardShortcut(KeyCode.Delete),
                new ConfigDescription("The hotkey to discard an item", new AcceptableShortcuts()), false);
            /*InventoryDiscard.hotKey = config("Inventory Discard", "DiscardHotkey", KeyCode.Delete,
                "The hotkey to discard an item", false);*/
            InventoryDiscard.returnUnknownResources = config("Inventory Discard", "ReturnUnknownResources", false,
                "Return resources if recipe is unknown");
            InventoryDiscard.returnEnchantedResources = config("Inventory Discard", "ReturnEnchantedResources", false,
                "Return resources for Epic Loot enchantments");
            InventoryDiscard.returnResources = config("Inventory Discard", "ReturnResources", 1f,
                "Fraction of resources to return (0.0 - 1.0)");


            if (!modEnabled.Value)
                return;

            if (!Directory.Exists(OdinQOL_DatadirectoryPath)) Directory.CreateDirectory(OdinQOL_DatadirectoryPath);
            currentFont = GetFont(fontName.Value, 20);
            lastFontName = currentFont?.name;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            if (ZNet.m_isServer && shareMapProgression.Value)
            {
                mapSyncSaveTimer.AutoReset = true;
                mapSyncSaveTimer.Elapsed += (sender, args) => MapSync.SaveMapDataToDisk();
            }
            
            // On.Humanoid.GetInventory += GamePatches.Humanoid_GetInventory;
            CraftFromContainersInstalledAndActive = false;
            var bepInExManager = GameObject.Find("BepInEx_Manager");
            var plugins = bepInExManager.GetComponentsInChildren<BaseUnityPlugin>();
            foreach (var plugin in plugins)
                if (plugin.Info.Metadata.GUID == "aedenthorn.CraftFromContainers")
                {
                    CraftFromContainersInstalledAndActive = BepInExPlugin.modEnabled.Value;
                    Debug.Log("Found CraftFromContainers");
                }

            QuickAccessBar.hotkeys = new[]
            {
                QuickAccessBar.hotKey1,
                QuickAccessBar.hotKey2,
                QuickAccessBar.hotKey3
            };
        }

        private void Start()
        {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot"))
            {
                epicLootAssembly = Chainloader.PluginInfos["randyknapp.mods.epicloot"].Instance.GetType().Assembly;
                OdinQOLplugin.Dbgl("Epic Loot found, providing compatibility");
            }
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
                Debug.Log((pref ? typeof(OdinQOLplugin).Namespace + " " : "") + str);
        }

        public static void DbglError(string str = "", bool pref = true)
        {
            Debug.LogError((pref ? typeof(OdinQOLplugin).Namespace + " " : "") + str);
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
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

            if (CraftFromContainersInstalledAndActive)
            {
                if (_cachedContainers == null)
                    _cachedContainers = BepInExPlugin.GetNearbyContainers(player.transform.position);
                foreach (var container in _cachedContainers)
                    containerCount += container.GetInventory().CountItems(itemName);
            }

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
            var fraction = (float)typeof(EnvMan)
                .GetField("m_smoothDayFraction", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(EnvMan.instance);

            var hour = (int)(fraction * 24);
            var minute = (int)((fraction * 24 - hour) * 60);
            var second = (int)(((fraction * 24 - hour) * 60 - minute) * 60);

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
                if (!modEnabled.Value || !__result)
                    return;
                if (MapDetail.MapDetailOn.Value)
                    context.StartCoroutine(MapDetail.UpdateMap(true));
            }
        }

        [HarmonyPatch(typeof(Player), "RemovePiece")]
        private static class Player_RemovePiece_Patch
        {
            private static void Postfix(bool __result)
            {
                if (!modEnabled.Value || !__result)
                    return;
                if (MapDetail.MapDetailOn.Value)
                    context.StartCoroutine(MapDetail.UpdateMap(true));
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        public static class Game_Start_Patch
        {
            private static void Prefix()
            {
                ZRoutedRpc.instance.Register("OdinQOLMapSync",
                    new Action<long, ZPackage>(MapSync.RPC_OdinQOLMapSync)); //Map Sync
                ZRoutedRpc.instance.Register("OdinQOLMapPinSync",
                    new Action<long, ZPackage>(VmpMapPinSync.RPC_OdinQOLMapPinSync)); //Map Pin Sync
                ZRoutedRpc.instance.Register("OdinQOLAck", OdinQOLAck.RPC_OdinQOLAck); //Ack
            }
        }
    }
}