using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreHPEffect : EffectBase, IScaleableEffect, ILimitedUsageTime
{
	public float RestoreAmount { get; set; }
	public float RestoreThreshold { get; set; }

	private EffectUsageTracker usageTracker;
	public RestoreHPEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float amount = 0) : base(data, owner, target, duration)
	{
		RestoreAmount = amount;
	}

	public override IEnumerator ApplyEffect()
	{
		if (Target.GetCurrentHP() <= 0) yield break;
		
		float normalizedThreshold = RestoreThreshold > 1f ? RestoreThreshold / 100f : RestoreThreshold;
		if (RestoreThreshold > 0 && Target.GetCurrentHP() / (float)Target.MaxHp > normalizedThreshold)
		{
			yield break;
		}
		
		if (!TryConsumeUse()) yield break;
		int amount = Mathf.CeilToInt(Target.MaxHp * RestoreAmount);
		Target.Heal(amount);
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} restored {amount} HP from {Name}!");
	}
	public override void RefreshEffect()
	{
		ApplyEffect();
	}

	public override IEnumerator RemoveEffect()
	{
		yield return null;
	}

	public void ApplyGradeTunning(float magnituMulplier, int durationBonus, float procChanceMultiplier, EffectData additionalEffect = null)
	{
		RestoreAmount = RestoreAmount * magnituMulplier;
	}

	public void SetUsageTracker(EffectUsageTracker tracker) => this.usageTracker = tracker;

	public bool TryConsumeUse()
	{
		if (usageTracker != null && usageTracker.CanUse())
		{
			usageTracker.RecordUse();
			return true;
		}

		// If usageTracker is null for an item, we block consumption to be safe and prevent infinite loops
		// In some pure passive setups this might be changed, but for items it ensures strict limit conformity.
		return false; 
	}
}
