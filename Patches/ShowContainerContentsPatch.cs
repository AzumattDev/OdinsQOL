using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using VMP_Mod;

namespace VMP_Mod.Patches
{
    class ShowContainerContentsPatch
    {
        private static VMP_Modplugin context;
        public static ConfigEntry<int> maxEntries;
        public static ConfigEntry<SortType> sortType;
        public static ConfigEntry<bool> sortAsc;
        public static ConfigEntry<string> entryString;
        public static ConfigEntry<string> overFlowText;
        public enum SortType
        {
            Name,
            Weight,
            Amount,
            Value
        }
        [HarmonyPatch(typeof(Container), "GetHoverText")]
        static class GetHoverText_Patch
        {
            static void Postfix(Container __instance, ref string __result)
            {
                if (__instance.m_checkGuardStone && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false) || __instance.GetInventory().NrOfItems() == 0)
                    return;

                var items = new List<ItemData>();
                foreach (ItemDrop.ItemData idd in __instance.GetInventory().GetAllItems())
                {
                    items.Add(new ItemData(idd));
                }
                SortUtils.SortByType(SortType.Value, items, sortAsc.Value);
                int entries = 0;
                int amount = 0;
                string name = "";
                for (int i = 0; i < items.Count; i++)
                {
                    if (maxEntries.Value >= 0 && entries >= maxEntries.Value)
                    {
                        if (overFlowText.Value.Length > 0)
                            __result += "\n" + overFlowText.Value;
                        break;
                    }
                    ItemData item = items[i];

                    if (item.m_shared.m_name == name || name == "")
                    {
                        amount += item.m_stack;
                    }
                    else
                    {
                        __result += "\n" + string.Format(entryString.Value, amount, Localization.instance.Localize(name));
                        entries++;

                        amount = item.m_stack;
                    }
                    name = item.m_shared.m_name;
                    if (i == items.Count - 1)
                    {
                        __result += "\n" + string.Format(entryString.Value, amount, Localization.instance.Localize(name));
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Console), "InputText")]
        static class InputText_Patch
        {
            static bool Prefix(Console __instance)
            {
                string text = __instance.m_input.text;
                if (text.ToLower().Equals($"{typeof(VMP_Modplugin).Namespace.ToLower()} reset"))
                {
                    context.Config.Reload();
                    context.Config.Save();

                    Traverse.Create(__instance).Method("AddString", new object[] { text }).GetValue();
                    Traverse.Create(__instance).Method("AddString", new object[] { $"{context.Info.Metadata.Name} config reloaded" }).GetValue();
                    return false;
                }
                return true;
            }
        }
    }
}
