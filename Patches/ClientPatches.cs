using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OdinQOL.Patches
{
    public class ClientPatches
    {

        private static bool _overridePlayerName;

        [HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
        static class Chat_SendText_Patch
        {
            static void Prefix(Chat __instance, Talker.Type type, string text)
            {
                _overridePlayerName = true;
            }

            static void Postfix(Chat __instance, Talker.Type type, string text)
            {
                _overridePlayerName = false;
            }
        }

        [HarmonyPatch(typeof(Chat), nameof(Chat.SendPing))]
        static class Chat_SendPing_Patch
        {
            static void Prefix(Chat __instance, Vector3 position)
            {
                _overridePlayerName = true;
            }

            static void Postfix(Chat __instance, Vector3 position)
            {
                _overridePlayerName = false;
            }
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.Awake))]
        public static class shipfix
        {
            private static void Postfix(ref Ship __instance)
            {
                __instance.m_minWaterImpactForce = 100f;
            }
        }

        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Interact))]
        private class StandFix
        {
            private static bool Prefix(Humanoid user, bool hold, ItemStand __instance)
            {
                if (PrivateArea.CheckAccess(__instance.transform.position)) return true;
                return false;
            }
        }
    }
}