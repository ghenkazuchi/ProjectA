using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : EffectBase, IStatModify, ITurnEnd
{
	public float BurnDamage;
	public Stat ReducedStats;
	public float ReducedStatPercent;
	public BurnEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
	{
		TriggerPhase = EffectTriggerPhase.EndOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		Target.CalculateAllStats();
		return base.ApplyEffect();

	}

	public float ModifyStat(Stat statType, float currentValue, EntityBase target)
	{
		return currentValue * (1 - ReducedStatPercent);
	}

	public override IEnumerator RemoveEffect()
	{
		Target.CalculateAllStats();
		return base.RemoveEffect();
	}

	public IEnumerator OnTurnEnd()
	{
		if (Target.GetCurrentHP() <= 0) yield break;
		int finalDamage = Mathf.CeilToInt(BurnDamage * Target.MaxHp);
		Debug.Log(finalDamage);
		if (finalDamage > 0)
		{
			yield return BattleSystem.Instance.ApplyEffectDamage(Target, finalDamage, Owner, "burn");
		}
	}
}
