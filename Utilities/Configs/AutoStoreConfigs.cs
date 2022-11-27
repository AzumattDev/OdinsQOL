using System;
using BepInEx.Configuration;
using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class AutoStoreConfigs
{
    internal static void Generate()
    {
        AutoStorePatch.isOn =
            OdinQOLplugin.context.config("Auto Storage", "AutoStorageIsOn", false, "Behaviour is currently on or not");
        AutoStorePatch.StoreShortcut =
            OdinQOLplugin.context.config("Auto Storage", "Store Shortcut", new KeyboardShortcut(KeyCode.Period),
                "Keyboard shortcut/Hotkey to store your inventory into nearby containers.", false);
        AutoStorePatch.playerRange = OdinQOLplugin.context.config("Auto Storage", "Player Range", 5f,
            "The maximum distance from the player to store items when the Store Shortcut is pressed. Follows storage rules for allowed items.");
        AutoStorePatch.dropRangeChests = OdinQOLplugin.context.config("Auto Storage", "DropRangeChests", 5f,
            "The maximum range to pull dropped items for Chests (Default chest)");
        AutoStorePatch.dropRangePersonalChests = OdinQOLplugin.context.config("Auto Storage", "DropRangePersonalChests",
            5f,
            "The maximum range to pull dropped items for Personal chests");
        AutoStorePatch.dropRangeReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "DropRangeReinforcedChests", 5f,
            "The maximum range to pull dropped items for Re-inforced Chests");
        AutoStorePatch.dropRangeBlackmetalChests = OdinQOLplugin.context.config("Auto Storage",
            "DropRangeBlackmetalChests", 5f,
            "The maximum range to pull dropped items for Blackmetal Chests");
        AutoStorePatch.dropRangeCustomChests = OdinQOLplugin.context.config("Auto Storage",
            "DropRangeCustomChests", 5f,
            "The maximum range to pull dropped items for Custom Chests");
        AutoStorePatch.dropRangeCarts = OdinQOLplugin.context.config("Auto Storage", "DropRangeCarts", 5f,
            "The maximum range to pull dropped items for Carts");
        AutoStorePatch.dropRangeShips = OdinQOLplugin.context.config("Auto Storage", "DropRangeShips", 5f,
            "The maximum range to pull dropped items for Ships");
        AutoStorePatch.itemDisallowTypes = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemDisallowTypes", "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names.");

        AutoStorePatch.itemDisallowCategories = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemDisallowCategories",
            "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategories = OdinQOLplugin.context.config("Auto Storage", "ItemAllowCategories", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategories.  \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowCategoriesChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowCategoriesChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesPersonalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowCategoriesPersonalChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesPersonalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowCategoriesPersonalChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesPersonalChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesBlackMetalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowCategoriesBlackMetalChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesBlackMetalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowCategoriesBlackMetalChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesBlackMetalChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesReinforcedChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowCategoriesReinforcedChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesReinforcedChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowCategoriesReinforcedChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesReinforcedChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesCarts = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowCategoriesCarts", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesCarts = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowCategoriesCarts", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesCarts. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesShips = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowCategoriesShips", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesShips = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowCategoriesShips", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesShips. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");


        AutoStorePatch.itemAllowTypes = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemAllowTypes", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypes");
        AutoStorePatch.itemDisallowTypesChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemDisallowTypesChests",
            "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Chests)");
        AutoStorePatch.itemAllowTypesChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemAllowTypesChests", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesChests");
        AutoStorePatch.itemDisallowTypesPersonalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowTypesPersonalChests", "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Personal Chests)");
        AutoStorePatch.itemAllowTypesPersonalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowTypesPersonalChests",
            "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesPersonalChests");
        AutoStorePatch.itemDisallowTypesReinforcedChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowTypesReinforcedChests", "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For ReinforcedChests)");
        AutoStorePatch.itemAllowTypesReinforcedChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowTypesReinforcedChests", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesReinforcedChests");
        AutoStorePatch.itemDisallowTypesBlackMetalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowTypesBlackMetalChests",
            "", "Types of item to disallow pulling for, comma-separated. Uses Prefab names.");
        AutoStorePatch.itemAllowTypesBlackMetalChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowTypesBlackMetalChests", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesBlackMetalChests");
        AutoStorePatch.customChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "CustomChests", "",
            "Custom Chests to use, comma-separated. Uses Prefab names.");
        AutoStorePatch.itemDisallowTypesCustomChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemDisallowTypesCustomChests",
            "", "Types of item to disallow pulling for, comma-separated. Uses Prefab names.");
        AutoStorePatch.itemAllowTypesCustomChests = OdinQOLplugin.context.TextEntryConfig("Auto Storage",
            "ItemAllowTypesCustomChests", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesCustomChests");
        
        AutoStorePatch.itemDisallowTypesCarts = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemDisallowTypesCarts",
            "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Carts)");
        AutoStorePatch.itemAllowTypesCarts = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemAllowTypesCarts", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesCarts");
        AutoStorePatch.itemDisallowTypesShips = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemDisallowTypesShips",
            "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Ships)");
        AutoStorePatch.itemAllowTypesShips = OdinQOLplugin.context.TextEntryConfig("Auto Storage", "ItemAllowTypesShips", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesShips");
        AutoStorePatch.toggleString = OdinQOLplugin.context.config("Auto Storage", "ToggleString", "Auto Pull: {0}",
            "Text to show on toggle. {0} is replaced with true/false");
        AutoStorePatch.toggleKey = OdinQOLplugin.context.config("Auto Storage", "ToggleKey", "",
            "Key to toggle behaviour. Leave blank to disable the toggle key.", false);
        AutoStorePatch.mustHaveItemToPull = OdinQOLplugin.context.config("Auto Storage", "MustHaveItemToPull", false,
            "If true, a container must already have at least one of the item to pull.");
    }
}