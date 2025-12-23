using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameProgressPool
{
	[Header("Progress Requirements")]
	public int minGameProgress;
	public string poolName;

	[Header("Level Range")]
	public int minLevel;
	public int maxLevel;

	[Header("Available Characters")]
	public List<RecruitableCharacterTemplate> availableCharacters = new List<RecruitableCharacterTemplate>();

	public int GetRandomLevel()
	{
		return Random.Range(minLevel, maxLevel);
	}
}