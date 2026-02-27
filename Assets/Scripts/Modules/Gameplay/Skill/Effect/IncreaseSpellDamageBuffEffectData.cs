using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = " Increase Spell Damage Effect Data", menuName = "Effects/Magic Surge Effect")]
public class IncreaseSpellDamageBuffEffectData : EffectData
{
	public float increasePercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new IncreaseSpellDamageBuffData(this, owner, target, duration)
		{
			IncreaseDamagePercentage = increasePercentage,
		};
	}
}
