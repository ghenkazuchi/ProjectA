using System.Collections;
using UnityEngine;

/// <summary>
/// Petrified debuff: the target cannot swap positions and cannot receive healing.
/// Implements ITurnStart to ban the Switch action, and IOnHealingReceived to block all healing.
/// </summary>
public class PetrifiedEffect : EffectBase, ITurnStart, IOnHealingReceived
{
	public PetrifiedEffect(EffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public IEnumerator OnTurnStart()
	{
		// Ban the Switch action for this turn
		var directive = new TurnDirective
		{
			Bans = ActionBan.Switch,
			Priority = 50,
			SourceEffectRuntimeId = this.RuntimeId,
			SourceEffectName = this.Name
		};
		Target.ProposeTurnDirective(directive);
		yield break;
	}

	public IEnumerator OnHealingReceived(HealingContext ctx)
	{
		if (ctx.Target != Target) yield break;

		int blockedAmount = ctx.FinalHealing;
		ctx.FinalHealing = 0;

		yield return BattleSystem.Instance.ShowDialog(
			$"{Target.entityData.EntityName} is petrified and can't be healed! ({blockedAmount} HP blocked)");
	}

	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog(
			$"{Target.entityData.EntityName} is no longer petrified!");
	}

	public override string GetExpireMessage()
	{
		return $"{Target.entityData.EntityName} is no longer petrified!";
	}
}
