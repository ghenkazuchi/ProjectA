using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SleepEffect : EffectBase, IBeforeTakingDamage, ITurnStart
{
	public SleepEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public IEnumerator OnBeforeTakingDamage(DamageContext ctx)
	{
		if (ctx.Target != Target) yield break;
		ctx.IsCritical = true;
		ctx.CritMultiplier = 1.5f;
		ctx.CritDecided = true;

		ctx.CritForce = CritFocedType.Sleep;
		yield return Target.RemoveEffectCoroutine(this);
	}

	public IEnumerator OnTurnStart()
	{
		var d = new TurnDirective
		{
			SkipThisTurn = true,
			SkipReason = "Sleeping",
			Priority = 110,
			SourceEffectRuntimeId = this.RuntimeId,
			SourceEffectName = this.Name,
		};
		Target.ProposeTurnDirective(d);
		yield return Target.RemoveEffectCoroutine(this);
	}
	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} woke up!");
	}
}
