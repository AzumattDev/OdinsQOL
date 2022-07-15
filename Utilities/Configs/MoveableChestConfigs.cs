using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class MoveableChestConfigs
{
    internal static void Generate()
    {
        /* Moveable Chest Inventory */
        MoveableChestInventory.chestInventoryX = OdinQOLplugin.context.config("General", "ChestInventoryX", -1f,
            "Current X of chest (Not Synced with server)", false);
        MoveableChestInventory.chestInventoryY = OdinQOLplugin.context.config("General", "ChestInventoryY", -1f,
            "Current Y of chest (Not Synced with server)", false);
        MoveableChestInventory.modKeyOneChestMove = OdinQOLplugin.context.config("General", "ModKeyOne", KeyCode.Mouse0,
            "First modifier key (to move the container). Use https://docs.unity3d.com/Manual/class-InputManager.html format.",
            false);
        MoveableChestInventory.modKeyTwoChestMove = OdinQOLplugin.context.config("General", "ModKeyTwo",
            KeyCode.LeftControl,
            "Second modifier key (to move the container). Use https://docs.unity3d.com/Manual/class-InputManager.html format.",
            false);
    }
}