using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParalyzeEffect", menuName = "Effect/Paralyze")]
public class ParalyzeEffectData : EffectData
{
	public float chanceToSkipTurn = 0.5f;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ParalyzeEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack)
		{
			ChanceToSkipTurn = chanceToSkipTurn,
		};
	}
}
