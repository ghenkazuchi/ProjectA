using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainTouchEffect : RestoreHPEffect
{
	public float OnHitRestorePercentage { get; set; }
	public DrainTouchEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1, float amount = 0, float onHitRestorePercentage = 0f) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack, amount)
	{
		OnHitRestorePercentage = onHitRestorePercentage;
	}

	public override IEnumerator RemoveEffect()
	{
		throw new System.NotImplementedException();
	}
}
