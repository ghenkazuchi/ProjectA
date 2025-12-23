using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LifeStealEffect", menuName = "Skill/PassiveEffect/LifeStealEffect")]
public class LifeStealEffectData : PassiveEffectData
{
	public float lifeStealPercentage;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new LifeStealEffect(lifeStealPercentage);
	}
}
