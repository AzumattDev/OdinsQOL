using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches;

public class DisplayPinsOnMap
{
    private static readonly Dictionary<ZDO, Minimap.PinData> customPins = new();
    private static readonly Dictionary<int, Sprite> icons = new();
    private static readonly int CartHashcode = "Cart".GetStableHashCode();
    private static readonly int RaftHashcode = "Raft".GetStableHashCode();
    private static readonly int KarveHashcode = "Karve".GetStableHashCode();
    private static readonly int LongshipHashcode = "VikingShip".GetStableHashCode();
    private static readonly int hammerHashCode = "Hammer".GetStableHashCode();
    private static readonly float updateInterval = 5.0f;

    // clear dictionary if the user logs out
    [HarmonyPatch(typeof(Minimap), "OnDestroy")]
    public static class Minimap_OnDestroy_Patch
    {
        private static void Postfix()
        {
            customPins.Clear();
            icons.Clear();
        }
    }

    [HarmonyPatch(typeof(Minimap), "UpdateMap")]
    public static class Minimap_UpdateMap_Patch
    {
        private static float timeCounter = updateInterval;

        private static void FindIcons()
        {
            GameObject hammer = ObjectDB.instance.m_itemByHash[hammerHashCode];
            if (!hammer)
                return;
            ItemDrop hammerDrop = hammer.GetComponent<ItemDrop>();
            if (!hammerDrop)
                return;
            PieceTable hammerPieceTable = hammerDrop.m_itemData.m_shared.m_buildPieces;
            foreach (GameObject piece in hammerPieceTable.m_pieces)
            {
                Piece p = piece.GetComponent<Piece>();
                icons.Add(p.name.GetStableHashCode(), p.m_icon);
            }
        }

        private static bool CheckPin(Minimap __instance, Player player, ZDO zdo, int hashCode, string pinName)
        {
            if (zdo.m_prefab != hashCode)
                return false;

            Minimap.PinData customPin;
            bool pinWasFound = customPins.TryGetValue(zdo, out customPin);

            // turn off associated pin if player controlled ship is in that position
            Ship controlledShip = player.GetControlledShip();
            if (controlledShip && Vector3.Distance(controlledShip.transform.position, zdo.m_position) < 0.01f)
            {
                if (pinWasFound)
                {
                    __instance.RemovePin(customPin);
                    customPins.Remove(zdo);
                }

                return true;
            }

            if (!pinWasFound)
            {
                customPin = __instance.AddPin(zdo.m_position, Minimap.PinType.Death, pinName, false, false);

                Sprite sprite;
                if (icons.TryGetValue(hashCode, out sprite))
                    customPin.m_icon = sprite;

                customPin.m_doubleSize = true;
                customPins.Add(zdo, customPin);
            }
            else
            {
                customPin.m_pos = zdo.m_position;
            }

            return true;
        }

        public static void Postfix(ref Minimap __instance, Player player, float dt, bool takeInput)
        {
            timeCounter += dt;

            if (timeCounter < updateInterval || !MapDetail.MapDetailOn.Value ||
                !OdinQOLplugin.displayCartsAndBoats.Value)
                return;

            timeCounter -= updateInterval;

            if (icons.Count == 0)
                FindIcons();

            // search zones for ships and carts
            foreach (List<ZDO> zdoarray in ZDOMan.instance.m_objectsBySector)
                if (zdoarray != null)
                    foreach (ZDO zdo in zdoarray)
                    {
                        if (CheckPin(__instance, player, zdo, CartHashcode, "Cart"))
                            continue;
                        if (CheckPin(__instance, player, zdo, RaftHashcode, "Raft"))
                            continue;
                        if (CheckPin(__instance, player, zdo, KarveHashcode, "Karve"))
                            continue;
                        if (CheckPin(__instance, player, zdo, LongshipHashcode, "Longship"))
                            continue;
                    }

            // clear pins for destroyed objects
            foreach (KeyValuePair<ZDO, Minimap.PinData> pin in customPins.Where(pin => !pin.Key.IsValid()))
            {
                __instance.RemovePin(pin.Value);
                customPins.Remove(pin.Key);
            }
        }
    }
}