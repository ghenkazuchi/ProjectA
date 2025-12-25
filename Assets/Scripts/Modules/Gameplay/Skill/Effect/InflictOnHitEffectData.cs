using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On Hit Effect Data", menuName = "Effects/On Hit Effect")]
public class InflictOnHitEffectData : EffectData
{
	public EffectData onHitEffectToApply;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return  new InflictOnHitEffect(EffectType, Effect, Name, owner, target, duration, effectIcon, CanBeRemoved, Stackable, MaxStack){
			OnHitEffectToApply = onHitEffectToApply
		};
	}
}
