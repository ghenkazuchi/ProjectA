using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On dealing damage, forces the hit target to swap grid positions
/// with the ally in the opposite row but the same lane.
/// If there is no living ally in that paired slot, no swap happens.
/// </summary>
public class ForceSwapEffect : EffectBase, IOnDealingDamage
{
	public ForceSwapEffect(EffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
	}

	public override IEnumerator ApplyEffect()
	{
		yield return null; // Instant effect — logic runs in OnDealingDamage
	}

	public IEnumerator OnDealingDamage(DamageContext ctx)
	{
		var sys = BattleSystem.Instance;
		EntityBase target = ctx.Target;
		if (target == null || target.GetCurrentHP() <= 0) yield break;

		// Find which party the target belongs to
		BaseParty targetParty = BattleGridUtils.GetPartyOf(target, sys.playerParty, sys.monsterParty);
		if (targetParty == null) yield break;

		// Get the target's current position
		GridPosition targetPos = targetParty.GetCharacterPosition(target);
		if (targetPos == null) yield break;

		// Find the paired slot in the opposite row but same lane.
		// Front row = X=0, Back row = X=1, lane = Y.
		int swapRow = (targetPos.x == 0) ? 1 : 0;
		GridPosition swapPos = new GridPosition(swapRow, targetPos.y);

		EntityBase swapPartner = targetParty.GetEntityAtPosition(swapPos);

		if (swapPartner == null || swapPartner.GetCurrentHP() <= 0)
			yield break;

		targetParty.TrySwitchPositions(targetPos, swapPos);

		if (targetParty is PlayerParty)
			sys.SyncPlayerBattleUnitsFromPartySlots();

		string targetName = target.entityData.EntityName;
		string partnerName = swapPartner.entityData.EntityName;
		yield return sys.ShowDialog($"{targetName} was forced to swap positions with {partnerName}!");

		sys.UpdateTimelineUI();
	}

	public override IEnumerator RemoveEffect()
	{
		yield return null;
	}
}
