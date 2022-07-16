﻿using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class ClockPatchConfigs
{
    internal static void Generate()
    {
        ClockPatches.toggleClockKeyMod = OdinQOLplugin.context.config("Clock", "ShowClockKeyMod", "",
            "Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
            false);
        ClockPatches.toggleClockKey = OdinQOLplugin.context.config("Clock", "ShowClockKey", "home",
            "Key used to toggle the clock display. use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
            false);
        ClockPatches.clockLocationString = OdinQOLplugin.context.config("Clock", "ClockLocationString", "50%,3%",
            "Location on the screen to show the clock (x,y) or (x%,y%)", false);
        ClockPatches.showingClock = OdinQOLplugin.context.config("Clock", "ShowClock", true, "Show the clock?");
        ClockPatches.showClockOnChange = OdinQOLplugin.context.config("Clock", "ShowClockOnChange", false,
            "Only show the clock when the time changes?", false);
        ClockPatches.showClockOnChangeFadeTime = OdinQOLplugin.context.config("Clock", "ShowClockOnChangeFadeTime", 5f,
            "If only showing on change, length in seconds to show the clock before begining to fade", false);
        ClockPatches.showClockOnChangeFadeLength = OdinQOLplugin.context.config("Clock", "ShowClockOnChangeFadeLength",
            1f,
            "How long fade should take in seconds", false);
        ClockPatches.clockUseOSFont = OdinQOLplugin.context.config("Clock", "ClockUseOSFont", false,
            "Set to true to specify the name of a font from your OS; otherwise limited to fonts in the game resources",
            false);
        ClockPatches.clockUseShadow =
            OdinQOLplugin.context.config("Clock", "ClockUseShadow", false, "Add a shadow behind the text", false);
        ClockPatches.clockShadowOffset =
            OdinQOLplugin.context.config("Clock", "ClockShadowOffset", 2, "Shadow offset in pixels", false);
        ClockPatches.clockFontName = OdinQOLplugin.context.config("Clock", "ClockFontName", "AveriaSerifLibre-Bold",
            "Name of the font to use", false);
        ClockPatches.clockFontSize = OdinQOLplugin.context.config("Clock", "ClockFontSize", 24,
            "Location on the screen in pixels to show the clock", false);
        ClockPatches.clockFontColor = OdinQOLplugin.context.config("Clock", "ClockFontColor", Color.white, "Font color for the clock", false);
        ClockPatches.clockShadowColor = OdinQOLplugin.context.config("Clock", "ClockShadowColor", Color.black, "Color for the shadow", false);
        ClockPatches.toggleClockKeyMod = OdinQOLplugin.context.config("Clock", "ShowClockKeyMod", "",
            "Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
            false);
        ClockPatches.toggleClockKeyOnPress = OdinQOLplugin.context.config("Clock", "ShowClockKeyOnPress", false,
            "If true, limit clock display to when the hotkey is down", false);
        ClockPatches.clockFormat = OdinQOLplugin.context.config("Clock", "ClockFormat", "HH:mm",
            "Time format; set to 'fuzzy' for fuzzy time", false);
        ClockPatches.clockString = OdinQOLplugin.context.config("Clock", "ClockString", "<b>{0}</b>",
            "Formatted clock string - {0} is replaced by the actual time string, {1} is replaced by the fuzzy string, {2} is replaced by the current day",
            false);
        ClockPatches.clockTextAlignment = OdinQOLplugin.context.config("Clock", "ClockTextAlignment",
            TextAnchor.MiddleCenter,
            "Clock text alignment.", false);
        ClockPatches.clockFuzzyStrings = OdinQOLplugin.context.config("Clock", "ClockFuzzyStrings",
            "Midnight,Early Morning,Early Morning,Before Dawn,Before Dawn,Dawn,Dawn,Morning,Morning,Late Morning,Late Morning,Midday,Midday,Early Afternoon,Early Afternoon,Afternoon,Afternoon,Evening,Evening,Night,Night,Late Night,Late Night,Midnight",
            "Fuzzy time strings to split up the day into custom periods if ClockFormat is set to 'fuzzy'; comma-separated",
            false);

        ClockPatches.newTimeString = "";
        ClockPatches.style = new GUIStyle
        {
            richText = true,
            fontSize = ClockPatches.clockFontSize.Value,
            alignment = ClockPatches.clockTextAlignment.Value
        };
        ClockPatches.style2 = new GUIStyle
        {
            richText = true,
            fontSize = ClockPatches.clockFontSize.Value,
            alignment = ClockPatches.clockTextAlignment.Value
        };
    }
}