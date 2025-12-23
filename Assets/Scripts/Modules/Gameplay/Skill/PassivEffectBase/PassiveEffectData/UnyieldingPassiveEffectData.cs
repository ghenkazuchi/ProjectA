using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnyieldingEffect", menuName = "Skill/PassiveEffect/UnyieldingEffect")]
public class UnyieldingPassiveEffectData : PassiveEffectData
{
	public float restorationPercentage;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new UnyieldingPassiveEffect(restorationPercentage);
	}
}
