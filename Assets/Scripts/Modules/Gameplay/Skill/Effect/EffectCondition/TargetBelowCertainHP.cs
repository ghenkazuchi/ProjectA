using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetBBelowCertainHP", menuName = "Effect/EffectCondition/TargetBelowCertainHP")]
public class TargetBelowCertainHP : ScriptableObject, IEffectCondition
{
	public float thresholdPercentage;
	public bool IsSastisfied(EntityBase owner, EntityBase target, BattleSystem battleSystem)
	{
		Debug.Log(target.GetCurrentHP());
		return (float)target.GetCurrentHP() / target.MaxHp < thresholdPercentage;
	}


}
