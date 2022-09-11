using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class PlantGrowthConfigs
{
    internal static void Generate()
    {
        PlantGrowth.DisplayGrowth =
            OdinQOLplugin.context.config("PlantGrowth", "DisplayGrowth", true, "Display growth progress in hover text (applies to pickables as well)",
                false);
        PlantGrowth.PlantAnywhere = OdinQOLplugin.context.config("PlantGrowth", "PlantAnywhere", false,
            "Don't require cultivated ground to plant anything");
        PlantGrowth.IgnoreBiome =
            OdinQOLplugin.context.config("PlantGrowth", "IgnoreBiome", false, "Allow planting anything in any biome.");
        PlantGrowth.IgnoreSun =
            OdinQOLplugin.context.config("PlantGrowth", "IgnoreSun", false, "Allow planting under roofs.");
        PlantGrowth.PreventPlantTooClose = OdinQOLplugin.context.config("PlantGrowth", "PreventPlantTooClose", true,
            "Prevent plants from being planted if they are too close together to grow.");
        PlantGrowth.PreventDestroyIfCantGrow = OdinQOLplugin.context.config("PlantGrowth", "PreventDestroyIfCantGrow",
            false,
            "Prevent destruction of plants that normally are destroyed if they can't grow.");
        PlantGrowth.GrowthTimeMultTree = OdinQOLplugin.context.config("PlantGrowth", "GrowthTimeMultTree", 1f,
            "Multiply time taken to grow by this amount.");
        PlantGrowth.GrowRadiusMultTree = OdinQOLplugin.context.config("PlantGrowth", "GrowthRadiusMultTree", 1f,
            "Multiply required space to grow by this amount.");
        PlantGrowth.MinScaleMultTree = OdinQOLplugin.context.config("PlantGrowth", "MinScaleMultTree", 1f,
            "Multiply minimum size by this amount.");
        PlantGrowth.MaxScaleMultTree = OdinQOLplugin.context.config("PlantGrowth", "MaxScaleMultTree", 1f,
            "Multiply maximum size by this amount.");
        PlantGrowth.GrowthTimeMultPlant = OdinQOLplugin.context.config("PlantGrowth", "GrowthTimeMultPlant", 1f,
            "Multiply time taken to grow by this amount.");
        PlantGrowth.GrowRadiusMultPlant = OdinQOLplugin.context.config("PlantGrowth", "GrowthRadiusMultPlant", 1f,
            "Multiply required space to grow by this amount.");
        PlantGrowth.MinScaleMultPlant = OdinQOLplugin.context.config("PlantGrowth", "MinScaleMultPlant", 1f,
            "Multiply minimum size by this amount.");
        PlantGrowth.MaxScaleMultPlant = OdinQOLplugin.context.config("PlantGrowth", "MaxScaleMultPlant", 1f,
            "Multiply maximum size by this amount.");
    }
}