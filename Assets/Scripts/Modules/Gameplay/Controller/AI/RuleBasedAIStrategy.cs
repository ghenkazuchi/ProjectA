using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Rule-based AI that walks a priority list top-to-bottom.
/// The first rule that matches produces the action.
/// Players can observe and learn the pattern:
///   1. Finish the Kill
///   2. Heal Critical Ally
///   3. Focus Before Enemy Heals (timeline-aware)
///   4. Exploit Element Advantage
///   5. Buff Before Ally Turn (timeline-aware)
///   6. Random Target
///   7. Fallback: random skill, or basic attack if no skills affordable
/// </summary>
public class RuleBasedAIStrategy : IAIStrategy
{
	public AIDecision DecideAction(EntityBase monster, BattleSystem sys)
	{
		var config = GetConfig(monster);
		AIDecision decision;

		// --- Priority 1: Finish the Kill ---
		if (config.finishKill)
		{
			decision = TryFinishKill(monster, sys);
			if (decision.IsValid)
			{
				Debug.Log("[AI] Rule 1 triggered: Finish the Kill");
				return decision;
			}
		}

		// --- Priority 2: Heal Critical Ally ---
		if (config.healCriticalAlly)
		{
			decision = TryHealCriticalAlly(monster, sys, config.healThreshold);
			if (decision.IsValid) return decision;
		}

		// --- Priority 3: Focus Before Enemy Heals ---
		if (config.focusBeforeEnemyHeals)
		{
			decision = TryFocusBeforeEnemyHeals(monster, sys);
			if (decision.IsValid) return decision;
		}

		// --- Priority 4: Exploit Element Advantage ---
		if (config.exploitElement)
		{
			decision = TryExploitElement(monster, sys);
			if (decision.IsValid) return decision;
		}

		// --- Priority 5: Removed (Buffing is now handled by Priority 6 via smart timeline targeting) ---

		// --- Priority 6: Random Target (Attack or Buff) ---
		if (config.randomTarget)
		{
			decision = TryRandomTarget(monster, sys);
			if (decision.IsValid) return decision;
		}

		// --- Fallback: random affordable skill, or basic attack if broke ---
		return FallbackRandom(monster, sys);
	}

	// ─────────────────────────────────────────────────────────────
	// Rule Implementations
	// ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Rule 1: If any offensive skill can kill a target, use it.
	/// </summary>
	private AIDecision TryFinishKill(EntityBase monster, BattleSystem sys)
	{
		var lethalOptions = new List<(ActiveSkill skill, EntityBase target)>();

		foreach (var skill in GetOffensiveSkills(monster))
		{
			if (!CanAfford(monster, skill)) continue;

			var enemies = GetAliveEnemies(monster, sys);
			foreach (var enemy in enemies.Where(e => IsValidTargetForSkill(e, skill, sys)))
			{
				int estimatedDmg = BattleAIController.EstimateDamage(monster, skill, enemy, sys);
				if (estimatedDmg >= enemy.GetCurrentHP())
				{
					lethalOptions.Add((skill, enemy));
				}
			}
		}

		if (lethalOptions.Count == 0) return default;

		var pick = lethalOptions[Random.Range(0, lethalOptions.Count)];
		return MakeDecision(pick.skill, pick.target, monster, sys);
	}

	/// <summary>
	/// Rule 2: If any ally is below the HP threshold and monster has a heal, use it.
	/// </summary>
	private AIDecision TryHealCriticalAlly(EntityBase monster, BattleSystem sys, float threshold)
	{
		var healSkills = monster.usableSkills
			.Where(s => s.SkillData.activeSkillType == ActiveSkillType.Heal && CanAfford(monster, s))
			.ToList();

		if (healSkills.Count == 0) return default;

		var allies = GetAliveAllies(monster, sys);
		EntityBase worstAlly = null;
		float worstRatio = 1f;

		foreach (var ally in allies)
		{
			float ratio = (float)ally.GetCurrentHP() / Mathf.Max(1, ally.MaxHp);
			if (ratio < threshold && ratio < worstRatio)
			{
				worstRatio = ratio;
				worstAlly = ally;
			}
		}

		if (worstAlly == null) return default;

		var healSkill = healSkills[0];
		return MakeDecision(healSkill, worstAlly, monster, sys);
	}

	/// <summary>
	/// Rule 3: If an enemy healer acts soon and a player unit is low HP,
	/// burst the low-HP target before the healer can save them.
	/// </summary>
	private AIDecision TryFocusBeforeEnemyHeals(EntityBase monster, BattleSystem sys)
	{
		var upcoming = sys.timelineManager.PeekNextEntities(5);
		var enemies = GetAliveEnemies(monster, sys);

		// Check if any upcoming player entity has a heal skill
		bool healerActsSoon = false;
		foreach (var entity in upcoming)
		{
			if (enemies.Contains(entity) && entity.usableSkills.Any(s => s.SkillData.activeSkillType == ActiveSkillType.Heal))
			{
				healerActsSoon = true;
				break;
			}
		}

		if (!healerActsSoon) return default;

		// Find the lowest-HP enemy that we can hit hard (potential heal target)
		var lowHpEnemies = enemies.OrderBy(e => e.GetCurrentHP()).ToList();
		foreach (var target in lowHpEnemies)
		{
			float hpRatio = (float)target.GetCurrentHP() / Mathf.Max(1, target.MaxHp);
			if (hpRatio > 0.5f) continue; // only focus targets below 50% HP

			var bestSkill = GetOffensiveSkills(monster).Where(s => CanAfford(monster, s) && IsValidTargetForSkill(target, s, sys)).FirstOrDefault();
			if (bestSkill != null)
				return MakeDecision(bestSkill, target, monster, sys);
		}

		return default;
	}

	/// <summary>
	/// Rule 4 (element) is unchanged below.
	/// </summary>

	// TryBuffBeforeAllyTurn has been removed in favor of smart random buffing.

	/// <summary>
	/// Exploit Element: Pick a skill with element advantage.
	/// </summary>
	private AIDecision TryExploitElement(EntityBase monster, BattleSystem sys)
	{
		var enemies = GetAliveEnemies(monster, sys);

		foreach (var skill in GetOffensiveSkills(monster))
		{
			if (!CanAfford(monster, skill)) continue;

			foreach (var enemy in enemies.Where(e => IsValidTargetForSkill(e, skill, sys)))
			{
				float mult = ElementalChart.GetMultiplier(skill.element, enemy.entityData.EntityElement);
				if (mult > 1f)
				{
					return MakeDecision(skill, enemy, monster, sys);
				}
			}
		}
		return default;
	}

	/// <summary>
	/// Rule 4: Pick a random offensive OR buff skill and a valid target.
	/// Avoids unfairly focus-firing the lowest HP target when no kill is available.
	/// </summary>
	private AIDecision TryRandomTarget(EntityBase monster, BattleSystem sys)
	{
		var randomSkills = GetOffensiveAndBuffSkills(monster).Where(s => CanAfford(monster, s)).ToList();
		if (randomSkills.Count == 0) return default;

		var enemies = GetAliveEnemies(monster, sys);
		if (enemies.Count == 0) return default;

		var validOptions = new List<(ActiveSkill skill, EntityBase target)>();
		foreach (var s in randomSkills)
		{
			var targetPool = s.SkillData.targetType == TargetType.Ally ? GetAliveAllies(monster, sys) : enemies;
			
			foreach (var t in targetPool)
			{
				if (IsValidTargetForSkill(t, s, sys)) validOptions.Add((s, t));
			}
		}

		if (validOptions.Count == 0) return default;
		var pick = validOptions[Random.Range(0, validOptions.Count)];
		return MakeDecision(pick.skill, pick.target, monster, sys);
	}

	/// <summary>
	/// Fallback: pick a random affordable skill. If no skill is affordable,
	/// use basic attack (just like how players are forced to basic attack).
	/// </summary>
	private AIDecision FallbackRandom(EntityBase monster, BattleSystem sys)
	{
		var usable = monster.usableSkills.Where(s => CanAfford(monster, s)).ToList();

		ActiveSkill chosenSkill;
		if (usable.Count > 0)
		{
			chosenSkill = usable[Random.Range(0, usable.Count)];
		}
		else
		{
			// Can't afford any skill → use basic attack
			chosenSkill = sys.basicAttack;
		}

		var targets = BattleAIController.GetAITargetsForSkill(monster, chosenSkill, sys.playerParty, sys.monsterParty);

		if (targets.Count == 0) return default;
		return new AIDecision { Skill = chosenSkill, Targets = targets };
	}

	// ─────────────────────────────────────────────────────────────
	// Helpers
	// ─────────────────────────────────────────────────────────────

	private AIBehaviorConfig GetConfig(EntityBase monster)
	{
		if (monster.entityData is MonsterData md && md.aiBehavior != null)
			return md.aiBehavior;

		// Default config if none set
		return new AIBehaviorConfig();
	}

	private List<ActiveSkill> GetOffensiveSkills(EntityBase monster)
	{
		return monster.usableSkills
			.Where(s => s.SkillData.activeSkillType == ActiveSkillType.Damage)
			.ToList();
	}

	/// <summary>
	/// Gets a list of Damage and Buff skills the monster can use for its random action phase.
	/// </summary>
	private List<ActiveSkill> GetOffensiveAndBuffSkills(EntityBase monster)
	{
		return monster.usableSkills
			.Where(s => s.SkillData.activeSkillType == ActiveSkillType.Damage || s.SkillData.activeSkillType == ActiveSkillType.Buff)
			.ToList();
	}

	private List<EntityBase> GetAliveEnemies(EntityBase monster, BattleSystem sys)
	{
		return sys.playerParty.GetAllEntitiesInParty()
			.Where(e => e != null && e.GetCurrentHP() > 0)
			.ToList();
	}

	private List<EntityBase> GetAliveAllies(EntityBase monster, BattleSystem sys)
	{
		return sys.monsterParty.GetAllEntitiesInParty()
			.Where(e => e != null && e.GetCurrentHP() > 0)
			.ToList();
	}

	private bool IsValidTargetForSkill(EntityBase target, ActiveSkill skill, BattleSystem sys)
	{
		if (skill.SkillData.targetType == TargetType.Enemy && skill.SkillData.skillRange == SkillRange.SingleTarget && skill.SkillData.activeSkillType == ActiveSkillType.Damage)
		{
			return BattleGridUtils.IsTargetable(target, sys.playerParty, sys.monsterParty);
		}
		return true;
	}

	private bool CanAfford(EntityBase monster, ActiveSkill skill)
	{
		if (skill.currentMPCost > 0 && monster.GetCurrentMP() < skill.currentMPCost)
			return false;
		if (skill.currentSPCost > 0 && monster.GetCurrentSP() < skill.currentSPCost)
			return false;
		return true;
	}

	private bool IsAoE(ActiveSkill skill)
	{
		var range = skill.SkillData.skillRange;
		return range == SkillRange.AllTarget || range == SkillRange.AllAllies
			|| range == SkillRange.LineTarget || range == SkillRange.LineAllies;
	}

	/// <summary>
	/// Resolves the full target list for a skill, respecting SkillRange.
	/// For AoE skills: returns all valid targets (ignoring preferredTarget).
	/// For single-target skills: returns just the preferred target.
	/// </summary>
	private List<EntityBase> ResolveTargets(ActiveSkill skill, EntityBase preferredTarget, EntityBase monster, BattleSystem sys)
	{
		if (IsAoE(skill))
		{
			// AoE: use the standard target resolver which handles AllTarget/LineTarget
			return BattleAIController.GetAITargetsForSkill(monster, skill, sys.playerParty, sys.monsterParty);
		}
		// Single target: use the preferred target the rule picked
		return new List<EntityBase> { preferredTarget };
	}

	/// <summary>
	/// Build an AIDecision with proper target resolution based on SkillRange.
	/// preferredTarget = the entity the rule wants to hit (used for single-target;
	/// AoE skills resolve to all valid targets automatically).
	/// </summary>
	private AIDecision MakeDecision(ActiveSkill skill, EntityBase preferredTarget, EntityBase monster, BattleSystem sys)
	{
		var targets = ResolveTargets(skill, preferredTarget, monster, sys);
		if (targets.Count == 0) return default;
		return new AIDecision
		{
			Skill = skill,
			Targets = targets
		};
	}
}
