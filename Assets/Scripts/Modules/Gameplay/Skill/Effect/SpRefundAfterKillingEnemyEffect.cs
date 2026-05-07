using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpRefundAfterKillingEnemyEffect : EffectBase
{
    public float SpRefundAmount;
	public SpRefundAfterKillingEnemyEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}


}
