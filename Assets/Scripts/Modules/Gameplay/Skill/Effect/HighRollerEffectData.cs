using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "High Roller Effect Data", menuName = "Effects/High Roller Effect")]
public class HighRollerEffectData : EffectData
{
	public float increasedProcChance;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new HighRollerEffect(this, owner, target, duration)
		{
			IncreasedProcChance = increasedProcChance
		};
	}
}
