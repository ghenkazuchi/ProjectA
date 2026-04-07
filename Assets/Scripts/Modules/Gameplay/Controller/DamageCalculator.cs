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

        // Apply attack increase percentage from context (buffs etc.)
        float totalDamage = (baseDamage + scalingDamage) * ctx.attackIncreasePercentage;

        // Determine which defense stat to use based on skill type
        float defenseStat = 0f;
        switch (skill.SkillData.skillDefinition)
        {
            case SkillDefinition.Spell:
                defenseStat = Mathf.Max(0f, target.GetFinalStat(Stat.MagicalDefense));
                break;
            case SkillDefinition.BattleArt:
                defenseStat = Mathf.Max(0f, target.GetFinalStat(Stat.PhysicalDefense));
                break;
            case SkillDefinition.Almighty:
                defenseStat = 0f; // Almighty ignores defense entirely
                break;
        }

        // Apply defense-ignore percentage (from skill modifiers)
        float ignore = Mathf.Clamp01(ctx.defenseIgnorePercentage);
        defenseStat *= (1f - ignore);

        // Percentage-based defense reduction: 100 / (100 + DEF)
        // DEF=0 -> 100% damage, DEF=100 -> 50%, DEF=300 -> 25%
        float defenseMultiplier = 100f / (100f + defenseStat);
        totalDamage *= defenseMultiplier;

        float damageMultiplier = ElementalChart.GetMultiplier(skill.element, target.entityData.EntityElement);
        if (damageMultiplier > 1f) ctx.HasElementalAdvantage = true;
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

    public static bool TryRollCritical(EntityBase attacker, DamageContext ctx, out float critMul)
    {
        if (ctx.Origin != SkillDefinition.BattleArt)
        {
            critMul = 1f;
            return false;
        }

        // Base values
        float chance = baseCritChance;
        float mul = baseCritMultiplier;

        // Layer 1: Skill modifier bonuses (Night Slash style, written into ctx by ModifyPreview)
        chance += ctx.BonusCritChance;
        mul += ctx.BonusCritMultiplier;

        // Layer 2: Equipment / buff bonuses (Scope Lens / Sniper style)
        var attackerEffects = attacker.GetAllEffect();
        foreach (var e in attackerEffects)
        {
            if (e is IModifyCritical critMod)
            {
                chance += critMod.GetBonusCritChance();
                mul += critMod.GetBonusCritMultiplier();
            }
        }

        critMul = Mathf.Max(1f, mul);
        return Random.value <= Mathf.Clamp01(chance);
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
        if (TryRollCritical(ctx.Source, ctx, out var cm))
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
