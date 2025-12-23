using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectData : ScriptableObject
{
	public EffectType EffectType;
	public Effect Effect;
	public string Name;
	public int MaxDuration;
	public bool CanBeRemoved;
	public bool Stackable;
	public int MaxStack;
	public TargetType AppliesTo;
	public bool isInstantEffect;
	public bool hasLimitedUsage;

	public EffectActiveTiming timing = EffectActiveTiming.OnCast;
	public bool requiredHit = false;

	public Sprite effectIcon;

	public abstract EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration);
}
