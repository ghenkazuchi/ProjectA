using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resolve Effect Data", menuName = "Effects/Resolve Effect")]
public class ResolveEffectData : EffectData
{
	public float reviveRestorationPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ResolveEffectBase(this, owner, target, duration)
		{
			restorationPercentage = reviveRestorationPercentage,
		};
	}
}
