using UnityEngine;

[System.Serializable]
public class TutorialStepData
{
	[TextArea(3, 10)] public string guidanceText;

	[Header("Timing")]
	public TutorialStepTiming timing;

	[Tooltip("Target area or object to point at.")]
	public TutorialPointerTarget pointerTarget;

	[Tooltip("Direction the finger should point from.")]
	public PointerDirection pointerDirection;

	[Tooltip("Distance between the finger and the target. Adjust this if the finger is overlapping the target.")]
	public float pointerOffset = 60f;

	[Tooltip("Shape of the highlight frame (Square, Circle, Triangle). The UI script must have these sprites assigned!")]
	public HighlightShape frameShape = HighlightShape.Square;

	[Tooltip("If greater than 0, forces the Highlight Frame to this exact width and height. If 0, it auto-sizes based on the target.")]
	public Vector2 customFrameSize = Vector2.zero;

	[Tooltip("Shifts the Highlight Frame by this many pixels (X, Y). Useful if the target is against the screen edge and the frame gets cut off.")]
	public Vector2 customFrameOffset = Vector2.zero;

	[Tooltip("Extra padding to add to the highlight frame around the target (only used if customFrameSize is 0).")]
	public Vector2 highlightPadding = new Vector2(20f, 20f);

	[Tooltip("If greater than 0, forces the Finger Icon to this exact width and height. If 0, uses the default size.")]
	public Vector2 customFingerSize = Vector2.zero;

	[Header("Text Panel Customization")]
	[Tooltip("If true, completely hides the text panel and guidance text for this step.")]
	public bool hideTextPanel = false;

	[Tooltip("If not (0,0), forces the Text Panel to this specific anchored position on the Canvas.")]
	public Vector2 customTextPosition = Vector2.zero;

	[Tooltip("If not (0,0), forces the Text Panel to this exact width and height. Text will resize to fit if using Stretch anchors.")]
	public Vector2 customTextPanelSize = Vector2.zero;

	[Tooltip("If greater than 0, forces the Guidance Text to this specific font size. If 0, uses the default size set in the Inspector.")]
	public float customFontSize = 0f;

	[Header("Player Interaction")]
	public TutorialInteractionMode interactionMode;

	[Header("Input Requirements (used together in WaitForCorrectAction)")]
	[Tooltip("The main action the player must select (e.g., Skill, Attack)")]
	public BattleAction requiredActionType;

	[Tooltip("Check this to force the player to pick a specific skill from the list")]
	public bool requireSpecificSkill;
	public int requiredSkillIndex;

	[Tooltip("Check this to force the player to pick a specific target")]
	public bool requireSpecificTarget;
	public int requiredTargetIndex;

	[Header("Specific Unit Target (for unit-based pointers)")]
	public bool targetSpecificUnit;
	public UnitType targetUnitSide;
	public int targetUnitSlotIndex;

	[Header("Safety")]
	public bool preventPlayerDeath;
}

public enum TutorialStepTiming
{
	OnBattleStart,
	BeforePlayerTurn,
	DuringPlayerTurn,
	AfterPlayerAction,
	BeforeMonsterTurn,
	AfterMonsterAction,
	OnBattleEnd
}

public enum TutorialInteractionMode
{
	TapToContinue,
	WaitForCorrectAction,
	WaitForUnitClick,
	WaitForCloseEffectList
}

public enum HighlightShape
{
	Square,
	Circle,
	Triangle
}

public enum TutorialPointerTarget
{
	None,
	ActionSelector,
	SkillSelector,
	TargetArena,
	Timeline,
	CurrentTurnUnitHealthBar,
	SpecificUnitHealthBar,
	SpecificUnit,
	SpecificAction_Skill,
	SpecificAction_Attack,
	SpecificAction_Defend,
	SpecificAction_Switch,
	SpecificSkill,
	EnemyUnits,
	AllyUnits,
	ActiveEffectContainer,
	ActiveEffectExitButton
}

public enum PointerDirection
{
	FromBelow,
	FromAbove,
	FromLeft,
	FromRight
}