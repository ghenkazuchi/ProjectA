using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "newMonsterTypeData", menuName = "Monster/Create Type Data")]
public class MonsterRaceData : BaseRaceData
{
	public MonsterTypeName monsterTypeDefinition;
	public MonsterType type;
	[Header("Skill Set")]
	public List<SkillEntry> skillSet = new List<SkillEntry>();

}
