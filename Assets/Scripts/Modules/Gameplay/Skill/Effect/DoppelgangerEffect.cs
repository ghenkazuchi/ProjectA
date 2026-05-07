using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class DoppelgangerPassiveEffect : PassiveEffectBase
{
	private readonly Sprite[] transformVfxFrames;
	private readonly float transformVfxFps;
	private readonly Sprite statCopyIcon;

	public DoppelgangerPassiveEffect(Sprite[] vfxFrames, float vfxFps, Sprite icon)
	{
		transformVfxFrames = vfxFrames;
		transformVfxFps = vfxFps;
		statCopyIcon = icon;
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		// --- 1. Pick a random alive player character ---
		var playerUnits = battleSystem.UnitRegistry.PlayerBattleUnits;
		var aliveTargets = new List<EntityBase>();
		foreach (var unit in playerUnits)
		{
			if (unit != null && unit.character != null && unit.IsAlive())
			{
				aliveTargets.Add(unit.character);
			}
		}

		if (aliveTargets.Count == 0)
		{
			Debug.LogWarning("[Doppelganger] No alive player targets found!");
			yield break;
		}

		EntityBase victim = aliveTargets[Random.Range(0, aliveTargets.Count)];
		string originalName = owner.entityData.EntityName;

		// --- 2. Show initial dialog ---
		yield return battleSystem.ShowDialog(
			$"{originalName} is studying {victim.entityData.EntityName}...");

		// --- 3. Play transform VFX on the monster's BattleUnit ---
		BattleUnit ownerUnit = battleSystem.UnitRegistry.FindUnit(owner);
		if (ownerUnit != null)
		{
			// Play VFX frames if available
			if (transformVfxFrames != null && transformVfxFrames.Length > 0 && ownerUnit.HitVfx != null)
			{
				ownerUnit.HitVfx.SetVFX(transformVfxFrames, transformVfxFps > 0 ? transformVfxFps : 15f);
				yield return ownerUnit.HitVfx.PlayOnce(0.3f);
			}

			// Flash white and fade out, then swap sprite and fade back in
			Image unitImage = ownerUnit.unitImage;
			if (unitImage != null)
			{
				Color originalColor = unitImage.color;

				// Flash white
				Sequence flashSequence = DOTween.Sequence();
				flashSequence.Append(unitImage.DOColor(Color.white, 0.3f));
				flashSequence.AppendInterval(0.15f);
				// Fade out
				flashSequence.Append(unitImage.DOFade(0f, 0.3f));
				yield return flashSequence.WaitForCompletion();

				// --- 4. Now do the actual data copy while invisible ---
				CopyEntityData(owner, victim);

				// Swap the sprite to the new appearance
				unitImage.sprite = owner.entityData.EntitySprite;

				// Fade back in with new appearance
				Sequence revealSequence = DOTween.Sequence();
				revealSequence.Append(unitImage.DOFade(1f, 0.4f));
				revealSequence.Join(unitImage.DOColor(originalColor, 0.2f));
				yield return revealSequence.WaitForCompletion();

				unitImage.color = originalColor;
			}
			else
			{
				// No unitImage — just copy data
				CopyEntityData(owner, victim);
			}

			// Refresh the full BattleUnit UI (health bar, level, etc.)
			ownerUnit.SetUp();
		}
		else
		{
			// Fallback: just copy data without animation
			CopyEntityData(owner, victim);
		}

		// --- 5. Refresh the timeline UI portraits ---
		battleSystem.UpdateTimelineUI();

		// --- 6. Show result dialog ---
		yield return battleSystem.ShowDialog(
			$"Dopplerganger has transformed into {victim.entityData.EntityName}!");
	}

	private void CopyEntityData(EntityBase owner, EntityBase victim)
	{
		owner.entityData = ScriptableObject.Instantiate(owner.entityData);
		owner.entityData.EntitySprite = victim.entityData.EntitySprite;
		owner.entityElement = victim.entityElement;
		owner.usableSkills.Clear();
		foreach (var skill in victim.usableSkills)
		{
			if (skill != null && skill.SkillData != null)
			{
				owner.usableSkills.Add(new ActiveSkill(skill.SkillData));
			}
		}

		// Copy passive skills
		owner.activePassiveSkills.Clear();
		foreach (var passive in victim.activePassiveSkills)
		{
			if (passive != null && passive.PassiveSkillData != null)
			{
				owner.activePassiveSkills.Add(
					new PassiveSkill(passive.PassiveSkillData, passive.PassiveSkillData.initCoolDown));
			}
		}

		// Reinitialize passive runner with new passives
		owner.InitializePassiveRunner(BattleSystem.Instance);

		// --- Copy all stats EXCEPT Speed and Max HP using an invisible effect ---
		var statData = ScriptableObject.CreateInstance<DoppelgangerStatCopyEffectData>();
		statData.Name = "Doppelganger Stats";
		statData.EffectType = EffectType.Buff;
		statData.Effect = Effect.StatModifier;
		statData.CanBeRemoved = false;
		statData.Stackable = false;
		statData.effectIcon = statCopyIcon; // Set the icon so it isn't blank in the UI!

		var statCopyEffect = new DoppelgangerStatCopyEffect(statData, owner, owner, 9999);
		var snapshotStats = new Dictionary<Stat, float>();
		var victimTraits = new Dictionary<Trait, int>();
		foreach (Trait t in victim.TraitListCache)
		{
			victimTraits[t] = victim.GetCurrentTrait(t);
		}

		foreach (Stat stat in System.Enum.GetValues(typeof(Stat)))
		{
			snapshotStats[stat] = victim.CalculateSingleStat(stat, victimTraits);
		}
		statCopyEffect.Initialize(snapshotStats);
		
		BattleSystem.Instance.StartCoroutine(owner.AddEffect(statCopyEffect));
	}
}

public class DoppelgangerStatCopyEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new DoppelgangerStatCopyEffect(this, owner, target, duration);
	}
}

public class DoppelgangerStatCopyEffect : EffectBase, IStatModify
{
	private Dictionary<Stat, float> copiedStats;

	public DoppelgangerStatCopyEffect(EffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
	}

	public void Initialize(Dictionary<Stat, float> statsToCopy)
	{
		copiedStats = statsToCopy;
	}

	public float ModifyStat(Stat statType, float baseValue, EntityBase entity)
	{
		if (copiedStats != null && copiedStats.TryGetValue(statType, out float copiedValue))
		{
			if (statType != Stat.HP && statType != Stat.ActionSpeed)
			{
				return copiedValue;
			}
		}
		return baseValue;
	}
	public override IEnumerator ApplyEffect()
	{
		yield break;
	}
}
