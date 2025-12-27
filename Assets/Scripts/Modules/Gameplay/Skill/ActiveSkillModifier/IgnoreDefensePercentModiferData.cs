using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skill/Modifiers/Ignore Defense Modifiers")]
public class IgnoreDefensePercentModiferData : SkillModifierData
{
	public float ignorePercent;
	public override void ModifyPreview(ref DamageContext ctx)
	{
		ctx.defenseIgnorePercentage = Mathf.Clamp01(ctx.defenseIgnorePercentage + ignorePercent);
	}
}
