using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpringPill", menuName = "Effect/SpringPill")]
public class SpringPillEffectData : EffectData
{
	public float restoreAmount;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new SpringPillEffect(this, owner, target, duration, restoreAmount)
		{
			RestoreAmount = restoreAmount
		};
	}
}
