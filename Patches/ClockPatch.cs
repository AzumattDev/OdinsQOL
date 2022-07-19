using System;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    public class ClockPatches
    {
        public static ConfigEntry<bool> ShowingClock = null!;
        public static ConfigEntry<bool> ShowClockOnChange = null!;
        public static ConfigEntry<float> ShowClockOnChangeFadeTime = null!;
        public static ConfigEntry<float> ShowClockOnChangeFadeLength = null!;
        public static ConfigEntry<bool> ToggleClockKeyOnPress = null!;
        public static ConfigEntry<bool> ClockUseOSFont = null!;
        public static ConfigEntry<bool> ClockUseShadow = null!;
        public static ConfigEntry<Color> ClockFontColor = null!;
        public static ConfigEntry<Color> ClockShadowColor = null!;
        public static ConfigEntry<int> ClockShadowOffset = null!;
        public static ConfigEntry<string> ClockLocationString = null!;
        public static ConfigEntry<int> ClockFontSize = null!;
        public static ConfigEntry<string> ToggleClockKeyMod = null!;
        public static ConfigEntry<string> ToggleClockKey = null!;
        public static ConfigEntry<string> ClockFontName = null!;
        public static ConfigEntry<string> ClockFormat = null!;
        public static ConfigEntry<string> ClockString = null!;
        public static ConfigEntry<TextAnchor> ClockTextAlignment = null!;
        public static ConfigEntry<string> ClockFuzzyStrings = null!;

        private static Font _clockFont = null!;
        internal static GUIStyle Style = null!;
        internal static GUIStyle Style2 = null!;
        private static bool _configApplied;
        private static Vector2 _clockPosition;
        private static float _shownTime;
        private static string _lastTimeString = "";
        private static Rect _windowRect;
        private static Rect _timeRect;
        internal static string NewTimeString = "";

        private void OnGUI()
        {
            if (OdinQOLplugin.ModEnabled.Value && _configApplied && Player.m_localPlayer && Hud.instance)
            {
                float alpha = 1f;
                NewTimeString = OdinQOLplugin.GetCurrentTimeString();
                if (ShowClockOnChange.Value)
                {
                    if (NewTimeString == _lastTimeString)
                    {
                        _shownTime = 0;

                        if (!ToggleClockKeyOnPress.Value || !CheckKeyHeld(ToggleClockKey.Value))
                            return;
                    }

                    if (_shownTime > ShowClockOnChangeFadeTime.Value)
                    {
                        if (_shownTime > ShowClockOnChangeFadeTime.Value + ShowClockOnChangeFadeLength.Value)
                        {
                            _shownTime = 0;
                            _lastTimeString = NewTimeString;
                            if (!ToggleClockKeyOnPress.Value || !CheckKeyHeld(ToggleClockKey.Value))
                                return;
                        }

                        alpha = (ShowClockOnChangeFadeLength.Value + ShowClockOnChangeFadeTime.Value - _shownTime) /
                                ShowClockOnChangeFadeLength.Value;
                    }

                    _shownTime += Time.deltaTime;
                }

                Style.normal.textColor = new Color(ClockFontColor.Value.r, ClockFontColor.Value.g,
                    ClockFontColor.Value.b, ClockFontColor.Value.a * alpha);
                Style2.normal.textColor = new Color(ClockShadowColor.Value.r, ClockShadowColor.Value.g,
                    ClockShadowColor.Value.b, ClockShadowColor.Value.a * alpha);
                if ((!ToggleClockKeyOnPress.Value && ShowingClock.Value || ToggleClockKeyOnPress.Value &&
                        (ShowClockOnChange.Value || CheckKeyHeld(ToggleClockKey.Value))) &&
                    Traverse.Create(Hud.instance).Method("IsVisible").GetValue<bool>())
                {
                    GUI.backgroundColor = Color.clear;
                    _windowRect = GUILayout.Window(OdinQOLplugin.windowId,
                        new Rect(_windowRect.position, _timeRect.size),
                        WindowBuilder,
                        "");
                }
            }

            if (!Input.GetKey(KeyCode.Mouse0) &&
                (_windowRect.x != _clockPosition.x || _windowRect.y != _clockPosition.y))
            {
                _clockPosition = new Vector2(_windowRect.x, _windowRect.y);
                ClockLocationString.Value = $"{_windowRect.x},{_windowRect.y}";
                OdinQOLplugin.context.Config.Save();
            }
        }


        private void WindowBuilder(int id)
        {
            _timeRect = GUILayoutUtility.GetRect(new GUIContent(NewTimeString), Style);

            GUI.DragWindow(_timeRect);

            if (ClockUseShadow.Value)
                GUI.Label(
                    new Rect(_timeRect.position + new Vector2(-ClockShadowOffset.Value, ClockShadowOffset.Value),
                        _timeRect.size), NewTimeString, Style2);
            GUI.Label(_timeRect, NewTimeString, Style);
        }

        private static void ApplyConfig()
        {
            string[]? split = ClockLocationString.Value.Split(',');
            _clockPosition = new Vector2(
                split[0].Trim().EndsWith("%")
                    ? float.Parse(split[0].Trim().Substring(0, split[0].Trim().Length - 1)) / 100f * Screen.width
                    : float.Parse(split[0].Trim()),
                split[1].Trim().EndsWith("%")
                    ? float.Parse(split[1].Trim().Substring(0, split[1].Trim().Length - 1)) / 100f * Screen.height
                    : float.Parse(split[1].Trim()));

            _windowRect = new Rect(_clockPosition, new Vector2(1000, 100));

            if (ClockUseOSFont.Value)
            {
                _clockFont = Font.CreateDynamicFontFromOSFont(ClockFontName.Value, ClockFontSize.Value);
            }
            else
            {
                OdinQOLplugin.QOLLogger.LogDebug("Getting fonts");
                Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (Font? font in fonts)
                    if (font.name == ClockFontName.Value)
                    {
                        _clockFont = font;
                        OdinQOLplugin.QOLLogger.LogDebug($"Got font {font.name}");
                        break;
                    }
            }

            Style = new GUIStyle
            {
                richText = true,
                fontSize = ClockFontSize.Value,
                alignment = ClockTextAlignment.Value,
                font = _clockFont
            };
            Style2 = new GUIStyle
            {
                richText = true,
                fontSize = ClockFontSize.Value,
                alignment = ClockTextAlignment.Value,
                font = _clockFont
            };

            _configApplied = true;
        }

        internal static string GetCurrentTimeString(DateTime theTime, float fraction, int days)
        {
            string[]? fuzzyStringArray = ClockFuzzyStrings.Value.Split(',');

            int idx = Math.Min((int)(fuzzyStringArray.Length * fraction), fuzzyStringArray.Length - 1);

            if (ClockFormat.Value == "fuzzy")
                return string.Format(ClockString.Value, fuzzyStringArray[idx]);

            return string.Format(ClockString.Value, theTime.ToString(ClockFormat.Value), fuzzyStringArray[idx],
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
                return Input.GetKeyDown(ToggleClockKey.Value.ToLower()) && CheckKeyHeld(ToggleClockKeyMod.Value);
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
                if (!OdinQOLplugin.ModEnabled.Value)
                    return;

                ApplyConfig();
            }
        }
    }
}