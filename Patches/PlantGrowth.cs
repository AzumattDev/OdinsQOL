using System;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    internal class PlantGrowth
    {
        public static ConfigEntry<bool> DisplayGrowth;
        public static ConfigEntry<bool> PlantAnywhere;
        public static ConfigEntry<bool> IgnoreBiome;
        public static ConfigEntry<bool> IgnoreSun;
        public static ConfigEntry<bool> PreventPlantTooClose;
        public static ConfigEntry<bool> PreventDestroyIfCantGrow;
        public static ConfigEntry<float> GrowthTimeMultTree;
        public static ConfigEntry<float> GrowRadiusMultTree;
        public static ConfigEntry<float> MinScaleMultTree;
        public static ConfigEntry<float> MaxScaleMultTree;
        public static ConfigEntry<float> GrowthTimeMultPlant;
        public static ConfigEntry<float> GrowRadiusMultPlant;
        public static ConfigEntry<float> MinScaleMultPlant;
        public static ConfigEntry<float> MaxScaleMultPlant;

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
                }
                else
                {
                    try
                    {
                        if (array[i] != plant &&
                            Vector3.Distance(
                                plant.transform.GetComponent<CapsuleCollider>()
                                    .ClosestPoint(array[i].transform.position),
                                array[i].transform.GetComponent<Collider>().ClosestPoint(plant.transform.position)) <
                            plant.m_growRadius)
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
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
                if (___m_placementMarkerInstance != null && ___m_placementGhost?.GetComponent<Plant>() != null)
                    if (PreventPlantTooClose.Value)
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

        [HarmonyPatch(typeof(Piece), nameof(Piece.Awake))]
        private static class Piece_Awake_Patch
        {
            private static void Postfix(ref Piece __instance)
            {
                if (__instance.gameObject.GetComponent<Plant>() != null)
                    if (PlantAnywhere.Value)
                    {
                        __instance.m_cultivatedGroundOnly = false;
                        __instance.m_groundOnly = false;
                    }
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
                DateTime dateTime = new(__instance.m_nview.GetZDO().GetLong("plantTime", ZNet.instance.GetTime().Ticks));
                float growTime = __instance.GetGrowTime();
                if (timeSincePlanted <= growTime)
                    __result += "\n" + Utilities.TimeCalc(dateTime, growTime);
            }
        }
        
        [HarmonyPatch(typeof(Pickable),nameof(Pickable.GetHoverText))]
        static class Pickable_GetHoverText_Patch
        {
            static void Postfix(Pickable __instance, bool ___m_picked, ZNetView ___m_nview, int ___m_respawnTimeMinutes, ref string __result)
            {
                if (!DisplayGrowth.Value)
                    return;
                if (!___m_picked || ___m_nview.GetZDO() == null) return;
                if (__instance.name.ToLower().Contains("surt"))
                    return;
                float growthTime = ___m_respawnTimeMinutes * 60;
                DateTime pickedTime = new(___m_nview.GetZDO().GetLong("picked_time"));
                string timeString = Utilities.TimeCalc(pickedTime,growthTime);
                __result = Localization.instance.Localize(__instance.GetHoverName()) + $"\n{timeString}";
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
                    Heightmap.Biome biome = 0;
                    foreach (Heightmap.Biome b in Enum.GetValues(typeof(Heightmap.Biome))) biome |= b;

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
                if (IgnoreSun.Value)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.HaveGrowSpace))]
        private static class Plant_HaveGrowSpace_Patch
        {
            private static bool Prefix(Plant __instance, ref bool __result)
            {
                //Dbgl($"checking too close?");

                if (__instance.m_grownPrefabs[0].GetComponent<TreeBase>() && GrowRadiusMultTree.Value == 0 ||
                    !__instance.name.ToLower().Contains("tree") && GrowRadiusMultPlant.Value == 0)
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