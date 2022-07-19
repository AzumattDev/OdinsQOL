using OdinQOL.Patches;

namespace OdinQOL.Configs;

public class SkillPatchConfigs
{
    internal static void Generate()
    {
        SkillPatches.ChangeSkills =
            OdinQOLplugin.context.config("Skills", "Change the skill gain factor", false, "Change skill gain factor");
        SkillPatches.ExperienceGainedNotifications = OdinQOLplugin.context.config("Skills",
            "Display notifications for skills gained", false, "Display notifications for skills gained");
        SkillPatches.Swordskill =
            OdinQOLplugin.context.config("Skills", "Sword Skill gain factor", 0f, "Sword skill gain factor");
        SkillPatches.Kniveskill =
            OdinQOLplugin.context.config("Skills", "Knives Skill gain factor", 0f, "Knives skill gain factor");
        SkillPatches.Clubskill =
            OdinQOLplugin.context.config("Skills", "Clubs Skill gain factor", 0f, "Clubs skill gain factor");
        SkillPatches.Polearmskill =
            OdinQOLplugin.context.config("Skills", "Polearm Skill gain factor", 0f, "Polearm skill gain factor");
        SkillPatches.Spearskill =
            OdinQOLplugin.context.config("Skills", "Spear Skill gain factor", 0f, "Spear skill gain factor");
        SkillPatches.Blockskill =
            OdinQOLplugin.context.config("Skills", "Block Skill gain factor", 0f, "Block skill gain factor");
        SkillPatches.Axeskill =
            OdinQOLplugin.context.config("Skills", "Axe Skill gain factor", 0f, "Axe skill gain factor");
        ;
        SkillPatches.Bowskill =
            OdinQOLplugin.context.config("Skills", "Bow Skill gain factor", 0f, "Bow skill gain factor");
        SkillPatches.Unarmed =
            OdinQOLplugin.context.config("Skills", "Unarmed Skill gain factor", 0f, "Unarmed skill gain factor");
        SkillPatches.Pickaxe =
            OdinQOLplugin.context.config("Skills", "Pickaxe Skill gain factor", 0f, "Pickaxe skill gain factor");
        SkillPatches.Woodcutting =
            OdinQOLplugin.context.config("Skills", "WoodCutting Skill gain factor", 0f,
                "WoodCutting skill gain factor");
        SkillPatches.Jump =
            OdinQOLplugin.context.config("Skills", "Jump Skill gain factor", 0f, "Jump skill gain factor");
        SkillPatches.Run = OdinQOLplugin.context.config("Skills", "Run Skill gain factor", 0f, "Run skill gain factor");
        SkillPatches.Sneak =
            OdinQOLplugin.context.config("Skills", "Sneak Skill gain factor", 0f, "Sneak skill gain factor");
        SkillPatches.Swim =
            OdinQOLplugin.context.config("Skills", "Swim Skill gain factor", 0f, "Swim skill gain factor");
        SkillPatches.DeathPenaltyMultiplier = OdinQOLplugin.context.config("Skills", "Death Penalty Factor Multiplier",
            0f,
            "Change the death penalty in percentage, where higher will increase the death penalty and lower will reduce it. This is a modifier value. 50 will increase it by 50%, -50 will reduce it by 50%.");
    }
}