using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResolveEffectBase : EffectBase, IOnTakingDamage, ILimitedUsageTime
{
	private EffectUsageTracker tracker;
	public float restorationPercentage;

	public ResolveEffectBase(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public IEnumerator OnTakingDamage(DamageContext context)
	{
		Debug.Log("Triggered Resolve Effect");
		var damageReceived = context.EffectiveDamage;
		Debug.Log($"{Target.GetCurrentHP()} {damageReceived}");
		if (Target.GetCurrentHP() <= 0)
		{
			if (tracker != null && tracker.CanUse())
			{
				int healAmount = Mathf.CeilToInt(Target.GetFinalStat(Stat.HP) * restorationPercentage);
				Target.Heal(healAmount);
				Debug.Log($"{Target.GetCurrentHP()}");	
				context.EffectiveDamage = 0;
				yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName}'s {Name} activates, restoring {healAmount} HP and negating the fatal damage!");
				TryConsumeUse();
			}
		}
	}

	public void SetUsageTracker(EffectUsageTracker tracker)
	{
		this.tracker = tracker;
	}
		
	public bool TryConsumeUse()
	{
		if(tracker != null && tracker.CanUse())
		{
			tracker.RecordUse();
			return true;
		}
		return false;
	}
}
