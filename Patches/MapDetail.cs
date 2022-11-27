using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    internal class MapDetail
    {
        private static Vector2 _lastPos = Vector2.zero;
        private static List<int> _lastPixels = new();
        private static Texture2D _mapTexture = null!;

        public static ConfigEntry<float> ShowRange = null!;
        public static ConfigEntry<float> UpdateDelta = null!;
        public static ConfigEntry<bool> ShowBuildings = null!;
        public static ConfigEntry<bool> MapDetailOn = null!;
        public static ConfigEntry<bool> DisplayCartsAndBoats = null!;
        public static ConfigEntry<string> CustomBoats = null!;
        public static ConfigEntry<Color> PersonalBuildingColor = null!;
        public static ConfigEntry<Color> OtherBuildingColor = null!;
        public static ConfigEntry<Color> UnownedBuildingColor = null!;
        public static ConfigEntry<string> CustomPlayerColors = null!;

        public static IEnumerator UpdateMap(bool force)
        {
            if(!MapDetailOn.Value || !ShowBuildings.Value) { yield break; }

            if (force)
                yield return null;

            Vector3 position = Player.m_localPlayer.transform.position;
            Vector2 coords = new(position.x,
                position.z);

            if (!force && Vector2.Distance(_lastPos, coords) < UpdateDelta.Value)
                yield break;

            _lastPos = coords;

            Dictionary<int, long>? pixels = new();
            bool newPix = false;

            foreach (Collider? collider in Physics.OverlapSphere(Player.m_localPlayer.transform.position,
                         Mathf.Max(ShowRange.Value, 0), LayerMask.GetMask("piece")))
            {
                Piece? piece = collider.GetComponentInParent<Piece>();
                if (piece == null || !piece.GetComponent<ZNetView>().IsValid()) continue;
                Vector3 pos = piece.transform.position;
                int num = Minimap.instance.m_textureSize / 2;
                float mx = pos.x / Minimap.instance.m_pixelSize + num;
                float my = pos.z / Minimap.instance.m_pixelSize + num;

                int x = Mathf.RoundToInt(mx / Minimap.instance.m_textureSize * _mapTexture.width);
                int y = Mathf.RoundToInt(my / Minimap.instance.m_textureSize * _mapTexture.height);

                int idx = x + y * _mapTexture.width;
                if (pixels.ContainsKey(idx)) continue;
                if (!_lastPixels.Contains(idx))
                    newPix = true;
                pixels[idx] = piece.GetCreator();
            }

            if (!newPix)
            {
                foreach (int i in _lastPixels.Where(i => !pixels.ContainsKey(i)))
                    goto newpixels;
                yield break;
            }

            newpixels:

            _lastPixels = new List<int>(pixels.Keys);

            if (pixels.Count == 0)
            {
                SetMaps(_mapTexture);
                yield break;
            }

            List<string>? customColors = new();
            if (CustomPlayerColors.Value.Length > 0) customColors = CustomPlayerColors.Value.Split(',').ToList();
            Dictionary<long, Color>? assignedColors = new();
            bool named = CustomPlayerColors.Value.Contains(":");

            Color32[]? data = _mapTexture.GetPixels32();
            foreach (KeyValuePair<int, long> kvp in pixels)
            {
                Color color = Color.clear;
                if (assignedColors.ContainsKey(kvp.Value))
                {
                    color = assignedColors[kvp.Value];
                }
                else if (customColors.Count > 0 && kvp.Value != 0)
                {
                    if (!named)
                    {
                        ColorUtility.TryParseHtmlString(customColors[0], out color);
                        if (color != Color.clear)
                        {
                            assignedColors[kvp.Value] = color;
                            customColors.RemoveAt(0);
                        }
                    }
                    else
                    {
                        string? pair = customColors.Find(s =>
                            s.StartsWith(Player.GetPlayer(kvp.Value)?.GetPlayerName() + ":"));
                        if (pair is { Length: > 0 })
                            ColorUtility.TryParseHtmlString(pair.Split(':')[1], out color);
                    }
                }

                if (color == Color.clear)
                    GetUserColor(kvp.Value, out color);
                data[kvp.Key] = color;
            }

            Texture2D? tempTexture = new(_mapTexture.width, _mapTexture.height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            tempTexture.SetPixels32(data);
            tempTexture.Apply();

            SetMaps(tempTexture);

            OdinQOLplugin.QOLLogger.LogDebug($"Added {pixels.Count} pixels");
        }

        private static void GetUserColor(long id, out Color color)
        {
            color = id == 0 ? UnownedBuildingColor.Value :
                id == Player.m_localPlayer.GetPlayerID() ? PersonalBuildingColor.Value : OtherBuildingColor.Value;
        }

        private static void SetMaps(Texture2D texture)
        {
            Minimap.instance.m_mapImageSmall.material.SetTexture("_MainTex", texture);
            Minimap.instance.m_mapImageLarge.material.SetTexture("_MainTex", texture);
            if (!Minimap.instance.m_smallRoot.activeSelf) return;
            Minimap.instance.m_smallRoot.SetActive(false);
            Minimap.instance.m_smallRoot.SetActive(true);
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.GenerateWorldMap))]
        private static class GenerateWorldMap_Patch
        {
            private static void Postfix(Texture2D ___m_mapTexture)
            {
                if (!OdinQOLplugin.ModEnabled.Value) return;
                if (!MapDetailOn.Value) return;
                Color32[]? data = ___m_mapTexture.GetPixels32();

                _mapTexture = new Texture2D(___m_mapTexture.width, ___m_mapTexture.height, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp
                };
                _mapTexture.SetPixels32(data);
                _mapTexture.Apply();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
        private static class Player_PlacePiece_Patch
        {
            private static void Postfix(bool __result)
            {
                if (!OdinQOLplugin.ModEnabled.Value || !__result)
                    return;
                if (MapDetailOn.Value)
                    OdinQOLplugin.context.StartCoroutine(UpdateMap(true));
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
        private static class Player_RemovePiece_Patch
        {
            private static void Postfix(bool __result)
            {
                if (!OdinQOLplugin.ModEnabled.Value || !__result)
                    return;
                if (MapDetailOn.Value)
                    OdinQOLplugin.context.StartCoroutine(UpdateMap(true));
            }
        }
    }
}