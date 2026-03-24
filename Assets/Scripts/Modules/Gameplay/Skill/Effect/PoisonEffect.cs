using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEffect : EffectBase,ITurnEnd
{
	public float BasePoisonDamage { get;  set; }
	public PoisonEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
		TriggerPhase = EffectTriggerPhase.EndOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		yield return base.ApplyEffect();
		Debug.Log($"{Target.entityData.EntityName} is now poisoned! Will take {BasePoisonDamage} damage per turn for {CurrentDuration} turns.");
	}

	public IEnumerator OnTurnEnd()
	{
		if(Target.GetCurrentHP() <= 0)
		{
			yield break;
		}
		RequestVfx(EffectVfxTrigger.TurnEnd);
		int finalDamage = Mathf.CeilToInt(BasePoisonDamage);
		yield return BattleSystem.Instance.ApplyEffectDamage(Target, finalDamage, Owner, "poison");
	}

	public override  IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} no longer in {Name}");
	}
	public override string GetExpireMessage()
	{
		return $"{Target.entityData.EntityName} is no longer poisoned.";
	}
}
