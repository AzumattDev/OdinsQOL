using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace VMP_Mod.Patches
{
    class CraftingPatch
    {
        public static ConfigEntry<int> WorkbenchRange;
        public static ConfigEntry<int> workbenchEnemySpawnRange;
        public static ConfigEntry<bool> AlterWorkBench;

        /// <summary>
        /// Alter workbench range
        /// </summary>
        [HarmonyPatch(typeof(CraftingStation), "Start")]
        public static class WorkbenchRangeIncrease
        {
            private static void Prefix(ref CraftingStation __instance, ref float ___m_rangeBuild, GameObject ___m_areaMarker)
            {
                if (AlterWorkBench.Value && WorkbenchRange.Value > 0)
                {
                    try
                    {
                        ___m_rangeBuild = WorkbenchRange.Value;
                        ___m_areaMarker.GetComponent<CircleProjector>().m_radius = ___m_rangeBuild;
                        float scaleIncrease = (WorkbenchRange.Value - 20f) / 20f * 100f;
                        ___m_areaMarker.gameObject.transform.localScale = new Vector3(scaleIncrease / 100, 1f, scaleIncrease / 100);

                        // Apply this change to the child GameObject's EffectArea collision.
                        // Various other systems query this collision instead of the PrivateArea radius for permissions (notably, enemy spawning).
                        ResizeChildEffectArea(__instance, EffectArea.Type.PlayerBase, workbenchEnemySpawnRange.Value > 0 ? workbenchEnemySpawnRange.Value : WorkbenchRange.Value);
                    }
                    catch
                    {
                        // is not a workbench
                    }
                }
            }
        }
        public static void ResizeChildEffectArea(MonoBehaviour parent, EffectArea.Type includedTypes, float newRadius)
        {
            if (parent != null)
            {
                EffectArea effectArea = parent.GetComponentInChildren<EffectArea>();
                if (effectArea != null)
                {
                    if ((effectArea.m_type & includedTypes) != 0)
                    {
                        SphereCollider collision = effectArea.GetComponent<SphereCollider>();
                        if (collision != null)
                        {
                            collision.radius = newRadius;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Disable roof requirement on workbench
        /// </summary>
        [HarmonyPatch(typeof(CraftingStation), "CheckUsable")]
        public static class WorkbenchRemoveRestrictions
        {
            private static bool Prefix(ref CraftingStation __instance, ref Player player, ref bool showMessage, ref bool __result)
            {
                if (AlterWorkBench.Value)
                {
                    __instance.m_craftRequireRoof = false;
                }

                return true;
            }
        }
    }
}
