using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighRollerEffect : EffectBase, IProcChanceModifier
{
	public float IncreasedProcChance;
	public HighRollerEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public float ModifyProcChance(float baseProcChance)
	{
		return baseProcChance + IncreasedProcChance;
	}
}
