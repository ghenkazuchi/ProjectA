using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Core tutorial brain. Drives the step sequence, restricts input, and scripts monster AI.
/// Lives on a GameObject in the InGame scene alongside the BattleSystem.
/// 
/// KEY DESIGN: The player controls the game with keyboard (Arrow keys + Z confirm + X cancel).
/// Tutorial steps that require a specific action (WaitForCorrectAction) do NOT block the
/// coroutine. Instead, they show the overlay and set restriction state, then RETURN so
/// HandlePlayerTurnPublic() can give the player keyboard control. The guard clauses in
/// BattleInputHandler enforce the restrictions. When the correct action completes, the
/// overlay auto-hides.
/// </summary>
public class TutorialSequenceRunner : MonoBehaviour
{
	public static TutorialSequenceRunner Instance { get; private set; }
	public bool IsTutorialActive => scenario != null;

	private TutorialScenarioData scenario;
	private int stepIndex;
	private int turnNumber;

	// Active restriction state (set by WaitForCorrectAction steps)
	private TutorialStepData activeRestriction;
	private bool hasActiveRestriction;

	private bool waitingForUnitClick;
	private int requiredUnitSlotIndex;
	private UnitType requiredUnitSide;
	
	private bool waitingForEffectListClose;

	public bool IsWaitingForUnitClick => waitingForUnitClick;
	public bool IsWaitingForEffectListClose => waitingForEffectListClose;

	private void Awake()
	{
		Instance = this;
	}

	// ─── Lifecycle ──────────────────────────────────────────────────

	public void Begin(TutorialScenarioData data)
	{
		scenario = data;
		stepIndex = 0;
		turnNumber = 0;
		activeRestriction = null;
		hasActiveRestriction = false;
		Debug.Log($"[Tutorial] Started scenario: {data.scenarioTitle}");
	}

	public void End()
	{
		Debug.Log("[Tutorial] Ended.");
		scenario = null;
		stepIndex = 0;
		turnNumber = 0;
		activeRestriction = null;
		hasActiveRestriction = false;
		waitingForUnitClick = false;
		waitingForEffectListClose = false;
	}

	// ─── Called by BattleLoopState at each phase ────────────────────

	/// <summary>
	/// Runs all consecutive steps that match the given timing phase.
	/// 
	/// TapToContinue steps: block here, wait for player to press Z, then continue.
	/// WaitForCorrectAction steps: show overlay + set restrictions, then RETURN
	///   immediately so the battle flow can give the player keyboard control.
	/// </summary>
	public IEnumerator RunStepsForPhase(TutorialStepTiming phase)
	{
		if (scenario == null) yield break;

		var overlay = TutorialOverlayUI.Instance;
		if (overlay == null)
		{
			Debug.LogWarning("[Tutorial] TutorialOverlayUI not found!");
			yield break;
		}

		while (stepIndex < scenario.steps.Count)
		{
			var step = scenario.steps[stepIndex];
			if (step.timing != phase) break;

			if (step.interactionMode == TutorialInteractionMode.TapToContinue)
			{
				// BLOCKING: Show text, wait for Z press, dismiss, advance
				yield return overlay.ShowStep(step);
				stepIndex++;
			}
			else if (step.interactionMode == TutorialInteractionMode.WaitForCorrectAction)
			{
				activeRestriction = step;
				hasActiveRestriction = true;
				yield return overlay.ShowStep(step);
				stepIndex++;
				yield break;
			}
			else if (step.interactionMode == TutorialInteractionMode.WaitForUnitClick)
			{
				waitingForUnitClick = true;
				requiredUnitSide = step.targetUnitSide;
				requiredUnitSlotIndex = step.targetUnitSlotIndex;
				activeRestriction = step; // Store to know the phase
				yield return overlay.ShowStep(step);
				stepIndex++;
				yield break;
			}
			else if (step.interactionMode == TutorialInteractionMode.WaitForCloseEffectList)
			{
				waitingForEffectListClose = true;
				activeRestriction = step; // Store to know the phase
				yield return overlay.ShowStep(step);
				stepIndex++;
				yield break;
			}
		}
	}

	// ─── Called when the player successfully performs an action ──────

	public bool HasActiveRestriction => hasActiveRestriction;

	/// <summary>
	/// True while the tutorial is transitioning between steps (fading out → fading in).
	/// All battle inputs should be blocked during this window.
	/// </summary>
	public bool IsTransitioning { get; private set; }

	/// <summary>
	/// True if the tutorial overlay is currently active and animating or waiting for tap.
	/// Under these conditions, all standard game inputs should be disabled.
	/// </summary>
	public bool IsInputBlocked
	{
		get
		{
			if (!IsTutorialActive) return false;
			if (IsTransitioning) return true;
			if (TutorialOverlayUI.Instance != null)
			{
				var state = TutorialOverlayUI.Instance.CurrentState;
				if (state == TutorialOverlayUI.TutorialOverlayState.FadingIn ||
					state == TutorialOverlayUI.TutorialOverlayState.WaitingForTap ||
					state == TutorialOverlayUI.TutorialOverlayState.FadingOut)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Called when the player successfully passes a restricted menu (e.g. Action or Skill confirm).
	/// This immediately hides the current restriction overlay and advances to the next step,
	/// allowing for seamless multi-step menus.
	/// </summary>
	public void AdvanceIfRestrictionMet()
	{
		if (!hasActiveRestriction) return;

		var phase = activeRestriction.timing;
		hasActiveRestriction = false;
		activeRestriction = null;
		IsTransitioning = true;

		StartCoroutine(AdvanceCoroutine(phase));
	}

	public void OnUnitClicked(BattleUnit unit)
	{
		if (!waitingForUnitClick || activeRestriction == null) return;
		if (unit.type != requiredUnitSide) return;

		int index = -1;
		var overlay = TutorialOverlayUI.Instance;
		if (overlay == null) return;

		if (unit.type == UnitType.PlayerUnit)
		{
			index = System.Array.IndexOf(overlay.PlayerBattleUnits, unit);
		}
		else
		{
			index = System.Array.IndexOf(overlay.MonsterBattleUnits, unit);
		}

		if (index == requiredUnitSlotIndex)
		{
			waitingForUnitClick = false;
			var phase = activeRestriction.timing;
			activeRestriction = null;
			IsTransitioning = true;
			StartCoroutine(AdvanceCoroutine(phase));
			
			// Open the panel
			var effectUI = FindFirstObjectByType<BattleUnitActiveEffectListUI>(FindObjectsInactive.Include);
			if (effectUI != null)
				effectUI.Show(unit);
		}
	}

	public void OnEffectListClosed()
	{
		if (!waitingForEffectListClose || activeRestriction == null) return;

		waitingForEffectListClose = false;
		var phase = activeRestriction.timing;
		activeRestriction = null;
		IsTransitioning = true;
		StartCoroutine(AdvanceCoroutine(phase));
	}

	private IEnumerator AdvanceCoroutine(TutorialStepTiming phase)
	{	
		// Wait for the current overlay to finish fading out BEFORE running the next step
		if (TutorialOverlayUI.Instance != null)
			yield return StartCoroutine(TutorialOverlayUI.Instance.HideOverlay());

		yield return StartCoroutine(RunStepsForPhase(phase));
		IsTransitioning = false;
	}

	/// <summary>
	/// Called by BattleStateMachine after HandlePlayerTurnPublic completes.
	/// If there was an active restriction overlay, hide it now.
	/// </summary>
	public void OnPlayerActionCompleted()
	{
		if (hasActiveRestriction)
		{
			hasActiveRestriction = false;
			activeRestriction = null;

			// Hide the overlay
			if (TutorialOverlayUI.Instance != null)
				StartCoroutine(TutorialOverlayUI.Instance.HideOverlay());
		}
	}

	/// <summary>
	/// Called when a turn ends and the next entity is about to act.
	/// </summary>
	public void OnTurnAdvanced()
	{
		turnNumber++;
	}

	// ─── Input Restriction Queries (called by BattleInputHandler) ───

	/// <summary>
	/// Is the player allowed to confirm this action?
	/// Only restricts when a WaitForCorrectAction step is active.
	/// </summary>
	public bool IsActionAllowed(BattleAction action)
	{
		if (!hasActiveRestriction) return true;
		return action == activeRestriction.requiredActionType;
	}

	/// <summary>
	/// Is the player allowed to confirm this skill index?
	/// </summary>
	public bool IsSkillAllowed(int skillIndex)
	{
		if (!hasActiveRestriction) return true;
		if (!activeRestriction.requireSpecificSkill) return true;
		return skillIndex == activeRestriction.requiredSkillIndex;
	}

	/// <summary>
	/// Is the player allowed to select this target index?
	/// </summary>
	public bool IsTargetAllowed(int targetIndex)
	{
		if (!hasActiveRestriction) return true;
		if (!activeRestriction.requireSpecificTarget) return true;
		return targetIndex == activeRestriction.requiredTargetIndex;
	}

	/// <summary>
	/// Should player characters be prevented from dying?
	/// Checks both the active restriction step and the current step.
	/// </summary>
	public bool ShouldPreventDeath()
	{
		if (!IsTutorialActive) return false;
		if (hasActiveRestriction && activeRestriction.preventPlayerDeath) return true;
		if (stepIndex < scenario.steps.Count && scenario.steps[stepIndex].preventPlayerDeath) return true;
		return false;
	}

	// ─── Monster AI Scripting ───────────────────────────────────────

	/// <summary>
	/// Returns a scripted AI decision if a directive exists for this turn + monster,
	/// otherwise falls back to normal AI.
	/// </summary>
	public AIDecision GetScriptedMonsterDecision(EntityBase monster, BattleSystem sys)
	{
		int slotIndex = GetMonsterSlotIndex(monster, sys);

		var directive = scenario.monsterDirectives?.Find(
			d => d.turnNumber == turnNumber && d.monsterSlotIndex == slotIndex);

		if (directive == null)
			return BattleAIController.ChooseAction(monster, sys);

		// Build forced decision — use specified skill or fallback to first usable skill
		ActiveSkill skill = (directive.forcedSkillIndex >= 0 && directive.forcedSkillIndex < monster.usableSkills.Count)
			? monster.usableSkills[directive.forcedSkillIndex]
			: (monster.usableSkills.Count > 0 ? monster.usableSkills[0] : null);

		var targets = new List<EntityBase>();
		if (directive.forcedTargetSlotIndex >= 0
			&& directive.forcedTargetSlotIndex < sys.playerParty.partySlots.Count)
		{
			var targetEntity = sys.playerParty.partySlots[directive.forcedTargetSlotIndex].entity;
			if (targetEntity != null)
				targets.Add(targetEntity);
		}

		// Fallback if no valid target from directive
		if (targets.Count == 0)
			targets = BattleAIController.GetAITargetsForSkill(monster, skill, sys.playerParty, sys.monsterParty);

		return new AIDecision { Skill = skill, Targets = targets };
	}

	private int GetMonsterSlotIndex(EntityBase monster, BattleSystem sys)
	{
		for (int i = 0; i < sys.monsterParty.partySlots.Count; i++)
		{
			if (sys.monsterParty.partySlots[i].entity == monster)
				return i;
		}
		return -1;
	}

	// ─── Tutorial Completion ────────────────────────────────────────

	public IEnumerator RunCompletionFlow()
	{
		var overlay = TutorialOverlayUI.Instance;
		if (overlay != null)
		{
			var completionStep = new TutorialStepData
			{
				guidanceText = scenario?.completionMessage ?? "Tutorial Complete!",
				timing = TutorialStepTiming.OnBattleEnd,
				pointerTarget = TutorialPointerTarget.None,
				interactionMode = TutorialInteractionMode.TapToContinue
			};
			yield return overlay.ShowStep(completionStep);
		}

		End();
		SceneManager.LoadScene("MenuScene");
	}
}
