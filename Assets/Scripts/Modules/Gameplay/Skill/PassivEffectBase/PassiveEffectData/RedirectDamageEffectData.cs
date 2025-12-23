using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RedirectDamageEffectData", menuName = "Skill/PassiveEffect/RedirectDamage")]
public class RedirectDamageEffectData : PassiveEffectData
{
	public ProtectRangeType protectRange;
	public float redirectPercentage;

	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new RedirectDamageEffect(redirectPercentage, protectRange);
	}
}
