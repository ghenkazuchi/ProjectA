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
                baseValue = (20 + (vit * 2f)) * rankMultiplier;
                break;
            case Stat.MP:
                baseValue = 5 + ((inte + pie) * rankMultiplier * 0.5f);
                break;
            case Stat.SP:
                baseValue = 5 + ((str + dex) * rankMultiplier * 0.5f);
                break;
            case Stat.AttackPower:
                baseValue = str * rankMultiplier;
                break;
            case Stat.MagicPower:
                baseValue = inte * rankMultiplier;
                break;
            case Stat.DivinePower:
                baseValue = pie * rankMultiplier;
                break;
            case Stat.PhysicalDefense:
                baseValue = vit * rankMultiplier;
                break;
            case Stat.MagicalDefense:
                baseValue = 0.5f * pie * rankMultiplier + 0.5f * inte * rankMultiplier;
                break;
            case Stat.ActionSpeed:
                baseValue = agi * rankMultiplier;
                break;
            case Stat.Evasion:
                baseValue = agi * rankMultiplier * 0.6f + luk * 0.4f * rankMultiplier;
                break;
            case Stat.Accuracy:
                baseValue = rankMultiplier * ((dex * 0.7f) + (luk * 0.3f));
                break;
            case Stat.Resistance:
                baseValue = rankMultiplier * ((pie * 0.5f + vit * 0.5f));
                break;
            default:
                baseValue = 0;
                break;
        }
        return Mathf.Round(baseValue);
    }
}
