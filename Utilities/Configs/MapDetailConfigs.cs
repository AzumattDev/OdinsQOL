using OdinQOL.Patches;
using UnityEngine;

namespace OdinQOL.Configs;

public class MapDetailConfigs
{
    internal static void Generate()
    {
        MapDetail.MapDetailOn = OdinQOLplugin.context.config("Map Details", "MapDetail On", false,
            "Toggle this whole section off/on");
        MapDetail.DisplayCartsAndBoats =
            OdinQOLplugin.context.config("Map Details", "Display Boats/Carts", true, "Show Boats and carts on the map");
        // Custom boats string config
        MapDetail.CustomBoats = OdinQOLplugin.context.TextEntryConfig("Map Details", "Custom Boats", "CargoShip,Skuldelev,LittleBoat,MercantShip,BigCargoShip,FishingBoat,FishingCanoe,WarShip",
            "Custom boats to show on the map. Format: \"BoatName1,BoatName2,BoatName3\".");
        MapDetail.ShowRange = OdinQOLplugin.context.config("Map Details", "ShowRange", 50f,
            "Range in metres around player to show details");
        MapDetail.UpdateDelta = OdinQOLplugin.context.config("Map Details", "UpdateDelta", 5f,
            "Distance in metres to move before automatically updating the map details");
        MapDetail.ShowBuildings =
            OdinQOLplugin.context.config("Map Details", "ShowBuildings", true, "Show building pieces");
        MapDetail.PersonalBuildingColor = OdinQOLplugin.context.config("Map Details", "PersonalBuildingColor",
            Color.green,
            "Color of one's own build pieces", false);
        MapDetail.OtherBuildingColor = OdinQOLplugin.context.config("Map Details", "OtherBuildingColor", Color.red,
            "Color of other players' build pieces");
        MapDetail.UnownedBuildingColor = OdinQOLplugin.context.config("Map Details", "UnownedBuildingColor",
            Color.yellow,
            "Color of npc build pieces", false);
        MapDetail.CustomPlayerColors = OdinQOLplugin.context.config("Map Details", "CustomPlayerColors", "",
            "Custom color list, comma-separated. Use either <name>:<colorCode> pair entries or just <colorCode> entries. E.g. Erinthe:FF0000 or just FF0000. The latter will assign a color randomly to each connected peer.");
    }
}