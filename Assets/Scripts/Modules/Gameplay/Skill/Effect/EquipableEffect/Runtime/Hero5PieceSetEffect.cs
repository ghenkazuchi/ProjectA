using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero5PieceSetEffect : EffectBase
{
	private Hero5PieceSetEffectData heroData;
	private List<ActiveSkill> modifiedSkills = new List<ActiveSkill>();

	public Hero5PieceSetEffect(Hero5PieceSetEffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
		heroData = data;
	}

	public override IEnumerator ApplyEffect()
	{
		if (Target == null || Target.usableSkills == null) yield break;

		foreach (var skill in Target.usableSkills)
		{
			var def = skill.SkillData.skillDefinition;

			// Skip Almighty skills entirely — no bonuses
			if (def == SkillDefinition.Almighty) continue;

			if (def == SkillDefinition.BattleArt)
			{
				// 1. Change element to Light
				skill.SetSkillElement(heroData.battleArtElement);

				// 2. Boost damage by battleArtDamageBonus (15%)
				int baseDamage = skill.SkillData.baseSkillDamage;
				int bonus = Mathf.CeilToInt(baseDamage * heroData.battleArtDamageBonus);
				skill.IncreaseDamage(baseDamage + bonus);
				modifiedSkills.Add(skill);

				Debug.Log($"[Hero5PC] {Target.entityData.EntityName}: {skill.SkillData.skillName} → Light element, damage {baseDamage} → {baseDamage + bonus} (+{heroData.battleArtDamageBonus * 100}%)");
			}
			else if (def == SkillDefinition.Spell)
			{
				// Only boost Light element spells
				if (skill.element == Element.Light)
				{
					int baseDamage = skill.SkillData.baseSkillDamage;
					int bonus = Mathf.CeilToInt(baseDamage * heroData.lightSpellDamageBonus);
					skill.IncreaseDamage(baseDamage + bonus);
					modifiedSkills.Add(skill);

					Debug.Log($"[Hero5PC] {Target.entityData.EntityName}: Light spell {skill.SkillData.skillName} damage {baseDamage} → {baseDamage + bonus} (+{heroData.lightSpellDamageBonus * 100}%)");
				}
				// Non-Light spells get no bonus
			}
		}

		yield return null;
	}

	public override IEnumerator RemoveEffect()
	{
		// Reset modified skills back to their original values
		foreach (var skill in modifiedSkills)
		{
			if (skill == null) continue;

			// Reset element to original
			skill.SetSkillElement(skill.SkillData.skillElement);
			// Reset damage to base
			skill.IncreaseDamage(skill.SkillData.baseSkillDamage);
		}
		modifiedSkills.Clear();

		yield return null;
	}
}
