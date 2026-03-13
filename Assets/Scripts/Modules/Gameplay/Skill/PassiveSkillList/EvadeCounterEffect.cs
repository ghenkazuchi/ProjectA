using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeCounterEffect : PassiveEffectBase, IStatModify
{
	private bool useBasicAttack;
	private int bonusEvasion;

	public EvadeCounterEffect(bool useBasicAttack, int bonusEvasion)
	{
		this.useBasicAttack = useBasicAttack;
		this.bonusEvasion = bonusEvasion;
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if (args.Length >= 1 && args[0] is EntityBase attacker)
		{
			if (attacker.GetCurrentHP() > 0 && useBasicAttack)
			{
				yield return battleSystem.ShowDialog($"{owner.entityData.EntityName} counters {attacker.entityData.EntityName}'s missed attack!");

				yield return battleSystem.actionExecutor.PerformSkillAction(
					owner, 
					new List<EntityBase> { attacker }, 
					battleSystem.basicAttack
				);
			}
		}
	}

	public float ModifyStat(Stat statType, float currentValue, EntityBase target)
	{
		if (statType == Stat.Evasion)
		{
			return currentValue + bonusEvasion;
		}
		return currentValue;
	}
}
