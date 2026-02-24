using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MadnessEnchancement", menuName = "Skill/PassiveEffect/MadnessEnchancement")]
public class MadnessEnchancementPassiveEffectData : PassiveEffectData
{
	public float healingPercentage;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new MadnessEnchancementPassiveEffect(healingPercentage);
	}
}
