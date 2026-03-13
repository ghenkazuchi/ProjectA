using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EvadeCounterEffect", menuName = "Skill/PassiveEffect/EvadeCounterEffect")]
public class EvadeCounterEffectData : PassiveEffectData
{
	[Tooltip("If true, the counter attack uses the standard Basic Attack. If false, you could expand this later to use a specific skill.")]
	public bool useBasicAttack = true;
	
	[Tooltip("Amount of flat Evasion to grant the character who equips this Passive.")]
	public int bonusEvasion = 20;

	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new EvadeCounterEffect(useBasicAttack, bonusEvasion);
	}
}
