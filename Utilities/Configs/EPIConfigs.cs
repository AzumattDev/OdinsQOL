using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class EPIConfigs
{
    internal static void Generate()
    {
        /* Extended Player Inventory Config options */
        QuickAccessBar.ExtraRows = OdinQOLplugin.context.config("Extended Inventory", "ExtraRows", 0,
            "Number of extra ordinary rows. (This can cause overlap with chest GUI, make sure you hold CTRL (the default key) and drag to desired position)");
        QuickAccessBar.AddEquipmentRow = OdinQOLplugin.context.config("Extended Inventory", "AddEquipmentRow", false,
            "Add special row for equipped items and quick slots. (IF YOU ARE USING RANDY KNAPPS EAQs KEEP THIS VALUE OFF)");
        QuickAccessBar.DisplayEquipmentRowSeparate = OdinQOLplugin.context.config("Extended Inventory",
            "DisplayEquipmentRowSeparate",
            false,
            "Display equipment and quickslots in their own area. (IF YOU ARE USING RANDY KNAPPS EAQs KEEP THIS VALUE OFF)");

        QuickAccessBar.HelmetText = OdinQOLplugin.context.config("Extended Inventory", "HelmetText", "Head",
            "Text to show for helmet slot.", false);
        QuickAccessBar.ChestText = OdinQOLplugin.context.config("Extended Inventory", "ChestText", "Chest",
            "Text to show for chest slot.", false);
        QuickAccessBar.LegsText = OdinQOLplugin.context.config("Extended Inventory", "LegsText", "Legs",
            "Text to show for legs slot.", false);
        QuickAccessBar.BackText = OdinQOLplugin.context.config("Extended Inventory", "BackText", "Back",
            "Text to show for back slot.", false);
        QuickAccessBar.UtilityText = OdinQOLplugin.context.config("Extended Inventory", "UtilityText", "Utility",
            "Text to show for utility slot.", false);

        QuickAccessBar.QuickAccessScale = OdinQOLplugin.context.config("Extended Inventory", "QuickAccessScale", 1f,
            "Scale of quick access bar. ", false);

        QuickAccessBar.HotKey1 = OdinQOLplugin.context.config("Extended Inventory", "HotKey1", KeyCode.Z,
            "Hotkey 1 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", false);
        QuickAccessBar.HotKey2 = OdinQOLplugin.context.config("Extended Inventory", "HotKey2", KeyCode.X,
            "Hotkey 2 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", false);
        QuickAccessBar.HotKey3 = OdinQOLplugin.context.config("Extended Inventory", "HotKey3", KeyCode.C,
            "Hotkey 3 - Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", false);

        QuickAccessBar.ModKeyOne = OdinQOLplugin.context.config("Extended Inventory", "ModKey1", KeyCode.Mouse0,
            "First modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.",
            false);
        QuickAccessBar.ModKeyTwo = OdinQOLplugin.context.config("Extended Inventory", "ModKey2", KeyCode.LeftControl,
            "Second modifier key to move quick slots. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html format.",
            false);

        QuickAccessBar.QuickAccessX = OdinQOLplugin.context.config("Extended Inventory", "quickAccessX", 9999f,
            "Current X of Quick Slots (Not Synced with server)", false);
        QuickAccessBar.QuickAccessY = OdinQOLplugin.context.config("Extended Inventory", "quickAccessY", 9999f,
            "Current Y of Quick Slots (Not Synced with server)", false);
    }
}