using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddElementToBattleArtEffect : EffectBase
{
	public Element ElementToAdd { get; set; }
	public AddElementToBattleArtEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1, Element elementToAdd = default) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
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
