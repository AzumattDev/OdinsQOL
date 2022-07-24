using BepInEx.Configuration;
using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class CraftingConfigs
{
    internal static void Generate()
    {
        CraftingPatch.AlterWorkbench =
            OdinQOLplugin.context.config("WorkBench", "Allow Workbench Alterations", false,
                "Toggles the WorkBench section.");
        CraftingPatch.WorkbenchRange = OdinQOLplugin.context.config("WorkBench", "WorkBenchRange", 20,
            new ConfigDescription("Range you can build from workbench in meters",
                new AcceptableValueRange<int>(6, 650)));
        CraftingPatch.WorkbenchEnemySpawnRange = OdinQOLplugin.context.config("WorkBench",
            "WorkBenchRange (Playerbase size)", 20,
            new ConfigDescription("Workbench PlayerBase radius, this is how far away enemies spawn",
                new AcceptableValueRange<int>(6, 650)));
        CraftingPatch.ChangeRoofBehavior =
            OdinQOLplugin.context.config("WorkBench", "Change No Roof Behavior", false,
                "Turns off the need for a roof to be above the workbench");
        CraftingPatch.WorkbenchAttachmentRange = OdinQOLplugin.context.config("WorkBench", "WorkBench Extension", 5,
            new ConfigDescription("Range for workbench extensions", new AcceptableValueRange<int>(5, 100)));
        CraftingPatch.MaxEntries =
            OdinQOLplugin.context.config("Show Chest Contents", "MaxEntries", -1,
                "Max number of entries to show (-1 means show all)", false);
        CraftingPatch.sortType = OdinQOLplugin.context.config("Show Chest Contents", "SortType",
            CraftingPatch.SortType.Value,
            "Type by which to sort entries.", false);
        CraftingPatch.SortAsc =
            OdinQOLplugin.context.config("Show Chest Contents", "SortAsc", false, "Sort ascending?", false);
        CraftingPatch.EntryString = OdinQOLplugin.context.config("Show Chest Contents", "EntryText",
            "<color=#FFFFAAFF>{0}</color> <color=#AAFFAAFF>{1}</color>",
            "Entry text. {0} is replaced by the total amount, {1} is replaced by the item name.", false);
        CraftingPatch.OverFlowText = OdinQOLplugin.context.config("Show Chest Contents", "OverFlowText",
            "<color=#AAAAAAFF>...</color>", "Overflow text if more items than max entries.", false);
        CraftingPatch.CapacityText = OdinQOLplugin.context.config("General", "CapacityText",
            "<color=#FFFFAAFF> {0}/{1}</color>",
            "Text to show capacity. {0} is replaced by number of full slots, {1} is replaced by total slots.",
            false);
    }
}