using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RageBuffEffectData", menuName = "Skill/PassiveEffect/RageBuffEffectData")]
public class RageBuffEffectData : PassiveEffectData
{
	public EffectData rageEffectToApply;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new RageBuffEffect(rageEffectToApply);
	}
}
