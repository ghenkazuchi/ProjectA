using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TutorialSequenceRunner : MonoBehaviour
{
	public static TutorialSequenceRunner Instance { get; private set; }
	public bool IsTutorialActive => scenario != null;

	private TutorialScenarioData scenario;
	private int stepIndex;
	private int turnNumber;

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
				// Blocking
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
				activeRestriction = step; 
				yield return overlay.ShowStep(step);
				stepIndex++;
				yield break;
			}
			else if (step.interactionMode == TutorialInteractionMode.WaitForCloseEffectList)
			{
				waitingForEffectListClose = true;
				activeRestriction = step; 
				yield return overlay.ShowStep(step);
				stepIndex++;
				yield break;
			}
		}
	}
	public bool HasActiveRestriction => hasActiveRestriction;

	public bool IsTransitioning { get; private set; }

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
		if (TutorialOverlayUI.Instance != null)
			yield return StartCoroutine(TutorialOverlayUI.Instance.HideOverlay());

		yield return StartCoroutine(RunStepsForPhase(phase));
		IsTransitioning = false;
	}

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

	public void OnTurnAdvanced()
	{
		turnNumber++;
	}

	public bool IsActionAllowed(BattleAction action)
	{
		if (!hasActiveRestriction) return true;
		return action == activeRestriction.requiredActionType;
	}

	public bool IsSkillAllowed(int skillIndex)
	{
		if (!hasActiveRestriction) return true;
		if (!activeRestriction.requireSpecificSkill) return true;
		return skillIndex == activeRestriction.requiredSkillIndex;
	}

	public bool IsTargetAllowed(int targetIndex)
	{
		if (!hasActiveRestriction) return true;
		if (!activeRestriction.requireSpecificTarget) return true;
		return targetIndex == activeRestriction.requiredTargetIndex;
	}
	public bool ShouldPreventDeath()
	{
		if (!IsTutorialActive) return false;
		if (hasActiveRestriction && activeRestriction.preventPlayerDeath) return true;
		if (stepIndex < scenario.steps.Count && scenario.steps[stepIndex].preventPlayerDeath) return true;
		return false;
	}

	//Monster AI
	public AIDecision GetScriptedMonsterDecision(EntityBase monster, BattleSystem sys)
	{
		int slotIndex = GetMonsterSlotIndex(monster, sys);

		var directive = scenario.monsterDirectives?.Find(
			d => d.turnNumber == turnNumber && d.monsterSlotIndex == slotIndex);

		if (directive == null)
			return BattleAIController.ChooseAction(monster, sys);

		// Build forced decision
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

		// Fallback
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
		GameOverUIController.CleanupPersistentObjects();
		SceneManager.LoadScene("MenuScene");
	}
}
