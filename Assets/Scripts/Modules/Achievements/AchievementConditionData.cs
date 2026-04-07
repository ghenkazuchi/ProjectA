using System.Collections.Generic;
using UnityEngine;

public enum MonsterKillFilterMode
{
	Any,
	MonsterType,
	SpecificMonster
}

public enum InteractionAchievementFilterMode
{
	Any,
	InteractableType,
	SpecificSpawnableObject
}

public enum RecruitAchievementFilterMode
{
	Any,
	SpecificCharacterData,
	SpecificTemplate
}

public abstract class AchievementConditionData : ScriptableObject
{
	[SerializeField] private int requiredCount = 1;

	public int RequiredCount => Mathf.Max(1, requiredCount);

    public virtual int GetProgressIncrement(IGameEvent achievementEvent)
    {
        return 1;
    }

	public abstract bool Matches(IGameEvent achievementEvent);
	public abstract string GetDescription();
}

public abstract class AchievementConditionData<T> : AchievementConditionData where T : struct, IGameEvent
{
    public override bool Matches(IGameEvent gameEvent)
    {
        if (gameEvent is T typedEvent)
        {
            return MatchesTyped(typedEvent);
        }
        return false;
    }

    protected abstract bool MatchesTyped(T typedEvent);
}


