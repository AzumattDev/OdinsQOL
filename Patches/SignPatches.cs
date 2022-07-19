using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    internal class SignPatches
    {
        public static ConfigEntry<bool> UseRichText = null!;
        public static ConfigEntry<string> FontName = null!;
        public static ConfigEntry<Vector2> TextPositionOffset = null!;
        public static ConfigEntry<Vector3> SignScale = null!;
        public static ConfigEntry<string> SignDefaultColor = null!;
        public static Font CurrentFont = null!;
        public static Material SaveOrigMat = null!;
        public static string LastFontName = null!;

        public static void FixSign(ref Sign sign)
        {
            sign.transform.localScale = SignScale.Value;

            sign.m_textWidget.supportRichText = UseRichText.Value;
            sign.m_characterLimit = 0;
            sign.m_textWidget.material = UseRichText.Value ? null : SaveOrigMat;
            sign.m_textWidget.gameObject.GetComponent<RectTransform>().anchoredPosition = TextPositionOffset.Value;
            if (LastFontName != FontName.Value) // call when config changes
            {
                LastFontName = FontName.Value;
                OdinQOLplugin.QOLLogger.LogDebug($"new font {FontName.Value}");
                Font? font = GetFont(FontName.Value, 20);
                if (font == null)
                    OdinQOLplugin.QOLLogger.LogDebug("new font not found");
                else
                    CurrentFont = font;
            }

            if (CurrentFont == null || sign.m_textWidget.font?.name == CurrentFont.name) return;
            OdinQOLplugin.QOLLogger.LogDebug($"setting font {CurrentFont.name}");
            sign.m_textWidget.font = CurrentFont;
        }

        public static Font GetFont(string fontName, int fontSize)
        {
            Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (Font? font in fonts)
                if (font.name == fontName)
                    return font;
            return Font.CreateDynamicFontFromOSFont(fontName, fontSize);
        }

        [HarmonyPatch(typeof(Sign), nameof(Sign.Awake))]
        private static class Sign_Awake_Patch
        {
            private static void Prefix(Sign __instance)
            {
                // Save the original material of the sign so we can dynamic swap later.
                SaveOrigMat = __instance.m_textWidget.material;
            }

            private static void Postfix(Sign __instance)
            {
                FixSign(ref __instance);
            }
        }

        [HarmonyPatch(typeof(Sign), nameof(Sign.UpdateText))]
        private static class Sign_UpdateText_Patch
        {
            private static void Postfix(Sign __instance)
            {
                FixSign(ref __instance);
                if (!UseRichText.Value) return;
                if (!__instance.m_nview.IsValid() || __instance.m_nview == null) return;

                if (SignDefaultColor.Value is not { Length: > 0 }) return;
                if (__instance.m_defaultText.Contains("<color=")) return;
                string newText = $"<color={SignDefaultColor.Value}>" +
                                 __instance.m_nview.GetZDO().GetString("text", __instance.m_defaultText) +
                                 "</color>";
                __instance.m_nview.ClaimOwnership();
                __instance.m_textWidget.text = newText;
                __instance.m_nview.GetZDO().Set(nameof(newText), newText);
            }
        }
    }
}