using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CovertCloakEffectBase : EffectBase, IBeforeStatusApplied, ILimitedUsageTime
{
	private EffectUsageTracker tracker;
	public void SetUsageTracker(EffectUsageTracker tracker)
	{
		this.tracker = tracker;
	}
	public override IEnumerator ApplyEffect()
	{
		yield break;
	}

	public CovertCloakEffectBase(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public IEnumerator OnBeforeStatusApplied(StatusApplyContext context)
	{
		if(context == null || context.IncomingEffect == null) yield break;
		if (context.Target != Target) yield break;
		if (context.IncomingEffect.Effect != Effect.CrownControl) yield break;
		if(tracker != null && !tracker.CanUse()) yield break;

		context.Cancle = true;
		context.CancleReason = $"{Target.entityData.EntityName}'s {Name} negate the effect!";
		yield break;
	}

	public bool TryConsumeUse()
	{
		throw new System.NotImplementedException();
	}
}
