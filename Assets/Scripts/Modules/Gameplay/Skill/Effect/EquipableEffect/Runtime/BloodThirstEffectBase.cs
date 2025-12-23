using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodThirstEffectBase : EffectBase, IOnDealingDamage
{
	public float lifeStealPercentage; 
	public BloodThirstEffectBase(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{

	}

	public IEnumerator OnDealingDamage(DamageContext ctx)
	{
		var skillDefinition = ctx.Origin;
		if (skillDefinition != SkillDefinition.BattleArt) yield return null;
		int restoreAmount = Mathf.CeilToInt(ctx.EffectiveDamage * lifeStealPercentage);
		yield return BattleSystem.Instance.ShowDialog($"{Owner.entityData.EntityName} restored {restoreAmount} HP !");
		Owner.Heal(restoreAmount);
	}
}
