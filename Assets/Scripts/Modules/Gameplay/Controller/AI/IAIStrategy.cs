using System.Collections.Generic;

public enum AIStrategyType
{
	Random,
	RuleBased,
}

public struct AIDecision
{
	public ActiveSkill Skill;
	public List<EntityBase> Targets;

	public bool IsValid => Skill != null && Targets != null && Targets.Count > 0;
}

public interface IAIStrategy
{
	AIDecision DecideAction(EntityBase monster, BattleSystem sys);
}
