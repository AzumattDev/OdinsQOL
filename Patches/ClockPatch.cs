using System;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL
{
    public partial class OdinQOLplugin
    {
        public static ConfigEntry<bool> showingClock;
        public static ConfigEntry<bool> showClockOnChange;
        public static ConfigEntry<float> showClockOnChangeFadeTime;
        public static ConfigEntry<float> showClockOnChangeFadeLength;
        public static ConfigEntry<bool> toggleClockKeyOnPress;
        public static ConfigEntry<bool> clockUseOSFont;
        public static ConfigEntry<bool> clockUseShadow;
        public static ConfigEntry<Color> clockFontColor;
        public static ConfigEntry<Color> clockShadowColor;
        public static ConfigEntry<int> clockShadowOffset;
        public static ConfigEntry<Vector2> clockLocation;
        public static ConfigEntry<string> clockLocationString;
        public static ConfigEntry<int> clockFontSize;
        public static ConfigEntry<string> toggleClockKeyMod;
        public static ConfigEntry<string> toggleClockKey;
        public static ConfigEntry<string> clockFontName;
        public static ConfigEntry<string> clockFormat;
        public static ConfigEntry<string> clockString;
        public static ConfigEntry<TextAnchor> clockTextAlignment;
        public static ConfigEntry<string> clockFuzzyStrings;
        public static ConfigEntry<int> nexusID;

        private static Font clockFont;
        private static GUIStyle style;
        private static GUIStyle style2;
        private static bool configApplied;
        private static Vector2 clockPosition;
        private static float shownTime;
        private static string lastTimeString = "";
        private static Rect windowRect;
        private static Rect timeRect;
        private string newTimeString;

        private void OnGUI()
        {
            if (modEnabled.Value && configApplied && Player.m_localPlayer && Hud.instance)
            {
                var alpha = 1f;
                newTimeString = GetCurrentTimeString();
                if (showClockOnChange.Value)
                {
                    if (newTimeString == lastTimeString)
                    {
                        shownTime = 0;

                        if (!toggleClockKeyOnPress.Value || !CheckKeyHeld(toggleClockKey.Value))
                            return;
                    }

                    if (shownTime > showClockOnChangeFadeTime.Value)
                    {
                        if (shownTime > showClockOnChangeFadeTime.Value + showClockOnChangeFadeLength.Value)
                        {
                            shownTime = 0;
                            lastTimeString = newTimeString;
                            if (!toggleClockKeyOnPress.Value || !CheckKeyHeld(toggleClockKey.Value))
                                return;
                        }

                        alpha = (showClockOnChangeFadeLength.Value + showClockOnChangeFadeTime.Value - shownTime) /
                                showClockOnChangeFadeLength.Value;
                    }

                    shownTime += Time.deltaTime;
                }

                style.normal.textColor = new Color(clockFontColor.Value.r, clockFontColor.Value.g,
                    clockFontColor.Value.b, clockFontColor.Value.a * alpha);
                style2.normal.textColor = new Color(clockShadowColor.Value.r, clockShadowColor.Value.g,
                    clockShadowColor.Value.b, clockShadowColor.Value.a * alpha);
                if ((!toggleClockKeyOnPress.Value && showingClock.Value || toggleClockKeyOnPress.Value &&
                        (showClockOnChange.Value || CheckKeyHeld(toggleClockKey.Value))) &&
                    Traverse.Create(Hud.instance).Method("IsVisible").GetValue<bool>())
                {
                    GUI.backgroundColor = Color.clear;
                    windowRect = GUILayout.Window(windowId, new Rect(windowRect.position, timeRect.size), WindowBuilder,
                        "");
                    //Dbgl(""+windowRect.size);
                }
            }

            if (!Input.GetKey(KeyCode.Mouse0) && (windowRect.x != clockPosition.x || windowRect.y != clockPosition.y))
            {
                clockPosition = new Vector2(windowRect.x, windowRect.y);
                clockLocationString.Value = $"{windowRect.x},{windowRect.y}";
                Config.Save();
            }
        }


        public void LoadConfig()
        {
            context = this;


            showingClock = config("Clock", "ShowClock", true, "Show the clock?");
            showClockOnChange = config("Clock", "ShowClockOnChange", false,
                "Only show the clock when the time changes?", false);
            showClockOnChangeFadeTime = config("Clock", "ShowClockOnChangeFadeTime", 5f,
                "If only showing on change, length in seconds to show the clock before begining to fade", false);
            showClockOnChangeFadeLength = config("Clock", "ShowClockOnChangeFadeLength", 1f,
                "How long fade should take in seconds", false);
            clockUseOSFont = config("Clock", "ClockUseOSFont", false,
                "Set to true to specify the name of a font from your OS; otherwise limited to fonts in the game resources",
                false);
            clockUseShadow = config("Clock", "ClockUseShadow", false, "Add a shadow behind the text", false);
            clockShadowOffset = config("Clock", "ClockShadowOffset", 2, "Shadow offset in pixels", false);
            clockFontName = config("Clock", "ClockFontName", "AveriaSerifLibre-Bold", "Name of the font to use", false);
            clockFontSize = config("Clock", "ClockFontSize", 24,
                "Location on the screen in pixels to show the clock", false);
            clockFontColor = Config.Bind("Clock", "ClockFontColor", Color.white, "Font color for the clock");
            clockShadowColor = Config.Bind("Clock", "ClockShadowColor", Color.black, "Color for the shadow");
            toggleClockKeyMod = config("Clock", "ShowClockKeyMod", "",
                "Extra modifier key used to toggle the clock display. Leave blank to not require one. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html",
                false);
            toggleClockKeyOnPress = config("Clock", "ShowClockKeyOnPress", false,
                "If true, limit clock display to when the hotkey is down", false);
            clockFormat = config("Clock", "ClockFormat", "HH:mm", "Time format; set to 'fuzzy' for fuzzy time", false);
            clockString = config("Clock", "ClockString", "<b>{0}</b>",
                "Formatted clock string - {0} is replaced by the actual time string, {1} is replaced by the fuzzy string, {2} is replaced by the current day",
                false);
            clockTextAlignment = config("Clock", "ClockTextAlignment", TextAnchor.MiddleCenter,
                "Clock text alignment.", false);
            clockFuzzyStrings = config("Clock", "ClockFuzzyStrings",
                "Midnight,Early Morning,Early Morning,Before Dawn,Before Dawn,Dawn,Dawn,Morning,Morning,Late Morning,Late Morning,Midday,Midday,Early Afternoon,Early Afternoon,Afternoon,Afternoon,Evening,Evening,Night,Night,Late Night,Late Night,Midnight",
                "Fuzzy time strings to split up the day into custom periods if ClockFormat is set to 'fuzzy'; comma-separated",
                false);

            newTimeString = "";
            style = new GUIStyle
            {
                richText = true,
                fontSize = clockFontSize.Value,
                alignment = clockTextAlignment.Value
            };
            style2 = new GUIStyle
            {
                richText = true,
                fontSize = clockFontSize.Value,
                alignment = clockTextAlignment.Value
            };
        }


        private void WindowBuilder(int id)
        {
            timeRect = GUILayoutUtility.GetRect(new GUIContent(newTimeString), style);

            GUI.DragWindow(timeRect);

            if (clockUseShadow.Value)
                GUI.Label(
                    new Rect(timeRect.position + new Vector2(-clockShadowOffset.Value, clockShadowOffset.Value),
                        timeRect.size), newTimeString, style2);
            GUI.Label(timeRect, newTimeString, style);
        }

        private static void ApplyConfig()
        {
            var split = clockLocationString.Value.Split(',');
            clockPosition = new Vector2(
                split[0].Trim().EndsWith("%")
                    ? float.Parse(split[0].Trim().Substring(0, split[0].Trim().Length - 1)) / 100f * Screen.width
                    : float.Parse(split[0].Trim()),
                split[1].Trim().EndsWith("%")
                    ? float.Parse(split[1].Trim().Substring(0, split[1].Trim().Length - 1)) / 100f * Screen.height
                    : float.Parse(split[1].Trim()));

            windowRect = new Rect(clockPosition, new Vector2(1000, 100));

            if (clockUseOSFont.Value)
            {
                clockFont = Font.CreateDynamicFontFromOSFont(clockFontName.Value, clockFontSize.Value);
            }
            else
            {
                Debug.Log("getting fonts");
                var fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (var font in fonts)
                    if (font.name == clockFontName.Value)
                    {
                        clockFont = font;
                        Debug.Log($"got font {font.name}");
                        break;
                    }
            }

            style = new GUIStyle
            {
                richText = true,
                fontSize = clockFontSize.Value,
                alignment = clockTextAlignment.Value,
                font = clockFont
            };
            style2 = new GUIStyle
            {
                richText = true,
                fontSize = clockFontSize.Value,
                alignment = clockTextAlignment.Value,
                font = clockFont
            };

            configApplied = true;
        }

        private string GetCurrentTimeString(DateTime theTime, float fraction, int days)
        {
            var fuzzyStringArray = clockFuzzyStrings.Value.Split(',');

            var idx = Math.Min((int)(fuzzyStringArray.Length * fraction), fuzzyStringArray.Length - 1);

            if (clockFormat.Value == "fuzzy")
                return string.Format(clockString.Value, fuzzyStringArray[idx]);

            return string.Format(clockString.Value, theTime.ToString(clockFormat.Value), fuzzyStringArray[idx],
                days.ToString());
        }

        private static string GetFuzzyFileName(string lang)
        {
            return context.Info.Location.Replace("ClockMod.dll", "") + string.Format("clockmod.lang.{0}.cfg", lang);
        }

        private static bool CheckKeyHeld(string value)
        {
            try
            {
                return Input.GetKey(value.ToLower());
            }
            catch
            {
                return true;
            }
        }

        private bool PressedToggleKey()
        {
            try
            {
                return Input.GetKeyDown(toggleClockKey.Value.ToLower()) && CheckKeyHeld(toggleClockKeyMod.Value);
            }
            catch
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        private static class ZNetScene_Awake_Patch
        {
            private static void Postfix()
            {
                if (!modEnabled.Value)
                    return;

                ApplyConfig();
            }
        }
    }
}