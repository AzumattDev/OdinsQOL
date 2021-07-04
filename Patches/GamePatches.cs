using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace VMP_Mod.Patches
{
    class GamePatches
    {

        [HarmonyPatch(typeof(Game), nameof(Game.UpdateRespawn))]
        public static class Game_UpdateRespawn_Patch
        {
            private static void Prefix(ref Game __instance, float dt)
            {
                if (VMP_Modplugin.iHaveArrivedOnSpawn.Value)
                    __instance.m_firstSpawn = false;
            }
        }

        [HarmonyPatch]
        public static class AreaRepair
        {
            private static int m_repair_count;

            [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
            public static class Player_UpdatePlacement_Transpiler
            {
                private static MethodInfo method_Player_Repair = AccessTools.Method(typeof(Player), nameof(Player.Repair));
                private static AccessTools.FieldRef<Player, Piece> field_Player_m_hoveringPiece = AccessTools.FieldRefAccess<Player, Piece>(nameof(Player.m_hoveringPiece));
                private static MethodInfo method_RepairNearby = AccessTools.Method(typeof(Player_UpdatePlacement_Transpiler), nameof(Player_UpdatePlacement_Transpiler.RepairNearby));

                /// <summary>
                /// Patches the call to Repair from Player::UpdatePlacement with our own stub which handles repairing multiple pieces rather than just one.
                /// </summary>
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    if (!Configuration.Current.Building.IsEnabled) return instructions;

                    List<CodeInstruction> il = instructions.ToList();

                    if (Configuration.Current.Building.enableAreaRepair)
                    {
                        // Replace call to Player::Repair with our own stub.
                        // Our stub calls the original repair multiple times, one for each nearby piece.
                        for (int i = 0; i < il.Count; ++i)
                        {
                            if (il[i].Calls(method_Player_Repair))
                            {
                                il[i].operand = method_RepairNearby;
                            }
                        }
                    }

                    return il.AsEnumerable();
                }

                public static void RepairNearby(Player instance, ItemDrop.ItemData toolItem, Piece _1)
                {
                    Piece selected_piece = instance.GetHoveringPiece();
                    Vector3 position = selected_piece != null ? selected_piece.transform.position : instance.transform.position;

                    List<Piece> pieces = new List<Piece>();
                    Piece.GetAllPiecesInRadius(position, Configuration.Current.Building.areaRepairRadius, pieces);

                    m_repair_count = 0;

                    Piece original_piece = instance.m_hoveringPiece;

                    foreach (Piece piece in pieces)
                    {
                        bool has_stamina = instance.HaveStamina(toolItem.m_shared.m_attack.m_attackStamina);
                        bool uses_durability = toolItem.m_shared.m_useDurability;
                        bool has_durability = toolItem.m_durability > 0.0f;

                        if (!has_stamina || (uses_durability && !has_durability)) break;

                        // The repair function takes a piece to repair but then completely ignores it and repairs the hovering piece instead...
                        // I really don't like this, but Valheim's spaghetti code makes it required.
                        instance.m_hoveringPiece = piece;
                        instance.Repair(toolItem, _1);
                        instance.m_hoveringPiece = original_piece;
                    }

                    instance.Message(MessageHud.MessageType.TopLeft, string.Format("{0} pieces repaired", m_repair_count));
                }
            }

            [HarmonyPatch(typeof(Player), nameof(Player.Repair))]
            public static class Player_Repair_Transpiler
            {
                private static MethodInfo method_Character_Message = AccessTools.Method(typeof(Character), nameof(Character.Message));
                private static MethodInfo method_MessageNoop = AccessTools.Method(typeof(Player_Repair_Transpiler), nameof(Player_Repair_Transpiler.MessageNoop));

                /// <summary>
                /// Noops the original message notification when one piece is repaired, and counts them instead - the other transpiler
                /// will dispatch one notification for a batch of repairs using this count.
                /// </summary>
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    if (!Configuration.Current.Building.IsEnabled) return instructions;

                    List<CodeInstruction> il = instructions.ToList();

                    if (Configuration.Current.Building.enableAreaRepair)
                    {
                        // Replace calls to Character::Message with our own noop stub
                        // We don't want to spam messages for each piece so we patch the messages out here and dispatch our own messages in the other transpiler.
                        // First call pushes 1, then subsequent calls 0 - the first call is the branch where the repair succeeded.
                        int count = 0;
                        for (int i = 0; i < il.Count; ++i)
                        {
                            if (il[i].Calls(method_Character_Message))
                            {
                                il[i].operand = method_MessageNoop;
                                il.Insert(i++, new CodeInstruction(count++ == 0 ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0, null));
                            }
                        }
                    }

                    return il.AsEnumerable();
                }

                public static void MessageNoop(Character _0, MessageHud.MessageType _1, string _2, int _3, Sprite _4, int repaired)
                {
                    m_repair_count += repaired;
                }
            }
        }

        [HarmonyPatch(typeof(Player), "StartGuardianPower")]
        public static class Player_StartGuardianPower_Patch
        {
            private static bool Prefix(ref Player __instance, ref bool __result)
            {
                if (!Configuration.Current.Player.disableGuardianBuffAnimation || !Configuration.Current.Player.IsEnabled)
                    return true;

                if (__instance.m_guardianSE == null)
                {
                    __result = false;
                    return false;
                }
                if (__instance.m_guardianPowerCooldown > 0f)
                {
                    __instance.Message(MessageHud.MessageType.Center, "$hud_powernotready", 0, null);
                    __result = false;
                    return false;
                }
                __instance.ActivateGuardianPower();
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "HaveSeenTutorial")]
        public class Player_HaveSeenTutorial_Patch
        {
            [HarmonyPrefix]
            private static void Prefix(Player __instance, ref string name)
            {
                if (Configuration.Current.Player.IsEnabled && Configuration.Current.Player.skipTutorials)
                {
                    if (!__instance.m_shownTutorials.Contains(name))
                    {
                        __instance.m_shownTutorials.Add(name);
                    }
                }
            }
        }

        public static class UpdateEquipmentState
        {
            public static bool shouldReequipItemsAfterSwimming = false;
        }

        [HarmonyPatch(typeof(Humanoid), "UpdateEquipment")]
        public static class Humanoid_UpdateEquipment_Patch
        {
            private static bool Prefix(Humanoid __instance)
            {
                if (!Configuration.Current.Player.IsEnabled || !Configuration.Current.Player.reequipItemsAfterSwimming)
                    return true;

                if (__instance.IsPlayer() && __instance.IsSwiming() && !__instance.IsOnGround())
                {
                    // The above is only enough to know we will eventually exit swimming, but we still don't know if the items were visible prior or not.
                    // We only want to re-show them if they were shown to begin with, so we need to check.
                    // This is also why this must be a prefix patch; in a postfix patch, the items are already hidden, and we don't know
                    // if they were hidden by UpdateEquipment or by the user far earlier.

                    if (__instance.m_leftItem != null || __instance.m_rightItem != null)
                        UpdateEquipmentState.shouldReequipItemsAfterSwimming = true;
                }
                else if (__instance.IsPlayer() && !__instance.IsSwiming() && __instance.IsOnGround() && UpdateEquipmentState.shouldReequipItemsAfterSwimming)
                {
                    __instance.ShowHandItems();
                    UpdateEquipmentState.shouldReequipItemsAfterSwimming = false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SE_Stats), nameof(SE_Stats.Setup))]
        public static class SE_Stats_Setup_Patch
        {
            private static void Postfix(ref SE_Stats __instance)
            {
                if (Configuration.Current.Player.IsEnabled)
                    if (__instance.m_addMaxCarryWeight != null && __instance.m_addMaxCarryWeight > 0)
                        __instance.m_addMaxCarryWeight = (__instance.m_addMaxCarryWeight - 150) + Configuration.Current.Player.baseMegingjordBuff;
            }
        }

        [HarmonyPatch(typeof(Player), "Awake")]
        public static class Player_Awake_Patch
        {
            private static void Postfix(ref Player __instance)
            {
                if (Configuration.Current.Stamina.IsEnabled)
                {
                    __instance.m_dodgeStaminaUsage = Helper.applyModifierValue(__instance.m_dodgeStaminaUsage, Configuration.Current.Stamina.dodgeStaminaUsage);
                    __instance.m_encumberedStaminaDrain = Helper.applyModifierValue(__instance.m_encumberedStaminaDrain, Configuration.Current.Stamina.encumberedStaminaDrain);
                    __instance.m_sneakStaminaDrain = Helper.applyModifierValue(__instance.m_sneakStaminaDrain, Configuration.Current.Stamina.sneakStaminaDrain);
                    __instance.m_runStaminaDrain = Helper.applyModifierValue(__instance.m_runStaminaDrain, Configuration.Current.Stamina.runStaminaDrain);
                    __instance.m_staminaRegenDelay = Helper.applyModifierValue(__instance.m_staminaRegenDelay, Configuration.Current.Stamina.staminaRegenDelay);
                    __instance.m_staminaRegen = Helper.applyModifierValue(__instance.m_staminaRegen, Configuration.Current.Stamina.staminaRegen);
                    __instance.m_swimStaminaDrainMinSkill = Helper.applyModifierValue(__instance.m_swimStaminaDrainMinSkill, Configuration.Current.Stamina.swimStaminaDrain);
                    __instance.m_swimStaminaDrainMaxSkill = Helper.applyModifierValue(__instance.m_swimStaminaDrainMaxSkill, Configuration.Current.Stamina.swimStaminaDrain);
                    __instance.m_jumpStaminaUsage = Helper.applyModifierValue(__instance.m_jumpStaminaUsage, Configuration.Current.Stamina.jumpStaminaDrain);
                }
                if (Configuration.Current.Player.IsEnabled)
                {
                    __instance.m_autoPickupRange = Configuration.Current.Player.baseAutoPickUpRange;
                    __instance.m_baseCameraShake = Configuration.Current.Player.disableCameraShake ? 0f : 4f;
                    __instance.m_maxCarryWeight = Configuration.Current.Player.baseMaximumWeight;

                }
                if (Configuration.Current.Building.IsEnabled)
                {
                    __instance.m_maxPlaceDistance = Configuration.Current.Building.maximumPlacementDistance;
                }
            }
        }

        [HarmonyPatch(typeof(Beehive), "Awake")]
        public static class Beehive_Awake_Patch
        {
            private static bool Prefix(ref float ___m_secPerUnit, ref int ___m_maxHoney)
            {
                if (Configuration.Current.Beehive.IsEnabled)
                {
                    ___m_secPerUnit = Configuration.Current.Beehive.honeyProductionSpeed;
                    ___m_maxHoney = Configuration.Current.Beehive.maximumHoneyPerBeehive;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Hud), "DamageFlash")]
        public static class Hud_DamageFlash_Patch
        {
            private static void Postfix(Hud __instance)
            {
                 __instance.m_damageScreen.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(UnityEngine.EventSystems.EventSystem), "OnApplicationFocus")]
        public static class EventSystem_OnApplicationFocus_Patch
        {
            private static void Postfix(bool hasFocus)
            {
                if (PlayerPrefs.GetInt("MuteGameInBackground", 0) == 1)
                {
                    if (hasFocus)
                        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                    else
                        AudioListener.volume = 0f;
                }
            }
        }

    }
}
