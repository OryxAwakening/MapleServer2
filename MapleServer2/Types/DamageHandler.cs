﻿using Maple2Storage.Tools;

namespace MapleServer2.Types;

public class DamageHandler
{
    public double Damage { get; private set; }
    public bool IsCrit { get; private set; }

    private DamageHandler(double damage, bool isCrit)
    {
        Damage = damage;
        IsCrit = isCrit;
    }

    public static DamageHandler CalculateSkillDamage(SkillCast skillCast)
    {
        return new(skillCast.GetDamageRate(), false);
    }

    public static DamageHandler CalculateDamage(SkillCast skill, Player player, Mob mob, bool isCrit = false)
    {
        if (player.GmFlags.Contains("oneshot"))
        {
            return new(mob.Stats.Hp.Total, true);
        }
        // TODO: Calculate attack damage w/ stats
        double attackDamage = 300;
        double skillDamageRate = isCrit ? skill.GetCriticalDamage() : skill.GetDamageRate();
        double skillDamage = skillDamageRate * attackDamage;
        double enemyRes = skill.GetSkillDamageType() == DamageType.Physical ? mob.Stats.PhysRes.Total : mob.Stats.MagRes.Total;
        double resPierce = skill.GetSkillDamageType() == DamageType.Physical ? player.Stats[PlayerStatId.PhysAtk].Current : player.Stats[PlayerStatId.MagAtk].Current;
        // TODO: Fix damage multiplier (add pet?)
        double numerator = skillDamage * (1 + player.Stats[PlayerStatId.BonusAtk].Current) * (1500 - (enemyRes - resPierce * 15));

        double pierceCoeff = 1 - player.Stats[PlayerStatId.Pierce].Current;
        // TODO: Find correct enemy defense stats
        double denominator = mob.Stats.Cad.Total * pierceCoeff * 15;

        return new(numerator / denominator, isCrit);
    }

    public static bool RollCrit(int critRate = 0)
    {
        return RandomProvider.Get().Next(1000) < Math.Clamp(50 + critRate, 0, 400);
    }
}
