using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterPoolByTime", menuName = "GameData/MonsterPoolByTime")]
public class MonsterPoolDatabase : ScriptableObject
{
	public List<MonsterPool> pools;
	public List<MonsterSpawnData> GetMonster(GameDay day, TimeOfDay time)
	{
		Debug.Log($"[MonsterPoolDatabase] GetMonster: {day} - {time}");
		var pool = pools.Find(p => p.day == day && p.timeOfDay == time);
		if (pool == null)
			Debug.LogWarning($"[MonsterPoolDatabase] Pool NOT FOUND for {day} - {time}");
		else
			Debug.Log($"[MonsterPoolDatabase] Found pool, monsters count: {pool.monsters.Count}");

		return pool != null ? pool.monsters : new List<MonsterSpawnData>();
	}
}

