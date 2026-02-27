using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreHPEffect : EffectBase, IScaleableEffect, ILimitedUsageTime
{
	public float RestoreAmount { get; set; }

	private EffectUsageTracker usageTracker;
	public RestoreHPEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float amount = 0) : base(data, owner, target, duration)
	{
		RestoreAmount = amount;
	}

	public override IEnumerator ApplyEffect()
	{
		if (Target.GetCurrentHP() <= 0) yield break;
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
		return true;
	}
}
