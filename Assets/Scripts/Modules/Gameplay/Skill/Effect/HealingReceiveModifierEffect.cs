using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingReceiveModifierEffect : EffectBase, IOnHealingReceived
{
	public float HealingReceivedModifyPercentage;
	public HealingReceiveModifierEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public IEnumerator OnHealingReceived(HealingContext ctx)
	{
		if (ctx.Target == null) yield break;
		int newFinal;
		if (HealingReceivedModifyPercentage >= 0)
		{
			newFinal = Mathf.CeilToInt(ctx.FinalHealing * (1f + HealingReceivedModifyPercentage));
		}
		else
		{
			float healingPercentage = 1 + HealingReceivedModifyPercentage;
			newFinal = Mathf.FloorToInt(ctx.FinalHealing * Mathf.Max(0f, healingPercentage));
		}

		ctx.FinalHealing = newFinal;
		yield break;
	}
}
