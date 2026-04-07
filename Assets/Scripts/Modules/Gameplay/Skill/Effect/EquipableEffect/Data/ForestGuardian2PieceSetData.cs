using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ForestGuardian2PiecesSet Effect Data", menuName = "Effects/ForestGuradian2PieceseSetEffect")]
public class ForestGuardian2PieceSetData : EffectData
{
	[SerializeField] private float endOfBattleHealingPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ForestGuardian2PieceSetEffectBase(this,owner, target, duration)
		{
			EndOfBattleHealingPercentage = endOfBattleHealingPercentage,
		};
	}
}
