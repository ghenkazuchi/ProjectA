using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlowTargetOnHitEffectData", menuName = "Skill/PassiveEffect/SlowTarget")]

public class SlowTargetOnHitEffectData : PassiveEffectData
{
	public float slowPercentage;

	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new SlowTargetOnHitEffect(slowPercentage);
	}
}
