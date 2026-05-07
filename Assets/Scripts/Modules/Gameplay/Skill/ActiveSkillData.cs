using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newActiveSkillData", menuName = "Skill/Create Active Skill Data")]
public class ActiveSkillData : BaseSkillData
{
	[Header("Proprerties")]
	public SkillDefinition skillDefinition;
	public TargetType targetType;
	public SkillRange skillRange;
	public int baseSpCost;
	public int baseMpCost;
	public int baseSkillDamage;
	public float baseHitChance;
	public int hitCount = 1;
	public Element skillElement;
	public ActiveSkillType activeSkillType;
	[Header("Scaling")]
	public SerializableDictionaryBase<Stat, float> scalingStatAndMutiply;
	[Header("Effect Can Apply")]
	public List<ActiveSkillEffect> effectsToApply = new List<ActiveSkillEffect>();

	public List<SkillModifierData> modifiers;
}