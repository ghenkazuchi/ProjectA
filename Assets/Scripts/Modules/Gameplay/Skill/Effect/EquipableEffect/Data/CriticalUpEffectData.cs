using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Critical Up Effect", menuName = "Effects/Critical Up Effect")]
public class CriticalUpEffectData : EffectData
{
	[SerializeField] private float criticalChanceIncrease;
	[SerializeField] private float criticalDamageMultiplierIncrease;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new CriticalUpEffect(this, owner, target, duration)
		{
			CriticalChanceIncrease = criticalChanceIncrease,
			CriticalDamageMultiplierIncrease = criticalDamageMultiplierIncrease
		};
	}

}
