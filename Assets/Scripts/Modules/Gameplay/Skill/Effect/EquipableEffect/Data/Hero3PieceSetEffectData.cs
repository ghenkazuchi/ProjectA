using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Hero 3 Piece Set Effect Data", menuName = "Effects/Hero 3 Piece Set Effect Data")]
public class Hero3PieceSetEffectData : EffectData
{
	public float damageBonusPercent;
	public float damageReductionPercent;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new Hero3PieceSetEffect(this, owner, target, duration);
	}
}
