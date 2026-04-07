using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skill/Modifiers/Increase Critical")]
public class IncreaseCriticalModifierData : SkillModifierData
{
	[Tooltip("Added to base crit chance (e.g. 0.125 = +12.5%)")]
	public float bonusCritChance;

	[Tooltip("Added to crit damage multiplier (e.g. 0.5 turns 1.5x into 2.0x)")]
	public float bonusCritMultiplier;

	public override void ModifyPreview(ref DamageContext ctx)
	{
		ctx.BonusCritChance += bonusCritChance;
		ctx.BonusCritMultiplier += bonusCritMultiplier;
	}
}
