using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraTurnEffect : EffectBase
{
	public ExtraTurnEffect(EffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
		TriggerPhase = EffectTriggerPhase.Instant;
	}

	public override IEnumerator ApplyEffect()
	{
		// Grant bonus turn via TimelineManager
		if (TimelineManager.Instance.GrantBonusTurn(Target))
		{
			// Since this was approved as an instant effect on kill, we show the dialog
			yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} gained an EXTRA TURN!");
		}
		
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{
		yield break;
	}
}
