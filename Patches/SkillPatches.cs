using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace VMP_Mod.Patches
{
    public static class SkillPatches
	{
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
			double mult = Math.Pow(10.0, digits);
			double result = Math.Truncate(mult * value) / mult;
			return (float)result;
		}

		[HarmonyPatch(typeof(Skills), "RaiseSkill")]
		public static class AddExpGainedDisplay
		{
			/// <summary>
			/// Updates experience modifiers
			/// </summary>
			private static void Prefix(ref Skills __instance, ref Skills.SkillType skillType, ref float factor)
			{
				if (SkillPatches.ChangeSkills.Value)
				{
					switch ((SkillType)skillType)
					{
						case SkillType.Swords:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.swordskill.Value);
							break;
						case SkillType.Knives:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.kniveskill.Value);
							break;
						case SkillType.Clubs:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.clubskill.Value);
							break;
						case SkillType.Polearms:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.polearmskill.Value);
							break;
						case SkillType.Spears:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.spearskill.Value);
							break;
						case SkillType.Blocking:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.blockskill.Value);
							break;
						case SkillType.Axes:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.axeskill.Value);
							break;
						case SkillType.Bows:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.bowskill.Value);
							break;
						case SkillType.Unarmed:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.unarmed.Value);
							break;
						case SkillType.Pickaxes:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.pickaxe.Value);
							break;
						case SkillType.WoodCutting:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.woodcutting.Value);
							break;
						case SkillType.Jump:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.jump.Value);
							break;
						case SkillType.Sneak:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.sneak.Value);
							break;
						case SkillType.Run:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.run.Value);
							break;
						case SkillType.Swim:
							factor = VMP_Modplugin.applyModifierValue(factor, SkillPatches.swim.Value);
							break;
						default:
							break;
					}
				}
			}

			/// <summary>
			/// Experience gained notifications
			/// </summary>
			private static void Postfix(Skills __instance, Skills.SkillType skillType, float factor = 1f)
			{
				if (SkillPatches.experienceGainedNotifications.Value)
				{
					try
					{
						Skills.Skill skill;
						skill = __instance.GetSkill(skillType);
						float percent = skill.m_accumulator / (skill.GetNextLevelRequirement() / 100);
						__instance.m_player.Message(MessageHud.MessageType.TopLeft, "Level " + SkillPatches.tFloat(skill.m_level, 0) + " " + skill.m_info.m_skill
							+ " [" + SkillPatches.tFloat(skill.m_accumulator, 2) + "/" + SkillPatches.tFloat(skill.GetNextLevelRequirement(), 2) + "]"
							+ " (" + SkillPatches.tFloat(percent, 0) + "%)", 0, skill.m_info.m_icon);
					}
					catch
					{ return; }
				}
			}
		}

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

		[HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
		public static class Skills_OnDeath_Transpiler
		{
			private static MethodInfo method_Skills_LowerAllSkills = AccessTools.Method(typeof(Skills), nameof(Skills.LowerAllSkills));
			private static MethodInfo method_LowerAllSkills = AccessTools.Method(typeof(Skills_OnDeath_Transpiler), nameof(Skills_OnDeath_Transpiler.LowerAllSkills));

			/// <summary>
			/// We replace the call to Skills.LowerAllSkills with our own stub, which then applies the death multiplier.
			/// </summary>
			[HarmonyTranspiler]
			public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> il = instructions.ToList();

				for (int i = 0; i < il.Count; ++i)
				{
					if (il[i].Calls(method_Skills_LowerAllSkills))
					{
						il[i].operand = method_LowerAllSkills;
					}
				}

				return il.AsEnumerable();
			}

			public static void LowerAllSkills(Skills instance, float factor)
			{
				if (SkillPatches.deathPenaltyMultiplier.Value > -100.0f)
				{
					instance.LowerAllSkills(VMP_Modplugin.applyModifierValue(factor, SkillPatches.deathPenaltyMultiplier.Value));
				}
			}
		}

	}
}
