using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OwnerBBelowCertainHP", menuName = "Effect/EffectCondition/OwnerBelowCertainHP")]
public class OwnerBelowCertainHP : ScriptableObject, IEffectCondition
{
	public float thresholdPercentage;

	public bool IsSastisfied(EntityBase owner, EntityBase target, BattleSystem battleSystem)
	{
		return (float) owner.GetCurrentHP()/owner.MaxHp < thresholdPercentage;
	}

}
