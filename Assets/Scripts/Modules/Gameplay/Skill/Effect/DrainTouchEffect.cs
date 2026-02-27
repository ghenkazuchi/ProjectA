using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainTouchEffect : RestoreHPEffect
{
	public float OnHitRestorePercentage { get; set; }
	public DrainTouchEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float amount = 0, float onHitRestorePercentage = 0f) : base(data, owner, target, duration)
	{
		OnHitRestorePercentage = onHitRestorePercentage;
	}

	public override IEnumerator RemoveEffect()
	{
		throw new System.NotImplementedException();
	}
}
