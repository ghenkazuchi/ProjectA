using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// On dealing damage, forces the hit target to swap grid positions
/// with a back-row ally in the same column (if one exists).
/// If the target IS in the back row, swaps them to the front instead.
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

		// Find someone in the opposite row (same column) to swap with
		// Front row = X=0, Back row = X=1
		int swapRow = (targetPos.x == 0) ? 1 : 0;
		GridPosition swapPos = new GridPosition(swapRow, targetPos.y);

		EntityBase swapPartner = targetParty.GetEntityAtPosition(swapPos);

		if (swapPartner != null && swapPartner.GetCurrentHP() > 0)
		{
			// Swap the two entities
			targetParty.TrySwitchPositions(targetPos, swapPos);

			// Sync visual positions in battle
			if (targetParty is PlayerParty)
				sys.SyncPlayerBattleUnitsFromPartySlots();

			string targetName = target.entityData.EntityName;
			string partnerName = swapPartner.entityData.EntityName;
			yield return sys.ShowDialog($"{targetName} was forced to swap positions with {partnerName}!");

			sys.UpdateTimelineUI();
		}
		else
		{
			// No valid swap partner in the opposite row — try any alive ally in the other row
			var candidates = targetParty.partySlots
				.Where(s => s.entity != target
					&& s.entity != null
					&& s.entity.GetCurrentHP() > 0
					&& s.position.x != targetPos.x)
				.ToList();

			if (candidates.Count > 0)
			{
				var pick = candidates[Random.Range(0, candidates.Count)];
				targetParty.TrySwitchPositions(targetPos, pick.position);

				if (targetParty is PlayerParty)
					sys.SyncPlayerBattleUnitsFromPartySlots();

				string targetName = target.entityData.EntityName;
				string partnerName = pick.entity.entityData.EntityName;
				yield return sys.ShowDialog($"{targetName} was forced to swap positions with {partnerName}!");

				sys.UpdateTimelineUI();
			}
		}
	}

	public override IEnumerator RemoveEffect()
	{
		yield return null;
	}
}
