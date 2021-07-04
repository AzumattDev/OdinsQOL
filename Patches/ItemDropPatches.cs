using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace VMP_Mod.Patches
{
    class ItemDropPatches
    {
        public static ConfigEntry<int> WeightReduction;
        public static ConfigEntry<int> itemStackMultiplier;
        public static ConfigEntry<bool> NoTeleportPrevention;

        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
        public static class ItemDrop_Awake_Patch
        {
            private static void Prefix(ref ItemDrop __instance)
            {
                if (NoTeleportPrevention.Value)
                {
                    __instance.m_itemData.m_shared.m_teleportable = true;
                }

                if (itemStackMultiplier.Value > 0)
                {

                    __instance.m_itemData.m_shared.m_weight = WeightReduction.Value;

                    if (__instance.m_itemData.m_shared.m_maxStackSize > 1)
                    {
                        if (itemStackMultiplier.Value >= 1)
                        {
                            __instance.m_itemData.m_shared.m_maxStackSize = itemStackMultiplier.Value;
                        }
                    }
                }
            }
        }

    }
}
