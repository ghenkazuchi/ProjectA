using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationBonusPassiveEffect : PassiveEffectBase, IStatModify
{
	private readonly List<FormationStatBonus> frontRowBonuses;
	private readonly List<FormationStatBonus> backRowBonuses;

	public FormationBonusPassiveEffect(List<FormationStatBonus> frontRowBonuses, List<FormationStatBonus> backRowBonuses)
	{
		this.frontRowBonuses = frontRowBonuses ?? new List<FormationStatBonus>();
		this.backRowBonuses = backRowBonuses ?? new List<FormationStatBonus>();
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		// Stat modification is handled entirely by ModifyStat() during stat calculation.
		// No runtime action needed here.
		yield break;
	}

	public float ModifyStat(Stat statType, float currentValue, EntityBase target)
	{
		var bonuses = GetActiveBonuses(target);
		if (bonuses == null) return currentValue;

		foreach (var bonus in bonuses)
		{
			if (bonus.stat != statType) continue;

			if (bonus.modType == ModType.Flat)
			{
				currentValue += bonus.value;
			}
			else if (bonus.modType == ModType.Percentage)
			{
				currentValue += currentValue * bonus.value;
			}
		}

		return currentValue;
	}

	private List<FormationStatBonus> GetActiveBonuses(EntityBase entity)
	{
		var bs = BattleSystem.Instance;
		if (bs == null) return null;

		var pos = BattleGridUtils.GetEntityPosition(entity, bs.playerParty, bs.monsterParty);
		if (pos == null) return null;

		// x == 0 → Front Row, x == 1 → Back Row
		return pos.x == 0 ? frontRowBonuses : backRowBonuses;
	}
}
