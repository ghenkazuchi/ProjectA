using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEffect : EffectBase,ITurnStart
{
	public StunEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
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
