using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateAttackEffect : EffectBase, IOnAllyDealingDamage, IOnAllyAfterSkillUsed
{
	private CoordinateAttackEffectData caData;
	private int currentStacks = 0;

	public CoordinateAttackEffect(CoordinateAttackEffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
		caData = data;
	}

	public override IEnumerator ApplyEffect()
	{
		yield break; 
	}

	public IEnumerator OnAllyDealingDamage(EntityBase ally, EntityBase target, DamageContext ctx)
	{
		int priorStacks = currentStacks;
		currentStacks++;
		currentStacks = Mathf.Min(currentStacks, caData.maxStacks);
		
		yield break;
	}

	public IEnumerator OnAllyAfterSkillUsed(SkillUseContext ctx, List<EntityBase> targetsGotHit)
	{

		if (ctx.Skill.SkillData.activeSkillType != ActiveSkillType.Damage) yield break;

		if (ctx.Skill.SkillData.skillRange != SkillRange.SingleTarget) yield break;

		float procChance = caData.baseProcChance + (currentStacks * caData.procChancePerStack);
		float roll = Random.value;

		if (roll > procChance)
		{
			Debug.Log($"[CoordinateAttack] {Owner.entityData.EntityName} failed proc roll ({roll} > {procChance}).");
			yield break;
		}

		if (currentStacks > 0)
        {
            currentStacks = 0;
        }
		
		yield return new WaitForSeconds(0.4f);
		
		yield return BattleSystem.Instance.ShowDialog($"{Owner.entityData.EntityName} coordinates an attack!");

		EntityBase target = null;
		if (targetsGotHit != null && targetsGotHit.Count > 0 && BattleSystem.Instance.IsEntityAlivePublic(targetsGotHit[0]))
		{
			target = targetsGotHit[0];
		}
		else
		{
			target = BattleAIController.PickDefaultTargetForBasicAttack(Owner, BattleSystem.Instance.playerParty, BattleSystem.Instance.monsterParty);
		}
		
		if (target != null)
		{
			yield return BattleSystem.Instance.actionExecutor.ForceBasicAttack(Owner, target);
		}
	}

	public override IEnumerator RemoveEffect()
	{
		currentStacks = 0;
		yield break;
	}
}
