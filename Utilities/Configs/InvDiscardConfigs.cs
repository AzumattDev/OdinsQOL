using BepInEx.Configuration;
using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class InvDiscardConfigs
{
    internal static void Generate()
    {
        InventoryDiscard.DiscardInvEnabled =
            OdinQOLplugin.context.config("Inventory Discard", "Enabled", false, "Enable Inventory Discard Section");
        InventoryDiscard.HotKey = OdinQOLplugin.context.config("Inventory Discard", "DiscardHotkey",
            new KeyboardShortcut(KeyCode.Delete),
            new ConfigDescription("The hotkey to discard an item", new AcceptableShortcuts()), false);
        InventoryDiscard.ReturnUnknownResources = OdinQOLplugin.context.config("Inventory Discard",
            "ReturnUnknownResources", false,
            "Return resources if recipe is unknown");
        InventoryDiscard.ReturnEnchantedResources = OdinQOLplugin.context.config("Inventory Discard",
            "ReturnEnchantedResources", false,
            "Return resources for Epic Loot enchantments");
        InventoryDiscard.ReturnResources = OdinQOLplugin.context.config("Inventory Discard", "ReturnResources", 1f,
            "Fraction of resources to return (0.0 - 1.0)");
    }
}