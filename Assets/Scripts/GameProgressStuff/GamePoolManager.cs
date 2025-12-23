using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePoolManager : Singleton<GamePoolManager>
{
	public MonsterPoolDatabase monsterPool;
	public RecruitabeCharacterPool recruitableCharacterPool;

	[SerializeField]
	public GameTime CurrentGameTinme = new GameTime(GameDay.Day1,TimeOfDay.Morning);

	private void Awake()
	{
		Debug.Log(CurrentGameTinme.day.ToString() + " " + CurrentGameTinme.time.ToString());	
	}
	public void SetGameTime(GameDay day, TimeOfDay time)
	{
		CurrentGameTinme = new GameTime(day, time);
	}
	public List<MonsterSpawnData> GetCurrentMonsterPool()
	{
		var pool = monsterPool.GetMonster(CurrentGameTinme.day, CurrentGameTinme.time);
		foreach(var monster in pool)
		{
			Debug.Log($"[GamePoolManager] Monster: {monster.monsterData.name}, Level: {monster.level}");
		}	
		return pool;
	}

	public List<RecruitableCharacterTemplate> GetCurrentRecruitablePool()
	{
		return recruitableCharacterPool.GetAvailableCharacters(CurrentGameTinme.day, CurrentGameTinme.time);
	}
}
