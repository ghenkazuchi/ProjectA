using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ProtectAlly Effect Data", menuName = "Effects/ProtectAlly Effect")]
public class ProtectAllyEffectData : EffectData
{
	public ProtectRangeType protectRange;
	public float reducedDamagePercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ProtectAllyEffect(this, owner, target, duration)
		{
			ProtectRange = protectRange,
			ReducedPercentage = reducedDamagePercentage,
			SharePercentage = Mathf.Clamp01(reducedDamagePercentage / 100f)
		};
	}
}
