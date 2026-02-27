using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddElementToBattleArtEffect : EffectBase
{
	public Element ElementToAdd { get; set; }
	public AddElementToBattleArtEffect(EffectData data, EntityBase owner, EntityBase target, int duration, Element elementToAdd = default) : base(data, owner, target, duration)
	{
		ElementToAdd = elementToAdd;
	}

	public override IEnumerator ApplyEffect()
	{
		if (Target == null || Target.usableSkills == null) yield break;
		foreach(var skill in Target.usableSkills)
		{
			if(skill.SkillData.skillDefinition == SkillDefinition.BattleArt)
			{
				skill.SetSkillElement(ElementToAdd);
			}
		}
		Debug.Log("Element added to Battle Art skills for " + Target.entityData.EntityName + "!");
		yield return null;

	}
	public override IEnumerator RemoveEffect()
	{
		yield return null;
	}
}
