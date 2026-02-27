using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Default AI: picks a random usable skill and random valid targets.
/// This preserves the original BattleSystem behavior exactly.
/// </summary>
public class RandomAIStrategy : IAIStrategy
{
	public AIDecision DecideAction(EntityBase monster, BattleSystem sys)
	{
		var decision = new AIDecision();

		if (monster.usableSkills.Count == 0)
			return decision;

		decision.Skill = monster.usableSkills[Random.Range(0, monster.usableSkills.Count)];
		decision.Targets = BattleAIController.GetAITargetsForSkill(
			monster, decision.Skill, sys.playerParty, sys.monsterParty);

		return decision;
	}
}
