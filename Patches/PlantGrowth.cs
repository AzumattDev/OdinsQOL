using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    internal static class PlantGrowth
    {
        public static ConfigEntry<bool> DisplayGrowth = null!;
        public static ConfigEntry<bool> PlantAnywhere = null!;
        public static ConfigEntry<bool> IgnoreBiome = null!;
        public static ConfigEntry<bool> IgnoreSun = null!;
        public static ConfigEntry<bool> PreventPlantTooClose = null!;
        public static ConfigEntry<bool> PreventDestroyIfCantGrow = null!;
        public static ConfigEntry<float> GrowthTimeMultTree = null!;
        public static ConfigEntry<float> GrowRadiusMultTree = null!;
        public static ConfigEntry<float> MinScaleMultTree = null!;
        public static ConfigEntry<float> MaxScaleMultTree = null!;
        public static ConfigEntry<float> GrowthTimeMultPlant = null!;
        public static ConfigEntry<float> GrowRadiusMultPlant = null!;
        public static ConfigEntry<float> MinScaleMultPlant = null!;
        public static ConfigEntry<float> MaxScaleMultPlant = null!;

        private static bool HaveGrowSpace(Plant plant)
        {
            int spaceMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "piece_nonsolid");
            float plantMaxDist = plant.m_growRadius;
            if (!plant.m_grownPrefabs[0]
                    .GetComponent<TreeBase>()) //Check if not tree as their m_grownPrefabs don't have colliders
                plantMaxDist = plantMaxDist + Math.Abs(plant.GetComponent<CapsuleCollider>().radius -
                                                       plant.m_grownPrefabs[0].GetComponent<CapsuleCollider>().radius);
            Collider[]? array = Physics.OverlapSphere(plant.transform.position, 2.5f, spaceMask);
            foreach (Collider t in array)
                if (t.GetComponent<Plant>())
                {
                    Plant? collidingPlant = t.GetComponent<Plant>();
                    if (collidingPlant == plant) continue;
                    float collidingPlantMaxDist = collidingPlant.m_growRadius;
                    if (!collidingPlant.m_grownPrefabs[0]
                            .GetComponent<
                                TreeBase>()) //Check if not tree as their m_grownPrefabs don't have colliders
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
                else if (t != plant &&
                         Vector3.Distance(
                             plant.transform.GetComponent<CapsuleCollider>().ClosestPoint(t.transform.position),
                             t.transform.GetComponent<Collider>().ClosestPoint(plant.transform.position)) <
                         plant.m_growRadius)
                {
                    return false;
                }


            if (plant.m_needCultivatedGround && !Heightmap.FindHeightmap(plant.transform.position)
                    .IsCultivated(plant.transform.position))
                return false;
            return true;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
        private static class Player_UpdatePlacementGhost_Patch
        {
            private static void Postfix(Player __instance, ref GameObject ___m_placementGhost,
                GameObject ___m_placementMarkerInstance)
            {
                if (___m_placementMarkerInstance == null || ___m_placementGhost?.GetComponent<Plant>() == null) return;
                if (!PreventPlantTooClose.Value) return;
                Plant? plant = ___m_placementGhost.GetComponent<Plant>();
                if (HaveGrowSpace(plant)) return;
                __instance.m_placementStatus = Player.PlacementStatus.MoreSpace;
                __instance.SetPlacementGhostValid(false);
            }
        }

        [HarmonyPatch(typeof(Piece), nameof(Piece.Awake))]
        private static class Piece_Awake_Patch
        {
            private static void Postfix(ref Piece __instance)
            {
                if (__instance.gameObject.GetComponent<Plant>() == null) return;
                if (!PlantAnywhere.Value) return;
                __instance.m_cultivatedGroundOnly = false;
                __instance.m_groundOnly = false;
            }
        }


        [HarmonyPatch(typeof(Plant), nameof(Plant.GetHoverText))]
        private static class Plant_GetHoverText_Patch
        {
            private static void Postfix(ref Plant __instance, ref string __result)
            {
                if (!DisplayGrowth.Value)
                    return;
                double timeSincePlanted = __instance.TimeSincePlanted();
                float growTime = __instance.GetGrowTime();
                if (timeSincePlanted < growTime)
                    __result += "\n" + Mathf.RoundToInt((float)timeSincePlanted) + "/" + Mathf.RoundToInt(growTime);
            }
        }


        [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
        private static class Plant_Awake_Patch
        {
            private static void Postfix(ref Plant __instance)
            {
                if (PlantAnywhere.Value) __instance.m_needCultivatedGround = false;
                if (PreventDestroyIfCantGrow.Value) __instance.m_destroyIfCantGrow = false;
                if (IgnoreBiome.Value)
                {
                    Heightmap.Biome biome = Enum.GetValues(typeof(Heightmap.Biome)).Cast<Heightmap.Biome>()
                        .Aggregate<Heightmap.Biome, Heightmap.Biome>(0, (current, b) => current | b);

                    __instance.m_biome = biome;
                }

                if (__instance.m_grownPrefabs[0].GetComponent<TreeBase>())
                {
                    __instance.m_growTime *= GrowthTimeMultTree.Value;
                    __instance.m_growTimeMax *= GrowthTimeMultTree.Value;
                    __instance.m_growRadius *= GrowRadiusMultTree.Value;
                    __instance.m_minScale *= MinScaleMultTree.Value;
                    __instance.m_maxScale *= MaxScaleMultTree.Value;
                }
                else
                {
                    __instance.m_growTime *= GrowthTimeMultPlant.Value;
                    __instance.m_growTimeMax *= GrowthTimeMultPlant.Value;
                    __instance.m_growRadius *= GrowRadiusMultPlant.Value;
                    __instance.m_minScale *= MinScaleMultPlant.Value;
                    __instance.m_maxScale *= MaxScaleMultPlant.Value;
                }
            }
        }


        [HarmonyPatch(typeof(Plant), nameof(Plant.GetGrowTime))]
        private static class Plant_GetGrowTime_Patch
        {
            private static void Postfix(ref Plant __instance, ref float __result)
            {
                __result *= __instance.m_grownPrefabs[0].GetComponent<TreeBase>()
                    ? GrowthTimeMultTree.Value
                    : GrowthTimeMultPlant.Value;
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.HaveRoof))]
        private static class Plant_HaveRoof_Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (!IgnoreSun.Value) return true;
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.HaveGrowSpace))]
        private static class Plant_HaveGrowSpace_Patch
        {
            private static bool Prefix(Plant __instance, ref bool __result)
            {
                if ((!__instance.m_grownPrefabs[0].GetComponent<TreeBase>() || GrowRadiusMultTree.Value != 0) &&
                    (__instance.name.ToLower().Contains("tree") || GrowRadiusMultPlant.Value != 0)) return true;
                __result = true;
                return false;
            }

            private static void Postfix(Plant __instance, ref bool __result)
            {
                if (__result) return;
                int spaceMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece",
                    "piece_nonsolid");
                Collider[]? array = Physics.OverlapSphere(__instance.transform.position, __instance.m_growRadius,
                    spaceMask);
                foreach (Collider t in array)
                {
                    Plant? component = t.GetComponent<Plant>();
                    if (component && component != __instance)
                    {
                        //OdinQOLplugin.QOLLogger.LogDebug($"{Vector3.Distance(__instance.transform.position, component.transform.position)} {component.m_growRadius} {__instance.m_growRadius}");
                    }
                }
            }
        }
    }
}