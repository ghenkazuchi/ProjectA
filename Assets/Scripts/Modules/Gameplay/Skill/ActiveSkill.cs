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

	public IEnumerator ExecuteEffect(SkillContext ctx,EffectActiveTiming timing)
	{
		if (SkillData.effectsToApply == null || SkillData.effectsToApply.Count == 0 ) yield break;
		foreach(var effect in SkillData.effectsToApply)
		{
			if (effect.activeTiming != EffectActiveTiming.OnCast) continue;
			if (effect.effectData == null) continue;
			float procChance = effect.procChance;
			foreach(var e in ctx.Owner.GetAllEffect())
			{
				if(e is IProcChanceModifier m)
				{
					procChance = m.ModifyProcChance(procChance);
				}
			}
			IEnumerable<EntityBase> recipients = SkillData.targetType switch
			{
				TargetType.Self => new[] { ctx.Owner },
				TargetType.Enemy => (timing == EffectActiveTiming.OnHit ? ctx.HitTarget : ctx.AllTarget),
				TargetType.Ally => (timing == EffectActiveTiming.OnHit ? ctx.HitTarget : ctx.AllTarget),
				TargetType.SelfOrAllies => (timing == EffectActiveTiming.OnHit ? ctx.HitTarget : ctx.AllTarget),
				_ => ctx.AllTarget
			};
			foreach (var r in recipients)
			{
				if (r == null) continue;

				if (procChance < 1f && !DamageCalculator.CheckEffectApplication(ctx.Owner, r, procChance))
				{
				    continue;
				}

				var runtime = effect.effectData.CreateRuntimeEffect(ctx.Owner, r, effect.turnDuration);

				if (effect.effectData.isInstantEffect) yield return r.TriggerEffectDirectly(runtime);
				else yield return r.AddEffect(runtime);
			}
		}
	}

	public IEnumerator ExecuteBeforeDealingDamageEffect(SkillContext context)
	{
		if (context.AllTarget == null && context.HitTarget == null) yield break;
		if(SkillData.effectsToApply == null) yield break;
		foreach (var effect in SkillData.effectsToApply)
		{
			if (effect.effectData == null) continue;

			float procChance = effect.procChance;
			foreach (var e in context.Owner.GetAllEffect())
			{
				if (e is IProcChanceModifier m)
				{
					procChance = m.ModifyProcChance(procChance);
				}
			}
			if (effect.activeTiming == EffectActiveTiming.OnCast)
			{
				foreach (var entity in context.AllTarget)
				{
					if (procChance < 1f && !DamageCalculator.CheckEffectApplication(context.Owner, entity, procChance))
					{
						yield return BattleSystem.Instance.ShowDialog($"{effect.effectData.name} resisted by {entity.entityData.EntityName}!");
						continue;
					}

					var effectInstance = effect.effectData.CreateRuntimeEffect(context.Owner, entity, effect.turnDuration);
					if (effectInstance is IBeforeDealingDamage beforeDealingDamage)
					{
						Debug.Log("Triggering BeforeDealingDamage Effect");
						var damageCtx = new DamageContext()
						{
							Source = context.Owner,
							Target = entity,
							Origin = SkillData.skillDefinition,
							SkillName = SkillData.skillName
						};
						yield return beforeDealingDamage.OnBeforeDealingDamage(damageCtx);
					}
				}
			}
		}
	}
	public IEnumerator ExecuteOnDealingDamageEffect(SkillContext skillContext)
	{
		if (skillContext.AllTarget == null && skillContext.HitTarget == null) yield break;
		foreach(var entity in skillContext.HitTarget)
		{
			Debug.Log($"{entity.entityData.EntityName}");
		}
		if (SkillData.effectsToApply == null) yield break;
		foreach (var effect in SkillData.effectsToApply)
		{
			if (effect.effectData == null) continue;
			float procChance = effect.procChance;
			foreach (var e in skillContext.Owner.GetAllEffect())
			{
				if (e is IProcChanceModifier m)
				{
					procChance = m.ModifyProcChance(procChance);
				}
			}
			foreach (var entity in skillContext.HitTarget)
			{
				if (procChance < 1f && !DamageCalculator.CheckEffectApplication(skillContext.Owner, entity, procChance))
				{
					continue;
				}

				var effectInstance = effect.effectData.CreateRuntimeEffect(skillContext.Owner, entity, effect.turnDuration);
				if (!effect.effectData.isInstantEffect)
				{
					Debug.Log("Adding OnDealingDamage Effect");
					yield return entity.AddEffect(effectInstance);
				}
				if (effectInstance is IOnDealingDamage onDealingDamage)
				{
					var damageCtx = new DamageContext()
					{
						Source = skillContext.Owner,
						Target = entity,
						EffectiveDamage = skillContext.totalDamageDeal,
						Origin = SkillData.skillDefinition,
						SkillName = SkillData.skillName
					};
					yield return onDealingDamage.OnDealingDamage(damageCtx);
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
	public int totalDamageDeal;
	public int totalHealing;
}

