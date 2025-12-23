using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReduceMPCostEffectData", menuName = "Skill/PassiveEffect/ReduceMPCost")]
public class ReduceMPCostEffectData : PassiveEffectData
{
	public float reducePercentage;
	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new ReduceMPCostEffect(reducePercentage);
	}
}
