using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatCalculator : IStatCalculator
{
    public float CalculateSingleStat(Stat statToCalculate, Dictionary<Trait, int> effTraits, EntityBase entity)
    {
        PlayerCharacter player = entity as PlayerCharacter;
        if (player == null) return 0;

        int str = effTraits.GetValueOrDefault(Trait.Strength, 0);
        int inte = effTraits.GetValueOrDefault(Trait.Intelligence, 0);
        int pie = effTraits.GetValueOrDefault(Trait.Piety, 0);
        int vit = effTraits.GetValueOrDefault(Trait.Vitality, 0);
        int agi = effTraits.GetValueOrDefault(Trait.Agility, 0);
        int luk = effTraits.GetValueOrDefault(Trait.Luck, 0);
        int dex = effTraits.GetValueOrDefault(Trait.Dexterity, 0);

        float classMultiplier = player.GetClassData.statMultipliers.ContainsKey(statToCalculate) ? player.GetClassData.statMultipliers[statToCalculate] : 1.0f;
        float baseValue = 0;
        switch (statToCalculate)
        {
            case Stat.HP:
                baseValue = (20 + (vit * 2f)) * classMultiplier;
                break;
            case Stat.MP:
                baseValue = 5 + ((inte + pie) * classMultiplier * 0.5f);
                break;
            case Stat.SP:
                baseValue = 5 + ((str + dex) * classMultiplier * 0.5f);
                break;
            case Stat.AttackPower:
                baseValue = str * classMultiplier * 0.7f + dex * 0.3f * classMultiplier;
                break;
            case Stat.MagicPower:
                baseValue = inte * classMultiplier;
                break;
            case Stat.DivinePower:
                baseValue = pie * classMultiplier;
                break;
            case Stat.PhysicalDefense:
                baseValue = vit * classMultiplier;
                break;
            case Stat.MagicalDefense:
                baseValue = 0.5f * pie * classMultiplier + 0.5f * inte * classMultiplier;
                break;
            case Stat.ActionSpeed:
                baseValue = agi * classMultiplier;
                break;
            case Stat.Evasion:
                baseValue = agi * classMultiplier * 0.6f + luk * 0.4f * classMultiplier;
                break;
            case Stat.Accuracy:
                baseValue = classMultiplier * ((dex * 0.7f) + (luk * 0.3f));
                break;
            case Stat.Resistance:
                baseValue = classMultiplier * ((pie * 0.5f + vit * 0.5f));
                break;
            default:
                baseValue = 0;
                break;
        }
        float gearRaw = 0f;
        float gearPercentSum = 0f;
        if (player.weapon != null && player.weapon.WeaponBaseData != null)
        {
            foreach (var b in player.weapon.WeaponBaseData.EquipableStatBonus)
            {
                if (b.Stat != statToCalculate) continue;
                if (b.ModType == ModType.Flat) gearRaw += b.value;
                else gearPercentSum += b.value;
            }
        }

        foreach (var it in player.items)
        {
            if (it == null || it.itemBaseData == null) continue;
            var bonuses = it.itemBaseData.EquipableStatBonus;
            if (bonuses == null) continue;
            foreach (var b in bonuses)
            {
                if (b.Stat != statToCalculate) continue;
                float v = b.value;
                if (b.ModType == ModType.Flat) gearRaw += v;
                else gearPercentSum += v;
            }
        }
        float effectRawModifier = 0f;
        float effectPercentageModifier = 1f;
        foreach (var effect in player.currentActiveBuffs.Concat(player.currentActiveDebuffs).ToList())
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
        }
        float afterGear = (baseValue + gearRaw);
        float afterEffects = (afterGear + effectRawModifier);
        float final = afterEffects * (1f + gearPercentSum) * effectPercentageModifier;

        return Mathf.Round(final);
    }
}
