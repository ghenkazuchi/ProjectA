using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseHealingEffect : EffectBase, IOnHealingReceived
{
	public float HealingToDamgeRatio;
	public ReverseHealingEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public IEnumerator OnHealingReceived(HealingContext ctx)
	{
		if (ctx.Target == null) yield break;
		int damageAmount = Mathf.CeilToInt(ctx.FinalHealing * Mathf.Clamp01(HealingToDamgeRatio));
		ctx.FinalHealing = -damageAmount;
		yield break;
	}

}
