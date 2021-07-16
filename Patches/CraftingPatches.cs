using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VMP_Mod.Patches
{
    internal class CraftingPatch
    {
        public enum SortType
        {
            Name,
            Weight,
            Amount,
            Value
        }

        public static ConfigEntry<int> WorkbenchRange;
        public static ConfigEntry<int> workbenchEnemySpawnRange;
        public static ConfigEntry<bool> AlterWorkBench;
        public static ConfigEntry<int> maxEntries;
        public static ConfigEntry<SortType> sortType;
        public static ConfigEntry<bool> sortAsc;
        public static ConfigEntry<string> entryString;
        public static ConfigEntry<string> overFlowText;
        public static ConfigEntry<bool> useScrollWheel;
        public static ConfigEntry<bool> showMenu;
        public static ConfigEntry<string> scrollModKey;
        public static ConfigEntry<string> prevHotKey;
        public static ConfigEntry<string> nextHotKey;
        public static ConfigEntry<string> categoryFile;

        public static Dictionary<string, List<ItemDrop.ItemData.ItemType>> categoryDict =
            new Dictionary<string, List<ItemDrop.ItemData.ItemType>>();

        public static List<string> categoryNames = new List<string>();
        public static List<GameObject> dropDownList = new List<GameObject>();

        public static int lastCategoryIndex;
        public static Vector3 lastMousePos;
        public static bool isShowing;
        public static string craftText = "Craft";
        public static string assetPath;
        public static int tabCraftPressed;

        public static void ResizeChildEffectArea(MonoBehaviour parent, EffectArea.Type includedTypes, float newRadius)
        {
            if (parent != null)
            {
                var effectArea = parent.GetComponentInChildren<EffectArea>();
                if (effectArea != null)
                    if ((effectArea.m_type & includedTypes) != 0)
                    {
                        var collision = effectArea.GetComponent<SphereCollider>();
                        if (collision != null) collision.radius = newRadius;
                    }
            }
        }

        public static void SortByType(SortType type, List<ItemData> items, bool asc)
        {
            // combine
            SortByName(items, true);

            for (var i = 0; i < items.Count; i++)
                while (i < items.Count - 1 && items[i].m_stack < items[i].m_shared.m_maxStackSize &&
                       items[i + 1].m_shared.m_name == items[i].m_shared.m_name)
                {
                    var amount = Mathf.Min(items[i].m_shared.m_maxStackSize - items[i].m_stack, items[i + 1].m_stack);
                    items[i].m_stack += amount;
                    if (amount == items[i + 1].m_stack)
                        items.RemoveAt(i + 1);
                    else
                        items[i + 1].m_stack -= amount;
                }

            switch (type)
            {
                case SortType.Name:
                    SortByName(items, asc);
                    break;
                case SortType.Weight:
                    SortByWeight(items, asc);
                    break;
                case SortType.Value:
                    SortByValue(items, asc);
                    break;
                case SortType.Amount:
                    SortByAmount(items, asc);
                    break;
            }
        }

        public static void SortByName(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_shared.m_name == b.m_shared.m_name) return CompareInts(a.m_stack, b.m_stack, false);
                return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                    Localization.instance.Localize(b.m_shared.m_name), asc);
            });
        }

        public static void SortByWeight(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_shared.m_weight == b.m_shared.m_weight)
                {
                    if (a.m_shared.m_name == b.m_shared.m_name)
                        return CompareInts(a.m_stack, b.m_stack, false);
                    return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                        Localization.instance.Localize(b.m_shared.m_name), true);
                }

                return CompareFloats(a.m_shared.m_weight, b.m_shared.m_weight, asc);
            });
        }

        public static void SortByValue(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_shared.m_value == b.m_shared.m_value)
                {
                    if (a.m_shared.m_name == b.m_shared.m_name)
                        return CompareInts(a.m_stack, b.m_stack, false);
                    return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                        Localization.instance.Localize(b.m_shared.m_name), true);
                }

                return CompareInts(a.m_shared.m_value, b.m_shared.m_value, asc);
            });
        }

        public static void SortByAmount(List<ItemData> items, bool asc)
        {
            items.Sort(delegate(ItemData a, ItemData b)
            {
                if (a.m_stack == b.m_stack)
                    return CompareStrings(Localization.instance.Localize(a.m_shared.m_name),
                        Localization.instance.Localize(b.m_shared.m_name), true);
                return CompareInts(a.m_stack, b.m_stack, asc);
            });
        }

        public static int CompareStrings(string a, string b, bool asc)
        {
            if (asc)
                return a.CompareTo(b);
            return b.CompareTo(a);
        }

        public static int CompareFloats(float a, float b, bool asc)
        {
            if (asc)
                return a.CompareTo(b);
            return b.CompareTo(a);
        }

        public static int CompareInts(float a, float b, bool asc)
        {
            if (asc)
                return a.CompareTo(b);
            return b.CompareTo(a);
        }


        public static void LoadCategories()
        {
            if (!Directory.Exists(assetPath))
            {
                VMP_Modplugin.Dbgl("Creating mod folder");
                Directory.CreateDirectory(assetPath);
            }

            var file = Path.Combine(assetPath, categoryFile.Value);
            CategoryData data;
            if (!File.Exists(file))
            {
                VMP_Modplugin.Dbgl("Creating category file");
                data = new CategoryData();
                File.WriteAllText(file, JsonUtility.ToJson(data));
            }
            else
            {
                data = JsonUtility.FromJson<CategoryData>(File.ReadAllText(file));
            }

            VMP_Modplugin.Dbgl("Loaded" + data.categories.Count + " categories");

            categoryDict.Clear();
            categoryNames.Clear();

            foreach (var cat in data.categories)
            {
                if (!cat.Contains(":"))
                    continue;
                var parts = cat.Split(':');
                var types = parts[1].Split(',');
                categoryNames.Add(parts[0]);

                categoryDict[parts[0]] = new List<ItemDrop.ItemData.ItemType>();
                foreach (var type in types)
                    if (Enum.TryParse(type, out ItemDrop.ItemData.ItemType result))
                        categoryDict[parts[0]].Add(result);
            }

            categoryNames.Sort(delegate(string a, string b)
            {
                if (categoryDict[a].Contains(ItemDrop.ItemData.ItemType.None))
                    return -1;
                if (categoryDict[b].Contains(ItemDrop.ItemData.ItemType.None))
                    return 1;
                return a.CompareTo(b);
            });
        }

        private static void SwitchFilter(int idx)
        {
            VMP_Modplugin.Dbgl($"switching to filter {idx}");

            lastCategoryIndex = idx;
            UpdateDropDown(false);
            SwitchFilter();
        }

        public static void SwitchFilter(bool next)
        {
            //Dbgl($"switching to {(next ? "next" : "last")} filter");

            if (next)
            {
                lastCategoryIndex++;
                lastCategoryIndex %= categoryNames.Count;
            }
            else
            {
                lastCategoryIndex--;
                if (lastCategoryIndex < 0)
                    lastCategoryIndex = categoryNames.Count - 1;
            }

            var recipes = new List<Recipe>();
            Player.m_localPlayer.GetAvailableRecipes(ref recipes);
            var count = 0;
            while (!categoryDict[categoryNames[lastCategoryIndex]].Contains(ItemDrop.ItemData.ItemType.None) &&
                   recipes.FindAll(r =>
                           categoryDict[categoryNames[lastCategoryIndex]]
                               .Contains(r.m_item.m_itemData.m_shared.m_itemType))
                       .Count == 0 && count < categoryNames.Count)
            {
                count++;
                if (next)
                {
                    lastCategoryIndex++;
                    lastCategoryIndex %= categoryNames.Count;
                }
                else
                {
                    lastCategoryIndex--;
                    if (lastCategoryIndex < 0)
                        lastCategoryIndex = categoryNames.Count - 1;
                }
            }

            SwitchFilter();
        }

        private static void SwitchFilter()
        {
            var recipes = new List<Recipe>();
            Player.m_localPlayer.GetAvailableRecipes(ref recipes);
            VMP_Modplugin.Dbgl(
                $"Switching to filter {categoryNames[lastCategoryIndex]} {recipes.Count} total recipes ");
            var t = Traverse.Create(InventoryGui.instance);
            t.Method("UpdateRecipeList", recipes).GetValue();
            t.Method("SetRecipe", 0, true).GetValue();
            InventoryGui.instance.m_tabCraft.gameObject.GetComponentInChildren<Text>().text = craftText +
                (categoryDict[categoryNames[lastCategoryIndex]].Contains(ItemDrop.ItemData.ItemType.None)
                    ? ""
                    : "\n" + categoryNames[lastCategoryIndex]);
        }

        private static void GetFilteredRecipes(ref List<Recipe> recipes)
        {
            if (InventoryGui.instance.InCraftTab() && !categoryDict[categoryNames[lastCategoryIndex]]
                .Contains(ItemDrop.ItemData.ItemType.None))
            {
                recipes = recipes.FindAll(r =>
                    categoryDict[categoryNames[lastCategoryIndex]].Contains(r.m_item.m_itemData.m_shared.m_itemType));
                VMP_Modplugin.Dbgl(
                    $"using filter {categoryNames[lastCategoryIndex]}, {recipes.Count} filtered recipes");
            }
        }


        public static void UpdateDropDown(bool show)
        {
            if (show == isShowing)
                return;
            if (show)
            {
                var recipes = new List<Recipe>();
                Player.m_localPlayer.GetAvailableRecipes(ref recipes);

                var gameScale = GameObject.Find("GUI").GetComponent<CanvasScaler>().scaleFactor;
                Vector2 pos = InventoryGui.instance.m_tabCraft.gameObject.transform.GetComponent<RectTransform>()
                    .position;
                var height = InventoryGui.instance.m_tabCraft.gameObject.transform.GetComponent<RectTransform>().rect
                    .height * gameScale;

                var showCount = 0;
                for (var i = 0; i < categoryNames.Count; i++)
                {
                    var count = recipes.FindAll(r =>
                        categoryDict[categoryNames[i]].Contains(r.m_item.m_itemData.m_shared.m_itemType)).Count;
                    dropDownList[i].SetActive(count > 0 ||
                                              categoryDict[categoryNames[i]].Contains(ItemDrop.ItemData.ItemType.None));
                    if (count > 0 || categoryDict[categoryNames[i]].Contains(ItemDrop.ItemData.ItemType.None))
                    {
                        dropDownList[i].GetComponent<RectTransform>().position =
                            pos - new Vector2(0, height * (showCount++ + 1));
                        dropDownList[i].GetComponentInChildren<Text>().text =
                            categoryNames[i] + (count == 0 ? "" : $" ({count})");
                    }
                }
            }
            else
            {
                for (var i = 0; i < categoryNames.Count; i++) dropDownList[i].SetActive(false);
            }

            isShowing = show;
        }

        /// <summary>
        ///     Alter workbench range
        /// </summary>
        [HarmonyPatch(typeof(CraftingStation), "Start")]
        public static class WorkbenchRangeIncrease
        {
            private static void Prefix(ref CraftingStation __instance, ref float ___m_rangeBuild,
                GameObject ___m_areaMarker)
            {
                if (AlterWorkBench.Value && WorkbenchRange.Value > 0)
                    try
                    {
                        ___m_rangeBuild = WorkbenchRange.Value;
                        ___m_areaMarker.GetComponent<CircleProjector>().m_radius = ___m_rangeBuild;
                        var scaleIncrease = (WorkbenchRange.Value - 20f) / 20f * 100f;
                        ___m_areaMarker.gameObject.transform.localScale =
                            new Vector3(scaleIncrease / 100, 1f, scaleIncrease / 100);

                        // Apply this change to the child GameObject's EffectArea collision.
                        // Various other systems query this collision instead of the PrivateArea radius for permissions (notably, enemy spawning).
                        ResizeChildEffectArea(__instance, EffectArea.Type.PlayerBase,
                            workbenchEnemySpawnRange.Value > 0 ? workbenchEnemySpawnRange.Value : WorkbenchRange.Value);
                    }
                    catch
                    {
                        // is not a workbench
                    }
            }
        }

        /// <summary>
        ///     Disable roof requirement on workbench
        /// </summary>
        [HarmonyPatch(typeof(CraftingStation), "CheckUsable")]
        public static class WorkbenchRemoveRestrictions
        {
            private static bool Prefix(ref CraftingStation __instance, ref Player player, ref bool showMessage,
                ref bool __result)
            {
                if (AlterWorkBench.Value) __instance.m_craftRequireRoof = false;

                return true;
            }
        }

        [HarmonyPatch(typeof(Container), "GetHoverText")]
        private static class GetHoverText_Patch
        {
            private static void Postfix(Container __instance, ref string __result)
            {
                if (__instance.m_checkGuardStone &&
                    !PrivateArea.CheckAccess(__instance.transform.position, 0f, false) ||
                    __instance.GetInventory().NrOfItems() == 0)
                    return;

                var items = new List<ItemData>();
                foreach (var idd in __instance.GetInventory().GetAllItems()) items.Add(new ItemData(idd));
                SortByType(SortType.Value, items, sortAsc.Value);
                var entries = 0;
                var amount = 0;
                var name = "";
                for (var i = 0; i < items.Count; i++)
                {
                    if (maxEntries.Value >= 0 && entries >= maxEntries.Value)
                    {
                        if (overFlowText.Value.Length > 0)
                            __result += "\n" + overFlowText.Value;
                        break;
                    }

                    var item = items[i];

                    if (item.m_shared.m_name == name || name == "")
                    {
                        amount += item.m_stack;
                    }
                    else
                    {
                        __result += "\n" + string.Format(entryString.Value, amount,
                            Localization.instance.Localize(name));
                        entries++;

                        amount = item.m_stack;
                    }

                    name = item.m_shared.m_name;
                    if (i == items.Count - 1)
                        __result += "\n" + string.Format(entryString.Value, amount,
                            Localization.instance.Localize(name));
                }
            }
        }

        public class ItemData
        {
            public ItemDrop.ItemData.SharedData m_shared;
            public int m_stack;

            public ItemData(ItemDrop.ItemData idd)
            {
                m_shared = idd.m_shared;
                m_stack = idd.m_stack;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
        private static class UpdateRecipeList_Patch
        {
            private static void Prefix(ref List<Recipe> recipes)
            {
                VMP_Modplugin.Dbgl("updating recipes");

                GetFilteredRecipes(ref recipes);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "Hide")]
        private static class Hide_Patch
        {
            private static void Prefix()
            {
                InventoryGui.instance.m_tabCraft.gameObject.GetComponentInChildren<Text>().text = craftText;
                lastCategoryIndex = 0;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "OnTabCraftPressed")]
        private static class OnTabCraftPressed_Patch
        {
            private static void Prefix()
            {
                VMP_Modplugin.Dbgl("Tab craft pressed");
                tabCraftPressed = 2;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "Awake")]
        private static class InventoryGui_Awake_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                dropDownList.Clear();

                //buttonObj.transform.parent.SetAsLastSibling();
                craftText = __instance.m_tabCraft.gameObject.GetComponentInChildren<Text>().text;
                for (var i = 0; i < categoryNames.Count; i++)
                {
                    var idx = i;
                    var go = Object.Instantiate(__instance.m_tabCraft.gameObject);
                    go.name = categoryNames[i];
                    go.transform.SetParent(__instance.m_tabCraft.gameObject.transform.parent.parent);
                    go.GetComponent<Button>().interactable = true;
                    go.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    go.GetComponent<Button>().onClick.AddListener(() => SwitchFilter(idx));
                    go.SetActive(false);
                    dropDownList.Add(go);
                }
            }
        }
    }

    public class CategoryData
    {
        public List<string> categories = new List<string>();

        public CategoryData()
        {
            foreach (var type in Enum.GetNames(typeof(ItemDrop.ItemData.ItemType))) categories.Add(type + ":" + type);
        }
    }
}