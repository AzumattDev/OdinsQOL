using HarmonyLib;

namespace OdinQOL.Patches
{
    [HarmonyPatch(typeof(StationExtension), nameof(StationExtension.Awake))]
    public static class StationExtension_Awake_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float ___m_maxStationDistance)
        {
            if (CraftingPatch.AlterWorkBench.Value)
                ___m_maxStationDistance = OdinQOLplugin.workbenchAttachmentRange.Value;
        }
    }
}