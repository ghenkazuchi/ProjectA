using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resolve Skill Effect Data", menuName = "Effects/Resolve Skill Effect")]
public class ResolveSkillEffectData : EffectData
{
	public float reviveRestorationPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ResolveSkillEffect(this, owner, target, duration)
		{
			restorationPercentage = reviveRestorationPercentage,
		};
	}
}
