using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalModifyEffect : EffectBase, IModifyCritical
{
	public float BonusCritChance { get; private set; }
	public float BonusCritMultiplier { get; private set; }

	public CriticalModifyEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float bonusCritChance, float bonusCritMultiplier)
		: base(data, owner, target, duration)
	{
		BonusCritChance = bonusCritChance;
		BonusCritMultiplier = bonusCritMultiplier;
		TriggerPhase = EffectTriggerPhase.Passive;
	}

	public float GetBonusCritChance()
	{
		return BonusCritChance;
	}

	public float GetBonusCritMultiplier()
	{
		return BonusCritMultiplier;
	}

	public override IEnumerator ApplyEffect()
	{
		// Passive set bonus, usually no dialog needed to avoid spam
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{
		yield break;
	}
}
