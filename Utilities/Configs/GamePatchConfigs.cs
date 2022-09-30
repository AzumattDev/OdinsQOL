using System;
using BepInEx.Configuration;
using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class GamePatchConfigs
{
    internal static void Generate()
    {
        GamePatches.HaveArrivedOnSpawn = OdinQOLplugin.context.config("Game", "I have arrived disable", true,
            new ConfigDescription("Disable the I have arrived message"));
        GamePatches.BuildInsideProtectedLocations = OdinQOLplugin.context.config("Game",
            "BuildInProtectedLocations", false,
            new ConfigDescription("Allow Building Inside Protected Locations"));
        GamePatches.CraftingDuration = OdinQOLplugin.context.config("Game",
            "Change Crafting Duration", .25f,
            new ConfigDescription("Change Crafting Duration time."), false);
        GamePatches.DisableGuardianAnimation = OdinQOLplugin.context.config("Game",
            "Disable Guardian Animation", true,
            new ConfigDescription("Disable Guardian Animation for the players"));
        GamePatches.SkipTuts = OdinQOLplugin.context.config("Game", "Skip Tuts", true,
            new ConfigDescription("Skip Tutorials"), false);
        GamePatches.ReequipItemsAfterSwimming = OdinQOLplugin.context.config("Player", "Re Equip after Swimming", true,
            new ConfigDescription("Re-equip Items After Swimming"), false);
        GamePatches.EnableAreaRepair = OdinQOLplugin.context.config("Player", "Area Repair", true,
            new ConfigDescription("Automatically repair build pieces within the repair radius"));
        GamePatches.AreaRepairRadius =
            OdinQOLplugin.context.config("Player", "Area Repair Radius", 15, "Area Repair Radius for build pieces");
        GamePatches.BaseMegingjordBuff =
            OdinQOLplugin.context.config("Player", "Base Meginjord Buff", 150, "Meginjord buff amount (Base)");
        GamePatches.HoneyProductionSpeed =
            OdinQOLplugin.context.config("Game", "Honey Speed", 1200, "Honey Production Speed");
        GamePatches.MaximumHoneyPerBeehive =
            OdinQOLplugin.context.config("Game", "Honey Count Per Hive", 4, "Honey Count Per Hive");
        GamePatches.MaxPlayers =
            OdinQOLplugin.context.config("Server", "Max Player Count", 50,
                "Max number of Players to allow in a server");

        GamePatches.StaminaIsEnabled =
            OdinQOLplugin.context.config("Player", "Stamina alterations enabled", false,
                "Stamina alterations enabled" + Environment.NewLine +
                "Note: These are not percent drains. They are direct drain values.");
        GamePatches.DodgeStaminaUsage =
            OdinQOLplugin.context.config("Player", "Dodge Stamina Usage", 10f, "Dodge Stamina Usage");
        GamePatches.EncumberedStaminaDrain =
            OdinQOLplugin.context.config("Player", "Encumbered Stamina drain", 10f, "Encumbered Stamina drain");
        GamePatches.SneakStaminaDrain =
            OdinQOLplugin.context.config("Player", "Sneak Stamina Drain", 5f, "Sneak stamina drain");
        GamePatches.RunStaminaDrain =
            OdinQOLplugin.context.config("Player", "Run Stamina Drain", 10f, "Run Stamina Drain");
        GamePatches.StaminaRegenDelay = OdinQOLplugin.context.config("Player",
            "Delay before stamina regeneration starts", 1f,
            "Delay before stamina regeneration starts");
        GamePatches.StaminaRegen =
            OdinQOLplugin.context.config("Player", "Stamina regen factor", 5f, "Stamina regen factor");
        GamePatches.SwimStaminaDrain =
            OdinQOLplugin.context.config("Player", "Stamina drain from swim", 5f, "Stamina drain from swim");
        GamePatches.JumpStaminaDrain =
            OdinQOLplugin.context.config("Player", "Jump stamina drain factor", 10f,
                "Stamina drain factor for jumping");
        GamePatches.BaseAutoPickUpRange =
            OdinQOLplugin.context.config("Player", "Auto pickup range adjustment", 2f, "Auto pickup range adjustment");
        GamePatches.DisableCameraShake =
            OdinQOLplugin.context.config<float>("Player", "Cam shake factor", 0, "Cam Shake factor", false);
        GamePatches.BaseMaximumWeight = OdinQOLplugin.context.config("Player",
            "Base maximum weight addition for player", 350f,
            "Base max weight addition for player");
        GamePatches.MaximumPlacementDistance = OdinQOLplugin.context.config<float>("WorkBench",
            "Build distance alteration", 15,
            "Build Distance  (Maximum Placement Distance)");
        GamePatches.HoverPortalTag = OdinQOLplugin.context.config("Game",
            "Portal Tag on Hover", true,
            "Enabled Portal Tag message while hovering over Portal");
        GamePatches.ShowDamageFlash = OdinQOLplugin.context.config("Player",
            "ShowDamageFlash", true,
            "Show the flashing red screen when taking damage");
        GamePatches.NoFoodDeg = OdinQOLplugin.context.config("Player",
            "No Food Degrade", false,
            "Disables food degrading");
        GamePatches.FoodModifications = OdinQOLplugin.context.config("Player",
            "Modify food", false,
            "Ensuring the food lasts longer while maintaining the same rate of regeneration. Needed to be on for No Food Degrade to work");

        ImprovedBuildHudConfig.InventoryAmountFormat = OdinQOLplugin.context.config("Building HUD",
            "Inventory Amount Format",
            "({0})",
            "Format for the amount of items in the player inventory to show after the required amount. Uses standard C# format rules. Leave empty to hide altogether.");
        ImprovedBuildHudConfig.InventoryAmountColor = OdinQOLplugin.context.config("Building HUD",
            "Inventory Amount Color",
            "lightblue",
            "Color to set the inventory amount after the requirement amount. Leave empty to set no color. You can use the #XXXXXX hex color format.",
            false);
        ImprovedBuildHudConfig.CanBuildAmountFormat = OdinQOLplugin.context.config("Building HUD",
            "Can Build Amount Format", "({0})",
            "Format for the amount of times you can build the currently selected item with your current inventory. Uses standard C# format rules. Leave empty to hide altogether.",
            false);
        ImprovedBuildHudConfig.CanBuildAmountColor = OdinQOLplugin.context.config("Building HUD",
            "Can Build Amount Color", "white",
            "Color to set the can-build amount. Leave empty to set no color. You can use the #XXXXXX hex color format.",
            false);
    }
}