using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;

namespace OdinQOL.Patches
{
    public static class SkillPatches
    {
        public enum SkillType
        {
            None,
            Swords,
            Knives,
            Clubs,
            Polearms,
            Spears,
            Blocking,
            Axes,
            Bows,
            FireMagic,
            FrostMagic,
            Unarmed,
            Pickaxes,
            WoodCutting,
            Jump = 100,
            Sneak,
            Run,
            Swim,
            All = 999
        }

        public static ConfigEntry<bool> ChangeSkills;
        public static ConfigEntry<bool> experienceGainedNotifications;
        public static ConfigEntry<float> swordskill;
        public static ConfigEntry<float> kniveskill;
        public static ConfigEntry<float> clubskill;
        public static ConfigEntry<float> polearmskill;
        public static ConfigEntry<float> spearskill;
        public static ConfigEntry<float> blockskill;
        public static ConfigEntry<float> axeskill;
        public static ConfigEntry<float> bowskill;
        public static ConfigEntry<float> unarmed;
        public static ConfigEntry<float> pickaxe;
        public static ConfigEntry<float> woodcutting;
        public static ConfigEntry<float> jump;
        public static ConfigEntry<float> sneak;
        public static ConfigEntry<float> run;
        public static ConfigEntry<float> swim;
        public static ConfigEntry<float> deathPenaltyMultiplier;

        public static float tFloat(this float value, int digits)
        {
            var mult = Math.Pow(10.0, digits);
            var result = Math.Truncate(mult * value) / mult;
            return (float)result;
        }

        [HarmonyPatch(typeof(Skills), "RaiseSkill")]
        public static class AddExpGainedDisplay
        {
            /// <summary>
            ///     Updates experience modifiers
            /// </summary>
            private static void Prefix(ref Skills __instance, ref Skills.SkillType skillType, ref float factor)
            {
                if (ChangeSkills.Value)
                    switch ((SkillType)skillType)
                    {
                        case SkillType.Swords:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, swordskill.Value);
                            break;
                        case SkillType.Knives:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, kniveskill.Value);
                            break;
                        case SkillType.Clubs:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, clubskill.Value);
                            break;
                        case SkillType.Polearms:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, polearmskill.Value);
                            break;
                        case SkillType.Spears:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, spearskill.Value);
                            break;
                        case SkillType.Blocking:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, blockskill.Value);
                            break;
                        case SkillType.Axes:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, axeskill.Value);
                            break;
                        case SkillType.Bows:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, bowskill.Value);
                            break;
                        case SkillType.Unarmed:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, unarmed.Value);
                            break;
                        case SkillType.Pickaxes:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, pickaxe.Value);
                            break;
                        case SkillType.WoodCutting:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, woodcutting.Value);
                            break;
                        case SkillType.Jump:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, jump.Value);
                            break;
                        case SkillType.Sneak:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, sneak.Value);
                            break;
                        case SkillType.Run:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, run.Value);
                            break;
                        case SkillType.Swim:
                            factor = OdinQOLplugin.ApplyModifierValue(factor, swim.Value);
                            break;
                    }
            }

            /// <summary>
            ///     Experience gained notifications
            /// </summary>
            private static void Postfix(Skills __instance, Skills.SkillType skillType, float factor = 1f)
            {
                if (experienceGainedNotifications.Value)
                    try
                    {
                        Skills.Skill skill;
                        skill = __instance.GetSkill(skillType);
                        var percent = skill.m_accumulator / (skill.GetNextLevelRequirement() / 100);
                        __instance.m_player.Message(MessageHud.MessageType.TopLeft, "Level " + skill.m_level.tFloat(0) +
                            " " + skill.m_info.m_skill
                            + " [" + skill.m_accumulator.tFloat(2) + "/" + skill.GetNextLevelRequirement().tFloat(2) +
                            "]"
                            + " (" + percent.tFloat(0) + "%)", 0, skill.m_info.m_icon);
                    }
                    catch
                    {
                    }
            }
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
        public static class Skills_OnDeath_Transpiler
        {
            private static readonly MethodInfo method_Skills_LowerAllSkills =
                AccessTools.Method(typeof(Skills), nameof(Skills.LowerAllSkills));

            private static readonly MethodInfo method_LowerAllSkills =
                AccessTools.Method(typeof(Skills_OnDeath_Transpiler), nameof(LowerAllSkills));

            /// <summary>
            ///     We replace the call to Skills.LowerAllSkills with our own stub, which then applies the death multiplier.
            /// </summary>
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
            {
                var il = instructions.ToList();

                for (var i = 0; i < il.Count; ++i)
                    if (il[i].Calls(method_Skills_LowerAllSkills))
                        il[i].operand = method_LowerAllSkills;

                return il.AsEnumerable();
            }

            public static void LowerAllSkills(Skills instance, float factor)
            {
                if (deathPenaltyMultiplier.Value > -100.0f)
                    instance.LowerAllSkills(OdinQOLplugin.ApplyModifierValue(factor, deathPenaltyMultiplier.Value));
            }
        }
    }
}