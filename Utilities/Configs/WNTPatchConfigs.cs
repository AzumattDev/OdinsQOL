using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class WNTPatchConfigs
{
    internal static void Generate()
    {
        WearNTear_Patches.NoWeatherDam = OdinQOLplugin.context.config("WearNTear_Patches",
            "No Weather Damage to buildings", false,
            "No Weather Damage to buildings");
        WearNTear_Patches.DisableStructintegrity = OdinQOLplugin.context.config("WearNTear_Patches",
            "Disable Structural Integrity system", false, "Disable Structural Integrity system");
        WearNTear_Patches.DisableBoatDamage =
            OdinQOLplugin.context.config("WearNTear_Patches", "Disable Boat Damage", false, "Disable Boat Damage");
        WearNTear_Patches.NoPlayerStructDam = OdinQOLplugin.context.config("WearNTear_Patches",
            "No Damage to player buildings", false,
            "No Damage to player buildings");

        WearNTear_Patches.StructuralIntegritywood = OdinQOLplugin.context.config("WearNTear_Patches",
            "Wood Structural Integrity",
            1f, "Wood Structural Integrity");
        WearNTear_Patches.StructuralIntegritystone = OdinQOLplugin.context.config("WearNTear_Patches",
            "Stone Structural Integrity", 1f, "Stone Structural Integrity");
        WearNTear_Patches.StructuralIntegrityiron = OdinQOLplugin.context.config("WearNTear_Patches",
            "Iron Structural Integrity",
            1f, "Iron Structural Integrity");
        WearNTear_Patches.StructuralIntegrityhardWood = OdinQOLplugin.context.config("WearNTear_Patches",
            "Hardwood Structural Integrity", 1f, "Hardwood Structural Integrity");
    }
}