using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class SignPatchConfigs
{
    internal static void Generate()
    {
        SignPatches.signScale = OdinQOLplugin.context.config("Signs", "SignScale", new Vector3(1, 1, 1), "Sign scale (w,h,d)");
        SignPatches.textPositionOffset =
            OdinQOLplugin.context.config("Signs", "TextPositionOffset", new Vector2(0, 0), "Default font size");
        SignPatches.useRichText = OdinQOLplugin.context.config("Signs", "UseRichText", true,
            "Enable rich text. If this is disabled, the sign reverts back to vanilla functionality.");
        SignPatches.fontName = OdinQOLplugin.context.config("Signs", "FontName", "Norsebold", "Font name", false);
        SignPatches.signDefaultColor = OdinQOLplugin.context.config("Signs", "SignDefaultColor", "black",
            "This uses string values to set the default color every sign should have. The code runs when the sign loads in for the first time. If the sign doesn't have a color tag already, it will wrap the text in one. Use values like \"red\" here to specify a default color.");
    }
}