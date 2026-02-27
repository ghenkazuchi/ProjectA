using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPillEffect : RestoreHPEffect, ILimitedUsageTime
{
	private EffectUsageTracker usageTracker;
	public SpringPillEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float amount = 0) : base(data, owner, target, duration)
	{
	}

	public int MaxUse { get; set; }
	public int MaxUsePerBattle { get; set; }
	public int CurrentUse { get; set; }
	public bool HasExpired { get; set; }
	public int CurrentBatteUsageTime { get; set; }
		
	public bool CheckExpirationOnBattleUsage()
	{
		throw new System.NotImplementedException();
	}

	public bool CheckLifeTimeExpiration()
	{
		throw new System.NotImplementedException();
	}

	public void Expired()
	{
	}

	public void ResetUse()
	{
		throw new System.NotImplementedException();
	}

	public void Use()
	{
		throw new System.NotImplementedException();
	}
}
