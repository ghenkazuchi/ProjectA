using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Boss Formation")]
public class BossFormation : ScriptableObject
{
	public string formationName;
	public List<Member> members = new();
	public Sprite bossIcon;

	[System.Serializable]
	public class Member
	{
		public MonsterData monster;
		public MonsterRankData rank;
		public MonsterRaceData race;
		public int level;

		public GridPosition position = new GridPosition(0, 0);
	}
}
