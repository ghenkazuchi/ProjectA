using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectCondition
{
	public bool IsSastisfied(EntityBase owner, EntityBase target,BattleSystem battleSystem);
}
