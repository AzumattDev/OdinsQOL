using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace VMP_Mod.Patches
{
    class InventoryHUD
    {

        public static ConfigEntry<string> modKey;
        public static ConfigEntry<Vector2> hudPosition;
        public static ConfigEntry<Vector2> infoStringOffset;
        public static ConfigEntry<Vector2> weightOffset;
        public static ConfigEntry<int> extraSlots;

        public static ConfigEntry<string> infoString;
        public static ConfigEntry<int> infoStringSize;
        public static ConfigEntry<string> infoStringFont;
        public static ConfigEntry<TextAnchor> infoStringAlignment;
        public static ConfigEntry<Color> infoStringColor;

        public static ConfigEntry<string> weightFile;
        public static ConfigEntry<Color> weightColor;
        public static ConfigEntry<Color> fillColor;
        public static ConfigEntry<float> weightScale;


        public static GameObject partialObject;
        public static GameObject fullObject;
        public static GameObject maskObject;
        public static GameObject infoObject;
        public static Texture2D weightTexture;

        private static Font GetFont(string fontName, int fontSize)
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

        private static void AddWeightObject(Hud hud)
        {
            //stream this from internal assets cuz ...this is the way
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "InventoryHUD");

            if (path == null)
                return;
            weightTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            byte[] data = File.ReadAllBytes(Path.Combine(path, weightFile.Value));
            weightTexture.LoadImage(data);
            weightTexture.filterMode = FilterMode.Point;

            Texture2D maskTexture = new Texture2D(weightTexture.width, weightTexture.height, TextureFormat.RGBA32, false);
            for (int i = 0; i < weightTexture.width; i++)
            {
                for (int j = 0; j < weightTexture.height; j++)
                {
                    maskTexture.SetPixel(i, j, Color.white);
                }
            }
            maskTexture.Apply();

            Sprite sprite = Sprite.Create(weightTexture, new Rect(0, 0, weightTexture.width, weightTexture.height), Vector2.zero);
            Sprite fsprite = Sprite.Create(weightTexture, new Rect(0, 0, weightTexture.width, weightTexture.height), Vector2.zero);
            Sprite maskSprite = Sprite.Create(maskTexture, new Rect(0, 0, weightTexture.width, weightTexture.height), Vector2.zero);

            // Full object

            fullObject = new GameObject
            {
                name = "InventoryHUDFullImage"
            };

            RectTransform frt = fullObject.AddComponent<RectTransform>();
            frt.SetParent(hud.m_rootObject.transform);
            frt.localScale = Vector3.one * weightScale.Value;
            frt.anchoredPosition = Vector2.zero;
            frt.sizeDelta = new Vector2(weightTexture.width, weightTexture.height);

            Image fimage = fullObject.AddComponent<Image>();
            fimage.sprite = fsprite;
            fimage.color = weightColor.Value;
            fimage.preserveAspect = true;

            // Mask object

            maskObject = new GameObject();
            maskObject.name = "InventoryHUDMaskImage";
            RectTransform prt = maskObject.AddComponent<RectTransform>();
            prt.SetParent(hud.m_rootObject.transform);
            prt.localScale = Vector3.one * weightScale.Value;
            prt.anchoredPosition = Vector2.zero;
            prt.sizeDelta = new Vector2(weightTexture.width, weightTexture.height);


            Image maskImage = maskObject.AddComponent<Image>();
            maskImage.sprite = maskSprite;
            maskImage.preserveAspect = true;

            Mask mask = maskObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;


            // Partial object

            partialObject = new GameObject
            {
                name = "Image"
            };

            RectTransform rt = partialObject.AddComponent<RectTransform>();
            rt.SetParent(maskObject.transform);
            rt.localScale = Vector3.one * weightScale.Value;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(weightTexture.width, weightTexture.height);

            Image image = partialObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = fillColor.Value;
            image.preserveAspect = true;


            VMP_Modplugin.Dbgl("Added weight object to hud");
        }

        private static void AddInfoString(Hud hud)
        {
            infoObject = new GameObject
            {
                name = "InventoryHUDInfo"
            };

            RectTransform rt = infoObject.AddComponent<RectTransform>();
            rt.SetParent(hud.m_rootObject.transform);
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;

            Text text = infoObject.AddComponent<Text>();
            text.font = GetFont(infoStringFont.Value, infoStringSize.Value);
        }


        [HarmonyPatch(typeof(Hud), "Awake")]
        static class Hud_Awake_Patch
        {
            static void Postfix(Hud __instance)
            {
               
                if (weightFile.Value.Length > 0)
                    AddWeightObject(__instance);
                if (infoString.Value.Length > 0)
                    AddInfoString(__instance);
            }

        }


        [HarmonyPatch(typeof(Hud), "Update")]
        static class Hud_Update_Patch
        {
            public static Vector3 lastPosition = Vector3.zero;
            static void Prefix(Hud __instance)
            {
                if( !Player.m_localPlayer)
                    return;

                Vector3 hudPos = new Vector3(hudPosition.Value.x, hudPosition.Value.y, 0);

                if (__instance.m_rootObject.transform.localPosition.x > 1000f)
                {
                    maskObject.SetActive(false);
                    partialObject.SetActive(false);
                    fullObject.SetActive(false);
                    infoObject.SetActive(false);
                    return;
                }
                maskObject.SetActive(true);
                partialObject.SetActive(true);
                fullObject.SetActive(true);
                infoObject.SetActive(true);

                Inventory inv = Player.m_localPlayer.GetInventory();
                Vector3 weightPos = hudPos + new Vector3(weightOffset.Value.x, weightOffset.Value.y, 0);

                float weight = inv.GetTotalWeight();
                float totalWeight = Player.m_localPlayer.GetMaxCarryWeight();
                if (fullObject != null)
                {
                    float hudScale = GameObject.Find("GUI").GetComponent<CanvasScaler>().scaleFactor;

                    float maskOffset = (1 - weight / totalWeight) * weightTexture.height * weightScale.Value * hudScale;

                    if (Utilities.CheckKeyHeld(modKey.Value, true) && Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        VMP_Modplugin.Dbgl($"{lastPosition} {hudPos} {Vector3.Distance(lastPosition, hudPos)} {(weightTexture.height + weightTexture.width) / 4f} {maskOffset}");

                        if (Vector3.Distance(Input.mousePosition, hudPos) < (weightTexture.height + weightTexture.width) / 4f * weightScale.Value * hudScale)
                        {
                            VMP_Modplugin.Dbgl("dragging start");
                            lastPosition = Input.mousePosition;
                        }
                    }
                    else if (Utilities.CheckKeyHeld(modKey.Value, true) && Input.GetKey(KeyCode.Mouse0) && Vector3.Distance(lastPosition, weightPos) < (weightTexture.height + weightTexture.width) / 4f * weightScale.Value * hudScale)
                    {
                        hudPos += Input.mousePosition - lastPosition;
                        hudPosition.Value = new Vector2(hudPos.x, hudPos.y);
                    }
                    lastPosition = Input.mousePosition;

                    partialObject.GetComponent<Image>().color = fillColor.Value;
                    fullObject.GetComponent<Image>().color = weightColor.Value;

                    maskObject.transform.position = weightPos - new Vector3(0, maskOffset, 0);
                    partialObject.transform.position = weightPos;
                    fullObject.transform.position = weightPos;
                }
                if (infoObject != null)
                {
                    infoObject.transform.position = hudPos + new Vector3(infoStringOffset.Value.x, infoStringOffset.Value.y, 0);

                    int items = inv.GetAllItems().Count;
                    int slots = inv.GetWidth() * inv.GetHeight() + extraSlots.Value;
                    Text text = infoObject.GetComponent<Text>();
                    text.text = string.Format(infoString.Value, items, slots, weight, totalWeight);
                    text.color = infoStringColor.Value;
                    text.alignment = infoStringAlignment.Value;
                    text.fontSize = infoStringSize.Value;
                }

                /*

                Rect rect = parentObject.GetComponent<Image>().sprite.rect;
                partialObject.transform.parent.GetComponent<RectTransform>().localScale = Vector3.one * weightScale.Value;
                */

            }
        }

    }
}
