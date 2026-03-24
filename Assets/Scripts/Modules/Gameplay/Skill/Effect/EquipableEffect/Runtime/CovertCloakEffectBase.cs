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

	public CovertCloakEffectBase(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public IEnumerator OnBeforeStatusApplied(StatusApplyContext context)
	{
		if(context == null || context.IncomingEffect == null) yield break;
		if (context.Target != Target) yield break;
		if (context.IncomingEffect.Effect != Effect.CrownControl) yield break;
		if (!TryConsumeUse()) yield break;

		context.Cancle = true;
		context.CancleReason = $"{Target.entityData.EntityName}'s {Name} negate the effect!";
		yield break;
	}

	public bool TryConsumeUse()
	{
		if (tracker != null && tracker.CanUse())
		{
			tracker.RecordUse();
			return true;
		}

		return tracker == null;
	}
}
