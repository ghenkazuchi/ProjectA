using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseSpellDamageEffectEffect", menuName = "Skill/PassiveEffect/IncreaseSpellDamage")]
public class IncreaseSpellDamageEffectData : PassiveEffectData
{
	public float DamageIncreasePercentage;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new IncreaseSpellDamageEffect(DamageIncreasePercentage);
	}
}
