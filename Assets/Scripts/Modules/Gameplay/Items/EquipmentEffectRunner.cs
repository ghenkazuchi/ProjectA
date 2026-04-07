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
	// ── Public overloads (thin wrappers) ──────────────────────────────

	public IEnumerator Trigger(EquipEffectTrigger trigger, EntityBase target = null)
		=> TriggerCore(trigger, target, ApplyDefault);

	public IEnumerator Trigger(EquipEffectTrigger trigger, EntityBase target, DamageContext ctx)
		=> TriggerCore(trigger, target, (binding, effect, tracker, effectTarget)
			=> ApplyWithDamageContext(binding, effect, tracker, effectTarget, trigger, ctx));

	public IEnumerator Trigger(EquipEffectTrigger trigger, EntityBase target, StatusApplyContext ctx)
		=> TriggerCore(trigger, target, (binding, effect, tracker, effectTarget)
			=> ApplyWithStatusContext(binding, effect, tracker, effectTarget, ctx));

	// ── Shared core ────────────────────────────────────────────────────

	private IEnumerator TriggerCore(
		EquipEffectTrigger trigger,
		EntityBase target,
		System.Func<EquipEffectBinding, EffectBase, EffectUsageTracker, EntityBase, IEnumerator> applyEffect)
	{
		foreach (var kv in sourceToBindings)
		{
			object source = kv.Key;
			List<EquipEffectBinding> bindings = kv.Value;

			foreach (var binding in bindings)
			{
				if (binding == null || binding.effect == null) continue;
				if (binding.trigger != trigger) continue;

				if (!ProcessEffectBinding(source, binding, target,
					out EffectBase effect, out EffectUsageTracker usageTracker, out EntityBase effectTarget))
					continue;

				yield return applyEffect(binding, effect, usageTracker, effectTarget);
			}
		}
	}

	// ── Apply strategies ───────────────────────────────────────────────

	private IEnumerator ApplyDefault(EquipEffectBinding binding, EffectBase effect, EffectUsageTracker usageTracker, EntityBase effectTarget)
	{
		if (effect is IBattleEnd battleEndEffect)
		{
			yield return battleEndEffect.OnBattleEnd();
			yield break;
		}
		if (binding.effect.isInstantEffect)
		{
			yield return effectTarget.TriggerEffectDirectly(effect);
			ConsumeUsage(effect, usageTracker);
		}
		else
		{
			yield return effectTarget.AddEffect(effect);
		}
	}

	private IEnumerator ApplyWithDamageContext(EquipEffectBinding binding, EffectBase effect, EffectUsageTracker usageTracker, EntityBase effectTarget,
		EquipEffectTrigger trigger, DamageContext ctx)
	{
		if (binding.effect.isInstantEffect && ctx != null)
		{
			bool hooked = false;
			if (trigger == EquipEffectTrigger.OnDealingDamage && effect is IOnDealingDamage dealingDamage)
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
			else if (trigger == EquipEffectTrigger.OnTakingDamage && effect is IOnTakingDamage takingDamage)
			{
				hooked = true;
				yield return takingDamage.OnTakingDamage(ctx);
			}
			if (!hooked)
			{
				yield return effectTarget.TriggerEffectDirectly(effect);
				ConsumeUsage(effect, usageTracker);
			}
		}
		else
		{
			yield return effectTarget.AddEffect(effect);
		}
	}

	private IEnumerator ApplyWithStatusContext(EquipEffectBinding binding, EffectBase effect, EffectUsageTracker usageTracker, EntityBase effectTarget,
		StatusApplyContext ctx)
	{
		bool hooked = false;
		if (binding.effect.isInstantEffect && effect is IBeforeStatusApplied statusHook)
		{
			hooked = true;
			yield return statusHook.OnBeforeStatusApplied(ctx);

			if (ctx != null && ctx.Cancle)
				ConsumeUsage(effect, usageTracker);
		}

		if (!hooked)
		{
			yield return effectTarget.TriggerEffectDirectly(effect);
			ConsumeUsage(effect, usageTracker);
		}
	}

	private void ConsumeUsage(EffectBase effect, EffectUsageTracker usageTracker)
	{
		if (usageTracker == null)
		{
			return;
		}

		if (effect is ILimitedUsageTime)
		{
			return;
		}

		usageTracker.RecordUse();
	}

	private bool ProcessEffectBinding(object source, EquipEffectBinding binding, EntityBase overrideTarget, out EffectBase effect, out EffectUsageTracker usageTracker, out EntityBase effectTarget)
	{
		effect = null;
		usageTracker = null;
		effectTarget = overrideTarget ?? owner;
		
		float chance = Mathf.Clamp01(binding.procChance);
		if (source is Item item)
		{
			var tune = item.itemBaseData.GetTuning(item.currentItemGrade);
			chance += tune.procChanceMultiplier;
		}
		foreach (var e in owner.GetAllEffect())
		{
			if (e is IProcChanceModifier modProc)
			{
				chance = modProc.ModifyProcChance(chance);
			}
		}
		if (chance < 1f && Random.value > chance)
		{
			return false; 
		}

		effect = binding.effect.CreateRuntimeEffect(owner, effectTarget, binding.effect.MaxDuration);

		if (source is Weapon weapon)
		{
			usageTracker = weapon.GetEffectTracker(binding.effect.Name);
			usageTracker?.SetOnRecordedUse(null);
		}
		else if (source is Item i)
		{
			usageTracker = i.GetEffectTracker(binding.effect.Name);
			var tune = i.itemBaseData.GetTuning(i.currentItemGrade);
			usageTracker?.SetGradeBonus(tune.bonusUsagePerBattle, tune.bonusUsagePerLifeCycle);
			usageTracker?.SetOnRecordedUse(() => battleSystem?.RegisterBattleItemEffectUse(i));
		}

		if (effect is ILimitedUsageTime limited && usageTracker != null)
		{
			limited.SetUsageTracker(usageTracker);
			if (!usageTracker.CanUse())
			{
				return false;
			}
		}

		return true;
	}

}
