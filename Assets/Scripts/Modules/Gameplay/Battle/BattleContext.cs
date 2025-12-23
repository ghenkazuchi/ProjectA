using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleContext
{
	public MonsterParty monsterParty;
	public BattleType battleType;

	public BattleContext(MonsterParty monsterParty, BattleType battleType)
	{
		this.monsterParty = monsterParty;
		this.battleType = battleType;
	}	
}

