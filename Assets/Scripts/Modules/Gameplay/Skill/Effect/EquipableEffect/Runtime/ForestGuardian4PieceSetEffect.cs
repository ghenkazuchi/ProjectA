using System.Collections;
using UnityEngine;

public class ForestGuardian4PieceSetEffect : EffectBase, ITurnStart, IAfterSkillUsed
{
	private ForestGuardian4PieceSetEffectData fgData;
	private int currentStacks = 0;

	public ForestGuardian4PieceSetEffect(ForestGuardian4PieceSetEffectData data, EntityBase owner, EntityBase target, int duration)
		: base(data, owner, target, duration)
	{
		fgData = data;
	}

	public override IEnumerator ApplyEffect()
	{
		yield break; // Logic is handled by ITurnStart and IAfterSkillUsed hooks
	}

	public IEnumerator OnTurnStart()
	{
		// Only trigger for the owner of the set
		if (BattleSystem.Instance.currentTurnEntity != Owner) yield break;

		int priorStacks = currentStacks;
		currentStacks += fgData.stacksGainedPerTurn;
		currentStacks = Mathf.Min(currentStacks, fgData.maxStacks);

		if (currentStacks != priorStacks)
		{
			Debug.Log($"[ForestGuardian4PC] {Owner.entityData.EntityName} gained {currentStacks - priorStacks} stack(s). Current Stacks: {currentStacks}/{fgData.maxStacks}.");
		}
		
		yield break;
	}

	public IEnumerator OnAfterSkillUsed(SkillUseContext ctx)
	{
		// Only trigger if the set wearer used the skill
		if (ctx.Caster != Owner) yield break;

		// Only trigger on damage-dealing attacks
		if (ctx.Skill.SkillData.activeSkillType != ActiveSkillType.Damage) yield break;

		// Only trigger on Single Target skills
		if (ctx.Skill.SkillData.skillRange != SkillRange.SingleTarget) yield break;

		// Ensure we actually have stacks
		if (currentStacks <= 0) yield break;

		float procChance = currentStacks * fgData.baseProcChancePerStack;
		float roll = Random.value;

		if (roll > procChance)
		{
			Debug.Log($"[ForestGuardian4PC] {Owner.entityData.EntityName} failed proc roll ({roll} > {procChance}).");
			yield break;
		}

		// Proc succeeded! Consume a stack.
		currentStacks--;
		
		// Wait a small delay to make the chain attack pacing look good
		yield return new WaitForSeconds(0.4f);
		
		yield return BattleSystem.Instance.ShowDialog($"{Owner.entityData.EntityName} performs an additional attack! (Stacks left: {currentStacks})");

		// Pick the skill's target if still alive, otherwise default to a random valid target
		EntityBase target = null;
		var targets = BattleSystem.Instance.selectedTargets;
		if (targets != null && targets.Count > 0 && BattleSystem.Instance.IsEntityAlivePublic(targets[0]))
		{
			target = targets[0];
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
