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

        public static ConfigEntry<bool> ChangeSkills = null!;
        public static ConfigEntry<bool> ExperienceGainedNotifications = null!;
        public static ConfigEntry<float> Swordskill = null!;
        public static ConfigEntry<float> Kniveskill = null!;
        public static ConfigEntry<float> Clubskill = null!;
        public static ConfigEntry<float> Polearmskill = null!;
        public static ConfigEntry<float> Spearskill = null!;
        public static ConfigEntry<float> Blockskill = null!;
        public static ConfigEntry<float> Axeskill = null!;
        public static ConfigEntry<float> Bowskill = null!;
        public static ConfigEntry<float> Unarmed = null!;
        public static ConfigEntry<float> Pickaxe = null!;
        public static ConfigEntry<float> Woodcutting = null!;
        public static ConfigEntry<float> Jump = null!;
        public static ConfigEntry<float> Sneak = null!;
        public static ConfigEntry<float> Run = null!;
        public static ConfigEntry<float> Swim = null!;
        public static ConfigEntry<float> DeathPenaltyMultiplier = null!;

        public static float TFloat(this float value, int digits)
        {
            double mult = Math.Pow(10.0, digits);
            double result = Math.Truncate(mult * value) / mult;
            return (float)result;
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.RaiseSkill))]
        public static class AddExpGainedDisplay
        {
            /// <summary>
            ///     Updates experience modifiers
            /// </summary>
            private static void Prefix(ref Skills __instance, ref Skills.SkillType skillType, ref float factor)
            {
                if (!ChangeSkills.Value) return;
                factor = (SkillType)skillType switch
                {
                    SkillType.Swords => Utilities.ApplyModifierValue(factor, Swordskill.Value),
                    SkillType.Knives => Utilities.ApplyModifierValue(factor, Kniveskill.Value),
                    SkillType.Clubs => Utilities.ApplyModifierValue(factor, Clubskill.Value),
                    SkillType.Polearms => Utilities.ApplyModifierValue(factor, Polearmskill.Value),
                    SkillType.Spears => Utilities.ApplyModifierValue(factor, Spearskill.Value),
                    SkillType.Blocking => Utilities.ApplyModifierValue(factor, Blockskill.Value),
                    SkillType.Axes => Utilities.ApplyModifierValue(factor, Axeskill.Value),
                    SkillType.Bows => Utilities.ApplyModifierValue(factor, Bowskill.Value),
                    SkillType.Unarmed => Utilities.ApplyModifierValue(factor, Unarmed.Value),
                    SkillType.Pickaxes => Utilities.ApplyModifierValue(factor, Pickaxe.Value),
                    SkillType.WoodCutting => Utilities.ApplyModifierValue(factor, Woodcutting.Value),
                    SkillType.Jump => Utilities.ApplyModifierValue(factor, Jump.Value),
                    SkillType.Sneak => Utilities.ApplyModifierValue(factor, Sneak.Value),
                    SkillType.Run => Utilities.ApplyModifierValue(factor, Run.Value),
                    SkillType.Swim => Utilities.ApplyModifierValue(factor, Swim.Value),
                    _ => factor
                };
            }

            /// <summary>
            ///     Experience gained notifications
            /// </summary>
            private static void Postfix(Skills __instance, Skills.SkillType skillType, float factor = 1f)
            {
                if (!ExperienceGainedNotifications.Value) return;
                try
                {
                    Skills.Skill skill = __instance.GetSkill(skillType);
                    float percent = skill.m_accumulator / (skill.GetNextLevelRequirement() / 100);
                    __instance.m_player.Message(MessageHud.MessageType.TopLeft, "Level " + skill.m_level.TFloat(0) +
                        " " + skill.m_info.m_skill
                        + " [" + skill.m_accumulator.TFloat(2) + "/" + skill.GetNextLevelRequirement().TFloat(2) +
                        "]"
                        + " (" + percent.TFloat(0) + "%)", 0, skill.m_info.m_icon);
                }
                catch
                {
                    // ignored
                }
            }
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
        public static class Skills_OnDeath_Transpiler
        {
            private static readonly MethodInfo MethodSkillsLowerAllSkills =
                AccessTools.Method(typeof(Skills), nameof(Skills.LowerAllSkills));

            private static readonly MethodInfo MethodLowerAllSkills =
                AccessTools.Method(typeof(Skills_OnDeath_Transpiler), nameof(LowerAllSkills));

            /// <summary>
            ///     We replace the call to Skills.LowerAllSkills with our own stub, which then applies the death multiplier.
            /// </summary>
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction>? il = instructions.ToList();

                foreach (CodeInstruction t in il.Where(t => t.Calls(MethodSkillsLowerAllSkills)))
                    t.operand = MethodLowerAllSkills;

                return il.AsEnumerable();
            }

            public static void LowerAllSkills(Skills instance, float factor)
            {
                if (DeathPenaltyMultiplier.Value > -100.0f)
                    instance.LowerAllSkills(Utilities.ApplyModifierValue(factor, DeathPenaltyMultiplier.Value));
            }
        }
    }
}