using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InstantKillActiveSkillEffect", menuName = "Effect/InstantKillActiveSkillEffectData")]
public class InstantKillActiveEffectData : EffectData
{
	public float thresholdPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new InstantKillActiveEffect(EffectType, Effect, Name, owner, target, duration, effectIcon, CanBeRemoved, Stackable, MaxStack, thresholdPercentage);
	}
}
