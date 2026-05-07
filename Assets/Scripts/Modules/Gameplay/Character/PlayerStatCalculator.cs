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
                baseValue = 50 + (vit * 1.5f * classMultiplier);
                break;
            case Stat.MP:
                baseValue = 20 + ((inte + pie) * classMultiplier);
                break;
            case Stat.SP:
                baseValue = 20 + ((str + dex) * classMultiplier);
                break;
            case Stat.AttackPower:
                baseValue = (str * 0.5f + dex * 0.2f) * classMultiplier;
                break;
            case Stat.MagicPower:
                baseValue = inte * 0.7f * classMultiplier;
                break;
            case Stat.DivinePower:
                baseValue = pie * 0.7f * classMultiplier;
                break;
            case Stat.PhysicalDefense:
                baseValue = 5 + (vit * 0.4f + str * 0.1f) * classMultiplier;
                break;
            case Stat.MagicalDefense:
                baseValue = 5 + (pie + inte) * 0.25f * classMultiplier;
                break;
            case Stat.ActionSpeed:
                baseValue = 20 + (agi * 1.5f * classMultiplier);
                break;
            case Stat.Evasion:
                baseValue = (agi * 0.5f + luk * 0.3f) * classMultiplier;
                break;
            case Stat.Accuracy:
                baseValue = (dex * 0.5f + luk * 0.1f) * classMultiplier;
                break;
            case Stat.Resistance:
                baseValue = (pie * 0.4f + luk * 0.3f) * classMultiplier;
                break;
            default:
                baseValue = 0;
                break;
        }
        float gearRaw = 0f;
        float gearPercentSum = 0f;
        if (player.weapon != null && player.weapon.WeaponBaseData != null)
        {
            int weaponStack = player.weapon.CurrentStack;
            foreach (var b in player.weapon.WeaponBaseData.EquipableStatBonus)
            {
                if (b.Stat != statToCalculate) continue;
                if (b.ModType == ModType.Flat) gearRaw += b.value * weaponStack;
                else gearPercentSum += b.value * weaponStack;
            }
        }

        foreach (var it in player.items)
        {
            if (it == null || it.itemBaseData == null) continue;
            var bonuses = it.itemBaseData.EquipableStatBonus;
            if (bonuses == null) continue;
            float gradeMult = ItemGradeConfig.Instance != null
                ? ItemGradeConfig.Instance.GetStatMultiplier(it.currentItemGrade)
                : 1f;
            foreach (var b in bonuses)
            {
                if (b.Stat != statToCalculate) continue;
                float v = b.value * gradeMult;
                if (b.ModType == ModType.Flat) gearRaw += v;
                else gearPercentSum += v;
            }
        }
        float effectRawModifier = 0f;
        float effectPercentageModifier = 1f;
        foreach (var effect in player.GetAllEffect())
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
            else if (effect is IStatModify statModifier) // Catch traditional buffs with IStatModify
            {
                effectRawModifier += (statModifier.ModifyStat(statToCalculate, baseValue, player) - baseValue);
            }
        }

        // Isolate Passive Skills and search them for IStatModify manually
        foreach (var passive in player.activePassiveSkills)
        {
            if (passive.RuntimeEffects == null) continue;
            foreach (var runtimeEffect in passive.RuntimeEffects)
            {
                if (runtimeEffect is IStatModify passiveStatModifier)
                {
                    effectRawModifier += (passiveStatModifier.ModifyStat(statToCalculate, baseValue, player) - baseValue);
                }
            }
        }
        float afterGear = (baseValue + gearRaw);
        float afterEffects = (afterGear + effectRawModifier);
        float final = afterEffects * (1f + gearPercentSum) * effectPercentageModifier;

        return Mathf.Round(final);
    }
}
