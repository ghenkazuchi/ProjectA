using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedirectDamageEffect : PassiveEffectBase
{
	public float redirectPercentage {  get; set; }
	public ProtectRangeType protectRange {  get; set; }

	public RedirectDamageEffect(float redirectPercentage, ProtectRangeType protectRange)
	{
		this.redirectPercentage = redirectPercentage;
		this.protectRange = protectRange;
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if (args.Length >= 3 && args[0] is EntityBase originalTarget && args[1] is EntityBase attacker && args[2] is int damage)
		{
			bool inRange = battleSystem.IsWithinProtectRange(owner, originalTarget, protectRange);
			if (!inRange) yield break;
		}
	}
	public bool CanRedirect(EntityBase protector, EntityBase target, BattleSystem battleSystem)
	{
		return battleSystem.IsWithinProtectRange(protector, target, protectRange);
	}
}

