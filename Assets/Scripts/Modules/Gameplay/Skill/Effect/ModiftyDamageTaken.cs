using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModiftyDamageTaken : EffectBase, IModifiIncomingDamage
{
	private readonly float percent; 

	public ModiftyDamageTaken(EntityBase owner, EntityBase target, int duration,Sprite icon, float percent)
		: base(EffectType.Debuff, Effect.Other, "Increased Damage Taken", owner, target, duration,icon,
			   /*isInstant*/ false, /*Stackable?*/ false, /*maxStack*/ 1)
	{
		this.percent = Mathf.Max(0f, percent);
	}

	public float GetInComingDamageModifier(EntityBase target, EntityBase source, int damage, BattleSystem battleSystem)
	{
		return 1f + percent;
	}
}
