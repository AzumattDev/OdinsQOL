using System;
using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches;

internal class FermenterPatches
{
    public static ConfigEntry<bool> ShowFermenterStatus = null!;
    
    [HarmonyPatch(typeof(Fermenter), nameof(Fermenter.GetHoverText))]
    static class Fermenter__Patch
    {
        static void Postfix(Fermenter __instance, ref string __result)
        {
            if (!ShowFermenterStatus.Value) return;
            if (!__instance.m_nview.IsValid() || __instance.m_nview == null) return;
            if (__instance.GetStatus() != Fermenter.Status.Fermenting) return;
            DateTime startedFermenting = new(__instance.m_nview.GetZDO().GetLong("StartTime"));
            __result += Environment.NewLine +
                        Utilities.TimeCalc(startedFermenting, __instance.m_fermentationDuration);
        }
    }
}