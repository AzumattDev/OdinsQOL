using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class FermenterPatchConfigs
{
    internal static void Generate()
    {
        FermenterPatches.ShowFermenterStatus =
            OdinQOLplugin.context.config("Fermenter", "ShowFermenterStatus", true, "Display time left in fermentation process in hover text",
                false);
    }
}