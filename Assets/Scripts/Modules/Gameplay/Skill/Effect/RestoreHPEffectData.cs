using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RestoreHP", menuName = "Effect/RestoreHPData")]
public class RestoreHPEffectData : EffectData
{
	public float restoreAmount;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new RestoreHPEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack, restoreAmount)
		{
			RestoreAmount = restoreAmount
		};
	}
}
