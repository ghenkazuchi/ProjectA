using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalUpEffect : EffectBase, IModifyCritical
{
	public float CriticalChanceIncrease;
	public float CriticalDamageMultiplierIncrease;
	public CriticalUpEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public float GetBonusCritChance()
	{
		return CriticalChanceIncrease;
	}

	public float GetBonusCritMultiplier()
	{
		return CriticalDamageMultiplierIncrease;
	}
}
