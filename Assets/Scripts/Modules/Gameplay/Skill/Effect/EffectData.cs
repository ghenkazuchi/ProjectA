using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectVfxTrigger
{
	Apply,
	TurnStart,
	TurnEnd,
	Expire
}

[System.Serializable]
public class EffectVfxClipData
{
	public Sprite[] frames;
	public float fps = 30f;
	public float fadeOut = 0.25f;	

	public bool HasFrames => frames != null && frames.Length > 0;
}

public abstract class EffectData : ScriptableObject
{
	public EffectType EffectType;
	public Effect Effect;
	public string Name;
	public string Description;
	public int MaxDuration;
	public bool CanBeRemoved;
	public bool Stackable;
	public int MaxStack;
	public bool isInstantEffect;
	public bool isPassiveEquipmentEffect;
	public EffectTag tags;


	public Sprite effectIcon;

	[Header("Status VFX (Optional)")]
	public EffectVfxClipData applyVfx;
	public EffectVfxClipData turnStartVfx;
	public EffectVfxClipData turnEndVfx;
	public EffectVfxClipData expireVfx;

	[Header("Audio (Optional)")]
	public AudioConfig applySFX;

	public bool TryGetStatusVfx(EffectVfxTrigger trigger, out EffectVfxClipData clip)
	{
		clip = trigger switch
		{
			EffectVfxTrigger.Apply => applyVfx,
			EffectVfxTrigger.TurnStart => turnStartVfx,
			EffectVfxTrigger.TurnEnd => turnEndVfx,
			EffectVfxTrigger.Expire => expireVfx,
			_ => null
		};

		return clip != null && clip.HasFrames;
	}

	public abstract EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration);
}
