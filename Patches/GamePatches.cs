using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OdinQOL.Patches
{
    internal class GamePatches
    {
        public static ConfigEntry<bool> DisableGuardianAnimation;
        public static ConfigEntry<bool> SkipTuts;
        public static ConfigEntry<bool> reequipItemsAfterSwimming;
        public static ConfigEntry<bool> enableAreaRepair;
        public static ConfigEntry<bool> StaminaIsEnabled;
        public static ConfigEntry<int> areaRepairRadius;
        public static ConfigEntry<int> baseMegingjordBuff;
        public static ConfigEntry<int> honeyProductionSpeed;
        public static ConfigEntry<int> maximumHoneyPerBeehive;

        public static ConfigEntry<bool> buildInsideProtectedLocations;
        public static ConfigEntry<float> craftingDuration;
        public static ConfigEntry<float> dodgeStaminaUsage;
        public static ConfigEntry<float> encumberedStaminaDrain;
        public static ConfigEntry<float> sneakStaminaDrain;
        public static ConfigEntry<float> runStaminaDrain;
        public static ConfigEntry<float> staminaRegenDelay;
        public static ConfigEntry<float> staminaRegen;
        public static ConfigEntry<float> swimStaminaDrain;
        public static ConfigEntry<float> jumpStaminaDrain;
        public static ConfigEntry<float> baseAutoPickUpRange;
        public static ConfigEntry<float> disableCameraShake;
        public static ConfigEntry<float> baseMaximumWeight;
        public static ConfigEntry<float> maximumPlacementDistance;
        public static ConfigEntry<int> maxPlayers;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Location), "IsInsideNoBuildLocation")]
        private static bool Placement_Patch_NoBuild(ref bool __result)
        {
            if (!OdinQOLplugin.modEnabled.Value) return true;
            if (!buildInsideProtectedLocations.Value) return true;
            __result = false;
            return false;
        }


        /*[HarmonyPatch(typeof(Game), nameof(Game.UpdateRespawn))]
        public static class Game_UpdateRespawn_Patch
        {
            private static void Prefix(ref Game __instance, float dt)
            {
                if (OdinQOLplugin.iHaveArrivedOnSpawn.Value)
                    __instance.m_firstSpawn = false;
            }
        }*/

        [HarmonyPatch(typeof(Chat), nameof(Chat.OnNewChatMessage))]
        public static class ChatOnNewChatMessage_Patch
        {
            private static bool Prefix(string user, string text)
            {
                return !OdinQOLplugin.iHaveArrivedOnSpawn.Value || !text.ToLower().Contains("i have arrived");
            }
        }

        [HarmonyPatch]
        public static class AreaRepair
        {
            private static int m_repair_count;

            [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
            public static class Player_UpdatePlacement_Transpiler
            {
                private static readonly MethodInfo method_Player_Repair =
                    AccessTools.Method(typeof(Player), nameof(Player.Repair));

                private static AccessTools.FieldRef<Player, Piece> field_Player_m_hoveringPiece =
                    AccessTools.FieldRefAccess<Player, Piece>(nameof(Player.m_hoveringPiece));

                private static readonly MethodInfo method_RepairNearby =
                    AccessTools.Method(typeof(Player_UpdatePlacement_Transpiler), nameof(RepairNearby));

                /// <summary>
                ///     Patches the call to Repair from Player::UpdatePlacement with our own stub which handles repairing multiple pieces
                ///     rather than just one.
                /// </summary>
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction>? il = instructions.ToList();

                    if (enableAreaRepair.Value)
                        // Replace call to Player::Repair with our own stub.
                        // Our stub calls the original repair multiple times, one for each nearby piece.
                        for (int i = 0; i < il.Count; ++i)
                            if (il[i].Calls(method_Player_Repair))
                                il[i].operand = method_RepairNearby;

                    return il.AsEnumerable();
                }

                public static void RepairNearby(Player instance, ItemDrop.ItemData toolItem, Piece _1)
                {
                    Piece? selected_piece = instance.GetHoveringPiece();
                    Vector3 position = selected_piece != null
                        ? selected_piece.transform.position
                        : instance.transform.position;

                    List<Piece>? pieces = new List<Piece>();
                    Piece.GetAllPiecesInRadius(position, areaRepairRadius.Value, pieces);

                    m_repair_count = 0;

                    Piece? original_piece = instance.m_hoveringPiece;

                    foreach (Piece? piece in pieces)
                    {
                        bool has_stamina = instance.HaveStamina(toolItem.m_shared.m_attack.m_attackStamina);
                        bool uses_durability = toolItem.m_shared.m_useDurability;
                        bool has_durability = toolItem.m_durability > 0.0f;

                        if (!has_stamina || uses_durability && !has_durability) break;

                        // The repair function takes a piece to repair but then completely ignores it and repairs the hovering piece instead...
                        // I really don't like this, but Valheim's spaghetti code makes it required.
                        instance.m_hoveringPiece = piece;
                        instance.Repair(toolItem, _1);
                        instance.m_hoveringPiece = original_piece;
                    }

                    instance.Message(MessageHud.MessageType.TopLeft,
                        $"{m_repair_count} pieces repaired");
                }
            }

            [HarmonyPatch(typeof(Player), nameof(Player.Repair))]
            public static class Player_Repair_Transpiler
            {
                private static readonly MethodInfo method_Character_Message =
                    AccessTools.Method(typeof(Character), nameof(Character.Message));

                private static readonly MethodInfo method_MessageNoop =
                    AccessTools.Method(typeof(Player_Repair_Transpiler), nameof(MessageNoop));

                /// <summary>
                ///     Noops the original message notification when one piece is repaired, and counts them instead - the other transpiler
                ///     will dispatch one notification for a batch of repairs using this count.
                /// </summary>
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction>? il = instructions.ToList();

                    if (enableAreaRepair.Value)
                    {
                        // Replace calls to Character::Message with our own noop stub
                        // We don't want to spam messages for each piece so we patch the messages out here and dispatch our own messages in the other transpiler.
                        // First call pushes 1, then subsequent calls 0 - the first call is the branch where the repair succeeded.
                        int count = 0;
                        for (int i = 0; i < il.Count; ++i)
                            if (il[i].Calls(method_Character_Message))
                            {
                                il[i].operand = method_MessageNoop;
                                il.Insert(i++, new CodeInstruction(count++ == 0 ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                            }
                    }

                    return il.AsEnumerable();
                }

                public static void MessageNoop(Character _0, MessageHud.MessageType _1, string _2, int _3, Sprite _4,
                    int repaired)
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
                if (!DisableGuardianAnimation.Value)
                    return true;

                if (__instance.m_guardianSE == null)
                {
                    __result = false;
                    return false;
                }

                if (__instance.m_guardianPowerCooldown > 0f)
                {
                    __instance.Message(MessageHud.MessageType.Center, "$hud_powernotready");
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
                if (SkipTuts.Value)
                    if (!__instance.m_shownTutorials.Contains(name))
                        __instance.m_shownTutorials.Add(name);
            }
        }

        public static class UpdateEquipmentState
        {
            public static bool shouldReequipItemsAfterSwimming;
        }

        [HarmonyPatch(typeof(Humanoid), "UpdateEquipment")]
        public static class Humanoid_UpdateEquipment_Patch
        {
            private static bool Prefix(Humanoid __instance)
            {
                if (!reequipItemsAfterSwimming.Value)
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
                else if (__instance.IsPlayer() && !__instance.IsSwiming() && __instance.IsOnGround() &&
                         UpdateEquipmentState.shouldReequipItemsAfterSwimming)
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
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                if (__instance.m_addMaxCarryWeight != null && __instance.m_addMaxCarryWeight > 0)
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                    __instance.m_addMaxCarryWeight = __instance.m_addMaxCarryWeight - 150 + baseMegingjordBuff.Value;
            }
        }

        [HarmonyPatch(typeof(Player), "Awake")]
        public static class Player_Awake_Patch
        {
            private static void Postfix(ref Player __instance)
            {
                if (StaminaIsEnabled.Value)
                {
                    __instance.m_dodgeStaminaUsage = dodgeStaminaUsage.Value;
                    __instance.m_encumberedStaminaDrain = encumberedStaminaDrain.Value;
                    __instance.m_sneakStaminaDrain = sneakStaminaDrain.Value;
                    __instance.m_runStaminaDrain = runStaminaDrain.Value;
                    __instance.m_staminaRegenDelay = staminaRegenDelay.Value;
                    __instance.m_staminaRegen = staminaRegen.Value;
                    __instance.m_swimStaminaDrainMinSkill = swimStaminaDrain.Value;
                    __instance.m_swimStaminaDrainMaxSkill = swimStaminaDrain.Value;
                    __instance.m_jumpStaminaUsage = jumpStaminaDrain.Value;
                }

                __instance.m_autoPickupRange = baseAutoPickUpRange.Value;
                __instance.m_baseCameraShake = disableCameraShake.Value;
                __instance.m_maxCarryWeight = baseMaximumWeight.Value;
                __instance.m_maxPlaceDistance = maximumPlacementDistance.Value;
            }
        }

        [HarmonyPatch(typeof(Beehive), "Awake")]
        public static class Beehive_Awake_Patch
        {
            private static bool Prefix(ref float ___m_secPerUnit, ref int ___m_maxHoney)
            {
                ___m_secPerUnit = honeyProductionSpeed.Value;
                ___m_maxHoney = maximumHoneyPerBeehive.Value;


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

        [HarmonyPatch(typeof(TeleportWorld), "GetHoverText")]
        public static class TeleportWorld_bigPortalText_Patch
        {
            private static void Postfix(TeleportWorld __instance, string __result)
            {
                string? portalName = __instance.GetText();


                __result = Localization.instance.Localize(string.Concat("$piece_portal $piece_portal_tag:", " ", "[",
                    portalName, "]"));

                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, __result);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
        public static class Player_UpdateFood_Transpiler
        {
            private static readonly FieldInfo field_Player_m_foodUpdateTimer =
                AccessTools.Field(typeof(Player), nameof(Player.m_foodUpdateTimer));

            private static readonly MethodInfo method_ComputeModifiedDt =
                AccessTools.Method(typeof(Player_UpdateFood_Transpiler), nameof(ComputeModifiedDT));

            /// <summary>
            ///     Replaces the first load of dt inside Player::UpdateFood with a modified dt that is scaled
            ///     by the food duration scaling multiplier. This ensures the food lasts longer while maintaining
            ///     the same rate of regeneration.
            /// </summary>
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction>? il = instructions.ToList();

                for (int i = 0; i < il.Count - 2; ++i)
                    if (il[i].LoadsField(field_Player_m_foodUpdateTimer) &&
                        il[i + 1].opcode == OpCodes.Ldarg_1 /* dt */ &&
                        il[i + 2].opcode == OpCodes.Add)
                        // We insert after Ldarg_1 (push dt) a call to our function, which computes the modified DT and returns it.
                        il.Insert(i + 2, new CodeInstruction(OpCodes.Call, method_ComputeModifiedDt));

                return il.AsEnumerable();
            }

            private static float ComputeModifiedDT(float dt)
            {
                return dt / Utilities.ApplyModifierValue(1.0f, 10f);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
        public static class Player_GetTotalFoodValue_Transpiler
        {
            private static readonly FieldInfo field_Food_m_health =
                AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_health));

            private static readonly FieldInfo field_Food_m_stamina =
                AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_stamina));

            private static readonly FieldInfo field_Food_m_item =
                AccessTools.Field(typeof(Player.Food), nameof(Player.Food.m_item));

            private static readonly FieldInfo field_ItemData_m_shared =
                AccessTools.Field(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.m_shared));

            private static readonly FieldInfo field_SharedData_m_food =
                AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), nameof(ItemDrop.ItemData.SharedData.m_food));

            private static readonly FieldInfo field_SharedData_m_foodStamina =
                AccessTools.Field(typeof(ItemDrop.ItemData.SharedData),
                    nameof(ItemDrop.ItemData.SharedData.m_foodStamina));

            /// <summary>
            ///     Replaces loads to the current health/stamina for food with loads to the original health/stamina for food
            ///     inside Player::GetTotalFoodValue. This disables food degradation.
            /// </summary>
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction>? il = instructions.ToList();

                for (int i = 0; i < il.Count; ++i)
                {
                    bool loads_health = il[i].LoadsField(field_Food_m_health);
                    bool loads_stamina = il[i].LoadsField(field_Food_m_stamina);

                    if (loads_health || loads_stamina)
                    {
                        il[i].operand = field_Food_m_item;
                        il.Insert(++i, new CodeInstruction(OpCodes.Ldfld, field_ItemData_m_shared));
                        il.Insert(++i,
                            new CodeInstruction(OpCodes.Ldfld,
                                loads_health ? field_SharedData_m_food : field_SharedData_m_foodStamina));
                    }
                }


                return il.AsEnumerable();
            }
        }


        [HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
        public static class Player_RemovePiece_Transpiler
        {
            private static readonly MethodInfo modifyIsInsideMythicalZone =
                AccessTools.Method(typeof(Player_RemovePiece_Transpiler), nameof(IsInsideNoBuildLocation));

            /// <summary>
            //  Replaces the RemovePiece().Location.IsInsideNoBuildLocation with a stub function
            /// </summary>
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction>? il = instructions.ToList();
                for (int i = 0; i < il.Count; ++i)
                    if (il[i].operand != null)
                        // search for every call to the function
                        if (il[i].operand.ToString().Contains(nameof(Location.IsInsideNoBuildLocation)))
                            il[i] = new CodeInstruction(OpCodes.Call, modifyIsInsideMythicalZone);
                // replace every call to the function with the stub
                return il.AsEnumerable();
            }

            private static bool IsInsideNoBuildLocation(Vector3 point)
            {
                return false;
            }
        }


        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
        static class Player_UpdatePlacementGhost_Patch
        {
            static void Postfix(Player __instance, bool flashGuardStone)
            {
                if (!buildInsideProtectedLocations.Value) return;
                if (Player.m_localPlayer == null) return;
                try
                {
                    switch (__instance.m_placementStatus)
                    {
                        case Player.PlacementStatus.NoBuildZone:
                        case Player.PlacementStatus.Invalid:
                            __instance.m_placementStatus = Player.PlacementStatus.Valid;
                            __instance.m_placementGhost.GetComponent<Piece>().SetInvalidPlacementHeightlight(false);
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }


        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public static class Player_OnSpawned_Patch
        {
            private static void Prefix(ref Player __instance)
            {
                if (SkipTuts.Value)
                    __instance.m_firstSpawn = false;
            }
        }

        [HarmonyPatch(typeof(EventSystem), "OnApplicationFocus")]
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

        [HarmonyPatch(typeof(Version), nameof(Version.GetVersionString))]
        private static class PatchVersionGetVersionString
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ref string __result)
            {
                if (ZNet.instance?.IsServer() == true)
                {
                    __result += $"-{OdinQOLplugin.ModName}{OdinQOLplugin.Version}";
                }
            }
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
        private static class PatchZNetRPC_PeerInfo
        {
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(ref ZPackage pkg)
            {
                long uid = pkg.ReadLong();
                string versionString = pkg.ReadString();

                if (ZNet.instance.IsServer())
                {
                    versionString += $"-{OdinQOLplugin.ModName}{OdinQOLplugin.Version}";
                }
                else
                {
                    versionString = versionString.Replace($"-{OdinQOLplugin.ModName}{OdinQOLplugin.Version}", "");
                }

                ZPackage newPkg = new();
                newPkg.Write(uid);
                newPkg.Write(versionString);
                newPkg.m_writer.Write(pkg.m_reader.ReadBytes((int)(pkg.m_stream.Length - pkg.m_stream.Position)));
                pkg = newPkg;
                pkg.SetPos(0);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
        private class fasterCrafting
        {
            private static void Prefix(ref InventoryGui __instance)
            {
                __instance.m_craftDuration = craftingDuration.Value;
            }
        }

        [HarmonyPatch(typeof(FejdStartup), "Awake")]
        public static class HookServerStart
        {
            private static void Postfix(ref FejdStartup __instance)
            {
                __instance.m_minimumPasswordLength = 0;
                __instance.m_serverPlayerLimit = maxPlayers.Value;
            }
        }

        [HarmonyPatch(typeof(SteamGameServer), "SetMaxPlayerCount")]
        public static class ChangeSteamServerVariables
        {
            private static void Prefix(ref int cPlayersMax)
            {
                int maxPlayers = GamePatches.maxPlayers.Value;
                if (maxPlayers >= 1) cPlayersMax = maxPlayers;
            }
        }

        [HarmonyPatch(typeof(ZNet), "Awake")]
        public static class ChangeGameServerVariables
        {
            private static void Postfix(ref ZNet __instance)
            {
                int maxPlayers = GamePatches.maxPlayers.Value;
                if (maxPlayers >= 1)
                    // Set Server Instance Max Players
                    __instance.m_serverPlayerLimit = maxPlayers;
            }
        }

        /// <summary>
        ///     Alters public password requirements
        /// </summary>
        [HarmonyPatch(typeof(FejdStartup), "IsPublicPasswordValid")]
        public static class ChangeServerPasswordBehavior
        {
            private static bool Prefix(ref bool __result)
            {
                // return always true
                __result = true;
                return false;
            }
        }

        /// <summary>
        ///     Override password error
        /// </summary>
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.GetPublicPasswordError))]
        public static class RemovePublicPasswordError
        {
            private static bool Prefix(ref string __result)
            {
                __result = "";
                return false;
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.SetupPieceInfo), typeof(Piece))]
        public static class Hud_Patch
        {
            private static void Postfix(Piece piece, Text ___m_buildSelection)
            {
                if (!OdinQOLplugin.modEnabled.Value || piece == null ||
                    string.IsNullOrEmpty(ImprovedBuildHudConfig.CanBuildAmountFormat.Value)) return;
                string? displayName = Localization.instance.Localize(piece.m_name);
                if (piece.m_resources.Length == 0) return;

                int fewestPossible =
                    (from requirement in piece.m_resources
                        let currentAmount =
                            OdinQOLplugin.GetAvailableItems(requirement.m_resItem.m_itemData.m_shared.m_name)
                        select currentAmount / requirement.m_amount).Prepend(int.MaxValue).Min();

                string? canBuildDisplay =
                    string.Format(ImprovedBuildHudConfig.CanBuildAmountFormat.Value, fewestPossible);
                if (!string.IsNullOrEmpty(ImprovedBuildHudConfig.CanBuildAmountColor.Value))
                    canBuildDisplay =
                        $"<color={ImprovedBuildHudConfig.CanBuildAmountColor.Value}>{canBuildDisplay}</color>";
                ___m_buildSelection.text = $"{displayName} {canBuildDisplay}";
            }
        }
    }
}