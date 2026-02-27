using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModiftyDamageTaken : EffectBase, IModifiIncomingDamage
{
	private readonly float percent; 

	public ModiftyDamageTaken(EffectData data, EntityBase owner, EntityBase target, int duration, float percent)
		: base(data, owner, target, duration)
	{
		this.percent = Mathf.Max(0f, percent);
	}

	public float GetInComingDamageModifier(EntityBase target, EntityBase source, int damage, BattleSystem battleSystem)
	{
		return 1f + percent;
	}
}
