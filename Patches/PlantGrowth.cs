using System;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    internal class PlantGrowth
    {
        public static ConfigEntry<bool> displayGrowth;
        public static ConfigEntry<bool> plantAnywhere;
        public static ConfigEntry<bool> ignoreBiome;
        public static ConfigEntry<bool> ignoreSun;
        public static ConfigEntry<bool> preventPlantTooClose;
        public static ConfigEntry<bool> preventDestroyIfCantGrow;
        public static ConfigEntry<float> growthTimeMultTree;
        public static ConfigEntry<float> growRadiusMultTree;
        public static ConfigEntry<float> minScaleMultTree;
        public static ConfigEntry<float> maxScaleMultTree;
        public static ConfigEntry<float> growthTimeMultPlant;
        public static ConfigEntry<float> growRadiusMultPlant;
        public static ConfigEntry<float> minScaleMultPlant;
        public static ConfigEntry<float> maxScaleMultPlant;

        private static bool HaveGrowSpace(Plant plant)
        {
            int spaceMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "piece_nonsolid");
            float plantMaxDist = plant.m_growRadius;
            if (!plant.m_grownPrefabs[0]
                .GetComponent<TreeBase>()) //Check if not tree as their m_grownPrefabs don't have colliders
                plantMaxDist = plantMaxDist + Math.Abs(plant.GetComponent<CapsuleCollider>().radius -
                                                       plant.m_grownPrefabs[0].GetComponent<CapsuleCollider>().radius);
            Collider[]? array = Physics.OverlapSphere(plant.transform.position, 2.5f, spaceMask);
            for (int i = 0; i < array.Length; i++)
                if (array[i].GetComponent<Plant>())
                {
                    Plant? collidingPlant = array[i].GetComponent<Plant>();
                    if (collidingPlant != plant)
                    {
                        float collidingPlantMaxDist = collidingPlant.m_growRadius;
                        if (!collidingPlant.m_grownPrefabs[0]
                            .GetComponent<TreeBase>()) //Check if not tree as their m_grownPrefabs don't have colliders
                            collidingPlantMaxDist = collidingPlantMaxDist +
                                                    Math.Abs(collidingPlant.GetComponent<CapsuleCollider>().radius -
                                                             collidingPlant.m_grownPrefabs[0]
                                                                 .GetComponent<CapsuleCollider>().radius);
                        if (collidingPlant && collidingPlant != plant && Vector3.Distance(
                                plant.transform.GetComponent<CapsuleCollider>()
                                    .ClosestPoint(collidingPlant.transform.position),
                                collidingPlant.transform.GetComponent<CapsuleCollider>()
                                    .ClosestPoint(plant.transform.position)) <
                            Math.Max(plantMaxDist, collidingPlantMaxDist))
                            return false;
                    }
                }
                else if (array[i] != plant &&
                         Vector3.Distance(
                             plant.transform.GetComponent<CapsuleCollider>().ClosestPoint(array[i].transform.position),
                             array[i].transform.GetComponent<Collider>().ClosestPoint(plant.transform.position)) <
                         plant.m_growRadius)
                {
                    return false;
                }


            if (plant.m_needCultivatedGround && !Heightmap.FindHeightmap(plant.transform.position)
                .IsCultivated(plant.transform.position))
                return false;
            return true;
        }

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        private static class Player_UpdatePlacementGhost_Patch
        {
            private static void Postfix(Player __instance, ref GameObject ___m_placementGhost,
                GameObject ___m_placementMarkerInstance)
            {
                if (___m_placementMarkerInstance != null && ___m_placementGhost?.GetComponent<Plant>() != null)
                    if (preventPlantTooClose.Value)
                    {
                        Plant? plant = ___m_placementGhost.GetComponent<Plant>();
                        if (!HaveGrowSpace(plant))
                        {
                            typeof(Player).GetField("m_placementStatus", BindingFlags.NonPublic | BindingFlags.Instance)
                                ?.SetValue(__instance, 5);
                            typeof(Player)
                                .GetMethod("SetPlacementGhostValid", BindingFlags.NonPublic | BindingFlags.Instance)
                                ?.Invoke(__instance, new object[] { false });
                        }
                    }
            }
        }

        [HarmonyPatch(typeof(Piece), "Awake")]
        private static class Piece_Awake_Patch
        {
            private static void Postfix(ref Piece __instance)
            {
                if (__instance.gameObject.GetComponent<Plant>() != null)
                    if (plantAnywhere.Value)
                    {
                        __instance.m_cultivatedGroundOnly = false;
                        __instance.m_groundOnly = false;
                    }
            }
        }


        [HarmonyPatch(typeof(Plant), "GetHoverText")]
        private static class Plant_GetHoverText_Patch
        {
            private static void Postfix(ref Plant __instance, ref string __result)
            {
                if (!displayGrowth.Value)
                    return;
                double timeSincePlanted = Traverse.Create(__instance).Method("TimeSincePlanted").GetValue<double>();
                float growTime = Traverse.Create(__instance).Method("GetGrowTime").GetValue<float>();
                if (timeSincePlanted < growTime)
                    __result += "\n" + Mathf.RoundToInt((float)timeSincePlanted) + "/" + Mathf.RoundToInt(growTime);
            }
        }


        [HarmonyPatch(typeof(Plant), "Awake")]
        private static class Plant_Awake_Patch
        {
            private static void Postfix(ref Plant __instance)
            {
                if (plantAnywhere.Value) __instance.m_needCultivatedGround = false;
                if (preventDestroyIfCantGrow.Value) __instance.m_destroyIfCantGrow = false;
                if (ignoreBiome.Value)
                {
                    Heightmap.Biome biome = 0;
                    foreach (Heightmap.Biome b in Enum.GetValues(typeof(Heightmap.Biome))) biome |= b;

                    __instance.m_biome = biome;
                }

                if (__instance.m_grownPrefabs[0].GetComponent<TreeBase>())
                {
                    __instance.m_growTime *= growthTimeMultTree.Value;
                    __instance.m_growTimeMax *= growthTimeMultTree.Value;
                    __instance.m_growRadius *= growRadiusMultTree.Value;
                    __instance.m_minScale *= minScaleMultTree.Value;
                    __instance.m_maxScale *= maxScaleMultTree.Value;
                }
                else
                {
                    __instance.m_growTime *= growthTimeMultPlant.Value;
                    __instance.m_growTimeMax *= growthTimeMultPlant.Value;
                    __instance.m_growRadius *= growRadiusMultPlant.Value;
                    __instance.m_minScale *= minScaleMultPlant.Value;
                    __instance.m_maxScale *= maxScaleMultPlant.Value;
                }
            }
        }


        [HarmonyPatch(typeof(Plant), "GetGrowTime")]
        private static class Plant_GetGrowTime_Patch
        {
            private static void Postfix(ref Plant __instance, ref float __result)
            {
                __result *= __instance.m_grownPrefabs[0].GetComponent<TreeBase>()
                    ? growthTimeMultTree.Value
                    : growthTimeMultPlant.Value;
            }
        }

        [HarmonyPatch(typeof(Plant), "HaveRoof")]
        private static class Plant_HaveRoof_Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (ignoreSun.Value)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Plant), "HaveGrowSpace")]
        private static class Plant_HaveGrowSpace_Patch
        {
            private static bool Prefix(Plant __instance, ref bool __result)
            {
                //Dbgl($"checking too close?");

                if (__instance.m_grownPrefabs[0].GetComponent<TreeBase>() && growRadiusMultTree.Value == 0 ||
                    !__instance.name.ToLower().Contains("tree") && growRadiusMultPlant.Value == 0)
                {
                    __result = true;
                    return false;
                }

                return true;
            }

            private static void Postfix(Plant __instance, ref bool __result)
            {
                if (!__result)
                {
                    int spaceMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece",
                        "piece_nonsolid");
                    Collider[]? array = Physics.OverlapSphere(__instance.transform.position, __instance.m_growRadius,
                        spaceMask);
                    for (int i = 0; i < array.Length; i++)
                    {
                        Plant? component = array[i].GetComponent<Plant>();
                        if (component && component != __instance)
                        {
                            //Dbgl($"{Vector3.Distance(__instance.transform.position, component.transform.position)} {component.m_growRadius} {__instance.m_growRadius}");
                        }
                    }
                }
                //Dbgl($"checking too close?");
            }
        }
    }
}