using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class ClockPatchConfigs
{
    internal static void Generate()
    {
        OdinQOLplugin.ShowingClock = OdinQOLplugin.context.config("Clock", "ShowClock", true, "Show the clock?");
        OdinQOLplugin.ToggleClockKeyMod = OdinQOLplugin.context.config("Clock", "ShowClockKeyMod", "",
            "Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
            false);
        OdinQOLplugin.ToggleClockKey = OdinQOLplugin.context.config("Clock", "ShowClockKey", "home",
            "Key used to toggle the clock display. use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
            false);
        OdinQOLplugin.ClockLocationString = OdinQOLplugin.context.config("Clock", "ClockLocationString", "48%,0%",
            "Location on the screen to show the clock (x,y) or (x%,y%)", false);
        OdinQOLplugin.ShowClockOnChange = OdinQOLplugin.context.config("Clock", "ShowClockOnChange", false,
            "Only show the clock when the time changes?", false);
        OdinQOLplugin.ShowClockOnChangeFadeTime = OdinQOLplugin.context.config("Clock", "ShowClockOnChangeFadeTime", 5f,
            "If only showing on change, length in seconds to show the clock before begining to fade", false);
        OdinQOLplugin.ShowClockOnChangeFadeLength = OdinQOLplugin.context.config("Clock", "ShowClockOnChangeFadeLength",
            1f,
            "How long fade should take in seconds", false);
        OdinQOLplugin.ClockUseOSFont = OdinQOLplugin.context.config("Clock", "ClockUseOSFont", false,
            "Set to true to specify the name of a font from your OS; otherwise limited to fonts in the game resources",
            false);
        OdinQOLplugin.ClockUseShadow =
            OdinQOLplugin.context.config("Clock", "ClockUseShadow", false, "Add a shadow behind the text", false);
        OdinQOLplugin.ClockShadowOffset =
            OdinQOLplugin.context.config("Clock", "ClockShadowOffset", 2, "Shadow offset in pixels", false);
        OdinQOLplugin.ClockFontName = OdinQOLplugin.context.config("Clock", "ClockFontName", "AveriaSerifLibre-Bold",
            "Name of the font to use", false);
        OdinQOLplugin.ClockFontSize = OdinQOLplugin.context.config("Clock", "ClockFontSize", 24,
            "Location on the screen in pixels to show the clock", false);
        OdinQOLplugin.ClockFontColor = OdinQOLplugin.context.config("Clock", "ClockFontColor", Color.white,
            "Font color for the clock", false);
        OdinQOLplugin.ClockShadowColor =
            OdinQOLplugin.context.config("Clock", "ClockShadowColor", Color.black, "Color for the shadow", false);
        OdinQOLplugin.ToggleClockKeyMod = OdinQOLplugin.context.config("Clock", "ShowClockKeyMod", "",
            "Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
            false);
        OdinQOLplugin.ToggleClockKeyOnPress = OdinQOLplugin.context.config("Clock", "ShowClockKeyOnPress", false,
            "If true, limit clock display to when the hotkey is down", false);
        OdinQOLplugin.ClockFormat = OdinQOLplugin.context.config("Clock", "ClockFormat", "HH:mm",
            "Time format; set to 'fuzzy' for fuzzy time", false);
        OdinQOLplugin.ClockString = OdinQOLplugin.context.config("Clock", "ClockString", "<b>{0}</b>",
            "Formatted clock string - {0} is replaced by the actual time string, {1} is replaced by the fuzzy string, {2} is replaced by the current day",
            false);
        OdinQOLplugin.ClockTextAlignment = OdinQOLplugin.context.config("Clock", "ClockTextAlignment",
            TextAnchor.MiddleCenter,
            "Clock text alignment.", false);
        OdinQOLplugin.ClockFuzzyStrings = OdinQOLplugin.context.config("Clock", "ClockFuzzyStrings",
            "Midnight,Early Morning,Early Morning,Before Dawn,Before Dawn,Dawn,Dawn,Morning,Morning,Late Morning,Late Morning,Midday,Midday,Early Afternoon,Early Afternoon,Afternoon,Afternoon,Evening,Evening,Night,Night,Late Night,Late Night,Midnight",
            "Fuzzy time strings to split up the day into custom periods if ClockFormat is set to 'fuzzy'; comma-separated",
            false);

        OdinQOLplugin.NewTimeString = "";
        OdinQOLplugin.Style = new GUIStyle
        {
            richText = true,
            fontSize = OdinQOLplugin.ClockFontSize.Value,
            alignment = OdinQOLplugin.ClockTextAlignment.Value
        };
        OdinQOLplugin.Style2 = new GUIStyle
        {
            richText = true,
            fontSize = OdinQOLplugin.ClockFontSize.Value,
            alignment = OdinQOLplugin.ClockTextAlignment.Value
        };
    }
}