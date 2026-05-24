using UnityEngine;

[System.Serializable]
public class TutorialStartingEffect
{
	[Tooltip("The effect asset to apply (drag any EffectData ScriptableObject here).")]
	public EffectData effectData;

	[Tooltip("Duration override. If 0, uses the effect's default MaxDuration.")]
	public int durationOverride = 0;

	[Tooltip("Number of stacks to apply (for stackable effects). Minimum 1.")]
	public int stacks = 1;
}
