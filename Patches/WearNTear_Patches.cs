using System.Diagnostics;
using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches
{
    public class WearNTear_Patches
    {
        public static ConfigEntry<bool> NoWeatherDam;
        public static ConfigEntry<bool> DisableStructintegrity;
        public static ConfigEntry<bool> DisableBoatDamage;
        public static ConfigEntry<bool> NoPlayerStructDam;
        public static ConfigEntry<float> StructuralIntegritywood;
        public static ConfigEntry<float> StructuralIntegritystone;
        public static ConfigEntry<float> StructuralIntegrityiron;
        public static ConfigEntry<float> StructuralIntegrityhardWood;
    }


    [HarmonyPatch(typeof(WearNTear), "HaveRoof")]
    public static class RemoveWearNTear
    {
        private static void Postfix(ref bool __result)
        {
            if (WearNTear_Patches.NoWeatherDam.Value) __result = true;
        }
    }

    /// <summary>
    ///     Disable weather damage under water
    /// </summary>
    [HarmonyPatch(typeof(WearNTear), "IsUnderWater")]
    public static class RemoveWearNTearFromUnderWater
    {
        private static void Postfix(ref bool __result)
        {
            if (WearNTear_Patches.NoWeatherDam.Value) __result = false;
        }
    }

    /// <summary>
    ///     Removes the integrity check for having a connected piece to the ground.
    /// </summary>
    [HarmonyPatch(typeof(WearNTear), "HaveSupport")]
    public static class WearNTearHaveSupportPatch
    {
        private static void Postfix(ref bool __result)
        {
            if (WearNTear_Patches.DisableStructintegrity.Value) __result = true;
        }
    }

    /// <summary>
    ///     Disable damage to player structures
    /// </summary>
    [HarmonyPatch(typeof(WearNTear), "ApplyDamage")]
    public static class WearNTearApplyDamagePatch
    {
        private static bool Prefix(ref WearNTear __instance, ref float damage)
        {
            // Gets the name of the method calling the ApplyDamage method
            StackTrace? stackTrace = new StackTrace();
            string? callingMethod = stackTrace.GetFrame(2).GetMethod().Name;

            if (!(WearNTear_Patches.NoPlayerStructDam.Value && __instance.m_piece &&
                  __instance.m_piece.IsPlacedByPlayer() && callingMethod != "UpdateWear"))
                return true;

            if (__instance.m_piece.m_name.StartsWith("$ship"))
            {
                if (stackTrace.GetFrame(15).GetMethod().Name == "UpdateWaterForce") return false;

                return true;
            }

            return !WearNTear_Patches.DisableStructintegrity.Value;
        }
    }

    /// <summary>
    ///     Disable structural integrity
    /// </summary>
    [HarmonyPatch(typeof(WearNTear), "GetMaterialProperties")]
    public static class RemoveStructualIntegrity
    {
        private static bool Prefix(ref WearNTear __instance, out float maxSupport, out float minSupport,
            out float horizontalLoss, out float verticalLoss)
        {
            if (WearNTear_Patches.DisableStructintegrity.Value)
            {
                maxSupport = 1500f;
                minSupport = 20f;
                verticalLoss = 0f;
                horizontalLoss = 0f;
                return false;
            }

            // This handling is choosen because we subtract from a value thats reduced by distance from ground contact.
            WearNTear_Patches.StructuralIntegritywood.Value = WearNTear_Patches.StructuralIntegritywood.Value >= 100
                ? 100
                : WearNTear_Patches.StructuralIntegritywood.Value;
            WearNTear_Patches.StructuralIntegritystone.Value = WearNTear_Patches.StructuralIntegritywood.Value >= 100
                ? 100
                : WearNTear_Patches.StructuralIntegritystone.Value;
            WearNTear_Patches.StructuralIntegrityiron.Value = WearNTear_Patches.StructuralIntegritywood.Value >= 100
                ? 100
                : WearNTear_Patches.StructuralIntegrityiron.Value;
            WearNTear_Patches.StructuralIntegrityhardWood.Value = WearNTear_Patches.StructuralIntegritywood.Value >= 100
                ? 100
                : WearNTear_Patches.StructuralIntegrityhardWood.Value;

            switch (__instance.m_materialType)
            {
                case WearNTear.MaterialType.Wood:
                    maxSupport = 100f;
                    minSupport = 10f;
                    verticalLoss = 0.125f - 0.125f / 100 * WearNTear_Patches.StructuralIntegritywood.Value;
                    horizontalLoss = 0.2f - 0.125f / 100 * WearNTear_Patches.StructuralIntegritywood.Value;
                    return false;
                case WearNTear.MaterialType.Stone:
                    maxSupport = 1000f;
                    minSupport = 100f;
                    verticalLoss = 0.125f - 0.125f / 100 * WearNTear_Patches.StructuralIntegritystone.Value;
                    horizontalLoss = 1f - 1f / 100 * WearNTear_Patches.StructuralIntegritystone.Value;
                    return false;
                case WearNTear.MaterialType.Iron:
                    maxSupport = 1500f;
                    minSupport = 20f;
                    verticalLoss = 0.07692308f - 0.07692308f / 100 * WearNTear_Patches.StructuralIntegrityiron.Value;
                    horizontalLoss = 0.07692308f - 0.07692308f / 100 * WearNTear_Patches.StructuralIntegrityiron.Value;
                    return false;
                case WearNTear.MaterialType.HardWood:
                    maxSupport = 140f;
                    minSupport = 10f;
                    verticalLoss = 0.1f - 0.1f / 100 * WearNTear_Patches.StructuralIntegrityhardWood.Value;
                    horizontalLoss = 0.16666667f -
                                     0.16666667f / 100 * WearNTear_Patches.StructuralIntegrityhardWood.Value;
                    return false;
                default:
                    maxSupport = 0f;
                    minSupport = 0f;
                    verticalLoss = 0f;
                    horizontalLoss = 0f;
                    return false;
            }
        }
    }
}