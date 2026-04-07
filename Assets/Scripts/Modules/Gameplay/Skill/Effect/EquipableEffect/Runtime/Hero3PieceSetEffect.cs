using System.Collections;
using UnityEngine;

public class Hero3PieceSetEffect : EffectBase, IModifiOutcomingDamage, IModifiIncomingDamage
{
	private Hero3PieceSetEffectData heroData;

	public Hero3PieceSetEffect(Hero3PieceSetEffectData data, EntityBase owner, EntityBase target, int duration) 
		: base(data, owner, target, duration)
	{
		heroData = data;
	}

	public float GetOutcomingDamageModifier(EntityBase source)
	{
		// E.g., damageBonusPercent of 0.2 means +20% damage (1.2x multiplier)
		return 1f + heroData.damageBonusPercent;
	}

	public float GetInComingDamageModifier(EntityBase target, EntityBase source, int damage, BattleSystem battleSystem)
	{
		// E.g., damageReductionPercent of 0.1 means -10% damage (0.9x multiplier)
		return 1f - heroData.damageReductionPercent;
	}

	public override IEnumerator ApplyEffect()
	{
		// Bypass the normal Apply dialog since this is a persistent passive
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{
		// Bypass the normal Expire dialog
		yield break;
	}
}
