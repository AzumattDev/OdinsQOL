using BepInEx.Configuration;
using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class CFCConfigs
{
    internal static void Generate()
    {
        CFC.CFCEnabled = OdinQOLplugin.context.config("CraftFromContainers", "CFC Enabled", true,
            "Enable CraftFromContainers code");

        CFC.mRange = OdinQOLplugin.context.config("CraftFromContainers", "ContainerRange", 10f,
            "The maximum range from which to pull items from");
        CFC.resourceString = OdinQOLplugin.context.config("CraftFromContainers", "ResourceCostString", "{0}/{1}",
            "String used to show required and available resources. {0} is replaced by how much is available, and {1} is replaced by how much is required. Set to nothing to leave it as default.",
            false);
        CFC.flashColor = OdinQOLplugin.context.config("CraftFromContainers", "FlashColor", Color.yellow,
            "Resource amounts will flash to this colour when coming from containers", false);
        CFC.unFlashColor = OdinQOLplugin.context.config("CraftFromContainers", "UnFlashColor", Color.white,
            "Resource amounts will flash from this colour when coming from containers (set both colors to the same color for no flashing)",
            false);
        CFC.pulledMessage = OdinQOLplugin.context.config("CraftFromContainers", "PulledMessage",
            "Pulled items to inventory",
            "Message to show after pulling items to player inventory", false);
        CFC.CFCFuelDisallowTypes = OdinQOLplugin.context.config("CraftFromContainers", "FuelDisallowTypes",
            "RoundLog,FineWood",
            "Types of item to disallow as fuel (i.e. anything that is consumed), comma-separated. Uses Prefab names.");
        CFC.CFCOreDisallowTypes = OdinQOLplugin.context.config("CraftFromContainers", "OreDisallowTypes",
            "RoundLog,FineWood",
            "Types of item to disallow as ore (i.e. anything that is transformed), comma-separated). Uses Prefab names.");
        CFC.CFCItemDisallowTypes = OdinQOLplugin.context.config("CraftFromContainers", "ItemDisallowTypes",
            "",
            "Types of items to disallow pulling from chests, comma-separated. Uses Prefab names.");
        CFC.showGhostConnections = OdinQOLplugin.context.config("CraftFromContainers",
            "ShowConnections", false,
            "If true, will display connections to nearby workstations within range when building containers", false);
        CFC.ghostConnectionStartOffset = OdinQOLplugin.context.config("CraftFromContainers",
            "ConnectionStartOffset", 1.25f,
            "Height offset for the connection VFX start position", false);
        CFC.ghostConnectionRemovalDelay =
            OdinQOLplugin.context.config("CraftFromContainers", "ConnectionRemoveDelay", 0.05f, "", false);

        CFC.switchPrevent = OdinQOLplugin.context.config("CraftFromContainers", "SwitchPrevent", false,
            "If true, holding down the PreventModKey modifier key will allow this mod's behavior; If false, holding down the key will prevent it.",
            false);

        CFC.preventModKey = OdinQOLplugin.context.config("CraftFromContainers", "PreventModKey",
            new KeyboardShortcut(KeyCode.LeftAlt),
            new ConfigDescription(
                "Modifier key to toggle fuel and ore filling behaviour when down. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
                new AcceptableShortcuts()), false);
        CFC.pullItemsKey = OdinQOLplugin.context.config("CraftFromContainers", "PullItemsKey",
            new KeyboardShortcut(KeyCode.LeftControl),
            new ConfigDescription(
                "Holding down this key while crafting or building will pull resources into your inventory instead of building. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
                new AcceptableShortcuts()), false);
        CFC.fillAllModKey = OdinQOLplugin.context.config("CraftFromContainers", "FillAllModKey",
            new KeyboardShortcut(KeyCode.LeftShift),
            new ConfigDescription(
                "Modifier key to pull all available fuel or ore when down. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
                new AcceptableShortcuts()), false);
    }
}