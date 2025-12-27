using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedEffect : EffectBase, ITurnEnd
{
	public float bleedMaxHealthPercentagePerTurn;
	public BleedEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
		TriggerPhase = EffectTriggerPhase.EndOfTurn;
	}

	public IEnumerator OnTurnEnd()
	{
		if(Target.GetCurrentHP() <= 0)
		{
			yield break;
		}
		int finalDamage = Mathf.CeilToInt(Target.GetFinalStat(Stat.HP) * bleedMaxHealthPercentagePerTurn * CurrentStack);
		Debug.Log($"{bleedMaxHealthPercentagePerTurn * CurrentStack}");
		yield return BattleSystem.Instance.ApplyEffectDamage(Target, finalDamage, Owner, "bleed");
	}
}
