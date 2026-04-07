using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ForestGuardian2PieceSetEffectBase : EffectBase, IBattleEnd
{
	public float EndOfBattleHealingPercentage;
	public ForestGuardian2PieceSetEffectBase(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public override IEnumerator ApplyEffect()
	{
		// Silent — passive equipment effect, no dialog
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{
		// Silent — passive equipment effect, no dialog
		yield break;
	}

	public IEnumerator OnBattleEnd()
	{
		int healingAmount = Mathf.CeilToInt(Owner.MaxHp * EndOfBattleHealingPercentage);
		Owner.Heal(healingAmount);
		yield return null;	
	}
}
