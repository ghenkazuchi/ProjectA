using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newMonsterData", menuName = "Monster/Create Monster Data")]
public class MonsterData : BaseEntityData
{
	[SerializeField] private Trait dominantTrait;
	[SerializeField, Range(1f, 10f)] private float dominantWeight;
	public Trait DominantTrait => dominantTrait;
	public float DominantTraitPreferenceWeight => dominantWeight;
	public int baseExp;

	[Header("AI Behavior")]
	public AIStrategyType aiStrategy = AIStrategyType.Random;
	[Tooltip("Rule configuration (only used when aiStrategy = RuleBased)")]
	public AIBehaviorConfig aiBehavior = new AIBehaviorConfig();
}
