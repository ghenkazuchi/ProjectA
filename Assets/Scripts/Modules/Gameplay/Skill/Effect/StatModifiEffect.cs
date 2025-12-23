using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifiEffect : EffectBase, IScaleableEffect
{
	public Stat StatToModify { get; private set; }
	public int RawValue { get; private set; }
	public float PercentageValue { get; private set; }
	public bool IsRawValue { get; private set; }

	public StatModifiEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon,
								bool canBeRemoved, bool stackable, int maxStack,
								Stat statToModify, bool isRawValue, int rawValue, float percentageValue)
			: base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
	{
		StatToModify = statToModify;
		IsRawValue = isRawValue;
		RawValue = rawValue;
		PercentageValue = percentageValue;
		TriggerPhase = EffectTriggerPhase.EndOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		yield return base.ApplyEffect();
	}

	public override IEnumerator RemoveEffect()
	{
		yield return null;
	}
	public void ApplyGradeTunning(float magnituMulplier, int durationBonus, float procChanceMultiplier, EffectData additionalEffect = null)
	{
		if (IsRawValue) RawValue = Mathf.RoundToInt(RawValue * magnituMulplier);
		else PercentageValue *= magnituMulplier;
	}
}
