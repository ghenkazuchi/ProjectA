using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "newPassiveSkillData", menuName = "Skill/Create Passive Skill Data")]
public class PassiveSkillData : BaseSkillData
{
	[Header("Passive Properties")]
	public PassiveTrigger trigger;
	public List<PassiveEffectData> effects = new List<PassiveEffectData>();
	public float activationChance;
	public int cooldownTurns;
	public int initCoolDown;
	[Header("Grant Buff/Debuff")]
	public List<GrantEffect> grantEffects = new List<GrantEffect>();
}
