using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParalyzeEffect : EffectBase, ITurnStart
{
	public float ChanceToSkipTurn;
	public ParalyzeEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		yield return BattleSystem.Instance.ShowDialog("${ Target.entityData.EntityName} is paralyzed and may be unable to move!");
	}
	public IEnumerator OnTurnStart()
	{
		if (Random.value > ChanceToSkipTurn)
			yield break;
		else
		{
			var d = new TurnDirective
			{
				Priority = 50,
				SourceEffectRuntimeId = RuntimeId,
				SourceEffectName = Name,
				SkipThisTurn = true
			};
			Target.ProposeTurnDirective(d);
			yield break;
		}
	}
}
