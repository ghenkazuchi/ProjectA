using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Dispel Buff Effec", menuName = "Effects/DispelBuff Effect")]
public class DispelBuffEffectData : EffectData
{
	public int dispelAmount;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new DispelBuffEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack)
		{
			DispelAmount = dispelAmount
		};
	}
}
