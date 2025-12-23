using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkillRunner
{
	private EntityBase owner;
	private BattleSystem battleSystem;

	public PassiveSkillRunner(EntityBase owner, BattleSystem battleSystem)
	{
		this.owner = owner;
		this.battleSystem = battleSystem;
	}

	public IEnumerator Trigger(PassiveTrigger trigger, params object[] args)
	{
		foreach (var skill in owner.activePassiveSkills)
		{
			var data = skill.PassiveSkillData;
			if (data == null || data.trigger != trigger) continue;

			if(skill.CurrentSkillCoolDown > 0)
			{
				skill.ReduceCoolDown();
			}
			if(skill.CurrentSkillCoolDown > 0) continue;
			

			if (data.activationChance > 0f && Random.value > data.activationChance) continue;

			foreach (var pe in skill.RuntimeEffects)
				yield return pe.ApplyEffect(owner, battleSystem, args);

			foreach (var spec in data.grantEffects)
			{
				if (spec?.effectToGrant == null) continue;
				int dur = spec.duration < 0 ? int.MaxValue : spec.duration;
				var runtime = spec.effectToGrant.CreateRuntimeEffect(owner, owner, dur);
				yield return owner.AddEffect(runtime); 
			}
			if(data.cooldownTurns > 0)
			{
				skill.StartCoolDown();
			}
		}
	}

	public void ResetPassiveEffect()
	{
		foreach (var skill in owner.activePassiveSkills)
		{
			foreach(var effect in skill.RuntimeEffects)
			{
				if (effect is ILimitedPassiveUse limitedUseEffect)
				{
					limitedUseEffect.ResetUse();
				}
			}	
		}
	}
}
