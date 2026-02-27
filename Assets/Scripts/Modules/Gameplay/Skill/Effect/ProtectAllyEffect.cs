using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectAllyEffect : EffectBase, IBeforeTakingDamage
{
	public ProtectRangeType ProtectRange { get; set; }
	public float ReducedPercentage { get; set; }

	public float SharePercentage { get; set; }

	public ProtectAllyEffect(EffectData data, EntityBase owner, EntityBase target, int duration, ProtectRangeType protectRangeType = ProtectRangeType.All, float reducedPercentage = 0) : base(data, owner, target, duration)
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
		return BattleGridUtils.IsWithinProtectRange(protector, target, ProtectRange, battleSystem.playerParty, battleSystem.monsterParty);
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
