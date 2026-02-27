using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Blood Thirst Effect Data", menuName = "Effects/Blood Thirst Effect")]
public class BloodThirstEffectData : EffectData
{
	public float lifeStealPercentage;	
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new BloodThirstEffectBase(this, owner, target, duration)
		{
			lifeStealPercentage = lifeStealPercentage
		};
	}
}
