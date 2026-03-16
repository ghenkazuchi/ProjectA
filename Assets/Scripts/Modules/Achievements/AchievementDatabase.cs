using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementDatabase", menuName = "Achievements/Achievement Database")]
public class AchievementDatabase : ScriptableObject
{
	[SerializeField] private List<AchievementDefinition> achievements = new List<AchievementDefinition>();

	public IReadOnlyList<AchievementDefinition> Achievements => achievements;
}
