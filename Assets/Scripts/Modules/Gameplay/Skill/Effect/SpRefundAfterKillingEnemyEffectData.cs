using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpRefundAfterKillingEnemyEffectData : EffectData
{
	[SerializeField] private float spRefundAmount = 0.2f;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new SpRefundAfterKillingEnemyEffect(this, owner, target, duration)
		{
			SpRefundAmount = spRefundAmount
		};
	}
}
