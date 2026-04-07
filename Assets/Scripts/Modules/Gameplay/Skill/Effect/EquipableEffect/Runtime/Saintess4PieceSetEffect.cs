using System.Collections;
using UnityEngine;

public class Saintess4PieceSetEffect : EffectBase, IOnHealingReceived, IAfterSkillUsed
{
	private Saintess4PieceSetEffectData saintessData;

	public Saintess4PieceSetEffect(Saintess4PieceSetEffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
		saintessData = data;
	}

	public override IEnumerator ApplyEffect()
	{
		yield break;
	}

	public IEnumerator OnHealingReceived(HealingContext ctx)
	{
		// Only boost healing when the set wearer is the one casting the heal
		if (ctx.Healer != Owner) yield break;

		int boosted = Mathf.CeilToInt(ctx.FinalHealing * (1f + saintessData.healingBonusPercent));
		Debug.Log($"[Saintess4PC] {Owner.entityData.EntityName}: Healing boosted {ctx.FinalHealing} → {boosted} (+{saintessData.healingBonusPercent * 100}%)");
		ctx.FinalHealing = boosted;
		yield break;
	}

	public IEnumerator OnAfterSkillUsed(SkillUseContext ctx)
	{
		// Only trigger MP refund for Spells
		if (ctx.Definition != SkillDefinition.Spell) yield break;
		if (ctx.Caster != Owner) yield break;
		if (ctx.MPCost <= 0) yield break;

		float roll = Random.value;
		if (roll > saintessData.mpRefundChance) yield break;

		ctx.Caster.RestoreMP(ctx.MPCost);
		yield return BattleSystem.Instance.ShowDialog($"{Owner.entityData.EntityName}'s divine blessing refunded {ctx.MPCost} MP!");
	}

	public override IEnumerator RemoveEffect()
	{
		yield break;
	}
}
