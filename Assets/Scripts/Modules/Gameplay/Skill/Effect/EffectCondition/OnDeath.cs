using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OwnerDie", menuName = "Effect/EffectCondition/OwnerDie")]
public class OnDeath : ScriptableObject, IEffectCondition
{
	public bool IsSastisfied(EntityBase owner, EntityBase target, BattleSystem battleSystem)
	{
		if(owner.GetCurrentHP() == 0)
		{
			return true;
		}
		return false;
	}
}
