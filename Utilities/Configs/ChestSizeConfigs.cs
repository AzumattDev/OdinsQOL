using BepInEx.Configuration;
using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class ChestSizeConfigs
{
    internal static void Generate()
    {
        Container_Configs.ContainerSectionOn = OdinQOLplugin.context.config("Containers", "Container Section On", true,
            "Toggle this value to turn the entire Containers section off/on");
        Container_Configs.ChestContainerControl = OdinQOLplugin.context.config("Containers", "Chest Container Control",
            true,
            "Toggle this value to turn off this mod's control over chest container size");
        Container_Configs.ShipContainerControl = OdinQOLplugin.context.config("Containers", "Ship Container Control",
            true,
            "Toggle this value to turn off this mod's control over ship chest container size");
        Container_Configs.KarveRow = OdinQOLplugin.context.config("Containers", "Karve Rows", 2,
            new ConfigDescription("Rows for Karve", new AcceptableValueRange<int>(2, 30)));
        Container_Configs.KarveCol = OdinQOLplugin.context.config("Containers", "Karve Columns", 2,
            new ConfigDescription("Columns for Karve", new AcceptableValueRange<int>(2, 8)));
        Container_Configs.LongRow = OdinQOLplugin.context.config("Containers", "Longboat Rows", 3,
            new ConfigDescription("Rows for longboat", new AcceptableValueRange<int>(3, 30)));
        Container_Configs.LongCol = OdinQOLplugin.context.config("Containers", "Longboat Columns", 6,
            new ConfigDescription("Columns for longboat", new AcceptableValueRange<int>(6, 8)));
        Container_Configs.CartRow = OdinQOLplugin.context.config("Containers", "Cart Rows", 3,
            new ConfigDescription("Rows for Cart", new AcceptableValueRange<int>(3, 30)));
        Container_Configs.CartCol = OdinQOLplugin.context.config("Containers", "Cart Columns", 6,
            new ConfigDescription("Columns for Cart", new AcceptableValueRange<int>(6, 8)));
        Container_Configs.PersonalRow = OdinQOLplugin.context.config("Containers", "Personal Chest Rows", 2,
            new ConfigDescription("Personal Chest Rows", new AcceptableValueRange<int>(2, 20)));
        Container_Configs.PersonalCol = OdinQOLplugin.context.config("Containers", "Personal Chest Columns", 3,
            new ConfigDescription("Personal Chest Columns", new AcceptableValueRange<int>(3, 8)));
        Container_Configs.WoodRow = OdinQOLplugin.context.config("Containers", "Wood Chest Rows", 2,
            new ConfigDescription("Wood Chest Rows", new AcceptableValueRange<int>(2, 10)));
        Container_Configs.WoodCol = OdinQOLplugin.context.config("Containers", "Wood Chest Columns", 5,
            new ConfigDescription("Wood Chest Columns", new AcceptableValueRange<int>(5, 8)));
        Container_Configs.IronRow = OdinQOLplugin.context.config("Containers", "Iron Chest Rows", 3,
            new ConfigDescription("Iron Chest Rows", new AcceptableValueRange<int>(3, 20)));
        Container_Configs.IronCol = OdinQOLplugin.context.config("Containers", "Iron Chest Columns", 6,
            new ConfigDescription("Iron Chest Columns", new AcceptableValueRange<int>(6, 8)));
        Container_Configs.BmRow = OdinQOLplugin.context.config("Containers", "Blackmetal Chest Rows", 4,
            new ConfigDescription("Blackmetal Chest Rows", new AcceptableValueRange<int>(3, 20)));
        Container_Configs.BmCol = OdinQOLplugin.context.config("Containers", "Blackmetal Chest Columns", 8,
            new ConfigDescription("Blackmetal Chest Columns", new AcceptableValueRange<int>(6, 8)));
    }
}