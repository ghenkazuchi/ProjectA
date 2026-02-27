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
		return new BurnEffect(this, owner, target, duration)
		{
			BurnDamage = burnDamage,
			ReducedStats = reducedStats,
			ReducedStatPercent = reducedStatPercent
		};
	}
}
