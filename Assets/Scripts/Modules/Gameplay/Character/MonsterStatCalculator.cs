using System.Collections.Generic;
using UnityEngine;

public class MonsterStatCalculator : IStatCalculator
{
    public float CalculateSingleStat(Stat statToCalculate, Dictionary<Trait, int> effTraits, EntityBase entity)
    {
        MonsterCharacter monster = entity as MonsterCharacter;
        if (monster == null) return 0;

        int str = effTraits.GetValueOrDefault(Trait.Strength, 0);
        int inte = effTraits.GetValueOrDefault(Trait.Intelligence, 0);
        int pie = effTraits.GetValueOrDefault(Trait.Piety, 0);
        int vit = effTraits.GetValueOrDefault(Trait.Vitality, 0);
        int agi = effTraits.GetValueOrDefault(Trait.Agility, 0);
        int luk = effTraits.GetValueOrDefault(Trait.Luck, 0);
        int dex = effTraits.GetValueOrDefault(Trait.Dexterity, 0);

        float rankMultiplier = monster.RankData != null && monster.RankData.statMultipliers.ContainsKey(statToCalculate) ? monster.RankData.statMultipliers[statToCalculate] : 1.0f;
        float baseValue = 0;
        switch (statToCalculate)
        {
            case Stat.HP:
                baseValue = 50 + (vit * 2.5f * rankMultiplier);
                break;
            case Stat.MP:
                baseValue = 20 + ((inte + pie) * 1.5f * rankMultiplier);
                break;
            case Stat.SP:
                baseValue = 20 + ((str + dex) * 1.5f * rankMultiplier);
                break;
            case Stat.AttackPower:
                baseValue = (str * 0.6f + dex * 0.25f) * rankMultiplier;
                break;
            case Stat.MagicPower:
                baseValue = inte * 0.85f * rankMultiplier;
                break;
            case Stat.DivinePower:
                baseValue = pie * 0.85f * rankMultiplier;
                break;
            case Stat.PhysicalDefense:
                baseValue = 5 + (vit * 0.6f + str * 0.1f) * rankMultiplier;
                break;
            case Stat.MagicalDefense:
                baseValue = 5 + (pie + inte) * 0.35f * rankMultiplier;
                break;
            case Stat.ActionSpeed:
                baseValue = 20 + (agi * 1.5f * rankMultiplier);
                break;
            case Stat.Evasion:
                baseValue = (agi * 0.5f + luk * 0.3f) * rankMultiplier;
                break;
            case Stat.Accuracy:
                baseValue = (dex * 0.5f + luk * 0.1f) * rankMultiplier;
                break;
            case Stat.Resistance:
                baseValue = (pie * 0.4f + luk * 0.3f) * rankMultiplier;
                break;
            default:
                baseValue = 0;
                break;
        }
        float effectRawModifier = 0f;
        float effectPercentageModifier = 1f;
        foreach (var effect in monster.GetAllEffect())
        {
            if (effect is StatModifiEffect statModifiEffect && statModifiEffect.StatToModify == statToCalculate)
            {
                if (statModifiEffect.IsRawValue)
                {
                    effectRawModifier += statModifiEffect.RawValue * statModifiEffect.CurrentStack;
                }
                else
                {
                    effectPercentageModifier *= statModifiEffect.PercentageValue * statModifiEffect.CurrentStack;
                }
            }
            else if (effect is IStatModify statModifier)
            {
                effectRawModifier += (statModifier.ModifyStat(statToCalculate, baseValue, monster) - baseValue);
            }
        }

        // Isolate Passive Skills and search them for IStatModify manually
        foreach (var passive in monster.activePassiveSkills)
        {
            if (passive.RuntimeEffects == null) continue;
            foreach (var runtimeEffect in passive.RuntimeEffects)
            {
                if (runtimeEffect is IStatModify passiveStatModifier)
                {
                    effectRawModifier += (passiveStatModifier.ModifyStat(statToCalculate, baseValue, monster) - baseValue);
                }
            }
        }

        float afterEffects = (baseValue + effectRawModifier);
        float final = afterEffects * effectPercentageModifier;

        return Mathf.Round(final);
    }
}
