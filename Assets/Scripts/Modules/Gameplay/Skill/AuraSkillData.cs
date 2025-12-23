using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "newAuraSkillData", menuName = "Skill/Create Aura Skill Data")]
public class AuraSkillData : BaseSkillData
{
	public AuraEffectTarget auraEffectTarget;
	public List<RaceType> auraConditionRace;
	public List<RaceType> raceAppliable;
}
