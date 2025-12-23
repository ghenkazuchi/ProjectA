using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReduceMPCostEffect : PassiveEffectBase
{
	public float ReductionPercentage { get; set; }

	public ReduceMPCostEffect(float reductionPercentage)
	{
		this.ReductionPercentage = reductionPercentage;
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if (args.Length >= 1 && args[0] is List<ActiveSkill> skillList)
		{
			foreach (var skill in skillList)
			{
				if (skill.SkillData.skillDefinition == SkillDefinition.Spell)
				{
					float reducePercentage = this.ReductionPercentage;
					int reducedCost = Mathf.CeilToInt(skill.SkillData.baseMpCost * (1 - reducePercentage));
					skill.SetMPCost(reducedCost);
					Debug.Log($"{owner.entityData.EntityName} reduced {skill.SkillData.skillName}'s MP cost to {reducedCost}!");
				}
			}
		}
		else
		{
			yield return battleSystem.ShowDialog("Invalid arguments for ReduceMPCostEffect.");
		}
	}
}
