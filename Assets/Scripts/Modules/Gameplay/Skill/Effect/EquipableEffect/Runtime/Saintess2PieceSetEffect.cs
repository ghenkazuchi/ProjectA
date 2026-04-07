using System.Collections;
using UnityEngine;

public class Saintess2PieceSetEffect : EffectBase, IOnHealingReceived
{
	private Saintess2PieceSetEffectData saintessData;

	public Saintess2PieceSetEffect(Saintess2PieceSetEffectData data, EntityBase owner, EntityBase target, int duration)
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
		Debug.Log($"[Saintess2PC] {Owner.entityData.EntityName}: Healing boosted {ctx.FinalHealing} → {boosted} (+{saintessData.healingBonusPercent * 100}%)");
		ctx.FinalHealing = boosted;
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{
		yield break;
	}
}
