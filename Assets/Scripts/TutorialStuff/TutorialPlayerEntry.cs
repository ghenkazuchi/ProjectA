using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TutorialPlayerEntry
{
	public RecruitableCharacterTemplate template;
	public int level;
	public GridPosition position;

	[Header("Unit State Overrides")]
	[Range(0, 100)]
	public int startingHPPercent = 0;

	[Tooltip("Effects to apply to this unit at battle start.")]
	public List<TutorialStartingEffect> startingEffects;
}
