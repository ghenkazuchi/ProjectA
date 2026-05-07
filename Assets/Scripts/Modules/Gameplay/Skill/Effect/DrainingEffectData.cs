using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Draining Effect Data", menuName = "Effects/Draining Effect Data")]
public class DrainingEffectData : EffectData
{
	[SerializeField] private float drainingPercentage = 0.2f; 
	public float DrainingPercentage => drainingPercentage;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new DrainingEffect(this, owner, target, duration);
	}
}
