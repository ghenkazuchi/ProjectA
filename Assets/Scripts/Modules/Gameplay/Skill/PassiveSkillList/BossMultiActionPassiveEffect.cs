using System.Collections;
using UnityEngine;

public class BossMultiActionPassiveEffect : PassiveEffectBase, IBonusTurnPreviewEffect
{
	private readonly int extraTurnsPerRound;
	private int lastRoundNumber = -1;
	private int grantedThisRound;

	public BossMultiActionPassiveEffect(int extraTurnsPerRound)
	{
		this.extraTurnsPerRound = Mathf.Max(0, extraTurnsPerRound);
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if (owner == null || battleSystem == null || extraTurnsPerRound <= 0)
		{
			yield break;
		}

		if (owner.GetCurrentHP() <= 0)
		{
			yield break;
		}

		if (!battleSystem.DidEntityTakeActionThisTurn(owner))
		{
			yield break;
		}

		int currentRound = battleSystem.CurrentTimelineRound;
		if (currentRound != lastRoundNumber)
		{
			lastRoundNumber = currentRound;
			grantedThisRound = 0;
		}

		if (grantedThisRound >= extraTurnsPerRound)
		{
			yield break;
		}

		if (!battleSystem.timelineManager.GrantBonusTurn(owner))
		{
			yield break;
		}

		grantedThisRound++;
		battleSystem.UpdateTimelineUI();
	}

	public int GetRemainingBonusTurnsForRound(int roundNumber)
	{
		if (extraTurnsPerRound <= 0)
		{
			return 0;
		}

		if (roundNumber != lastRoundNumber)
		{
			return extraTurnsPerRound;
		}

		return Mathf.Max(0, extraTurnsPerRound - grantedThisRound);
	}
}
