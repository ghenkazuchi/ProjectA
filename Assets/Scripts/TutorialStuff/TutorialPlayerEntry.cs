using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TutorialPlayerEntry
{
	public RecruitableCharacterTemplate template;
	public int level;
	public GridPosition position;

	[Header("Unit State Overrides")]
	[Tooltip("If > 0, set HP to this percentage of MaxHP (e.g. 50 = half health). 0 = full HP.")]
	[Range(0, 100)]
	public int startingHPPercent = 0;

	[Tooltip("Effects to apply to this unit at battle start.")]
	public List<TutorialStartingEffect> startingEffects;
}
