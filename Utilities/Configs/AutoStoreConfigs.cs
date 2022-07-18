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
        AutoStorePatch.dropRangeCarts = OdinQOLplugin.context.config("Auto Storage", "DropRangeCarts", 5f,
            "The maximum range to pull dropped items for Carts");
        AutoStorePatch.dropRangeShips = OdinQOLplugin.context.config("Auto Storage", "DropRangeShips", 5f,
            "The maximum range to pull dropped items for Ships");
        AutoStorePatch.itemDisallowTypes = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypes", "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names.");

        AutoStorePatch.itemDisallowCategories = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowCategories",
            "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategories = OdinQOLplugin.context.config("Auto Storage", "ItemAllowCategories", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategories.  \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowCategoriesChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowCategoriesChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesPersonalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowCategoriesPersonalChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesPersonalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowCategoriesPersonalChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesPersonalChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesBlackMetalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowCategoriesBlackMetalChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesBlackMetalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowCategoriesBlackMetalChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesBlackMetalChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowCategoriesReinforcedChests", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowCategoriesReinforcedChests", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesReinforcedChests. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesCarts = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowCategoriesCarts", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesCarts = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowCategoriesCarts", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesCarts. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");

        AutoStorePatch.itemDisallowCategoriesShips = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowCategoriesShips", "",
            $"Categories of item to disallow pulling for, comma-separated. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");
        AutoStorePatch.itemAllowCategoriesShips = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowCategoriesShips", "",
            $"Categories of item to only allow pulling for, comma-separated. Overrides ItemDisallowCategoriesShips. \nAccepted Values are {string.Join(",", OdinQOLplugin.Categories)}");


        AutoStorePatch.itemAllowTypes = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypes", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypes");
        AutoStorePatch.itemDisallowTypesChests = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypesChests",
            "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Chests)");
        AutoStorePatch.itemAllowTypesChests = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypesChests", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesChests");
        AutoStorePatch.itemDisallowTypesPersonalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowTypesPersonalChests", "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Personal Chests)");
        AutoStorePatch.itemAllowTypesPersonalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowTypesPersonalChests",
            "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesPersonalChests");
        AutoStorePatch.itemDisallowTypesReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowTypesReinforcedChests", "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For ReinforcedChests)");
        AutoStorePatch.itemAllowTypesReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowTypesReinforcedChests", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesReinforcedChests");
        AutoStorePatch.itemDisallowTypesBlackMetalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowTypesBlackMetalChests",
            "", "Types of item to disallow pulling for, comma-separated. Uses Prefab names.");
        AutoStorePatch.itemAllowTypesBlackMetalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowTypesBlackMetalChests", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesBlackMetalChests");
        AutoStorePatch.itemDisallowTypesCarts = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypesCarts",
            "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Carts)");
        AutoStorePatch.itemAllowTypesCarts = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypesCarts", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesCarts");
        AutoStorePatch.itemDisallowTypesShips = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypesShips",
            "",
            "Types of item to disallow pulling for, comma-separated. Uses Prefab names. (For Ships)");
        AutoStorePatch.itemAllowTypesShips = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypesShips", "",
            "Types of item to only allow pulling for, comma-separated. Uses Prefab names. Overrides ItemDisallowTypesShips");
        AutoStorePatch.toggleString = OdinQOLplugin.context.config("Auto Storage", "ToggleString", "Auto Pull: {0}",
            "Text to show on toggle. {0} is replaced with true/false");
        AutoStorePatch.toggleKey = OdinQOLplugin.context.config("Auto Storage", "ToggleKey", "",
            "Key to toggle behaviour. Leave blank to disable the toggle key.", false);
        AutoStorePatch.mustHaveItemToPull = OdinQOLplugin.context.config("Auto Storage", "MustHaveItemToPull", false,
            "If true, a container must already have at least one of the item to pull.");
    }
}