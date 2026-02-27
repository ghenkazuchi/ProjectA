using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolveSkillEffect : EffectBase, IOnTakingDamage
{
	public float restorationPercentage;
	public ResolveSkillEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public IEnumerator OnTakingDamage(DamageContext context)
	{
		Debug.Log("Triggered Resolve Effect");	
		var damageReceived = context.EffectiveDamage;
		Debug.Log($"{Target.GetCurrentHP()} {damageReceived}");
		if (Target.GetCurrentHP() <=  0)
		{
			int healAmount = Mathf.CeilToInt(Target.GetFinalStat(Stat.HP) * restorationPercentage);
			Target.Heal(healAmount);
			Debug.Log($"{Target.GetCurrentHP()}");
			context.EffectiveDamage = 0;
			yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName}'s {Name} activates, restoring {healAmount} HP and negating the fatal damage!");
			Target.RemoveEffect(this);
		}
	}
}
