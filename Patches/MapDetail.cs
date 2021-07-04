using BepInEx.Configuration;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VMP_Mod.Patches
{
    class MapDetail
    {
        private static Vector2 lastPos = Vector2.zero;
        private static List<int> lastPixels = new List<int>();
        private static Texture2D mapTexture;

        public static ConfigEntry<float> showRange;
        public static ConfigEntry<float> updateDelta;
        public static ConfigEntry<bool> showBuildings;

        public static ConfigEntry<Color> personalBuildingColor;
        public static ConfigEntry<Color> otherBuildingColor;
        public static ConfigEntry<Color> unownedBuildingColor;
        public static ConfigEntry<string> customPlayerColors;

        [HarmonyPatch(typeof(Minimap), "GenerateWorldMap")]
        static class GenerateWorldMap_Patch
        {
            static void Postfix(Texture2D ___m_mapTexture)
            {

                Color32[] data = ___m_mapTexture.GetPixels32();

                MapDetail.mapTexture = new Texture2D(___m_mapTexture.width, ___m_mapTexture.height, TextureFormat.RGBA32, false);
                mapTexture.wrapMode = TextureWrapMode.Clamp;
                mapTexture.SetPixels32(data);
                mapTexture.Apply();
            }
        }

        public static IEnumerator UpdateMap(bool force)
        {
            if (force)
                yield return null;

            Vector2 coords = new Vector2(Player.m_localPlayer.transform.position.x, Player.m_localPlayer.transform.position.z);

            if (!force && Vector2.Distance(MapDetail.lastPos, coords) < updateDelta.Value)
                yield break;

            MapDetail.lastPos = coords;

            Dictionary<int, long> pixels = new Dictionary<int, long>();
            bool newPix = false;

            foreach (Collider collider in Physics.OverlapSphere(Player.m_localPlayer.transform.position, Mathf.Max(showRange.Value, 0), LayerMask.GetMask(new string[] { "piece" })))
            {
                Piece piece = collider.GetComponentInParent<Piece>();
                if (piece != null && piece.GetComponent<ZNetView>().IsValid())
                {
                    Vector3 pos = piece.transform.position;
                    float mx;
                    float my;
                    int num = Minimap.instance.m_textureSize / 2;
                    mx = pos.x / Minimap.instance.m_pixelSize + num;
                    my = pos.z / Minimap.instance.m_pixelSize + num;

                    int x = Mathf.RoundToInt(mx / Minimap.instance.m_textureSize * mapTexture.width);
                    int y = Mathf.RoundToInt(my / Minimap.instance.m_textureSize * mapTexture.height);

                    int idx = x + y * mapTexture.width;
                    if (!pixels.ContainsKey(idx))
                    {
                        if (!lastPixels.Contains(idx))
                            newPix = true;
                        pixels[idx] = piece.GetCreator();
                    }
                    //Dbgl($"pos {pos}; map pos: {mx},{my} pixel pos {x},{y}; index {idx}");
                }
            }

            if (!newPix)
            {
                foreach (int i in lastPixels)
                {
                    if (!pixels.ContainsKey(i))
                        goto newpixels;
                }
                VMP_Modplugin.Dbgl("No new pixels");
                yield break;
            }
        newpixels:

            MapDetail.lastPixels = new List<int>(pixels.Keys);

            if (pixels.Count == 0)
            {
                VMP_Modplugin.Dbgl("No pixels to add");
                MapDetail.SetMaps(mapTexture);
                yield break;
            }

            List<string> customColors = new List<string>();
            if (customPlayerColors.Value.Length > 0)
            {
                customColors = customPlayerColors.Value.Split(',').ToList();
            }
            Dictionary<long, Color> assignedColors = new Dictionary<long, Color>();
            bool named = customPlayerColors.Value.Contains(":");

            Color32[] data = mapTexture.GetPixels32();
            foreach (var kvp in pixels)
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
                        string pair = customColors.Find(s => s.StartsWith(Player.GetPlayer(kvp.Value)?.GetPlayerName() + ":"));
                        if (pair != null && pair.Length > 0)
                        {
                            ColorUtility.TryParseHtmlString(pair.Split(':')[1], out color);
                        }
                    }
                }

                if (color == Color.clear)
                    MapDetail.GetUserColor(kvp.Value, out color);
                data[kvp.Key] = color;
                /*
                for (int i = 0; i < data.Length; i++)
                {
                    if (Vector2.Distance(new Vector2(i % mapTexture.width, i / mapTexture.width), new Vector2(kvp.Key % mapTexture.width, kvp.Key / mapTexture.width)) < 10)
                        data[i] = kvp.Value;
                }
                */
                //Dbgl($"pixel coords {kvp.Key % mapTexture.width},{kvp.Key / mapTexture.width}");
            }

            Texture2D tempTexture = new Texture2D(mapTexture.width, mapTexture.height, TextureFormat.RGBA32, false);
            tempTexture.wrapMode = TextureWrapMode.Clamp;
            tempTexture.SetPixels32(data);
            tempTexture.Apply();

            MapDetail.SetMaps(tempTexture);

            VMP_Modplugin.Dbgl($"Added {pixels.Count} pixels");
            yield break;
        }

        private static void GetUserColor(long id, out Color color)
        {
            color = id == 0 ? unownedBuildingColor.Value : (id == Player.m_localPlayer.GetPlayerID() ? personalBuildingColor.Value : otherBuildingColor.Value);
        }

        private static void SetMaps(Texture2D texture)
        {
            Minimap.instance.m_mapImageSmall.material.SetTexture("_MainTex", texture);
            Minimap.instance.m_mapImageLarge.material.SetTexture("_MainTex", texture);
            if (Minimap.instance.m_smallRoot.activeSelf)
            {
                Minimap.instance.m_smallRoot.SetActive(false);
                Minimap.instance.m_smallRoot.SetActive(true);
            }
        }

    }
}
