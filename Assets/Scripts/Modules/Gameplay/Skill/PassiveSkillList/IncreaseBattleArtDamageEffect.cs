using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseBattleArtDamageEffect : PassiveEffectBase
{
	public float DamageIncreasePercentage { get;  set; }
	public IncreaseBattleArtDamageEffect(float damageIncreasePercentage)
	{
		DamageIncreasePercentage = damageIncreasePercentage;
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if (args.Length >= 1 && args[0] is List<ActiveSkill> skillList)
		{
			foreach(var skill in skillList)
			{
				if(skill.SkillData.skillDefinition == SkillDefinition.BattleArt)
				{
					int baseDamage = skill.SkillData.baseSkillDamage;
					float increasePercentage = this.DamageIncreasePercentage;
					int increasedDamage = Mathf.CeilToInt(skill.SkillData.baseSkillDamage * increasePercentage);
					skill.IncreaseDamage(baseDamage + increasedDamage);
					Debug.Log($"{owner.entityData.EntityName} increased {skill.SkillData.skillName}'s damage from {skill.SkillData.baseSkillDamage} to {skill.currentSkillDamage}!");
				}
			}
		}
		else
		{
			yield return battleSystem.ShowDialog("Invalid arguments for ReduceMPCostEffect.");
			battleSystem.UpdateUnitHealth(owner);
		}
	}
}
