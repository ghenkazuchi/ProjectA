using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Covert Cloak Effect Data", menuName = "Effects/Covert Cloak Effect")]
public class CovertCloakEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new CovertCloakEffectBase(this, owner, target, duration);
	}
}
