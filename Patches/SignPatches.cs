﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace VMP_Mod.Patches
{
    class SignPatches
    {
        public static ConfigEntry<bool> useRichText;
        public static ConfigEntry<string> fontName;
        public static ConfigEntry<Vector2> textPositionOffset;
        public static ConfigEntry<Vector3> signScale;
        public static Font currentFont;
        public static string lastFontName;

        [HarmonyPatch(typeof(Sign), "Awake")]
        static class Sign_Awake_Patch
        {
            static void Postfix(Sign __instance)
            {
                FixSign(ref __instance);
            }
        }
        [HarmonyPatch(typeof(Sign), "UpdateText")]
        static class Sign_UpdateText_Patch
        {
            static void Postfix(Sign __instance)
            {
                FixSign(ref __instance);
            }
        }
        public static void FixSign(ref Sign sign)
        {
            sign.transform.localScale = signScale.Value;

            sign.m_textWidget.supportRichText = useRichText.Value;
            sign.m_characterLimit = 0;
            sign.m_textWidget.material = null;
            //sign.m_textWidget.fontSize = fontSize.Value;
            sign.m_textWidget.gameObject.GetComponent<RectTransform>().anchoredPosition = textPositionOffset.Value;
            if (lastFontName != fontName.Value) // call when config changes
            {
                lastFontName = fontName.Value;
                VMP_Modplugin.Dbgl($"new font {fontName.Value}");
                Font font = GetFont(fontName.Value, 20);
                if (font == null)
                    VMP_Modplugin.Dbgl($"new font not found");
                else
                    currentFont = font;
            }
            if (currentFont != null && sign.m_textWidget.font?.name != currentFont.name)
            {
                VMP_Modplugin.Dbgl($"setting font {currentFont.name}");
                sign.m_textWidget.font = currentFont;
            }
        }
        public static Font GetFont(string fontName, int fontSize)
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (Font font in fonts)
            {
                if (font.name == fontName)
                {
                    return font;
                }
            }
            return Font.CreateDynamicFontFromOSFont(fontName, fontSize);
        }



    }
}