using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispelBuffEffect : EffectBase, IBeforeDealingDamage
{
	public int DispelAmount;

	public DispelBuffEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}
	public override IEnumerator ApplyEffect()
	{
		yield return null;
	}

	public IEnumerator OnBeforeDealingDamage(DamageContext ctx)
	{
		Debug.Log("BeforeDealingDamage");
		foreach(var buff in ctx.Target.currentActiveBuffs)
		{
			Debug.Log($"{ctx.Target.entityData.EntityName} has buff: {buff.Name}");
		}
		var list = ctx.Target.currentActiveBuffs;
		if (list == null || list.Count == 0) yield break;
		int toRemove = Mathf.Min(DispelAmount, list.Count);
		var pool = new List<EffectBase>(list);
		for (int i = 0; i < toRemove && pool.Count > 0; i++)
		{
			int pick = Random.Range(0, pool.Count);
			var buff = pool[pick];
			pool.RemoveAt(pick);
			if (!buff.CanBeRemoved) { i--; continue; }
			ctx.Target.RemoveEffect(buff);
			Debug.Log($"{ctx.Target.entityData.EntityName} had {buff.Name} dispelled!");
		}
		Owner.RemoveEffect(this);
		yield return null;
	}

	public override IEnumerator RemoveEffect()
	{
		yield return null;
	}
}
