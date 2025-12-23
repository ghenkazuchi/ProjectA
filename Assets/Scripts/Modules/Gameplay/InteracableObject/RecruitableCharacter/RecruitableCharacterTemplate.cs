using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecruitableCharacterTemplate", menuName = "GameData/RecruitableCharacterTemplate")]
public class RecruitableCharacterTemplate : ScriptableObject
{
	[Header("CharacterInfo")]
	public string successRecruitmentQuote;

	[Header("DataInfo")]
	public ClassData classData;
	public CharacterRaceData raceData;
	public BaseEntityData entityData;

	[Header("RecruitmentCost")]
	public int minRecruimentCost;
	public int minLevel;
	public int maxLevel;

	[Header("Availability")]
	public int minGameProgress;
	public int maxGameProgress;

	public int GetRandomLevel()
	{
		return Random.Range(minLevel, maxLevel + 1);
	}
}