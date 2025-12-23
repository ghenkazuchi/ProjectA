using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlowTargetOnHitEffect : PassiveEffectBase
{
	public float slowPercentage;
	public SlowTargetOnHitEffect(float slowPercentage)
	{
		this.slowPercentage = slowPercentage;
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if (args.Length >= 3 && args[0] is EntityBase target && args[1] is EntityBase source)
		{
			if (source == owner)
			{
				yield return battleSystem.ShowDialog($"{target.entityData.EntityName} action speed is reduced!");
				TurnUnit targetTurnUnit = battleSystem.timelineManager.GetTurnUnit(target);

				if (targetTurnUnit != null)
				{
					float slowAmount = targetTurnUnit.actionValue * (slowPercentage / 100f);

					battleSystem.timelineManager.SlowDownEntity(target, slowAmount);
					battleSystem.UpdateTimelineUI();
				}
			}
		}
	}
}
