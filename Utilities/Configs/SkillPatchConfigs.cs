using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class SkillPatchConfigs
{
    internal static void Generate()
    {
        SkillPatches.ChangeSkills =
            OdinQOLplugin.context.config("Skills", "Change the skill gain factor", false, "Change skill gain factor");
        SkillPatches.experienceGainedNotifications = OdinQOLplugin.context.config("Skills",
            "Display notifications for skills gained", false, "Display notifications for skills gained");
        SkillPatches.swordskill =
            OdinQOLplugin.context.config("Skills", "Sword Skill gain factor", 0f, "Sword skill gain factor");
        SkillPatches.kniveskill =
            OdinQOLplugin.context.config("Skills", "Knives Skill gain factor", 0f, "Knives skill gain factor");
        SkillPatches.clubskill =
            OdinQOLplugin.context.config("Skills", "Clubs Skill gain factor", 0f, "Clubs skill gain factor");
        SkillPatches.polearmskill =
            OdinQOLplugin.context.config("Skills", "Polearm Skill gain factor", 0f, "Polearm skill gain factor");
        SkillPatches.spearskill =
            OdinQOLplugin.context.config("Skills", "Spear Skill gain factor", 0f, "Spear skill gain factor");
        SkillPatches.blockskill =
            OdinQOLplugin.context.config("Skills", "Block Skill gain factor", 0f, "Block skill gain factor");
        SkillPatches.axeskill =
            OdinQOLplugin.context.config("Skills", "Axe Skill gain factor", 0f, "Axe skill gain factor");
        ;
        SkillPatches.bowskill =
            OdinQOLplugin.context.config("Skills", "Bow Skill gain factor", 0f, "Bow skill gain factor");
        SkillPatches.unarmed =
            OdinQOLplugin.context.config("Skills", "Unarmed Skill gain factor", 0f, "Unarmed skill gain factor");
        SkillPatches.pickaxe =
            OdinQOLplugin.context.config("Skills", "Pickaxe Skill gain factor", 0f, "Pickaxe skill gain factor");
        SkillPatches.woodcutting =
            OdinQOLplugin.context.config("Skills", "WoodCutting Skill gain factor", 0f,
                "WoodCutting skill gain factor");
        SkillPatches.jump =
            OdinQOLplugin.context.config("Skills", "Jump Skill gain factor", 0f, "Jump skill gain factor");
        SkillPatches.run = OdinQOLplugin.context.config("Skills", "Run Skill gain factor", 0f, "Run skill gain factor");
        SkillPatches.sneak =
            OdinQOLplugin.context.config("Skills", "Sneak Skill gain factor", 0f, "Sneak skill gain factor");
        SkillPatches.swim =
            OdinQOLplugin.context.config("Skills", "Swim Skill gain factor", 0f, "Swim skill gain factor");
        SkillPatches.deathPenaltyMultiplier = OdinQOLplugin.context.config("Skills", "Death Penalty Factor Multiplier",
            0f,
            "Change the death penalty in percentage, where higher will increase the death penalty and lower will reduce it. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.");
    }
}