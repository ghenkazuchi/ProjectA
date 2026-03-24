using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewMonsterRank", menuName = "Monster/Create Rank Data")]
public class MonsterRankData :ScriptableObject
{
	public MonsterRank monsterRank;
	[Header("Stats Multipliers")]
	public SerializableDictionaryBase<Stat, float> statMultipliers = new SerializableDictionaryBase<Stat, float>();
	[Header("Trait Bonus")]
	public SerializableDictionaryBase<Trait, int> traitBonuses = new SerializableDictionaryBase<Trait, int> { };
	[Header("Exp Multipliers")]
	public float expMultipliers;
	[Header("Souldusk Reward")]
	public int baseSoulduskReward = 10;
}