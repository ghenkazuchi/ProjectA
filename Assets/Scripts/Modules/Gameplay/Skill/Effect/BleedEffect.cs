using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedEffect : EffectBase, ITurnEnd
{
	public float bleedMaxHealthPercentagePerTurn;
	public BleedEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
		TriggerPhase = EffectTriggerPhase.EndOfTurn;
	}

	public IEnumerator OnTurnEnd()
	{
		if(Target.GetCurrentHP() <= 0)
		{
			yield break;
		}
		RequestVfx(EffectVfxTrigger.TurnEnd);
		int finalDamage = Mathf.CeilToInt(Target.GetFinalStat(Stat.HP) * bleedMaxHealthPercentagePerTurn * CurrentStack);
		Debug.Log($"{bleedMaxHealthPercentagePerTurn * CurrentStack}");
		yield return BattleSystem.Instance.ApplyEffectDamage(Target, finalDamage, Owner, "bleed");
	}
}
