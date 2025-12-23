using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReduceSPCostEffectData", menuName = "Skill/PassiveEffect/ReduceSPCost")]
public class ReduceSPCostEffectData : PassiveEffectData
{
	public float reducePercentage;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new ReduceSPCostEffect(reducePercentage);
	}
}
