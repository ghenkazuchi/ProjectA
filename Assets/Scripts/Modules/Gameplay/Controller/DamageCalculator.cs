using UnityEngine;

public static class DamageCalculator
{
    public const float MIN_HIT = 0.85f;
    public const float MAX_HIT = 0.95f;
    public const float baseCritChance = 0.1f;
    public const float baseCritMultiplier = 1.5f;


    public static int ApplyDefenseReduction(EntityBase entity, int originaldamage, float defenseStateDamageReduction, bool isDefending)
    {
        if (isDefending)
        {
            float reduction = defenseStateDamageReduction;
            float bonus = 0f;
            var effects = entity.GetAllEffect();
            foreach (var e in effects)
            {
                if (e is IModifyIncomingDamageTakenOnDefenseState mod)
                {
                    bonus += Mathf.Max(0, mod.GetModifyOnDefenseState());
                }
            }
            reduction = Mathf.Clamp01(reduction + bonus);
            int reduced = Mathf.RoundToInt(originaldamage * (1f - reduction));
            Debug.Log($"{entity.entityData.EntityName} is defending! Damage reduced from {originaldamage} to {reduced}");
            return reduced;
        }
        return originaldamage;
    }

    public static int CalculateDamageWithContext(EntityBase source, ActiveSkill skill, EntityBase target, DamageContext ctx, BattleSystem battleSystem, float defenseStateDamageReduction, bool isDefending)
    {
        int baseDamage = Mathf.CeilToInt(skill.currentSkillDamage);
        float scalingDamage = 0f;

        foreach (var entry in skill.SkillData.scalingStatAndMutiply)
            scalingDamage += source.GetFinalStat(entry.Key) * entry.Value;

        float totalDamage = baseDamage + scalingDamage;

        float attackStat = 1f;
        float defenseStat = 0f;
        float kdef = 1f;

        switch (skill.SkillData.skillDefinition)
        {
            case SkillDefinition.Spell:
                attackStat = Mathf.Max(1f, source.GetFinalStat(Stat.MagicPower) * ctx.attackIncreasePercentage);
                defenseStat = Mathf.Max(0f, target.GetFinalStat(Stat.MagicalDefense));
                kdef = 1.2f;
                break;

            case SkillDefinition.BattleArt:
                attackStat = Mathf.Max(1f, source.GetFinalStat(Stat.AttackPower) * ctx.attackIncreasePercentage);
                defenseStat = Mathf.Max(0f, target.GetFinalStat(Stat.PhysicalDefense));
                kdef = 1.2f;
                break;

            case SkillDefinition.Almighty:
                attackStat = 1f;
                defenseStat = 1f;
                kdef = 1f;
                break;
        }

        float ignore = Mathf.Clamp01(ctx.defenseIgnorePercentage);
        defenseStat = defenseStat * (1f - ignore);

        float atkDefRatio = attackStat / (Mathf.Max(1f, defenseStat * kdef));
        totalDamage *= atkDefRatio;

        float damageMultiplier = ElementalChart.GetMultiplier(skill.element, target.entityData.EntityElement);
        totalDamage *= damageMultiplier;

        float rangePosMul = GetRangePositionMultiplier(source, skill, target, battleSystem);
        totalDamage *= rangePosMul;

        var finalDamage = ApplyDamageModifiers(source, target, Mathf.RoundToInt(totalDamage), battleSystem);
        finalDamage = ApplyDefenseReduction(target, finalDamage, defenseStateDamageReduction, isDefending);

        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }

    public static int ApplyDamageModifiers(EntityBase source, EntityBase target, int baseDamage, BattleSystem battleSystem)
    {
        float outgoingMult = 1f;
        float incomingMult = 1f;

        var sourceEffects = source?.GetAllEffect();
        var targetEffects = target?.GetAllEffect();
        if (sourceEffects != null)
        {
            foreach (var e in sourceEffects)
            {
                if (e is IModifiOutcomingDamage m)
                {
                    float k = Mathf.Max(0f, m.GetOutcomingDamageModifier(source));
                    if (k == 0f) { outgoingMult = 0f; break; }
                    outgoingMult *= k;
                }
            }
        }
        if (targetEffects != null && outgoingMult > 0f)
        {
            foreach (var e in targetEffects)
            {
                if (e is IModifiIncomingDamage m)
                {
                    float k = Mathf.Max(0f, m.GetInComingDamageModifier(target, source, baseDamage, battleSystem));
                    if (k == 0f) { incomingMult = 0f; break; }
                    incomingMult *= k;
                }
            }
        }
        Debug.Log("Outgoing Multiplier: " + outgoingMult + ", Incoming Multiplier: " + incomingMult);
        int modified = Mathf.RoundToInt(baseDamage * outgoingMult * incomingMult);
        Debug.Log("Base Damage: " + baseDamage + ", Modified Damage: " + modified);
        return Mathf.Max(0, modified);
    }

    public static bool CheckHit(EntityBase source, EntityBase target, ActiveSkill skill, out float finalHitChance, out float roll)
    {
        if (skill.SkillData.skillDefinition == SkillDefinition.Almighty)
        {
            finalHitChance = 1f;
            roll = 0f;
            return true;
        }
        float baseFromSkill = Mathf.Clamp01(skill.hitChance <= 0f ? 1f : skill.hitChance);

        if (skill.SkillData.skillDefinition == SkillDefinition.Spell || skill.SkillData.skillRange != SkillRange.SingleTarget)
        {
            // Spells and AoE/Line attacks cannot be dodged passively
            finalHitChance = baseFromSkill;
        }
        else
        {
            // Physical single-target attacks factor in Accuracy vs Evasion
            float acc = Mathf.Max(0f, source.GetFinalStat(Stat.Accuracy));
            float eva = Mathf.Max(0f, target.GetFinalStat(Stat.Evasion));

            finalHitChance = baseFromSkill + (acc / 100f) - (eva / 100f);
            finalHitChance = Mathf.Clamp(finalHitChance, 0.10f, 1f); // 10% min, 100% max
        }

        roll = Random.value;
        return roll <= finalHitChance;
    }

    public static bool CheckEffectApplication(EntityBase source, EntityBase target, float baseChance)
    {
        float res = Mathf.Max(0f, target.GetFinalStat(Stat.Resistance));
        float mitigationMultiplier = 100f / (100f + res);
        float finalChance = baseChance * mitigationMultiplier;
        finalChance = Mathf.Clamp01(finalChance);

        float roll = Random.value;
        bool applied = roll <= finalChance;
        Debug.Log($"Effect application on {target.entityData.EntityName}: Base {baseChance*100}%, Res {res}, Final Chance {finalChance*100}%, Roll {roll*100}%. Applied: {applied}");
        return applied;
    }

    public static bool TryRollCritical(EntityBase attacker, EntityBase target, SkillDefinition skill, out float critMul)
    {
        if (skill != SkillDefinition.BattleArt)
        {
            critMul = 1f;
            return false;
        }
        float chance = baseCritChance;
        critMul = baseCritMultiplier;

        var attackerEffects = attacker.GetAllEffect();
        return Random.value <= chance;
    }

    public static void ResolveCrit(DamageContext ctx, bool isDefending)
    {
        if (ctx.CritDecided)
        {
            if (ctx.IsCritical && ctx.CritForce == CritFocedType.Sleep)
            {
                return;
            }
            if (ctx.IsCritical && isDefending)
            {
                ctx.IsCritical = false;
                ctx.CritMultiplier = 1f;
                return;
            }
            return;
        }
        if (isDefending)
        {
            ctx.CritDecided = true;
            ctx.IsCritical = false;
            ctx.CritMultiplier = 1f;
            return;
        }
        if (TryRollCritical(ctx.Source, ctx.Target, ctx.Origin, out var cm))
        {
            ctx.IsCritical = true;
            ctx.CritMultiplier = Mathf.Max(1f, cm);
        }
        ctx.CritDecided = true;
    }
    private static float GetRangePositionMultiplier(EntityBase source, ActiveSkill skill, EntityBase target, BattleSystem battleSystem)
    {
        return 1f;
    }
}
