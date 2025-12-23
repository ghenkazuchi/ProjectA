using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnData
{
	public MonsterData monsterData;
	public MonsterRankData rankData;
	public MonsterRaceData raceData;
	public int level;
}
[System.Serializable]
public class MonsterPool
{
	public GameDay day;
	public TimeOfDay timeOfDay;
	public List<MonsterSpawnData> monsters;
}
