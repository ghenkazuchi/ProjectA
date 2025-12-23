using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(fileName = "RecruitableCharacterPoolDatabase", menuName = "GameData/RecruitableCharacterPoolDatabase")]
public class RecruitabeCharacterPool : ScriptableObject
{
	public List<RecruitableCharacterTimePool> pools;
	public List<RecruitableCharacterTemplate> GetAvailableCharacters(GameDay day,TimeOfDay time)
	{
		var pool = pools.Find(p => p.day == day && p.timeOfDay == time);
		return pool != null ? pool.characters : new List<RecruitableCharacterTemplate>();
	}
}
[System.Serializable]
public class RecruitableCharacterTimePool
{
	public GameDay day;
	public TimeOfDay timeOfDay;
	public int minLevel;
	public int maxLevel;
	public List<RecruitableCharacterTemplate> characters;
}
