using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseBattleArtDamageEffect", menuName = "Skill/PassiveEffect/IncreaseBattleArtDamage")]
public class IncreaseBattleArtDamageEffectData : PassiveEffectData
{
	public float DamageIncreasePercentage;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new IncreaseBattleArtDamageEffect(DamageIncreasePercentage);
	}
}
