using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEffect : EffectBase,ITurnStart
{
	public StunEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1)
		: base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public IEnumerator OnTurnStart()
	{
		var d = new TurnDirective
		{
			SkipThisTurn = true,
			SkipReason = "being stunned",
			Priority = 100,
			SourceEffectRuntimeId = this.RuntimeId,
			SourceEffectName = this.Name
		};
		Target.ProposeTurnDirective(d);
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} is no longer stunned!");
	}
}
