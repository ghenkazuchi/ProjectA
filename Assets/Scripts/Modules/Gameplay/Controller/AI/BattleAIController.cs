using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Central AI orchestrator. Selects a strategy for the monster
/// and provides shared targeting utilities used by all strategies.
/// </summary>
public static class BattleAIController
{
	private static readonly RandomAIStrategy _randomStrategy = new RandomAIStrategy();
	private static readonly RuleBasedAIStrategy _ruleBasedStrategy = new RuleBasedAIStrategy();

	/// <summary>
	/// Main entry point: pick the appropriate strategy and return a decision.
	/// </summary>
	public static AIDecision ChooseAction(EntityBase monster, BattleSystem sys)
	{
		IAIStrategy strategy = GetStrategyForMonster(monster);
		return strategy.DecideAction(monster, sys);
	}

	/// <summary>
	/// Determines which AI strategy a monster should use based on its MonsterData.
	/// Designers set aiStrategy in the Inspector on each MonsterData ScriptableObject.
	/// </summary>
	private static IAIStrategy GetStrategyForMonster(EntityBase monster)
	{
		if (monster.entityData is MonsterData md)
		{
			switch (md.aiStrategy)
			{
				case AIStrategyType.RuleBased:
					return _ruleBasedStrategy;
				case AIStrategyType.Random:
				default:
					return _randomStrategy;
			}
		}
		return _randomStrategy;
	}

	/// <summary>
	/// Given a skill, find valid targets for the AI entity (used by all strategies).
	/// Moved from BattleSystem.GetAITargets().
	/// </summary>
	public static List<EntityBase> GetAITargetsForSkill(EntityBase aiEntity, ActiveSkill skill, PlayerParty playerParty, MonsterParty monsterParty)
	{
		var targets = ListPool<EntityBase>.Get();
		var availableTargets = ListPool<EntityBase>.Get();

		if (skill.SkillData.targetType == TargetType.Enemy)
		{
			var alive = playerParty.GetAllEntitiesInParty().FindAll(e => e != null && e.GetCurrentHP() > 0);
			// Single-target damage skills can't hit protected back-row units
			if (skill.SkillData.skillRange == SkillRange.SingleTarget && skill.SkillData.activeSkillType == ActiveSkillType.Damage)
				alive.RemoveAll(e => !BattleGridUtils.IsTargetable(e, playerParty, monsterParty));
			
			availableTargets.AddRange(alive);
		}
		else if (skill.SkillData.targetType == TargetType.Ally || skill.SkillData.targetType == TargetType.SelfOrAllies)
		{
			// Determine which party the AI actor belongs to so they buff their own team!
			BaseParty myParty = BattleGridUtils.GetPartyOf(aiEntity, playerParty, monsterParty);
			if (myParty == null) myParty = monsterParty; // Fallback

			var aliveAllies = myParty.GetAllEntitiesInParty().FindAll(e => e != null && e.GetCurrentHP() > 0);
			
			if (skill.SkillData.activeSkillType == ActiveSkillType.Buff)
			{
				// Smart Buff Targeting
				var validUnbuffedAllies = new List<EntityBase>();
				
				foreach (var ally in aliveAllies)
				{
					bool alreadyHasBuffs = true;
					foreach (var effect in skill.SkillData.effectsToApply)
					{
						// If the ally doesn't have an active buff matching the effect to apply, they are a valid target
						if (!ally.currentActiveBuffs.Any(b => b.Effect == effect.effectData.Effect && b.Name == effect.effectData.Name))
						{
							alreadyHasBuffs = false;
							break;
						}
					}
					
					if (!alreadyHasBuffs)
					{
						validUnbuffedAllies.Add(ally);
					}
				}
				
				// Optional: If everyone is already buffed, abort the skill completely (Return empty list to force a re-roll in FallbackRandom)
				if (validUnbuffedAllies.Count == 0)
				{
					ListPool<EntityBase>.Release(targets);
					ListPool<EntityBase>.Release(availableTargets);
					return new List<EntityBase>();
				}

				// Priority: Pick the unbuffed ally who acts soonest in the timeline
				var upcoming = BattleSystem.Instance.timelineManager.PeekNextEntities(5);
				EntityBase chosenAlly = null;
				foreach (var entity in upcoming)
				{
					if (validUnbuffedAllies.Contains(entity))
					{
						chosenAlly = entity;
						break;
					}
				}

				// If no unbuffed ally is in the immediate timeline, just pick a random unbuffed one
				if (chosenAlly == null)
				{
					chosenAlly = validUnbuffedAllies[Random.Range(0, validUnbuffedAllies.Count)];
				}
				
				availableTargets.Add(chosenAlly);
			}
			else
			{
				availableTargets.AddRange(aliveAllies);
			}
		}
		else if (skill.SkillData.targetType == TargetType.Self)
		{
			targets.Add(aiEntity);
			var resultSelf = new List<EntityBase>(targets);
			ListPool<EntityBase>.Release(targets);
			ListPool<EntityBase>.Release(availableTargets);
			return resultSelf;
		}

		if (availableTargets.Count == 0)
		{
			ListPool<EntityBase>.Release(targets);
			ListPool<EntityBase>.Release(availableTargets);
			return new List<EntityBase>();
		}

		switch (skill.SkillData.skillRange)
		{
			case SkillRange.SingleTarget:
			case SkillRange.SingleAlly:
				targets.Add(availableTargets[Random.Range(0, availableTargets.Count)]);
				break;
			case SkillRange.AllTarget:
			case SkillRange.AllAllies:
				targets.AddRange(availableTargets);
				break;
			case SkillRange.LineTarget:
			case SkillRange.LineAllies:
				var lineTargets = GetRandomLineTargets(availableTargets);
				targets.AddRange(lineTargets);
				ListPool<EntityBase>.Release(lineTargets);
				break;
		}
		var result = new List<EntityBase>(targets);
		ListPool<EntityBase>.Release(targets);
		ListPool<EntityBase>.Release(availableTargets);

		return result;
	}

	/// <summary>
	/// Picks a random subset of targets for line-type skills.
	/// Moved from BattleSystem.GetRandomLineTargets().
	/// </summary>
	public static List<EntityBase> GetRandomLineTargets(List<EntityBase> availableTargets)
	{
		var lineTargets = ListPool<EntityBase>.Get();

		if (availableTargets.Count > 0)
		{
			int targetCount = Random.Range(1, Mathf.Min(4, availableTargets.Count + 1));
			var shuffled = ListPool<EntityBase>.Get();
			shuffled.AddRange(availableTargets);

			for (int i = 0; i < targetCount; i++)
			{
				int randomIndex = Random.Range(0, shuffled.Count);
				lineTargets.Add(shuffled[randomIndex]);
				shuffled.RemoveAt(randomIndex);
			}

			ListPool<EntityBase>.Release(shuffled);
		}

		var result = new List<EntityBase>(lineTargets);
		ListPool<EntityBase>.Release(lineTargets);
		return result;
	}

	/// <summary>
	/// Picks a default target for a forced basic attack.
	/// Moved from BattleSystem.PickDefaultTargetForBasicAttack().
	/// </summary>
	public static EntityBase PickDefaultTargetForBasicAttack(EntityBase actor, PlayerParty playerParty, MonsterParty monsterParty)
	{
		var playerTeam = playerParty.GetAllEntitiesInParty();
		var monsterTeam = monsterParty.GetAllEntitiesInParty();

		bool actorIsPlayer = playerTeam.Contains(actor);

		var sameSide = actorIsPlayer ? playerTeam : monsterTeam;

		return sameSide.FirstOrDefault(e => e != null && e != actor && e.GetCurrentHP() > 0);
	}

	/// <summary>
	/// Lightweight damage estimate for AI scoring (uses DamageCalculator without full context).
	/// </summary>
	public static int EstimateDamage(EntityBase source, ActiveSkill skill, EntityBase target, BattleSystem sys)
	{
		var ctx = new DamageContext();
		sys.ApplySkillModifiersPreview(skill, ref ctx);
		return DamageCalculator.CalculateDamageWithContext(
			source, skill, target, ctx, sys,
			0f, false);
	}
}
