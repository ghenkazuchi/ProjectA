using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public abstract class EffectBase
{
	private static int _nextRuntimeId = 1;
	public int RuntimeId { get; } = _nextRuntimeId++;
	private readonly EffectData sourceData;
	public EffectType EffectType { get; protected set; }
	public Effect Effect { get; protected set; }
	public EffectTriggerPhase TriggerPhase { get; protected set; }

	public string Name { get; protected set; }
	public EntityBase Owner { get; protected set; }
	public EntityBase Target { get; protected set; }

	public int InitialDuration { get; protected set; }
	public int CurrentDuration { get; protected set; }

	public bool CanBeRemoved { get; protected set; }
	public bool Stackable { get; protected set; }
	public int MaxStack { get; protected set; }
	public int CurrentStack { get; protected set; }

	public bool HasDuration => InitialDuration > 0;

	public Sprite EffectIcon { get; protected set; }
	public EffectTag Tags { get; protected set; }
	public EffectData SourceData => sourceData;

	public bool HasTag(EffectTag tag) => (Tags & tag) != 0;

	public event System.Action<EffectBase> OnChanged;
	public event System.Action<EffectBase, EffectVfxTrigger> OnVfxRequested;

	private void NotifyChanged() => OnChanged?.Invoke(this);
	protected void RequestVfx(EffectVfxTrigger trigger) => OnVfxRequested?.Invoke(this, trigger);

	/// <summary>New simplified constructor — pulls config from EffectData SO.</summary>
	public EffectBase(EffectData data, EntityBase owner, EntityBase target, int duration)
	{
		sourceData = data;
		EffectType = data.EffectType;
		Effect = data.Effect;
		Name = data.Name;
		Owner = owner;
		Target = target;
		InitialDuration = duration;
		CurrentDuration = InitialDuration;
		CanBeRemoved = data.CanBeRemoved;
		Stackable = data.Stackable;
		MaxStack = data.MaxStack;
		CurrentStack = 1;
		EffectIcon = data.effectIcon;
		Tags = data.tags;
	}

	[System.Obsolete("Use EffectBase(EffectData, owner, target, duration) instead.")]
	public EffectBase(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1)
	{
		EffectType = effectType;
		Effect = effect;
		Name = name;
		Owner = owner;
		Target = target;
		InitialDuration = duration;
		CurrentDuration = InitialDuration;
		CanBeRemoved = canBeRemoved;
		Stackable = stackable;
		MaxStack = maxStack;
		CurrentStack = 1;
		EffectIcon = icon;
	}

	public virtual string GetExpireMessage() => $"{Target.entityData.EntityName} is no longer affected by {Name}.";

	public bool TryGetStatusVfx(EffectVfxTrigger trigger, out EffectVfxClipData clip)
	{
		if (sourceData != null)
		{
			return sourceData.TryGetStatusVfx(trigger, out clip);
		}

		clip = null;
		return false;
	}

	public void PlayApplySFX()
	{
		if (sourceData != null && sourceData.isPassiveEquipmentEffect)
			return;

		if (sourceData != null && sourceData.applySFX != null)
		{
			AudioManager.Instance.PlaySFX(sourceData.applySFX);
		}
		else
		{
			AudioManager.Instance.PlayDefaultStatusApplySFX(Name);
		}
	}

	public virtual IEnumerator ApplyEffect()
	{
		RequestVfx(EffectVfxTrigger.Apply);
		// Note: Central status apply SFX is played by EntityBase (AddEffect / TriggerEffectDirectly)
		// to avoid double playing or playing during equipment rebuild/loading phases.
		yield return BattleSystem.Instance.ShowDialog($"{Name} applied to {Target.entityData.EntityName}!");
	}

	public virtual void RefreshEffect()
	{
		CurrentDuration = InitialDuration;
		Debug.Log($"Effect {Name} on {Target.entityData.EntityName} has been refreshed to {CurrentDuration} turns.");
		RequestVfx(EffectVfxTrigger.Apply);
		NotifyChanged();
	}
	public virtual void AddStack(int amount)
	{
		if (Stackable && CurrentStack < MaxStack)
		{
			CurrentStack += amount;
			Debug.Log(amount + $" stacks of {Name} added to {Target.entityData.EntityName}. Current stack: {CurrentStack}/{MaxStack}");
			RequestVfx(EffectVfxTrigger.Apply);
			NotifyChanged();
			if (this is IThreshholdable threshholdableEffect)
			{
				threshholdableEffect.CheckThreshold(CurrentStack, MaxStack);	
			}
		}
	}
	public virtual void RemoveStack(int amount)
	{
		if (Stackable && CurrentStack > 0)
		{
			CurrentStack -= amount;
			Debug.Log(amount + $" stacks of {Name} removed from {Target.entityData.EntityName}. Current stack: {CurrentStack}/{MaxStack}");
			NotifyChanged();
			if (CurrentStack <= 0)
			{
				RemoveEffect();
			}
		}
	}
	public virtual void ReduceDuration()
	{
		if (CurrentDuration > 0)
		{
			CurrentDuration--;
			Debug.Log($"Effect {Name} on {Target.entityData.EntityName} has {CurrentDuration} turns left.");
			NotifyChanged();
			if (CurrentDuration <= 0)
			{
				RemoveEffect();
			}
		}
	}

	public virtual IEnumerator TickDownAndRemove()
	{
		if(!HasDuration) yield break;
		if (CurrentDuration > 0) CurrentDuration--;
		if(CurrentDuration <= 0)
		{
			yield return Target.RemoveEffectCoroutine(this);
			yield break;
		}
	}
	public virtual IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog(GetExpireMessage());
	}
}
