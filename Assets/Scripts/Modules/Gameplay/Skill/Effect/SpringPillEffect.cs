using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPillEffect : RestoreHPEffect, ILimitedUsageTime
{
	private EffectUsageTracker usageTracker;
	public SpringPillEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1, float amount = 0) : base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack, amount)
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
