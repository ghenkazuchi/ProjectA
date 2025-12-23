using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnEffect", menuName = "Effects/Burn Effect")]
public class BurnEffectData : EffectData
{
	public float burnDamage;
	public Stat reducedStats;
	public float reducedStatPercent;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new BurnEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack)
		{
			BurnDamage = burnDamage,
			ReducedStats = reducedStats,
			ReducedStatPercent = reducedStatPercent
		};
	}
}
