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
                baseValue = 50 + (vit * 3f * rankMultiplier);
                break;
            case Stat.MP:
                baseValue = 20 + ((inte + pie) * 2f * rankMultiplier);
                break;
            case Stat.SP:
                baseValue = 20 + ((str + dex) * 2f * rankMultiplier);
                break;
            case Stat.AttackPower:
                baseValue = str * rankMultiplier * 0.7f + dex * 0.3f * rankMultiplier;
                break;
            case Stat.MagicPower:
                baseValue = inte * rankMultiplier;
                break;
            case Stat.DivinePower:
                baseValue = pie * rankMultiplier;
                break;
            case Stat.PhysicalDefense:
                baseValue = 5 + (vit * 1f * rankMultiplier + str * 0.1f * rankMultiplier);
                break;
            case Stat.MagicalDefense:
                baseValue = 5 + ((pie + inte) * 0.45f * rankMultiplier);
                break;
            case Stat.ActionSpeed:
                baseValue = 20 + (agi * 1.5f * rankMultiplier);
                break;
            case Stat.Evasion:
                baseValue = (agi * 1.5f * rankMultiplier) + (luk * 0.8f * rankMultiplier);
                break;
            case Stat.Accuracy:
                baseValue = (dex * 1.5f * rankMultiplier);
                break;
            case Stat.Resistance:
                baseValue = (pie * 0.5f * rankMultiplier) + (luk * 0.6f * rankMultiplier);
                break;
            default:
                baseValue = 0;
                break;
        }
        return Mathf.Round(baseValue);
    }
}
