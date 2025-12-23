using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectAllyEffect : EffectBase, IBeforeTakingDamage
{
	public ProtectRangeType ProtectRange { get; set; }
	public float ReducedPercentage { get; set; }

	public float SharePercentage { get; set; }

	public ProtectAllyEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1, ProtectRangeType protectRangeType = ProtectRangeType.All, float reducedPercentage = 0) : base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
	{
		ProtectRange = protectRangeType;
		ReducedPercentage = reducedPercentage;
		TriggerPhase = EffectTriggerPhase.EndOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		yield return base.ApplyEffect();
	}

	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} no longer pose protective stand ");
	}

	public bool CanProtect(EntityBase protector, EntityBase target, BattleSystem battleSystem)
	{
		return battleSystem.IsWithinProtectRange(protector, target, ProtectRange);
	}

	public IEnumerator OnBeforeTakingDamage(DamageContext ctx)
	{
		if (ctx.Target == Owner) yield break;
		if (!CanProtect(Owner, ctx.Target, BattleSystem.Instance)) yield break;
		if (ctx.Origin != SkillDefinition.BattleArt) yield break;
		float p = Mathf.Clamp01(SharePercentage);

		if (p >= 0.999f)
		{
			ctx.RedirectTo = Owner;
			ctx.BlockFurtherSharing = true; 
			if (ReducedPercentage > 0f)
				ctx.EffectiveDamage = Mathf.RoundToInt(ctx.EffectiveDamage * (ReducedPercentage / 100f));
		}
		else if (!ctx.BlockFurtherSharing && ctx.SplitRedirectTo == null && p > 0f)
		{
			ctx.SplitRedirectTo = Owner;
			ctx.SplitPercent = p;
		}
		yield break;
	}
}
