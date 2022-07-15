using System;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    public class ClockPatches
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
        public static ConfigEntry<string> clockLocationString;
        public static ConfigEntry<int> clockFontSize;
        public static ConfigEntry<string> toggleClockKeyMod;
        public static ConfigEntry<string> toggleClockKey;
        public static ConfigEntry<string> clockFontName;
        public static ConfigEntry<string> clockFormat;
        public static ConfigEntry<string> clockString;
        public static ConfigEntry<TextAnchor> clockTextAlignment;
        public static ConfigEntry<string> clockFuzzyStrings;

        private static Font clockFont;
        internal static GUIStyle style;
        internal static GUIStyle style2;
        private static bool configApplied;
        private static Vector2 clockPosition;
        private static float shownTime;
        private static string lastTimeString = "";
        private static Rect windowRect;
        private static Rect timeRect;
        internal static string newTimeString;

        private void OnGUI()
        {
            if (OdinQOLplugin.modEnabled.Value && configApplied && Player.m_localPlayer && Hud.instance)
            {
                float alpha = 1f;
                newTimeString = OdinQOLplugin.GetCurrentTimeString();
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
                    windowRect = GUILayout.Window(OdinQOLplugin.windowId, new Rect(windowRect.position, timeRect.size),
                        WindowBuilder,
                        "");
                    //OdinQOLplugin.QOLLogger.LogDebug(""+windowRect.size);
                }
            }

            if (!Input.GetKey(KeyCode.Mouse0) && (windowRect.x != clockPosition.x || windowRect.y != clockPosition.y))
            {
                clockPosition = new Vector2(windowRect.x, windowRect.y);
                clockLocationString.Value = $"{windowRect.x},{windowRect.y}";
                OdinQOLplugin.context.Config.Save();
            }
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
            string[]? split = clockLocationString.Value.Split(',');
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
                OdinQOLplugin.QOLLogger.LogDebug("Getting fonts");
                Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (Font? font in fonts)
                    if (font.name == clockFontName.Value)
                    {
                        clockFont = font;
                        OdinQOLplugin.QOLLogger.LogDebug($"Got font {font.name}");
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

        internal static string GetCurrentTimeString(DateTime theTime, float fraction, int days)
        {
            string[]? fuzzyStringArray = clockFuzzyStrings.Value.Split(',');

            int idx = Math.Min((int)(fuzzyStringArray.Length * fraction), fuzzyStringArray.Length - 1);

            if (clockFormat.Value == "fuzzy")
                return string.Format(clockString.Value, fuzzyStringArray[idx]);

            return string.Format(clockString.Value, theTime.ToString(clockFormat.Value), fuzzyStringArray[idx],
                days.ToString());
        }

        private static string GetFuzzyFileName(string lang)
        {
            return OdinQOLplugin.context.Info.Location.Replace("ClockMod.dll", "") +
                   string.Format("clockmod.lang.{0}.cfg", lang);
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

        internal static bool PressedToggleKey()
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

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        private static class ZNetScene_Awake_Patch
        {
            private static void Postfix()
            {
                if (!OdinQOLplugin.modEnabled.Value)
                    return;

                ApplyConfig();
            }
        }
    }
}