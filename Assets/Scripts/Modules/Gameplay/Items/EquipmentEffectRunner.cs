using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EquipmentEffectRunner
{
	private EntityBase owner;
	private BattleSystem battleSystem;
	private readonly Dictionary<object, List<EquipEffectBinding>> sourceToBindings = new();

	public EquipmentEffectRunner(EntityBase owner, BattleSystem battleSystem)
	{
		this.owner = owner;
		this.battleSystem = battleSystem;
	}

	public void UpdateBattleSystem(BattleSystem battleSystem)
	{
		this.battleSystem = battleSystem;
	}

	public void RegisterEffectBinding(object source, List<EquipEffectBinding> bindings)
	{
		if (!sourceToBindings.ContainsKey(source))
			sourceToBindings[source] = new List<EquipEffectBinding>();

		sourceToBindings[source].AddRange(bindings);
	}

	public void UnregisterEffectBinding(object source)
	{
		if (sourceToBindings.ContainsKey(source))
		{
			sourceToBindings.Remove(source);
		}
	}
	public IEnumerator Trigger(EquipEffectTrigger triggerType, EntityBase target = null)
	{
		foreach (var kv in sourceToBindings)
		{
			object source = kv.Key;
			List<EquipEffectBinding> bindings = kv.Value;
			foreach (var binding in bindings)
			{
				if (binding.trigger != triggerType) continue;


				var condition = binding.condition as IEffectCondition;
				if (condition != null && !condition.IsSastisfied(owner, target, battleSystem)) continue;

				EntityBase effectTarget = target ?? owner;
				EffectBase effect = binding.effect.CreateRuntimeEffect(owner, effectTarget, binding.effect.MaxDuration);
				EffectUsageTracker usageTracker = null;

				if (source is Weapon weapon)
				{
					usageTracker = weapon.GetEffectTracker(binding.effect.Name);
				}
				else if (source is Item item)
				{

					usageTracker = item.GetEffectTracker(binding.effect.Name);
					var tune = item.itemBaseData.GetTuning(item.currentItemGrade);
					usageTracker?.SetGradeBonus(tune.bonusUsagePerBattle, tune.bonusUsagePerLifeCycle);
				}
				ILimitedUsageTime limitedUsageTime = effect as ILimitedUsageTime;
				if (limitedUsageTime != null && usageTracker != null)
				{
					limitedUsageTime.SetUsageTracker(usageTracker);
					if (!usageTracker.CanUse())
					{
						continue;
					}
				}

				if (binding.effect.isInstantEffect)
				{
					yield return effectTarget.TriggerEffectDirectly(effect);
					usageTracker?.RecordUse();
				}
				else
				{
					yield return effectTarget.AddEffect(effect);
				}
			}
		}
	}
	public IEnumerator Trigger(EquipEffectTrigger trigger, EntityBase target, DamageContext ctx = null)
	{
		foreach (var kv in sourceToBindings) 
		{
			object source = kv.Key;
			List<EquipEffectBinding> bindings = kv.Value;

			foreach (var binding in bindings)
			{
				if (binding == null || binding.effect == null) continue;
				if (binding.trigger != trigger) continue;
				if (binding.condition is IEffectCondition cond && !cond.IsSastisfied(owner, target, battleSystem)) continue;

				var effectTarget = target ?? owner;
				var effect = binding.effect.CreateRuntimeEffect(owner, effectTarget, binding.effect.MaxDuration);
				EffectUsageTracker usageTracker = null;
				if (source is Weapon weapon) usageTracker = weapon.GetEffectTracker(binding.effect.Name);
				else if (source is Item item)
				{
					usageTracker = item.GetEffectTracker(binding.effect.Name);
					var tune = item.itemBaseData.GetTuning(item.currentItemGrade);
					usageTracker?.SetGradeBonus(tune.bonusUsagePerBattle, tune.bonusUsagePerLifeCycle);
				}
				ILimitedUsageTime limitedUsageTime = effect as ILimitedUsageTime;

				if (limitedUsageTime != null && usageTracker != null)
				{
					limitedUsageTime.SetUsageTracker(usageTracker);
					if (!usageTracker.CanUse())
					{
						continue;
					}
				}
				if (binding.effect.isInstantEffect && ctx != null)
				{
					bool hooked = false;
					if(trigger == EquipEffectTrigger.OnDealingDamage && effect is IOnDealingDamage dealingDamage)
					{
						hooked = true;
						yield return dealingDamage.OnDealingDamage(ctx);
					}
					else if (trigger == EquipEffectTrigger.OnBeforeDealingDamage && effect is IBeforeDealingDamage atkHook)
					{
						hooked = true;
						yield return atkHook.OnBeforeDealingDamage(ctx);
					}
					else if (trigger == EquipEffectTrigger.OnBeforeTakingDamage && effect is IBeforeTakingDamage defHook)
					{
						hooked = true;
						yield return defHook.OnBeforeTakingDamage(ctx);
					}
					else if(trigger == EquipEffectTrigger.OnTakingDamage && effect is IOnTakingDamage takingDamage)
					{
						hooked = true;
						yield return takingDamage.OnTakingDamage(ctx);
					}
					if (!hooked)
					{
							yield return effectTarget.TriggerEffectDirectly(effect);
					}
				}
				else
				{
					yield return effectTarget.AddEffect(effect);
				}
			}
		}
	}

	public IEnumerator Trigger(EquipEffectTrigger trigger, EntityBase target, StatusApplyContext ctx)
	{
		foreach (var kv in sourceToBindings)
		{
			object source = kv.Key;
			var bindings = kv.Value;

			foreach (var binding in bindings)
			{
				if (binding == null || binding.effect == null) continue;
				if (binding.trigger != trigger) continue;
				if (binding.condition is IEffectCondition cond && !cond.IsSastisfied(owner, target, battleSystem)) continue;

				var effectTarget = target ?? owner;
				var effect = binding.effect.CreateRuntimeEffect(owner, effectTarget, binding.effect.MaxDuration);

				// tracker
				EffectUsageTracker usageTracker = null;
				if (source is Weapon w) usageTracker = w.GetEffectTracker(binding.effect.Name);
				else if (source is Item item)
				{
					usageTracker = item.GetEffectTracker(binding.effect.Name);
					var tune = item.itemBaseData.GetTuning(item.currentItemGrade);
					usageTracker?.SetGradeBonus(tune.bonusUsagePerBattle, tune.bonusUsagePerLifeCycle);
				}

				if (effect is ILimitedUsageTime limited && usageTracker != null)
				{
					limited.SetUsageTracker(usageTracker);
					if (!usageTracker.CanUse()) continue;
				}

				// hook status
				bool hooked = false;
				if (binding.effect.isInstantEffect && effect is IBeforeStatusApplied statusHook)
				{
					hooked = true;
					yield return statusHook.OnBeforeStatusApplied(ctx);

					if (ctx != null && ctx.Cancle)
						usageTracker?.RecordUse();
				}

				if (!hooked)
				{
					yield return effectTarget.TriggerEffectDirectly(effect);
				}
			}
		}
	}

}
