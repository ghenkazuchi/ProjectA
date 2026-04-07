using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Achievements/Conditions/High Damage")]
public class HighDamageCondition : AchievementConditionData<DamageDealtEvent>
{
	public int minDamage;
	protected override bool MatchesTyped(DamageDealtEvent typedEvent)
	{
		return typedEvent.damageAmount >= minDamage;
	}

	public override string GetDescription() => $"Deal at least {minDamage} damage in a single hit.";
}
