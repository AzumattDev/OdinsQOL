using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches;

public class DisplayPinsOnMap
{
    private static readonly Dictionary<ZDO, Minimap.PinData> CustomPins = new();
    private static readonly Dictionary<int, Sprite> Icons = new();
    private static List<string> PrefabList = new();
    private static readonly int CartHashcode = "Cart".GetStableHashCode();
    private static readonly int RaftHashcode = "Raft".GetStableHashCode();
    private static readonly int KarveHashcode = "Karve".GetStableHashCode();
    private static readonly int LongshipHashcode = "VikingShip".GetStableHashCode();
    private static readonly int HammerHashCode = "Hammer".GetStableHashCode();
    private const float UpdateInterval = 5.0f;

    // clear dictionary if the user logs out
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnDestroy))]
    public static class Minimap_OnDestroy_Patch
    {
        private static void Postfix()
        {
            CustomPins.Clear();
            Icons.Clear();
            PrefabList.Clear();
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateMap))]
    public static class Minimap_UpdateMap_Patch
    {
        private static float _timeCounter = UpdateInterval;

        private static void FindIcons()
        {
            GameObject hammer = ObjectDB.instance.m_itemByHash[HammerHashCode];
            if (!hammer)
                return;
            ItemDrop hammerDrop = hammer.GetComponent<ItemDrop>();
            if (!hammerDrop)
                return;
            PieceTable hammerPieceTable = hammerDrop.m_itemData.m_shared.m_buildPieces;
            foreach (Piece p in hammerPieceTable.m_pieces.Select(piece => piece.GetComponent<Piece>()))
            {
                Icons.Add(p.name.GetStableHashCode(), p.m_icon);
            }
        }

        private static bool CheckPin(Minimap __instance, Player player, ZDO zdo, int hashCode, string pinName)
        {
            if (zdo.m_prefab != hashCode)
                return false;

            Minimap.PinData customPin;
            bool pinWasFound = CustomPins.TryGetValue(zdo, out customPin);

            // turn off associated pin if player controlled ship is in that position
            Ship controlledShip = player.GetControlledShip();
            if (controlledShip && Vector3.Distance(controlledShip.transform.position, zdo.m_position) < 0.01f)
            {
                if (!pinWasFound) return true;
                __instance.RemovePin(customPin);
                CustomPins.Remove(zdo);

                return true;
            }

            if (!pinWasFound)
            {
                customPin = __instance.AddPin(zdo.m_position, Minimap.PinType.Death, pinName, false, false);

                if (Icons.TryGetValue(hashCode, out Sprite sprite))
                    customPin.m_icon = sprite;

                customPin.m_doubleSize = true;
                CustomPins.Add(zdo, customPin);
            }
            else
            {
                if (customPin != null) customPin.m_pos = zdo.m_position;
            }

            return true;
        }

        public static void Postfix(ref Minimap __instance, Player player, float dt, bool takeInput)
        {
            _timeCounter += dt;

            if (_timeCounter < UpdateInterval || !MapDetail.MapDetailOn.Value ||
                !MapDetail.DisplayCartsAndBoats.Value)
                return;
            if (!string.IsNullOrWhiteSpace(MapDetail.CustomBoats.Value) && MapDetail.CustomBoats.Value.Length > 0)
            {
                PrefabList = MapDetail.CustomBoats.Value.Trim().Split(',')
                    .Select(s => s.Trim()).ToList();
            }
            else
            {
                PrefabList.Clear();
            }

            _timeCounter -= UpdateInterval;

            if (Icons.Count == 0)
                FindIcons();

            // search zones for ships and carts (use for loop to avoid enumerator errors. Foreach makes this immutable and it complains.)
            for (int i1 = 0; i1 < ZDOMan.instance.m_objectsBySector.Length; i1++)
            {
                List<ZDO> zdoarray = ZDOMan.instance.m_objectsBySector[i1];
                if (zdoarray != null)
                    for (int i = 0; i < zdoarray.Count; i++)
                    {
                        ZDO zdo = zdoarray[i];
                        if (CheckPin(__instance, player, zdo, CartHashcode, "Cart"))
                            continue;
                        if (CheckPin(__instance, player, zdo, RaftHashcode, "Raft"))
                            continue;
                        if (CheckPin(__instance, player, zdo, KarveHashcode, "Karve"))
                            continue;
                        if (CheckPin(__instance, player, zdo, LongshipHashcode, "Longship"))
                            continue;
                        for (int index = 0; index < PrefabList.Count; index++)
                        {
                            string prefab = PrefabList[index];
                            if (CheckPin(__instance, player, zdo, prefab.GetStableHashCode(), prefab))
                                break;
                        }
                    }
            }

            // clear pins for destroyed objects
            foreach (KeyValuePair<ZDO, Minimap.PinData> pin in CustomPins.Where(pin => !pin.Key.IsValid()))
            {
                __instance.RemovePin(pin.Value);
                CustomPins.Remove(pin.Key);
            }
        }
    }
}