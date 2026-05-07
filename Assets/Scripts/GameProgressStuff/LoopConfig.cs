using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Loop Config")]
public class LoopConfig : ScriptableObject
{
	[Header("Loop Identity")]
	public string loopName;

	[Header("Boss")]
	public BossPool bossPool;

	[Header("Pools")]
	public MonsterPoolDatabase monsterPool;
	public RecruitabeCharacterPool recruitPool;
}
