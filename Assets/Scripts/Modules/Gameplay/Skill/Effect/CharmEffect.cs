using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmEffect : EffectBase, ITurnStart
{
	public CharmEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public  IEnumerator OnTurnStart()
	{
		RequestVfx(EffectVfxTrigger.TurnStart);
		var d = new TurnDirective
		{
			SkipThisTurn = false,
			SkipReason = "being charmed",
			ForcedAction = ForcedActionKind.BasicAttack,
			Priority = 90,
			SourceEffectRuntimeId = this.RuntimeId,
			SourceEffectName = this.Name
		};
		Target.ProposeTurnDirective(d);
		yield break;
	}
	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} is no longer charmed");
	}
}
