using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodThirstEffectBase : EffectBase, IOnDealingDamage
{
	public float lifeStealPercentage; 
	public BloodThirstEffectBase(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{

	}

	public IEnumerator OnDealingDamage(DamageContext ctx)
	{
		var skillDefinition = ctx.Origin;
		if (skillDefinition != SkillDefinition.BattleArt) yield break;
		int restoreAmount = Mathf.CeilToInt(ctx.EffectiveDamage * lifeStealPercentage);
		yield return BattleSystem.Instance.ShowDialog($"{Owner.entityData.EntityName} restored {restoreAmount} HP !");
		Owner.Heal(restoreAmount);
	}
}
