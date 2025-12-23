using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ActiveSkill
{
	[field: SerializeField]
	public ActiveSkillData SkillData { get; set; }
	[field: SerializeField]
	public int currentMPCost;
	public int currentSPCost;
	public float hitChance;
	public float currentSkillDamage;
	public Element element;
	public ActiveSkill(ActiveSkillData skillData)
	{
		SkillData = skillData;
		currentMPCost = skillData.baseMpCost;
		currentSPCost = skillData.baseSpCost;
		hitChance = skillData.baseHitChance;
		element = skillData.skillElement;
		currentSkillDamage = skillData.baseSkillDamage;
	}
	public bool RequiresTarget
	{
		get
		{
			return SkillData.targetType == TargetType.Ally || SkillData.targetType == TargetType.Enemy;
		}
	}
	public IEnumerator Execute(EntityBase owner, List<EntityBase> targets)
	{
		var ctx = new SkillContext
		{
			Owner = owner,
			AllTarget = targets,
			HitTarget = targets,
		};
		yield return Execute(ctx);
	}

	public IEnumerator Execute(SkillContext context)
	{
		if (SkillData.effectsToApply == null) yield break;
		foreach (var effect in SkillData.effectsToApply)
		{
			if (effect.effectData == null) continue;	

			IEnumerable<EntityBase> effectRecipent = effect.effectData.AppliesTo switch
			{
				TargetType.Self => new [] { context.Owner },
				TargetType.Ally => (effect.effectData.requiredHit? context.HitTarget : context.AllTarget),
				TargetType.Enemy => (effect.effectData.requiredHit ? context.HitTarget : context.AllTarget),
				_ => context.AllTarget
			};
			foreach(var entity in effectRecipent)
			{
				if (Random.value > Mathf.Clamp01(effect.procChance)) continue;
				var runtimeEffect = effect.effectData.CreateRuntimeEffect(context.Owner, entity, effect.turnDuration);
				if (effect.effectData.isInstantEffect)
				{
					yield return entity.TriggerEffectDirectly(runtimeEffect);
				}
				else
				{
					yield return entity.AddEffect(runtimeEffect);
				}
			}
		}
	} 
	public void SetSkillElement(Element newElement)
	{
		element = newElement;
	}
	public void SetMPCost(int newCost)
	{
		currentMPCost = newCost;
	}
	public void SetSPCost(int newCost)
	{
		currentSPCost = newCost;
	}	
	public void IncreaseDamage(int increasedDamage)
	{
		currentSkillDamage = increasedDamage;
	}
}
public struct SkillContext
{
	public EntityBase Owner;
	public List<EntityBase> AllTarget;
	public List<EntityBase> HitTarget;
}

