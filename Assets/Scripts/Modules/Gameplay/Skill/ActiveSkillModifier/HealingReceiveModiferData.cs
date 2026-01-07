using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Modifiers/Increase Healing Modifiers")]
public class HealingReceiveModiferData : SkillModifierData
{
	public float multiplier;
	public override void ModifyHealingPreview(ref HealingContext healingContext)
	{
		Debug.Log($"healing increase: {multiplier}");
		Debug.Log("Final healing before mod: " + healingContext.FinalHealing);
		healingContext.FinalHealing = Mathf.RoundToInt(healingContext.FinalHealing * (1+multiplier));
	}
}
