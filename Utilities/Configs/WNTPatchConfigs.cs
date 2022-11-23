using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class WNTPatchConfigs
{
    internal static void Generate()
    {
        WearNTear_Patches.StructuralIntegrityControl = OdinQOLplugin.context.config("WearNTear_Patches",
            "Structural Integrity Control", false,
            "Set to true to enable the Structural Integrity settings in this mod.");
        WearNTear_Patches.NoWeatherDam = OdinQOLplugin.context.config("WearNTear_Patches",
            "No Weather Damage to buildings", false,
            "No Weather Damage to buildings");
        WearNTear_Patches.DisableStructintegrity = OdinQOLplugin.context.config("WearNTear_Patches",
            "Disable Structural Integrity system", false,
            "Disable Structural Integrity system. Allows for placement of things in the air, does not prevent building damage.");
        WearNTear_Patches.DisableBoatDamage =
            OdinQOLplugin.context.config("WearNTear_Patches", "Disable Boat Damage", false, "Disable Boat Damage");
        WearNTear_Patches.DisableBoatWaterDamage =
            OdinQOLplugin.context.config("WearNTear_Patches", "Disable Boat Water Damage", false,
                "Disable Boat Water Damage");
        WearNTear_Patches.NoPlayerStructDam = OdinQOLplugin.context.config("WearNTear_Patches",
            "No Damage to player buildings", false,
            "No Damage to player buildings");

        WearNTear_Patches.StructuralIntegritywood = OdinQOLplugin.context.config("WearNTear_Patches",
            "Wood Structural Integrity",
            0f,
            "Wood Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.");
        WearNTear_Patches.StructuralIntegritystone = OdinQOLplugin.context.config("WearNTear_Patches",
            "Stone Structural Integrity", 0f,
            "Stone Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.");
        WearNTear_Patches.StructuralIntegrityiron = OdinQOLplugin.context.config("WearNTear_Patches",
            "Iron Structural Integrity",
            0f,
            "Iron Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.");
        WearNTear_Patches.StructuralIntegrityhardWood = OdinQOLplugin.context.config("WearNTear_Patches",
            "Hardwood Structural Integrity", 0f,
            "Hardwood Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.");
        WearNTear_Patches.StructuralIntegrityMarble = OdinQOLplugin.context.config("WearNTear_Patches",
            "Marble Structural Integrity", 0f,
            "Hardwood Structural Integrity. Reduces the loss of structural integrity by distance by % less. The value 100 would result in disabled structural integrity over distance, does not allow for placement in free air without Disable Structural Integrity system.");
    }
}