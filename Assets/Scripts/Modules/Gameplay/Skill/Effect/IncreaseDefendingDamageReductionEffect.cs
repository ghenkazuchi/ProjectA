using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseDefendingDamageReductionEffect : EffectBase, IModifyIncomingDamageTakenOnDefenseState
{
	public float IncreasePercentage;
	public IncreaseDefendingDamageReductionEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public float GetModifyOnDefenseState()
	{
		Debug.Log("IncreaseDefendingDamageReductionEffect: " + IncreasePercentage);
		return IncreasePercentage;
	}
}
