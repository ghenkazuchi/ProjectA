using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseDefendingDamageReductionEffect : EffectBase, IModifyIncomingDamageTakenOnDefenseState
{
	public float IncreasePercentage;
	public IncreaseDefendingDamageReductionEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
	{
	}

	public float GetModifyOnDefenseState()
	{
		Debug.Log("IncreaseDefendingDamageReductionEffect: " + IncreasePercentage);
		return IncreasePercentage;
	}
}
