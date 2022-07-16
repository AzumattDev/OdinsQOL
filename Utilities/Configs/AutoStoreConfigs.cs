using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class AutoStoreConfigs
{
    internal static void Generate()
    {
        AutoStorePatch.dropRangeChests = OdinQOLplugin.context.config("Auto Storage", "DropRangeChests", 5f,
            "The maximum range to pull dropped items for Chests (Default chest)");
        AutoStorePatch.dropRangePersonalChests = OdinQOLplugin.context.config("Auto Storage", "DropRangePersonalChests",
            5f,
            "The maximum range to pull dropped items for Personal chests");
        AutoStorePatch.dropRangeReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "DropRangeReinforcedChests", 5f,
            "The maximum range to pull dropped items for Re-inforced Chests");
        AutoStorePatch.dropRangeCarts = OdinQOLplugin.context.config("Auto Storage", "DropRangeCarts", 5f,
            "The maximum range to pull dropped items for Carts");
        AutoStorePatch.dropRangeShips = OdinQOLplugin.context.config("Auto Storage", "DropRangeShips", 5f,
            "The maximum range to pull dropped items for Ships");
        AutoStorePatch.itemDisallowTypes = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypes", "",
            "Types of item to disallow pulling for, comma-separated.");
        AutoStorePatch.itemAllowTypes = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypes", "",
            "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypes");
        AutoStorePatch.itemDisallowTypesChests = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypesChests",
            "",
            "Types of item to disallow pulling for, comma-separated. (For Chests)");
        AutoStorePatch.itemAllowTypesChests = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypesChests", "",
            "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesChests");
        AutoStorePatch.itemDisallowTypesPersonalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowTypesPersonalChests", "",
            "Types of item to disallow pulling for, comma-separated. (For Personal Chests)");
        AutoStorePatch.itemAllowTypesPersonalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowTypesPersonalChests",
            "",
            "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesPersonalChests");
        AutoStorePatch.itemDisallowTypesReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowTypesReinforcedChests", "",
            "Types of item to disallow pulling for, comma-separated. (For ReinforcedChests)");
        AutoStorePatch.itemAllowTypesReinforcedChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowTypesReinforcedChests", "",
            "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesReinforcedChests");
        AutoStorePatch.itemDisallowTypesBlackMetalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemDisallowTypesBlackMetalChests",
            "", "Types of item to disallow pulling for, comma-separated.");
        AutoStorePatch.itemAllowTypesBlackMetalChests = OdinQOLplugin.context.config("Auto Storage",
            "ItemAllowTypesBlackMetalChests", "",
            "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesBlackMetalChests");
        AutoStorePatch.itemDisallowTypesCarts = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypesCarts",
            "",
            "Types of item to disallow pulling for, comma-separated. (For Carts)");
        AutoStorePatch.itemAllowTypesCarts = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypesCarts", "",
            "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesCarts");
        AutoStorePatch.itemDisallowTypesShips = OdinQOLplugin.context.config("Auto Storage", "ItemDisallowTypesShips",
            "",
            "Types of item to disallow pulling for, comma-separated. (For Ships)");
        AutoStorePatch.itemAllowTypesShips = OdinQOLplugin.context.config("Auto Storage", "ItemAllowTypesShips", "",
            "Types of item to only allow pulling for, comma-separated. Overrides ItemDisallowTypesShips");
        AutoStorePatch.toggleString = OdinQOLplugin.context.config("Auto Storage", "ToggleString", "Auto Pull: {0}",
            "Text to show on toggle. {0} is replaced with true/false");
        AutoStorePatch.toggleKey = OdinQOLplugin.context.config("Auto Storage", "ToggleKey", "",
            "Key to toggle behaviour. Leave blank to disable the toggle key.", false);
        AutoStorePatch.mustHaveItemToPull = OdinQOLplugin.context.config("Auto Storage", "MustHaveItemToPull", false,
            "If true, a container must already have at least one of the item to pull.");
        AutoStorePatch.isOn =
            OdinQOLplugin.context.config("Auto Storage", "AutoStorageIsOn", false, "Behaviour is currently on or not");
    }
}