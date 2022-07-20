using BepInEx.Configuration;
using OdinQOL.Patches.BiFrost;
using UnityEngine;

namespace OdinQOL.Configs;

public class BiFrostConfigs
{
    internal static void Generate()
    {
        BiFrost.UIAnchor = OdinQOLplugin.context.config("BiFrost", "Position of the UI", new Vector2(-900, 200),
            new ConfigDescription("Sets the anchor position of the UI"), false);

        BiFrost.DisableBiFrost = OdinQOLplugin.context.config("BiFrost", "Disable", true,
            new ConfigDescription("Disables the GUI for the BiFrost"), false);

        BiFrost.UIAnchor.SettingChanged += Utilities.SaveAndReset;
    }
}